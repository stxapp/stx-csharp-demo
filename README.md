# stx-csharp-demo

Reference applications for the **STX C# SDK** ([`STX.Sdk`](https://www.nuget.org/packages/STX.Sdk) on NuGet).

Two integration patterns, each self-contained in its own folder:

| Folder | Shape | Good for |
|---|---|---|
| [`console/`](./console) | Long-running background worker using `Microsoft.Extensions.Hosting`. Subscribes to channels, reacts to market events, places orders. | Market-maker bots, scheduled jobs, anything that runs 24/7 without HTTP. |
| [`webapi/`](./webapi) | ASP.NET Core 8 Web API that wraps the SDK behind REST endpoints, with Swagger UI. | Backends, internal tools, any service that needs to expose SDK functionality over HTTP. |

Both projects target **.NET 8** (LTS) and depend on `STX.Sdk 1.6.0` from NuGet. Each has its own README with a full quickstart — this page is the overview.

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) or newer
- An STX account on the environment you plan to target, and an API key from Account, API Keys.
  The samples default to the Ontario demo environment, which uses no real money.

## 60-second quickstart

Pick the pattern you want and jump into its folder:

```bash
# Console worker
cd console
export STX_API_KEY_ID="your-key-id"
export STX_API_KEY_PEM_PATH="$HOME/.stx/ontario-demo.pem"
export STX_ENV="ontario-demo"
dotnet run
```

The console worker cancels your resting orders and places new ones, so point it at a
demo environment until you have read what it does.

```bash
# Web API (in a different shell)
cd webapi
export STX_ENV="ontario-demo"
dotnet run
# Open http://localhost:5088/swagger for the UI
```

See [`console/README.md`](./console/README.md) and [`webapi/README.md`](./webapi/README.md) for the full per-project quickstarts, host tables for other envs, and notes on what each sample does.

## Authentication

Two options, and both samples support either. Set the environment variables for the one you want.

**API key (recommended).** Requests are signed per call with Ed25519. There is no login, no
token to expire and no refresh cycle, so a restart or an API deployment cannot leave you
without a session. Create a key under Account, API Keys.

```bash
export STX_API_KEY_ID="your-key-id"
export STX_API_KEY_PEM_PATH="$HOME/.stx/ontario-staging.pem"
```

**Email and password.** Still fully supported.

```bash
export EMAIL="you@example.com" PASSWORD="your-password"
```

Keep the private key out of source control, and pass a path rather than pasting the key into
an environment variable so it never lands in your shell history or process list.

Note that `userProfile` is not reachable with an API key, by design: it returns SSN, date of
birth and address. Use `me` for identity, which is what the `/me` endpoint shows.

## Environments

The SDK carries the hosts, so there are no URLs to assemble. Pick one with `STX_ENV`:

| `STX_ENV` | |
|---|---|
| `ontario-demo` (default) | Ontario, no real money |
| `ontario-production` | Ontario, **live money** |
| `us-demo` | United States, no real money |

```bash
export STX_ENV="ontario-demo"
```

Accounts and API keys do not carry across exchanges: a key issued for Ontario will not
authenticate against the US exchange. The canonical host list, if you need it, is at
[docs.stxapp.io/environments](https://docs.stxapp.io/environments/).

To point a sample somewhere the SDK does not name, set `GRAPHQL_URI` and `CHANNELS_URI`
instead and they take precedence.

## Docs

Full SDK documentation: [docs.stxapp.io](https://docs.stxapp.io) (preview: the Mintlify URL). Start with the [C# quickstart](https://docs.stxapp.io/sdks/csharp/quickstart).

## License

MIT — see [LICENSE](./LICENSE).

## Contributing

Bug reports and small improvements welcome via pull request. For larger changes, open an issue first so we can discuss fit before you invest time.
