using STX.Sdk;

namespace StxWebApi;

/// <summary>
/// Keeps one socket open for the app's lifetime with a live account view on it. The socket
/// reconnects and rejoins by itself, and the view starts over from fresh snapshots each time.
/// </summary>
public sealed class AccountFeed(STXClient client, ILogger<AccountFeed> logger) : IHostedService, IAsyncDisposable
{
    private STXWebSocket? _ws;

    public STXAccountView? View { get; private set; }

    public async Task StartAsync(CancellationToken ct)
    {
        _ws = client.CreateWebSocket(new STXWebSocketOptions
        {
            OnReconnect = () =>
            {
                logger.LogInformation("Socket reconnected; the account view reloads from fresh snapshots");
                return Task.CompletedTask;
            },
        });
        View = await _ws.AccountViewAsync(onChange: change =>
        {
            if (!change.Snapshot) logger.LogInformation("Account {Kind} ({Event})", change.Kind, change.Event);
        }, ct: ct);
        logger.LogInformation("Account view ready: available {Balance}, {Orders} open orders", View.Balance?.AvailableBalance, View.OpenOrders.Count);
    }

    public async Task StopAsync(CancellationToken ct) => await DisposeAsync();

    public async ValueTask DisposeAsync()
    {
        if (_ws is not null) await _ws.DisposeAsync();
        _ws = null;
    }
}
