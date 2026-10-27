// Buy a small combo as a player, then cash it out, over one WebSocket.
//
// A combo is one market over two or more legs: it pays only if every leg wins. You ask makers to
// quote on the legs and a stake, they quote what you would win, and accepting a quote places an
// order for you on the combo's own market. Cashing out works the same way in reverse, so the run
// ends holding nothing. It needs a read_write API key.
//
//   dotnet run                                  # a 1.00 stake on two legs
//   dotnet run -- --stake 2.50 --legs 3         # a 2.50 stake on three legs
//   dotnet run -- --wait 60                     # wait up to 60 s for a first quote
//   dotnet run -- --sport Football              # legs from one sport
//   dotnet run -- --profile my-demo             # a named profile from ~/.stx/credentials
//
// Makers answer at their own pace, so once the first quote is live the program keeps listening
// for --gather seconds (default 3) and then takes the best quote standing, at its exact amount.

using System.Globalization;
using STX.Sdk;
using STX.Sdk.Combos;
using STX.Sdk.Models;

var stake = Arg("--stake") ?? "1.00";
var legCount = int.Parse(Arg("--legs") ?? "2", CultureInfo.InvariantCulture);
var wait = TimeSpan.FromSeconds(double.Parse(Arg("--wait") ?? "30", CultureInfo.InvariantCulture));
var gather = TimeSpan.FromSeconds(double.Parse(Arg("--gather") ?? "3", CultureInfo.InvariantCulture));
var sport = Arg("--sport");

using var client = new STXClient(new STXClientOptions { Profile = Arg("--profile") });

// Closing the socket ends any request still open.
string comboId;
await using (var ws = client.CreateWebSocket())
{
    // 1. Join your player channel. An exchange without combos refuses the join.
    STXRfqChannel rfq;
    try
    {
        rfq = await ws.RfqAsync();
    }
    catch (STXCombosUnavailableException)
    {
        Console.WriteLine($"Combos are not available on {client.Host}.");
        return 1;
    }

    // 2. The legs: open markets from different events that start at least two hours from now.
    var legs = await PickLegsAsync();
    if (legs.Count < legCount)
    {
        Console.WriteLine($"Found {legs.Count} open market(s) from different events; {legCount} legs need {legCount}.");
        return 1;
    }
    for (var i = 0; i < legs.Count; i++)
        Console.WriteLine($"Leg {i + 1}:   {legs[i].Question ?? legs[i].Title}  ({legs[i].ShortTitle})");

    // 3. Ask for quotes. Every outcome is "won": the combo pays if all the legs win.
    QuoteRequest request;
    try
    {
        request = await rfq.RequestQuoteAsync(legs.Select(m => new ComboLeg(m.MarketId!, "won")), stake);
    }
    catch (STXComboException ex)
    {
        Console.WriteLine($"Request refused: {ex.ComboReason}"); // e.g. insufficient_assets, leg_not_quotable
        return 1;
    }
    Console.WriteLine();
    Console.WriteLine($"Request: {request.RfqId}  stake {stake}");

    // 4. Take the best quote: what you win on your stake.
    var (bought, boughtQuote) = await TakeBestAsync(rfq, request.RfqId!, "win");
    if (bought is null)
    {
        await CancelQuietlyAsync(rfq, request.RfqId!);
        return 1;
    }
    comboId = bought.MarketId!;
    Console.WriteLine($"Bought:  {bought.Quantity} @ {bought.Price}  order {bought.OrderId}  quote {boughtQuote}  combo {comboId}");

    // 5. Cash out: a request to sell everything you hold of the combo. A fill counts toward your
    //    position a moment after it trades, so nothing_to_sell is retried for a few seconds.
    CashOut? cashOut = null;
    for (var attempt = 1; cashOut is null; attempt++)
    {
        try { cashOut = await rfq.CashOutAsync(comboId); }
        catch (STXComboException ex) when (ex.ComboReason == "nothing_to_sell" && attempt < 15) { await Task.Delay(1000); }
    }
    Console.WriteLine();
    Console.WriteLine($"Cash out: {cashOut!.RfqId}  selling {cashOut.Quantity}");
    var (sold, soldQuote) = await TakeBestAsync(rfq, cashOut.RfqId!, "pays");
    if (sold is null)
    {
        await CancelQuietlyAsync(rfq, cashOut.RfqId!);
        Console.WriteLine($"You still hold {cashOut.Quantity} of combo {comboId}. Run again later, or sell it on its order book.");
        return 1;
    }
    Console.WriteLine($"Sold:    {sold.Quantity} @ {sold.Price}  order {sold.OrderId}  quote {soldQuote}");
}

// 6. Confirm nothing is left: no position, and no resting order on the combo (the unfilled rest of
//    an order the maker did not fully take). A trade shows in your position a moment after it fills.
var position = "0.00";
for (var i = 0; i < 10; i++)
{
    var held = await client.GetPositionsAsync([comboId]);
    position = held.FirstOrDefault()?.NetPosition ?? "0.00";
    if (decimal.Parse(position, CultureInfo.InvariantCulture) == 0) break;
    await Task.Delay(1000);
}
var resting = await client.GetOrdersAsync(marketIds: [comboId], status: ["open", "delayed"]);
foreach (var order in resting) await client.CancelOrderAsync(order.Id!);
Console.WriteLine($"Final:   position {position} on the combo  open orders {(resting.Count > 0 ? $"{resting.Count}, cancelled" : "0")}");
return decimal.Parse(position, CultureInfo.InvariantCulture) == 0 ? 0 : 1;

// Waits up to --wait for a live quote, listens --gather longer, then accepts the best at its exact
// amount. If the best moves before the accept lands, it takes the new best the refusal carries.
async Task<(ComboPlacement? Placement, string? QuoteId)> TakeBestAsync(STXRfqChannel rfq, string rfqId, string what)
{
    BestQuote? best = null;
    var until = DateTime.UtcNow + wait;
    while (DateTime.UtcNow < until)
    {
        STXChannelMessage msg;
        try { msg = await rfq.NextAsync(until - DateTime.UtcNow); }
        catch (TimeoutException) { break; }
        if (msg.Data is NegotiationClosed closed && closed.RfqId == rfqId)
        {
            Console.WriteLine($"The request ended without a trade: {closed.Reason}.");
            return (null, null);
        }
        if (msg.Data is not BestQuote quote || quote.RfqId != rfqId) continue;
        Console.WriteLine($"[quote]  {(quote.Amount is null ? "none live" : $"{what} {quote.Amount}")}  quote {quote.QuoteId}");
        best = quote.QuoteId is null ? null : quote;
        if (best is not null && until - DateTime.UtcNow > gather) until = DateTime.UtcNow + gather;
    }
    while (best is not null)
    {
        try
        {
            return (await rfq.AcceptAsync(rfqId, best.QuoteId!, best.Amount!), best.QuoteId);
        }
        catch (STXComboPriceMovedException ex)
        {
            best = ex.BestQuote.QuoteId is null ? null : ex.BestQuote;
        }
        catch (STXComboException ex)
        {
            Console.WriteLine($"Accept refused: {ex.ComboReason}");
            return (null, null);
        }
    }
    Console.WriteLine($"No quote arrived within {wait.TotalSeconds:0} s. Makers quote combos as they choose; try again, or wait longer with --wait 60.");
    return (null, null);
}

async Task<List<Market>> PickLegsAsync()
{
    var soon = DateTimeOffset.UtcNow.AddHours(2);
    var legs = new List<Market>();
    var events = new HashSet<string>();
    var seen = 0;
    await foreach (var m in client.GetAllMarketsAsync(status: ["open"], trading: true, sports: sport is null ? null : [sport], limit: 200))
    {
        if (++seen > 1000 || legs.Count == legCount) break;
        if (m.MarketId is null || m.Archived == true || m.EventType == "combo") continue;
        if (m.EventStatus is not (null or "scheduled")) continue;
        if (!DateTimeOffset.TryParse(m.EventStart, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var start) || start <= soon) continue;
        if (!events.Add(m.EventId ?? m.MarketId)) continue;
        legs.Add(m);
    }
    return legs;
}

static async Task CancelQuietlyAsync(STXRfqChannel rfq, string rfqId)
{
    try { await rfq.CancelAsync(rfqId); } catch (STXComboException) { }
}

string? Arg(string name)
{
    var i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}
