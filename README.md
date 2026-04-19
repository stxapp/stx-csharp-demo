# stx-csharp-demo

Reference applications for the **STX C# SDK** ([`STX.Sdk`](https://www.nuget.org/packages/STX.Sdk) on NuGet).

Two integration patterns, each self-contained in its own folder:

| Folder | Shape | Good for |
|---|---|---|
| [`console/`](./console) | Long-running background worker using `Microsoft.Extensions.Hosting`. Subscribes to channels, reacts to market events, places orders. | Market-maker bots, scheduled jobs, anything that runs 24/7 without HTTP. |
| [`webapi/`](./webapi) | ASP.NET Core 8 Web API that wraps the SDK behind REST endpoints, with Swagger UI. | Backends, internal tools, any service that needs to expose SDK functionality over HTTP. |

Both projects target **.NET 8** (LTS) and depend on `STX.Sdk 1.4.2+` from NuGet. Each has its own README with a full quickstart — this page is the overview.

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- An STX account on the environment you plan to target (staging recommended for first runs)

## 60-second quickstart

Pick the pattern you want and jump into its folder:

```bash
# Console worker
cd console
export EMAIL="you@example.com" PASSWORD="your-password"
export GRAPHQL_URI="https://api-staging.on.sportsxapp.com/api/graphql"
export CHANNELS_URI="wss://api-staging.on.sportsxapp.com/socket/websocket"
dotnet run
```

```bash
# Web API (in a different shell)
cd webapi
export GRAPHQL_URI="https://api-staging.on.sportsxapp.com/api/graphql"
export CHANNELS_URI="wss://api-staging.on.sportsxapp.com/socket/websocket"
dotnet run
# Open http://localhost:5088/swagger for the UI
```

See [`console/README.md`](./console/README.md) and [`webapi/README.md`](./webapi/README.md) for the full per-project quickstarts, host tables for other envs, and notes on what each sample does.

## Host table

Both projects take endpoints as env vars so you can switch environments without code changes:

| Env | GraphQL | Channels |
|---|---|---|
| production | `https://api.on.sportsxapp.com/api/graphql` | `wss://api.on.sportsxapp.com/socket/websocket` |
| staging | `https://api-staging.on.sportsxapp.com/api/graphql` | `wss://api-staging.on.sportsxapp.com/socket/websocket` |
| dev | `https://api-dev.on.sportsxapp.com/api/graphql` | `wss://api-dev.on.sportsxapp.com/socket/websocket` |
| qa | `https://api-qa.on.sportsxapp.com/api/graphql` | `wss://api-qa.on.sportsxapp.com/socket/websocket` |

## Docs

Full SDK documentation: [docs.stxapp.io](https://docs.stxapp.io) (preview: the Mintlify URL). Start with the [C# quickstart](https://docs.stxapp.io/sdks/csharp/quickstart).

## License

MIT — see [LICENSE](./LICENSE).

## Contributing

Bug reports and small improvements welcome via pull request. For larger changes, open an issue first so we can discuss fit before you invest time.
