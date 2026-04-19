using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Services;
using STX.Sdk.Data;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX Phoenix channel active trades.
    /// </summary>
    [ApiController]
    [Route("simple-active-trades")]
    public class SimpleActiveTradesController : ControllerBase
    {
        private readonly SimpleActiveTradesChannelWrapper _activeTradesChannelWrapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for ActiveTradesController.
        /// </summary>
        /// <param name="activeTradesChannelWrapper">
        /// SimpleActiveTradesChannelWrapper passed through DI container. 
        /// SimpleActiveTradesChannelWrapper is wrapper around STXActiveTradesChannel.
        /// See STXActiveTradesChannel and SimpleActiveTradesChannelWrapper for more details.
        /// </param>
        /// <param name="logger">Logger</param>
        public SimpleActiveTradesController(
            SimpleActiveTradesChannelWrapper activeTradesChannelWrapper,
            ILogger<ActiveTradesController> logger)
        {
            _activeTradesChannelWrapper = activeTradesChannelWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Method sets, starts and stops active trades channel and returns last received item.
        /// </summary>
        /// <returns>User active order data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXActiveTrades), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetActiveTrades()
        {
            //first set channel for use
            await _activeTradesChannelWrapper.SetChannelAsync();
            //second start accepting data through channel
            _activeTradesChannelWrapper.StartAsync();

            while (_activeTradesChannelWrapper.LastItem == null)
            {
                await Task.Delay(500);
            }

            //prepare last received item
            var result = _activeTradesChannelWrapper.LastItem;

            //stop channel to not receive data
            await _activeTradesChannelWrapper.StopAsync();

            return Ok(result);
        }
    }
}
