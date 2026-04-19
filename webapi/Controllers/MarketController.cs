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
        /// Method calls STXProfileService method GetSportAndCompetitions responsible getting all sports and competitions.
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
        /// Method calls STXProfileService method GetMarketInfoAsync responsible getting all market info.
        /// </summary>
        /// <returns>All market info</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXMarketInfosResponse<STXMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfo()
        {
            var result = await _marketService.GetShortMarketInfosAsync(new List<string> { "ed131718-3f2b-4fcf-a2ea-c04bf55fd6ed", "c022b034-85b0-4a55-81aa-852f5e832bba", "2a7912e2-62e1-4d8d-bfa4-2c8aab7423ca" });

            result.MarketInfos = result.MarketInfos
                .Take(50)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Method calls STXProfileService method GetMarketInfoAsync responsible getting all market info.
        /// </summary>
        /// <returns>All market info</returns>
        [HttpGet("nba")]
        [ProducesResponseType(typeof(STXMarketInfosResponse<STXMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfoOnlyNba()
        {
            var filter = new STXMarketInfosFilter
            {
                Sports = new List<string>() { "Basketball" },
                Competitions = new List<string>() { "NBA" }
            };

            var result = await _marketService.GetMarketInfosAsync(filter);

            result.MarketInfos = result.MarketInfos
                .Take(50)
                .ToList();

            return Ok(result);
        }

        /// <summary>
        /// Method calls STXProfileService genetic method GetMarketInfoAsync responsible getting custom market info.
        /// </summary>
        /// <returns>Custom market info</returns>
        [HttpGet("generic")]
        [ProducesResponseType(typeof(STXMarketInfosResponse<SimpleMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfoGeneric()
        {
            var result = await _marketService.GetMarketInfosAsync<SimpleMarketInfo>();

            result.MarketInfos = result.MarketInfos
                .Take(50)
                .ToList();

            return Ok(result);
        }
    }
}
