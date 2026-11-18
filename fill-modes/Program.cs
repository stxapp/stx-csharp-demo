// Post-only and fill-or-kill limit orders, one market, one account.
//
//   1. A post-only bid at 1 cent rests on the book as a maker; it is then cancelled.
//   2. A post-only bid at the best offer would match on arrival, so the exchange cancels it
//      (cancellation_reason auto_matching) and nothing trades.
//   3. A fill-or-kill bid fills in full or not at all. At 1 cent nothing can fill it, so it ends
//      cancelled with nothing filled. With --fill it bids at the best offer and ends filled
//      (this buys 1 contract).
//
// It needs a read_write API key.
//
//   dotnet run                          # steps 1 to 3, nothing traded
//   dotnet run -- --fill                # step 3 at the best offer: buys 1 contract
//   dotnet run -- --profile my-demo     # a named profile from ~/.stx/credentials
//
// An exchange without fill modes may ignore FillMode and place a normal order instead. The
// sample checks the first order's FillMode, and if it came back as a normal order it cancels it
// and stops before the steps that would trade.

using System.Globalization;
using STX.Sdk;
using STX.Sdk.Models;

using var client = new STXClient(new STXClientOptions { Profile = Arg("--profile") });

var market = await PickMarketAsync();
if (market is null)
{
    Console.WriteLine($"No open market on {client.Host} with offers above 1 cent and an event at least three hours out.");
    return 1;
}
var bestOffer = market.Offers!.Select(o => o.Price!).MinBy(Dec)!;
Console.WriteLine($"Market:  {market.Question ?? market.Title}  ({market.MarketId})");
Console.WriteLine($"Best offer: {bestOffer}");
Console.WriteLine();

// 1. Post-only at 1 cent: rests as a maker.
Order resting;
try
{
    resting = await client.PlaceOrderAsync(Bid("0.01", STXFillMode.PostOnly));
}
catch (STXNotAvailableException ex)
{
    Console.WriteLine($"Post-only is not available: {ex.Message}");
    return 0;
}
if (resting.FillMode != STXFillMode.PostOnly)
{
    // An exchange from before fill modes placed a normal order.
    await client.CancelOrderAsync(resting.Id!);
    Console.WriteLine($"This exchange placed the post-only order as a normal order ({resting.Status}, fill mode {resting.FillMode ?? "none"}); cancelled it.");
    Console.WriteLine("Fill modes are not offered here, so the steps that would trade are skipped.");
    return 0;
}
Console.WriteLine($"Resting: {resting.Id} {resting.Status} {resting.FillMode} @ {resting.Price}");
var cancelled = await client.CancelOrderAsync(resting.Id!);
Console.WriteLine($"Cancelled: {cancelled.Status}");

// 2. Post-only at the best offer: it would match, so the exchange does not place it.
try
{
    var crossed = await client.PlaceOrderAsync(Bid(bestOffer, STXFillMode.PostOnly));
    Console.WriteLine($"Post-only at {bestOffer} was placed: {crossed.Id} {crossed.Status}; cancelling.");
    await client.CancelOrderAsync(crossed.Id!);
}
catch (STXPostOnlyWouldMatchException ex)
{
    // auto_matching: the order is returned cancelled. match_with_self: your own offer was first in line.
    Console.WriteLine($"Would cross: {ex.Reason}; order {ex.Order?.Id} {ex.Order?.Status} ({ex.Order?.CancellationReason}), filled {ex.Order?.Filled ?? "0"}");
}

// 3. Fill-or-kill: filled in full, or cancelled with nothing filled.
var fokPrice = args.Contains("--fill") ? bestOffer : "0.01";
var fok = await client.PlaceOrderAsync(Bid(fokPrice, STXFillMode.FillOrKill));
Console.WriteLine($"Fill-or-kill placed: {fok.Id} {fok.Status} {fok.FillMode} @ {fok.Price}");
var final = fok;
for (var i = 0; i < 30 && final.Status is not ("filled" or "cancelled" or "rejected"); i++)
{
    await Task.Delay(1000);
    try { final = await client.GetOrderAsync(fok.Id!); }
    catch (STXNotFoundException) { } // the read can trail the placement by a moment
}
Console.WriteLine($"Fill-or-kill: {final.Status}, filled {final.Filled ?? "0"}{(final.CancellationReason is { } r ? $" ({r})" : "")}");
return 0;

STXOrderRequest Bid(string price, string fillMode) => new()
{
    MarketId = market.MarketId!, Action = STXOrderAction.Buy, OrderType = STXOrderType.Limit,
    Quantity = "1", Price = price, FillMode = fillMode,
};

// An open, trading market whose event starts at least three hours out, with offers, none at 1 cent.
async Task<Market?> PickMarketAsync()
{
    var cutoff = DateTimeOffset.UtcNow.AddHours(3);
    await foreach (var m in client.GetAllMarketsAsync(status: ["open"], trading: true, sortBy: "event_start", sortDirection: "asc", limit: 200))
    {
        if (m.EventStatus != "scheduled" || m.Offers is not { Count: > 0 } offers) continue;
        if (!DateTimeOffset.TryParse(m.EventStart, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var start) || start <= cutoff) continue;
        if (offers.Any(o => Dec(o.Price!) <= 0.01m)) continue;
        return m;
    }
    return null;
}

static decimal Dec(string s) => decimal.Parse(s, CultureInfo.InvariantCulture);

string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
