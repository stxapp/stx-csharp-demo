using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using STX.Sdk.Channels;
using STX.Sdk.Data;
using STX.Sdk.Enums;
using STX.Sdk.Services;

namespace STX.Sdk.Console
{
    public class STXWorker
    {
        private readonly STXLoginService m_LoginService;
        private readonly STXTokenService m_TokenService;
        private readonly STXMarketService m_MarketService;
        private readonly STXOrderService m_OrderService;
        private readonly STXMarketChannel m_MarketChannel;
        private readonly STXActiveTradesChannel m_TradesChannel;
        private readonly STXActiveOrdersChannel m_OrdersChannel;
        private readonly STXPortfolioChannel m_PortfolioChannel;
        private readonly STXSessionBackgroundService m_SessionBackgroundService;
        private readonly ILogger _logger;

        private readonly object m_Locker = new object();

        private readonly Random _randomGenerator = new Random();
        private readonly int _ordersPerMarketLimit = 100;
        private readonly int _orderMaxPrice = 1000;
        private readonly int _orderMaxQuantity = 2;
        private string m_PlacedOrderId;

        private readonly ICollection<STXMarketStatus> _openMarketStatuses = new[] { STXMarketStatus.open, STXMarketStatus.pre_open };

        public STXWorker(
            STXLoginService loginService,
            STXTokenService tokenService,
            STXMarketService marketService,
            STXOrderService orderService,
            STXMarketChannel marketChannel,
            STXActiveTradesChannel tradesChannel,
            STXActiveOrdersChannel ordersChannel,
            STXPortfolioChannel portfolioChannel,
            STXSessionBackgroundService sessionBackgroundService,
            ILoggerFactory loggerFactory
            )
        {
            m_LoginService = loginService;
            m_TokenService = tokenService;
            m_MarketService = marketService;
            m_OrderService = orderService;
            m_MarketChannel = marketChannel;
            m_TradesChannel = tradesChannel;
            m_OrdersChannel = ordersChannel;
            m_PortfolioChannel = portfolioChannel;
            m_SessionBackgroundService = sessionBackgroundService;
            _logger = loggerFactory.CreateLogger("STXWorker");
        }

        public async Task RunAsync()
        {
            _logger.LogInformation("Starting STX Worker");
            m_SessionBackgroundService.SetSessionMessageAction(SessionMessageReceived);

            STXUserDataCollection userData = await m_LoginService.LoginAsync(
                    Environment.GetEnvironmentVariable("EMAIL"),
                    Environment.GetEnvironmentVariable("PASSWORD"),
                    keepSessionAlive: true);

            STXTokens tokens = m_TokenService.Tokens;

            List<STXSportAndCompetitions> sportsAndComps = await m_MarketService.GetSportAndCompetitionsAsync();
            STXMarketInfoResponse<STXMarketInfo> markets = await m_MarketService.GetMarketsInfoAsync(
                new STXMarketInfoFilter
                {
                    FromTime = DateTime.UtcNow,
                    ToTime = DateTime.UtcNow.AddDays(7),
                    SportAndCompetitions = sportsAndComps,
                });

            m_MarketChannel.SetOnReceiveAction(MarketReceive);
            await m_MarketChannel.StartAsync();

            m_OrdersChannel.SetOnReceiveAction(OrderReceived);
            await m_OrdersChannel.StartAsync();

            m_TradesChannel.SetOnReceiveAction(TradesReceived);
            await m_TradesChannel.StartAsync();

            m_PortfolioChannel.SetOnReceiveAction(PortfolioReceived);
            await m_PortfolioChannel.StartAsync();

            while (true)
            {
                await Task.Delay(1000);

                // cancel all orders before placing new one.
                System.Console.WriteLine($"Cancelling orders.");
                await m_OrderService.CancelAllOrdersAsync();

                foreach (var marketInfo in markets.MarketInfos.Where(m => _openMarketStatuses.Contains(m.Status)))
                {
                    System.Console.WriteLine($"Placing orders on market: {marketInfo.MarketId}");
                    foreach (var orderNumber in Enumerable.Range(1, _ordersPerMarketLimit))
                    {
                        System.Console.WriteLine($"Placing order no. {orderNumber} on market: {marketInfo.MarketId}");

                        try
                        {
                            STXConfirmedOrder result = await m_OrderService.ConfirmOrderAsync(
                                                price: _randomGenerator.Next(100, _orderMaxPrice),
                                                quantity: _randomGenerator.Next(1, _orderMaxQuantity),
                                                marketId: marketInfo.MarketId.ToString(),
                                                action: STXOrderAction.BUY,
                                                orderType: STXOrderType.LIMIT);

                            m_PlacedOrderId = result.Order.Id;

                            lock (m_Locker)
                            {
                                System.Console.ForegroundColor = ConsoleColor.Yellow;
                                System.Console.WriteLine($"Order placed: {m_PlacedOrderId}");
                                System.Console.WriteLine(JsonConvert.SerializeObject(result));
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.ForegroundColor = ConsoleColor.Red;
                            System.Console.WriteLine($"Exception caught: {ex.Message}");
                        }
                    }
                }
            }
        }

        private void SessionMessageReceived(STXSessionMessage sessionMessage)
        {
            System.Console.WriteLine(sessionMessage.SessionMessageStatus + ": " + sessionMessage.AdditionalMessage);
        }

        private void PortfolioReceived(STXPortfolio portfolio)
        {
            if (portfolio != null)
            {
                System.Console.ForegroundColor = ConsoleColor.Magenta;
                System.Console.WriteLine("Portfolio");
                System.Console.WriteLine(JsonConvert.SerializeObject(portfolio));
            }
        }

        private void TradesReceived(STXActiveTrades trades)
        {
            if (string.IsNullOrWhiteSpace(m_PlacedOrderId) == true || trades?.Trades is null)
            {
                return;
            }

            lock (m_Locker)
            {
                STXActiveTrade trade = trades.Trades.Where(t => t.OrderId == m_PlacedOrderId).FirstOrDefault();

                if (trade is not null)
                {
                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine("TRADES");
                    System.Console.WriteLine(JsonConvert.SerializeObject(trade));
                }
            }
        }

        private void OrderReceived(STXActiveOrders orders)
        {
            lock (m_Locker)
            {
                System.Console.ForegroundColor = ConsoleColor.Cyan;
                System.Console.WriteLine("ORDERS");
                System.Console.WriteLine(JsonConvert.SerializeObject(orders));
            }
        }

        private void MarketReceive(STXMarketInfoChannelData data)
        {
        }
    }
}
