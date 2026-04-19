using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Services;
using STX.Sdk.Data;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX Phoenix channel portfolio.
    /// </summary>
    [ApiController]
    [Route("simple-portfolio")]
    public class SimplePortfolioController : ControllerBase
    {
        private readonly SimplePortfolioChannelWrapper _portfolioChannelWrapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for PortfolioController.
        /// </summary>
        /// <param name="portfolioChannelWrapper">
        /// SimplePortfolioChannelWrapper passed through DI container. 
        /// SimplePortfolioChannelWrapper is wrapper around STXPortfolioChannel.
        /// See STXPortfolioChannel and SimplePortfolioChannelWrapper for more details.
        /// </param>
        /// <param name="logger">Logger</param>
        public SimplePortfolioController(
            SimplePortfolioChannelWrapper portfolioChannelWrapper,
            ILogger<PortfolioController> logger)
        {
            _portfolioChannelWrapper = portfolioChannelWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Method sets, starts and stops positions channel and returns last received item.
        /// </summary>
        /// <returns>User portfolio data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXPortfolio), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPortfolio()
        {
            //first set channel for use
            await _portfolioChannelWrapper.SetChannelAsync();
            //second start accepting data through channel
            _portfolioChannelWrapper.StartAsync();

            while (_portfolioChannelWrapper.LastItem == null)
            {
                await Task.Delay(500);
            }

            //prepare last received item
            var result = _portfolioChannelWrapper.LastItem;

            //stop channel to not receive data
            await _portfolioChannelWrapper.StopAsync();

            return Ok(result);
        }
    }
}
