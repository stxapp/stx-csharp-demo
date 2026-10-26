# combo-maker: quote combos as a maker on STX.Sdk

A console program that joins the `combo_quote` channel as a maker and quotes every combo request on the exchange until you press Ctrl+C, printing how each one ends. Quoting needs a `read_write` API key.

1. Each request arrives as `RfqOpened`, from every player on the exchange.
2. On a buy, the program quotes what the player wins: the stake times `--win-multiple` (default `9`). On a cash out, it quotes what you pay for everything the player sells: `--cash-out-price` a contract (default `0.08`). Each quote stands for `--quote-seconds` (default `30`). It first checks that the amount prices inside the request's `MaxPrice` and `TickSize`, and skips the request if not.
3. It prints what follows: `QuoteAccepted` then `MakerPlacement` when your quote is placed, `NegotiationClosed` when it stops standing (`placed_elsewhere` when another maker's quote was placed), and `RfqClosed` for every request.

## Run

Set up your key as in the [main README](../README.md), with `region = ontario` or `region = us` and `env = demo`, then:

```bash
dotnet run                                                   # quote until Ctrl+C
dotnet run -- --seconds 120                                  # stop after 120 s
dotnet run -- --win-multiple 12 --cash-out-price 0.10 --quote-seconds 60
dotnet run -- --profile my-demo                              # a named profile from ~/.stx/credentials
```

This run, on the Ontario demo exchange with `--win-multiple 15 --cash-out-price 0.12`, quoted a buy and then the player's cash out of the same combo, and won both:

```text
Quoting combos on demo.stxapp.ca as a maker.
Buys: win 15x the stake. Cash outs: 0.12 a contract. Each quote stands 30 s. Ctrl+C to stop.

[request]  83336486  buy, stake 2.0000, 2 legs: WSH @ CHA won, LA @ NYR won
[quoted]   83336486  win 30.0000  for 30 s  quote cf1e4680-fcf2-4736-93b8-d2f32b8f6a6a
[accepted] 83336486  quote cf1e4680-fcf2-4736-93b8-d2f32b8f6a6a
[won]      83336486  order 75f00c9f-1dfb-4bca-90b6-f01e695b59b0  32.00 @ 0.0625  combo df0489de-8f36-4fde-818e-adeaf038bde3
[closed]   83336486  placed
[request]  0a5b1b1d  cash out of 32.00 on combo df0489de
[quoted]   0a5b1b1d  pay 3.8400 for 32.00  for 30 s  quote 84ef6bb8-aa84-45b7-b92f-b301241ca6ac
[accepted] 0a5b1b1d  quote 84ef6bb8-aa84-45b7-b92f-b301241ca6ac
[won]      0a5b1b1d  order c355f7c3-62dd-4ec2-a264-432a90f9919a  32.00 @ 0.1200  combo df0489de-8f36-4fde-818e-adeaf038bde3
[closed]   0a5b1b1d  placed
```

With `--win-multiple 3 --cash-out-price 0.02`, another maker quoted the player more, and both of these quotes lost:

```text
[request]  7a29add1  buy, stake 2.0000, 2 legs: WSH @ CHA won, LA @ NYR won
[quoted]   7a29add1  win 6.0000  for 30 s  quote 60d566eb-e460-42ff-aa8b-8755111a4943
[lost]     7a29add1  quote 60d566eb-e460-42ff-aa8b-8755111a4943  placed_elsewhere
[closed]   7a29add1  placed
[request]  dcd038c5  cash out of 32.00 on combo df0489de
[quoted]   dcd038c5  pay 0.6400 for 32.00  for 30 s  quote 3c12be82-d17b-45d6-9b2f-f7e15fd8495e
[lost]     dcd038c5  quote 3c12be82-d17b-45d6-9b2f-f7e15fd8495e  placed_elsewhere
[closed]   dcd038c5  placed
```

A quote can also end `[expired]`, when its time runs out and a later quote on the request replaces it, or with the request's own reason, such as `cancelled`. Other makers quote too, so expect to lose some requests. A request your own account opened is refused with `cannot_quote_own_request`. Every socket joined as one account gets the pushes about all of that account's quotes, so run one maker per account. Quotes belong to the connection that placed them: on Ctrl+C, or if the connection drops, they are withdrawn.

## Where to look

- `Program.cs`: the channel, the quoting and the log lines.
- `MakerQuote.cs`: the amount to quote, and the checks that keep it inside the request's price grid.

[Quote combos as a maker](https://docs.stxapp.io/websockets/combo-maker/) covers the maker side in every language, [Combos](https://docs.stxapp.io/sdks/csharp/combos/) covers the SDK calls, and [Combo negotiation](https://docs.stxapp.io/websockets/channels/combo-negotiation/) documents each frame.
