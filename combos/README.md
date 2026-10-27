# combos: buy a combo and cash it out on STX.Sdk

A console program that buys a small combo as a player and then cashes it out, over one WebSocket, so the run ends holding nothing. A combo is one market over two or more legs: it pays only if every leg wins. It needs a `read_write` API key.

1. It joins your `rfq` channel and picks the legs: open markets from different events that start at least two hours from now.
2. It asks makers for quotes on the legs and a stake (`--stake`, default `1.00`). Makers quote what you would win.
3. Once the first quote is live it keeps listening for `--gather` seconds (default `3`), then accepts the best quote at its exact amount. If the best moves before the accept lands, it takes the new best.
4. It cashes out: a request to sell everything you hold of the combo, answered the same way.
5. It checks your position on the combo is back to 0 and cancels any resting rest of an order.

## Run

Set up your key as in the [main README](../README.md), with `region = ontario` or `region = us` and `env = demo`, then:

```bash
dotnet run                                  # a 1.00 stake on two legs
dotnet run -- --stake 2.50 --legs 3         # a 2.50 stake on three legs
dotnet run -- --wait 60                     # wait up to 60 s for a first quote
dotnet run -- --sport Football              # legs from one sport
dotnet run -- --profile my-demo             # a named profile from ~/.stx/credentials
dotnet run -- --player-id                   # print the pseudonym makers see on your requests, and exit
```

Each line it prints:

```text
Request: <rfq_id>  stake 1.00
[quote]  win <amount>  quote <quote_id>
Bought:  <quantity> @ <price>  order <order_id>  quote <quote_id>  combo <market_id>
Cash out: <rfq_id>  selling <quantity>
[quote]  pays <amount>  quote <quote_id>
Sold:    <quantity> @ <price>  order <order_id>  quote <quote_id>
Final:   position 0.00 on the combo  open orders 0
```

Run [`combo-maker/`](../combo-maker) beside it, as a different account, to quote your own requests; `--player` with the `--player-id` value keeps it to yours. A request your own account opened cannot be quoted by that account.

[Combos](https://docs.stxapp.io/sdks/csharp/combos/) covers the SDK calls, and [Combo negotiation](https://docs.stxapp.io/websockets/channels/combo-negotiation/) documents each frame.
