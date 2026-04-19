using STX.Sdk.Data;
using STX.Sdk.Enums;
using STX.Sdk.Services;

namespace STX.Sdk.Api.Services
{
    public class STXWorker
    {
        private readonly STXLoginService m_LoginService;
        private readonly STXTokenService m_TokenService;
        private readonly STXMarketService m_MarketService;
        private readonly STXOrderService m_OrderService;

        public STXWorker(
            STXLoginService loginService,
            STXTokenService tokenService,
            STXMarketService marketService,
            STXOrderService orderService)
        {
            m_LoginService = loginService;
            m_TokenService = tokenService;
            m_MarketService = marketService;
            m_OrderService = orderService;
        }

        public async Task RunAsync()
        {
            STXUserDataCollection userData = await m_LoginService.LoginAsync(
                Environment.GetEnvironmentVariable("EMAIL"),
                Environment.GetEnvironmentVariable("PASSWORD"));

            STXTokens tokens = m_TokenService.Tokens;

            List<STXSportAndCompetitions> sportsAndComps = await m_MarketService.GetSportAndCompetitionsAsync();
            STXMarketInfosWithCountResponse<STXMarketInfo> markets = await m_MarketService.GetMarketInfosWithCountAsync(new STXMarketInfosFilter
            {
                Sports = sportsAndComps.Select(s => s.Sport).ToList(),
                Competitions = sportsAndComps.SelectMany(s => s.Competitions).ToList(),
            });

            STXMarketInfo[] baseball = markets.MarketInfos.Where(m => m.Sport == "Baseball").ToArray();

            STXConfirmedOrder result = await m_OrderService.ConfirmOrderAsync(4000, 2, baseball.First().MarketId.ToString(), STXOrderAction.BUY, STXOrderType.LIMIT);
        }
    }
}
