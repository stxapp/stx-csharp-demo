using System.Globalization;
using STX.Sdk;
using STX.Sdk.Models;

namespace StxConsole;

public sealed record WorkerOptions(bool Quote, bool Once, string? Profile);

public sealed class Worker(WorkerOptions options, IHostApplicationLifetime lifetime, ILoggerFactory loggerFactory, ILogger<Worker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var client = new STXClient(new STXClientOptions { Profile = options.Profile, LoggerFactory = loggerFactory });
        var me = await client.GetMeAsync(stoppingToken);
        logger.LogInformation("Exchange {Host}: key {KeyId}, user {UserId}, scope {Scope}", client.Host, me.KeyId, me.UserId, me.Scope);

        // Markets: amounts are decimal strings, exactly as the API sends them.
        var markets = await client.GetMarketsAsync(status: ["open"], trading: true, limit: 3, ct: stoppingToken);
        foreach (var m in markets)
            logger.LogInformation("{Symbol} bid {Bid} offer {Offer}", m.Symbol, m.Bids?.FirstOrDefault()?.Price ?? "-", m.Offers?.LastOrDefault()?.Price ?? "-");

        // One socket carries every channel. After a reconnect the channels are rejoined and their
        // snapshots arrive again; OnReconnect is the moment to re-read anything loaded over REST.
        await using var ws = client.CreateWebSocket(new STXWebSocketOptions
        {
            OnReconnect = () =>
            {
                logger.LogWarning("Reconnected: orders placed with cancel-on-disconnect may have been cancelled");
                return Task.CompletedTask;
            },
        });

        if (markets.Count > 0)
        {
            await ws.OrderbookAsync(markets.Select(m => m.MarketId!), onMessage: msg =>
                logger.LogInformation("Book {MarketId}: best bid {Bid}", msg.Payload?["market_id"], msg.Payload?["bids"]?[0]?["price"]), stoppingToken);
        }

        var view = await ws.AccountViewAsync(onChange: change =>
        {
            if (!change.Snapshot) logger.LogInformation("Account {Kind} ({Event})", change.Kind, change.Event);
        }, ct: stoppingToken);
        logger.LogInformation("Account: available {Balance}, {Orders} open orders, {Positions} positions",
            view.Balance?.AvailableBalance, view.OpenOrders.Count, view.Positions.Count);

        Order? quote = null;
        await using var codSocket = client.CreateWebSocket();
        if (options.Quote)
            quote = await PlaceProtectedBidAsync(client, codSocket, view, stoppingToken);

        try
        {
            if (options.Once)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            else
            {
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (quote is not null)
            {
                var cancelled = await client.CancelOrderAsync(quote.Id!);
                logger.LogInformation("Cancelled {OrderId}: {Status}", cancelled.OrderId, cancelled.Status);
            }
            if (options.Once) lifetime.StopApplication();
        }
    }

    // A 1-cent bid on a market whose event has not started, opted into cancel-on-disconnect: if
    // this process dies or loses its connection, the exchange cancels the order.
    private async Task<Order?> PlaceProtectedBidAsync(STXClient client, STXWebSocket codSocket, STXAccountView view, CancellationToken ct)
    {
        Market? market = null;
        await foreach (var m in client.GetAllMarketsAsync(status: ["open"], trading: true, sortBy: "event_start", sortDirection: "desc", ct: ct))
        {
            var offeredAtACent = (m.Offers ?? []).Any(o => decimal.Parse(o.Price!, CultureInfo.InvariantCulture) <= 0.01m);
            if (m.EventStatus == "scheduled" && !offeredAtACent) { market = m; break; }
        }
        if (market is null)
        {
            logger.LogWarning("No open market with an unstarted event right now");
            return null;
        }

        // Arm cancel-on-disconnect; the socket pings the channel for as long as it is joined.
        var orders = await codSocket.OrdersAsync(cancelOnDisconnect: true, pingTimeout: 5000, ct: ct);
        logger.LogInformation("Cancel-on-disconnect armed, timeout {Timeout} ms", orders.Reply["ping_timeout"]);

        try
        {
            var order = await client.PlaceOrderAsync(market.MarketId!, STXOrderAction.Buy, STXOrderType.Limit,
                quantity: "1", price: "0.01", clientOrderId: $"demo-{Guid.NewGuid():N}", cancelOnDisconnect: true, ct: ct);
            logger.LogInformation("Placed {OrderId} on {Symbol}: {Status} {Quantity} @ {Price}", order.Id, market.Symbol, order.Status, order.Quantity, order.Price);
            return order;
        }
        catch (STXApiException ex)
        {
            logger.LogWarning("The exchange refused the order ({Status}): {Message}", ex.StatusCode, ex.Message);
            return null;
        }
    }
}
