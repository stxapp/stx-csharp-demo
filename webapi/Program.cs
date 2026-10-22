// A small ASP.NET Core API in front of STX.Sdk: market data and orders through the REST client,
// and your account kept live by the socket.
//
//   dotnet run        # then open http://localhost:5088/markets
//
// The API key comes from ~/.stx/credentials or STX_* environment variables; see README.md.

using STX.Sdk;
using STX.Sdk.Models;
using StxWebApi;

var builder = WebApplication.CreateBuilder(args);

// One client per exchange, shared: it is thread-safe and holds the connection pool.
builder.Services.AddSingleton(sp => new STXClient(new STXClientOptions
{
    Profile = builder.Configuration["Stx:Profile"],
    LoggerFactory = sp.GetRequiredService<ILoggerFactory>(),
}));
builder.Services.AddSingleton<AccountFeed>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<AccountFeed>());

var app = builder.Build();

// Open markets with the best bid and offer.
app.MapGet("/markets", async (STXClient stx, int? limit, CancellationToken ct) =>
{
    var page = await stx.GetMarketsAsync(status: ["open"], trading: true, limit: limit ?? 10, ct: ct);
    return page.Select(m => new
    {
        m.MarketId,
        m.Symbol,
        m.Title,
        BestBid = m.Bids?.FirstOrDefault()?.Price,
        BestOffer = m.Offers?.LastOrDefault()?.Price,
        m.LastTradedPrice,
    });
});

// One market, in full.
app.MapGet("/markets/{id}", async (STXClient stx, string id, CancellationToken ct) =>
{
    try { return Results.Ok(await stx.GetMarketAsync(id, ct)); }
    catch (STXNotFoundException) { return Results.NotFound(); }
});

// Who the key belongs to, and its scope.
app.MapGet("/me", async (STXClient stx, CancellationToken ct) => await stx.GetMeAsync(ct));

// Balance, open orders, fills and positions, kept current by the socket: no REST call per request.
app.MapGet("/account", (AccountFeed feed) =>
    feed.View is { } view ? Results.Content(view.State().ToJsonString(), "application/json") : Results.StatusCode(503));

// {"marketId": "...", "action": "buy", "orderType": "limit", "price": "0.01", "quantity": "1"}
app.MapPost("/orders", async (STXClient stx, STXOrderRequest order, CancellationToken ct) =>
{
    try { return Results.Ok(await stx.PlaceOrderAsync(order, ct)); }
    catch (STXApiException ex) { return Results.Problem(ex.Message, statusCode: ex.StatusCode); }
});

app.MapGet("/orders", async (STXClient stx, CancellationToken ct) =>
    await stx.GetOrdersAsync(status: ["created", "requested", "accepted", "delayed", "open"], ct: ct));

app.MapDelete("/orders/{id}", async (STXClient stx, string id, CancellationToken ct) =>
{
    try { return Results.Ok(await stx.CancelOrderAsync(id, ct)); }
    catch (STXApiException ex) { return Results.Problem(ex.Message, statusCode: ex.StatusCode); }
});

app.Run();
