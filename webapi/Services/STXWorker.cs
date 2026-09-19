using STX.Sdk.Data;
using STX.Sdk.Enums;
using STX.Sdk.Services;

namespace STX.Sdk.Api.Services
{
    public class STXWorker
    {
        private readonly STXIdentityService m_IdentityService;
        private readonly STXMarketService m_MarketService;
        private readonly STXOrderService m_OrderService;

        public STXWorker(
            STXIdentityService identityService,
            STXMarketService marketService,
            STXOrderService orderService)
        {
            m_IdentityService = identityService;
            m_MarketService = marketService;
            m_OrderService = orderService;
        }

        public async Task RunAsync()
        {
            // No login with an API key: requests are signed per call. This one call supplies
            // the user id, which anything user-scoped needs.
            var me = await m_IdentityService.GetMeAsync();

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
