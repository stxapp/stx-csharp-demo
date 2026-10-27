# stx-csharp-demo

Example applications for the STX C# SDK ([`STX.Sdk`](https://www.nuget.org/packages/STX.Sdk) on NuGet), each self-contained in its own folder:

| Folder | Shape | Good for |
|---|---|---|
| [`console/`](./console) | A worker service (`Microsoft.Extensions.Hosting`) that streams order books and your account, and can rest a 1-cent bid protected by cancel-on-disconnect. | Bots, automated strategies, anything that runs without HTTP. |
| [`webapi/`](./webapi) | An ASP.NET Core minimal API in front of the SDK: markets, your live account, place and cancel orders. | Backends and internal tools that expose the exchange over HTTP. |
| [`combo-maker/`](./combo-maker) | A console program that quotes every combo request as a maker and reports how each one ends. | Market makers quoting combos. |
| [`combos/`](./combos) | A console program that buys a small combo from the best maker quote and cashes it out. | Players and apps that offer combos. |

All four target .NET 10 and use `STX.Sdk` 2.

## Prerequisites

- [.NET SDK 10](https://dotnet.microsoft.com/download/dotnet/10.0)
- An account on the STX exchange you plan to use, and an API key from it. Start on a demo exchange, which uses no real money. [Environments](https://docs.stxapp.io/environments/) lists them.

## Your API key

Save the key's private key as `~/.stx/stx-key.pem` and add a profile to `~/.stx/credentials`:

```ini
[default]
region   = ontario
env      = demo
key_id   = your-key-id
key_file = ~/.stx/stx-key.pem
```

`region` and `env` pick the exchange: `us`/`demo`, `ontario`/`demo` or `ontario`/`production`. The apps read this profile, or the one named in `STX_PROFILE`. `STX_REGION`, `STX_ENV`, `STX_KEY_ID` and `STX_PRIVATE_KEY` (a path, or the PEM text) override it, so the apps also run with no credentials file. [Authentication](https://docs.stxapp.io/sdks/csharp/authentication) has the details.

Keep the private key out of source control. A key is `read_only` or `read_write`; placing and cancelling orders needs `read_write`.

## Quickstart

```bash
cd console
dotnet run -- --once --quote
```

```text
Exchange demo.stxapp.ca: key 998bc79d2e3731a3489a004105736353, user ef28f25f-8119-483d-8a70-faf081056f4c, scope read_write
AHC_2602051410__1 bid 16.0000 offer 24.0000
AHC_2602051415__1 bid 34.0000 offer 40.0000
Book 49517949-3cb4-46d8-afe7-3fe87987a48a: best bid 34.0000
Account: available 1999809.2400, 0 open orders, 2 positions
Cancel-on-disconnect armed, timeout 5000 ms
Placed 6567da71-ec9c-48ef-b1ba-2e2b8d11dc42 on STX13AUG226NOLANBESTMOVIE-13AUG226NOLANBESTMOVIE-BAT: accepted 1.00 @ 0.0100
Account orders (new_open_order)
Cancelled 6567da71-ec9c-48ef-b1ba-2e2b8d11dc42: cancelled
```

```bash
cd webapi
dotnet run
# then: curl http://localhost:5088/markets
```

```bash
cd combo-maker
dotnet run
```

```bash
cd combos
dotnet run
```

See [`console/README.md`](./console/README.md), [`webapi/README.md`](./webapi/README.md), [`combo-maker/README.md`](./combo-maker/README.md) and [`combos/README.md`](./combos/README.md) for what each app does.

## Docs

The SDK guide is at [docs.stxapp.io/sdks/csharp](https://docs.stxapp.io/sdks/csharp/).

## License

MIT, see [LICENSE](./LICENSE). Use of the STX API and exchange is subject to the STX terms of use and privacy policy: United States ([terms of use](https://stxapp.io/legal/terms-of-use.pdf), [privacy policy](https://stxapp.io/legal/privacy-policy.pdf)), Ontario ([terms of use](https://stxapp.ca/legal/terms-of-use.pdf), [privacy policy](https://stxapp.ca/legal/privacy-policy.pdf)).

## Contributing

Bug reports and small improvements welcome via pull request. For larger changes, open an issue first so we can discuss fit before you invest time.
