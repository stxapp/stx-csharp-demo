using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Data;
using STX.Sdk.ChannelWrappers;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX Phoenix channel portfolio.
    /// </summary>
    [ApiController]
    [Route("portfolio")]
    public class PortfolioController : ControllerBase
    {
        private readonly STXPortfolioChannelWrapper _portfolioChannelWrapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for PortfolioController.
        /// </summary>
        /// <param name="portfolioChannelWrapper">
        /// STXPortfolioChannelWrapper passed through DI container. 
        /// STXPortfolioChannelWrapper is wrapper around STXPortfolioChannel.
        /// See STXPortfolioChannel and STXPortfolioChannelWrapper for more details.
        /// </param>
        /// <param name="logger">Logger</param>
        public PortfolioController(
            STXPortfolioChannelWrapper portfolioChannelWrapper,
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
