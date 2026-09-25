# webapi: STX C# SDK ASP.NET Web API sample

ASP.NET Core 8 Web API that exposes SDK functionality over REST. Controllers for market data, orders, portfolio, positions, trades, settlements, and auth. Channel wrappers (one per Phoenix channel) manage subscriptions lifecycle.

**Use as a starting point for:** backend services that proxy SDK calls, internal trading dashboards, or any app that needs an HTTP layer in front of the SDK.

## Quickstart

### 1. Prereqs

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- An STX account on the environment you plan to target

### 2. Configure endpoints

Two environment variables are read at startup. Staging values shown:

```bash
export STX_ENV="ontario-demo"
```

The API key is read from the environment at startup:

```bash
export STX_API_KEY_ID="your-key-id"
export STX_API_KEY_PEM_PATH="$HOME/.stx/ontario-demo.pem"
```

### 3. Run

```bash
dotnet run
```

Expected output:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5088
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

Swagger UI is wired up: open `http://localhost:5088/swagger` for an interactive browser.

### 4. Check who you are

There is no login step. Requests are signed with the API key, and the host resolves the
key's identity at startup so user-scoped endpoints know which user to ask about:

```bash
curl http://localhost:5088/api/Identity
```

That returns the user id, account id and the key's scope.

## Endpoints

Each controller maps to one SDK surface:

| Controller | What it exposes |
|---|---|
| `IdentityController` | Who the key belongs to, and its scope (`/api/Identity`) |
| `MarketController` | Market listings (`/api/Market` and `/api/Market/generic`) |
| `OrderController` | Place, cancel, list orders |
| `PortfolioController` / `SimplePortfolioController` | Portfolio snapshot + live channel |
| `PositionsController` / `SimplePositionsController` | Position snapshot + live channel |
| `ActiveOrdersController` / `SimpleActiveOrdersController` | Live order-book channel |
| `ActiveTradesController` / `SimpleActiveTradesController` | Live trades channel |
| `ActiveSettlementsController` / `SimpleActiveSettlementsController` | Live settlements channel |
| `ProfileController` | User profile read/update |
| `TokenController` | Token introspection |
| `WorkerController` | Lifecycle endpoints for the background `STXWorker` |

Prices, amounts, balances, fees, P&L and quantities are returned as strings, for example
`"price": "0.5600"` and `"quantity": "2.00"`. `Services/AmountStringsJsonModifier.cs` writes
each field with the SDK's `String` companion (`PriceString`, `QuantityString`, ...) under the
field's own name. Order requests still take `price` in cents and `quantity` as a whole number.

"Simple" variants use the `SimpleXxxChannelWrapper` classes in `Services/`. They buffer incoming channel messages into an in-memory queue so HTTP handlers can pull the latest snapshot on demand.

## Docker

```bash
docker build -t stx-csharp-webapi .
docker run -p 8080:8080 \
  -e STX_ENV="ontario-demo" \
  stx-csharp-webapi
```

## Further reading

- [C# quickstart](https://docs.stxapp.io/sdks/csharp/quickstart)
- [Authentication](https://docs.stxapp.io/sdks/csharp/authentication)
- [Market data](https://docs.stxapp.io/sdks/csharp/markets)
- [Trading](https://docs.stxapp.io/sdks/csharp/trading)
- [WebSockets](https://docs.stxapp.io/sdks/csharp/websockets)
