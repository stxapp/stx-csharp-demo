using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Models;
using STX.Sdk.Data;
using STX.Sdk.Services;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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
        /// Method calls STXProfileService method GetMarketInfoAsync responsible getting all market info.
        /// </summary>
        /// <returns>All market info</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXMarketInfoResponse<STXMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfo()
        {
            var result = await _marketService.GetMarketsInfoAsync();

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
        [ProducesResponseType(typeof(STXMarketInfoResponse<SimpleMarketInfo>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMarketsInfoGeneric()
        {
            var result = await _marketService.GetMarketsInfoAsync<SimpleMarketInfo>();

            result.MarketInfos = result.MarketInfos
                .Take(50)
                .ToList();

            return Ok(result);
        }
    }
}
