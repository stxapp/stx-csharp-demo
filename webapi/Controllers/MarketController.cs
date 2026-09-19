using Microsoft.AspNetCore.Mvc;
using STX.Sdk.Api.Models;
using STX.Sdk.Data;
using STX.Sdk.Services;
using System.Net;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX GraphQL marketInfo endpoint.
    /// </summary>
    [ApiController]
    [Route("markets")]
    public class MarketController : ControllerBase
    {
        private readonly STXMarketService _marketService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for TokenController.
        /// </summary>
        /// <param name="marketService">STXMarketService passed through DI container</param>
        /// <param name="logger">Logger</param>
        public MarketController(
            STXMarketService marketService,
            ILogger<MarketController> logger)
        {
            _marketService = marketService;
            _logger = logger;
        }

        /// <summary>
        /// Method calls STXMarketService method GetSportAndCompetitionsAsync, returning all sports and competitions.
        /// </summary>
        /// <returns>All market info</returns>
        [HttpGet("sports-and-competitions")]
        [ProducesResponseType(typeof(STXSportAndCompetitions), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetSportAndCompetitions()
        {
            var result = await _marketService.GetSportAndCompetitionsAsync();

            return Ok(result);
        }

        /// <summary>
        /// Method calls STXMarketService to return market info.
        /// </summary>
        /// <returns>All market info</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXMarketInfosWithCountResponse<STXShortMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfo()
        {
            var result = await _marketService.GetShortMarketInfosWithCountAsync(new List<string> { "ed131718-3f2b-4fcf-a2ea-c04bf55fd6ed", "c022b034-85b0-4a55-81aa-852f5e832bba", "2a7912e2-62e1-4d8d-bfa4-2c8aab7423ca" });

            result.MarketInfos = result.MarketInfos
                .Take(50)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Method calls STXMarketService to return market info.
        /// </summary>
        /// <returns>All market info</returns>
        [HttpGet("nba")]
        [ProducesResponseType(typeof(STXMarketInfosWithCountResponse<STXMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfoOnlyNba()
        {
            var filter = new STXMarketInfosFilter
            {
                Sports = new List<string>() { "Basketball" },
                Competitions = new List<string>() { "NBA" }
            };

            var result = await _marketService.GetMarketInfosWithCountAsync(filter);

            result.MarketInfos = result.MarketInfos
                .Take(50)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Method calls the generic STXMarketService overload to return a custom market projection.
        /// </summary>
        /// <returns>Custom market info</returns>
        [HttpGet("generic")]
        [ProducesResponseType(typeof(STXMarketInfosWithCountResponse<SimpleMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfoGeneric()
        {
            // Limit server-side. Asking for every market and calling Take(50) afterwards
            // makes the server assemble the whole set first, which times out at the
            // gateway on a populated environment - and throws away the payload saving
            // the generic overload exists to give you.
            var result = await _marketService.GetMarketInfosWithCountAsync<SimpleMarketInfo>(
                new STXMarketInfosFilter { Limit = 50 });

            return Ok(result);
        }
    }
}
