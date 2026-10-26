using System.Globalization;
using STX.Sdk.Combos;

/// <summary>The amount this maker quotes on a request.</summary>
static class MakerQuote
{
    /// <summary>
    /// The amount to quote on <paramref name="request"/>, in whole cents, or null when the exchange
    /// would refuse it. On a buy it is what the player wins: the stake times
    /// <paramref name="winMultiple"/>. On a cash out it is what you pay for the whole size:
    /// <paramref name="cashOutPrice"/> a contract. The exchange derives a price from the amount,
    /// rounded to the request's tick in your favour, and refuses a quote whose price is not above 0
    /// and below max_price (price_out_of_range), or, on a buy, whose stake buys no whole contract
    /// (below_one_contract). This checks the same rules first.
    /// </summary>
    public static string? Amount(RfqOpened request, decimal winMultiple, decimal cashOutPrice)
    {
        var maxPrice = Dec(request.MaxPrice);
        var tick = Dec(request.TickSize);
        decimal amount, price;
        if (request.Side == "buy")
        {
            var stake = Dec(request.Amount);
            amount = Math.Floor(stake * winMultiple * 100) / 100;
            if (amount <= 0) return null;
            // max_price x stake / (stake + amount), rounded up to the tick.
            price = Math.Ceiling(maxPrice * stake / (stake + amount) / tick) * tick;
            if (price <= 0 || price >= maxPrice || Math.Floor(stake / price) < 1) return null;
        }
        else
        {
            var quantity = Dec(request.Quantity);
            amount = Math.Floor(quantity * cashOutPrice * 100) / 100;
            if (quantity <= 0) return null;
            // amount / quantity, rounded down to the tick.
            price = Math.Floor(amount / quantity / tick) * tick;
            if (price <= 0 || price >= maxPrice) return null;
        }
        return amount.ToString("F2", CultureInfo.InvariantCulture);
    }

    private static decimal Dec(string? value) => decimal.Parse(value ?? "0", CultureInfo.InvariantCulture);
}
