# fill-modes: post-only and fill-or-kill orders on STX.Sdk

A console program that places post-only and fill-or-kill limit orders on one market and shows what the exchange does with each. It needs a `read_write` API key. By default nothing trades.

1. It picks an open market whose event starts at least three hours from now and that has offers, none at 1 cent.
2. A post-only bid at 1 cent rests on the book as a maker. It prints the resting order, then cancels it.
3. A post-only bid at the best offer would match on arrival, so the exchange does not place it: the order comes back cancelled with `cancellation_reason` `auto_matching` and nothing filled, and the SDK throws `STXPostOnlyWouldMatchException`.
4. A fill-or-kill bid fills in full or not at all. At 1 cent nothing can fill it, so it ends `cancelled` with nothing filled. With `--fill` it bids at the best offer instead and ends `filled`, which buys 1 contract.

Not every exchange offers fill modes. One that has a mode switched off refuses the order, and the program prints that the mode is not available. One on a release from before fill modes may ignore `FillMode` and place a normal order: the program checks the first order's `FillMode`, and when it came back as a normal order it cancels it and stops before the steps that would trade. [Order types](https://docs.stxapp.io/concepts/order-types/) says which exchanges offer them.

## Run

Set up your key as in the [main README](../README.md), then:

```bash
dotnet run                          # steps 1 to 4, nothing traded
dotnet run -- --fill                # the fill-or-kill bid at the best offer: buys 1 contract
dotnet run -- --profile my-demo     # a named profile from ~/.stx/credentials
```

Each line it prints:

```text
Market:  <question>  (<market_id>)
Best offer: <price>
Resting: <order_id> accepted post_only @ 0.0100
Cancelled: cancelled
Would cross: auto_matching; order <order_id> cancelled (auto_matching), filled 0
Fill-or-kill placed: <order_id> accepted fill_or_kill @ 0.0100
Fill-or-kill: cancelled, filled 0 (auto_matching)
```

See [Post-only and fill-or-kill](https://docs.stxapp.io/sdks/csharp/trading/#post-only-and-fill-or-kill) in the SDK guide.
