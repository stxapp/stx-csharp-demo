# webapi: an ASP.NET Core API on STX.Sdk

A minimal API in front of the SDK. One `STXClient` is registered as a singleton; a hosted service keeps a socket open with a live account view, so `/account` answers without a call to the exchange.

| Endpoint | What |
|---|---|
| `GET /markets?limit=10` | Open markets with best bid and offer |
| `GET /markets/{id}` | One market, in full |
| `GET /me` | The key's user and scope |
| `GET /account` | Balance, open orders, fills and positions, kept live |
| `GET /orders` | Your working orders |
| `POST /orders` | Place an order: `{"marketId": "...", "action": "buy", "orderType": "limit", "price": "0.01", "quantity": "1"}` |
| `DELETE /orders/{id}` | Cancel an order |

## Run

Set up your key as in the [main README](../README.md), then:

```bash
dotnet run
curl http://localhost:5088/markets?limit=3
curl http://localhost:5088/account
```

`Stx:Profile` in `appsettings.json` (or `STX_PROFILE`) picks a named profile. Prices and quantities are decimal strings in and out, exactly as the exchange sends them.

## Where to look

- `Program.cs`: the client registration and the endpoints.
- `AccountFeed.cs`: the socket and the live account view.
