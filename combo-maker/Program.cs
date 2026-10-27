// Quote every combo request as a maker, and report how each one ends.
//
// Players ask for quotes on a combo: a set of legs and a stake to buy, or a combo they hold to cash
// out. Every maker joined to its combo_quote channel receives each request. This program quotes all
// of them and prints what happens next: your quote placed (quote_accepted, then placed), another
// maker's placed instead (placed_elsewhere), your quote expiring, and the request closing. It needs
// a read_write API key.
//
//   dotnet run                                        # quote until Ctrl+C
//   dotnet run -- --seconds 120                       # stop after 120 s
//   dotnet run -- --win-multiple 12 --cash-out-price 0.10 --quote-seconds 60
//   dotnet run -- --profile my-demo                   # a named profile from ~/.stx/credentials
//   dotnet run -- --player <external_id>              # quote only that player's requests
//
// On a buy you quote what the player wins: the stake times --win-multiple (default 9). On a cash
// out you quote what you pay for everything the player sells: --cash-out-price a contract (default
// 0.08). Each quote stands for --quote-seconds (default 30).

using System.Globalization;
using STX.Sdk;
using STX.Sdk.Combos;

var winMultiple = decimal.Parse(Arg("--win-multiple") ?? "9", CultureInfo.InvariantCulture);
var cashOutPrice = decimal.Parse(Arg("--cash-out-price") ?? "0.08", CultureInfo.InvariantCulture);
var quoteMs = (int)(double.Parse(Arg("--quote-seconds") ?? "30", CultureInfo.InvariantCulture) * 1000);
// The external_id a player's requests carry; when set, other players' requests are ignored.
var player = Arg("--player");
var runSeconds = Arg("--seconds") is { } s ? double.Parse(s, CultureInfo.InvariantCulture) : (double?)null;

// Ctrl+C, or --seconds running out, ends the run.
using var stop = runSeconds is { } secs ? new CancellationTokenSource(TimeSpan.FromSeconds(secs)) : new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; stop.Cancel(); };

using var client = new STXClient(new STXClientOptions { Profile = Arg("--profile") });

// The SDK signs the socket, sends heartbeats and reconnects. Quotes belong to the connection that
// placed them: a drop withdraws them, so there is nothing to clean up.
await using var ws = client.CreateWebSocket(new STXWebSocketOptions
{
    OnReconnect = () =>
    {
        Console.WriteLine("[reconnected]  quotes from before the drop were withdrawn");
        return Task.CompletedTask;
    },
});

STXComboQuoteChannel maker;
try
{
    maker = await ws.ComboQuoteAsync();
}
catch (STXCombosUnavailableException)
{
    Console.WriteLine($"Combos are not available on {client.Host}.");
    return 1;
}

Console.WriteLine($"Quoting combos on {client.Host} as a maker.");
Console.WriteLine($"Buys: win {winMultiple}x the stake. Cash outs: {cashOutPrice} a contract. Each quote stands {quoteMs / 1000} s. Ctrl+C to stop.");
Console.WriteLine();

try
{
    await foreach (var msg in maker.ReadAllAsync(stop.Token))
    {
        switch (msg.Data)
        {
            case RfqOpened request:
                // Every request opened after the join, from every player, your own included.
                if (player is not null && request.ExternalId != player) break;
                Console.WriteLine($"[request]  {Short(request.RfqId)}  {Describe(request)}");
                await QuoteAsync(request);
                break;
            case QuoteAccepted accepted:
                // The player took your quote; the exchange is placing both orders now.
                Console.WriteLine($"[accepted] {Short(accepted.RfqId)}  quote {accepted.QuoteId}");
                break;
            case MakerPlacement placed:
                Console.WriteLine($"[won]      {Short(placed.RfqId)}  order {placed.OrderId}  {placed.Quantity} @ {placed.Price}  combo {placed.MarketId}");
                break;
            case NegotiationClosed closed:
                // One of your quotes stopped standing, once per quote, with the reason.
                var label = closed.Reason switch { "placed_elsewhere" => "[lost]   ", "expired" => "[expired]", _ => "[ended]  " };
                Console.WriteLine($"{label}  {Short(closed.RfqId)}  quote {closed.QuoteId}  {closed.Reason}");
                break;
            case RfqClosed ended:
                Console.WriteLine($"[closed]   {Short(ended.RfqId)}  {ended.Reason}");
                break;
        }
    }
}
catch (OperationCanceledException)
{
}

Console.WriteLine();
Console.WriteLine("Stopped. Quotes still standing were withdrawn with the connection.");
return 0;

async Task QuoteAsync(RfqOpened request)
{
    var amount = MakerQuote.Amount(request, winMultiple, cashOutPrice);
    if (amount is null)
    {
        Console.WriteLine($"[skip]     {Short(request.RfqId)}  no amount prices inside 0 to {request.MaxPrice}");
        return;
    }
    try
    {
        var quote = await maker.QuoteAsync(request.RfqId!, amount, quoteMs, clientOrderId: $"maker-{Short(request.RfqId)}", ct: stop.Token);
        var what = request.Side == "buy" ? $"win {quote.Amount}" : $"pay {quote.Amount} for {request.Quantity}";
        Console.WriteLine($"[quoted]   {Short(request.RfqId)}  {what}  for {quoteMs / 1000} s  quote {quote.QuoteId}");
    }
    catch (STXComboException ex)
    {
        // e.g. cannot_quote_own_request, or not_found when the request ended first
        Console.WriteLine($"[refused]  {Short(request.RfqId)}  {ex.ComboReason}");
    }
}

static string Describe(RfqOpened request)
{
    if (request.Side == "buy")
    {
        var legs = string.Join(", ", (request.Legs ?? []).Select(leg => $"{leg.ShortName ?? Short(leg.MarketId)} {leg.Outcome}"));
        return $"buy, stake {request.Amount}, {request.Legs?.Count} legs: {legs}";
    }
    return $"cash out of {request.Quantity} on combo {Short(request.MarketId)}";
}

static string Short(string? id) => id is null ? "-" : id[..Math.Min(8, id.Length)];

string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
