# stx-csharp-demo

Reference applications for the **STX C# SDK** ([`STX.Sdk`](https://www.nuget.org/packages/STX.Sdk) on NuGet).

Two shapes of integration are covered, each in its own folder:

| Folder | Shape | Good for |
|---|---|---|
| [`console/`](./console) | Long-running background worker using `Microsoft.Extensions.Hosting`. Subscribes to channels, reacts to market events, places orders. | Market-maker bots, scheduled jobs, anything that runs 24/7 without HTTP. |
| [`webapi/`](./webapi) | ASP.NET Web API that wraps the SDK behind REST endpoints. | Backends, internal tools, any service that needs to expose SDK functionality to other clients. |

Both projects target **.NET 7+** and depend on `STX.Sdk` from NuGet.

## Prerequisites

- [.NET SDK 7.0 or newer](https://dotnet.microsoft.com/download)
- An STX account on the environment you plan to target (staging / production)
- Account credentials exported as environment variables — see each project's `appsettings.json` / config for the exact keys it reads

## Running

```bash
# Console worker
cd console
dotnet run

# Web API
cd webapi
dotnet run
# Then hit http://localhost:5000 (or whatever Kestrel binds to on your box)
```

First-run auth: both projects log in with email + password on startup and cache the resulting JWT in memory for the life of the process.

## Docs

Full SDK documentation lives at [docs.stxapp.io](https://docs.stxapp.io) (preview: the Mintlify URL).

- [Quickstart](https://docs.stxapp.io/sdks/csharp/quickstart)
- [Authentication](https://docs.stxapp.io/sdks/csharp/authentication)
- [Market data](https://docs.stxapp.io/sdks/csharp/markets)
- [Trading](https://docs.stxapp.io/sdks/csharp/trading)
- [WebSockets](https://docs.stxapp.io/sdks/csharp/websockets)

## License

MIT — see [LICENSE](./LICENSE).

## Contributing

Bug reports and small improvements welcome via pull request. For larger changes, open an issue first so we can discuss fit before you invest time.
