# webapi — STX C# SDK ASP.NET Web API sample

ASP.NET Core 8 Web API that exposes SDK functionality over REST. Controllers for market data, orders, portfolio, positions, trades, settlements, and auth. Channel wrappers (one per Phoenix channel) manage subscriptions lifecycle.

**Use as a starting point for:** backend services that proxy SDK calls, internal trading dashboards, or any app that needs an HTTP layer in front of the SDK.

## Quickstart

### 1. Prereqs

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- An STX account on the environment you plan to target

### 2. Configure endpoints

Two environment variables are read at startup. Staging values shown:

```bash
export GRAPHQL_URI="https://api-staging.on.sportsxapp.com/graphql"
export CHANNELS_URI="wss://api-staging.on.sportsxapp.com/socket/websocket"
```

Credentials are not taken from the environment — this is an API; callers authenticate via the `/api/Login` endpoint (see below).

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

Swagger UI is wired up — open `http://localhost:5088/swagger` for an interactive browser.

### 4. Log in

POST to `/api/Login` with JSON body:

```bash
curl -X POST http://localhost:5088/api/Login \
  -H 'Content-Type: application/json' \
  -d '{"email": "you@example.com", "password": "your-password"}'
```

The SDK caches the JWT in the service; subsequent calls to other endpoints use it automatically.

## Endpoints

Each controller maps to one SDK surface:

| Controller | What it exposes |
|---|---|
| `LoginController` | Email/password login, 2FA confirm |
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

"Simple" variants use the `SimpleXxxChannelWrapper` classes in `Services/` — they buffer incoming channel messages into an in-memory queue so HTTP handlers can pull the latest snapshot on demand.

## Docker

```bash
docker build -t stx-csharp-webapi .
docker run -p 8080:8080 \
  -e GRAPHQL_URI="https://api-staging.on.sportsxapp.com/graphql" \
  -e CHANNELS_URI="wss://api-staging.on.sportsxapp.com/socket/websocket" \
  stx-csharp-webapi
```

## Further reading

- [C# quickstart](https://docs.stxapp.io/sdks/csharp/quickstart)
- [Authentication](https://docs.stxapp.io/sdks/csharp/authentication)
- [Market data](https://docs.stxapp.io/sdks/csharp/markets)
- [Trading](https://docs.stxapp.io/sdks/csharp/trading)
- [WebSockets](https://docs.stxapp.io/sdks/csharp/websockets)
