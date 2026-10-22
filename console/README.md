# console: a worker on STX.Sdk

A worker service built on `Microsoft.Extensions.Hosting`. On start it:

1. checks the API key (`GetMeAsync`),
2. lists three open markets with their best bid and offer,
3. opens one socket, streams the order books of those markets, and keeps a live copy of your account (balance, open orders, fills, positions) with `AccountViewAsync`,
4. with `--quote`, arms cancel-on-disconnect on a second socket and rests a 1-cent bid on a market whose event has not started. If the process dies or loses its connection, the exchange cancels that order. On shutdown the worker cancels it itself.

## Run

Set up your key as in the [main README](../README.md), then:

```bash
dotnet run                         # stream until Ctrl+C
dotnet run -- --quote              # also rest a 1-cent bid while running
dotnet run -- --once --quote       # one pass: place, check, cancel, exit
dotnet run -- --profile my-demo    # a named profile from ~/.stx/credentials
```

On production a 1-cent order uses real money, even though it rests below the market and is cancelled when the worker stops.

## Where to look

- `Program.cs`: the host and the command-line flags.
- `Worker.cs`: everything that talks to the exchange.

The [WebSockets guide](https://docs.stxapp.io/sdks/csharp/websockets/) covers the channels, the account view and reconnects; [Trading](https://docs.stxapp.io/sdks/csharp/trading/) covers orders and cancel-on-disconnect.
