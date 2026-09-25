# console: STX C# SDK worker sample

Long-running background worker using `Microsoft.Extensions.Hosting`. Logs in on startup, pulls a set of open markets, subscribes to market / orders / trades / portfolio channels, then enters a loop that cancels outstanding orders and places fresh random-price orders on each open market every second.

**Use as a starting point for:** market-maker bots, automated strategies, scheduled jobs, or anything that runs without HTTP.

## Quickstart

### 1. Prereqs

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- An STX account on the environment you plan to target

### 2. Configure credentials + endpoints

The app reads three environment variables. `STX_ENV` names a published environment, so there are no URLs to assemble.

```bash
export STX_API_KEY_ID="your-key-id"
export STX_API_KEY_PEM_PATH="$HOME/.stx/ontario-demo.pem"
export STX_ENV="ontario-demo"
```

Other environments: the canonical host list lives at
[docs.stxapp.io/environments](https://docs.stxapp.io/environments/). Take the base URL
for the environment you want and replace the host in both `GRAPHQL_URI` and
`CHANNELS_URI`.

### 3. Run

```bash
dotnet run
```

Expected output once authenticated:

```
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: STXWorker[0]
      Starting STX Worker
Cancelling orders.
Placing orders on market: mkt_abc123...
Placing order no. 1 on market: mkt_abc123...
Order placed: ord_xyz456... BUY 1.00 @ 5.6200 status OPEN, filled 0.00
Portfolio: balance 1000.0000, available 994.3800, fees 0.0000, settlement P&L 0.0000
```

Prices, balances and quantities are logged with the SDK's `String` companions
(`order.PriceString`, `portfolio.AvailableBalanceString`, `order.QuantityString`): money
as a dollar string with at least four decimals, quantities with at least two. Placing an
order still takes the price in cents and a whole number of contracts.

Press `Ctrl+C` to stop.

> **Warning:** `_ordersPerMarketLimit` and `_orderMaxQuantity` in `STXWorker.cs` are conservative demo values, but this code places **real orders** (at random prices!) on whatever env you point it at. Use staging for first runs, and don't connect it to production until you've reviewed the logic end-to-end.

## What's in here

| File | What it does |
|---|---|
| `Program.cs` | Entry point: wires DI via `ConfigureSTXServices(...)` and adds `STXWorker` as a hosted service. |
| `STXWorker.cs` | Main loop: resolve identity with `GetMeAsync`, fetch markets, subscribe to channels, cancel and re-place orders every second. |

## Further reading

Full SDK reference lives at the docs site:

- [C# quickstart](https://docs.stxapp.io/sdks/csharp/quickstart)
- [Authentication](https://docs.stxapp.io/sdks/csharp/authentication)
- [Market data](https://docs.stxapp.io/sdks/csharp/markets)
- [Trading](https://docs.stxapp.io/sdks/csharp/trading)
- [WebSockets](https://docs.stxapp.io/sdks/csharp/websockets)
