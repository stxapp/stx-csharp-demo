using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Services;
using STX.Sdk.Data;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX Phoenix channel active orders.
    /// </summary>
    [ApiController]
    [Route("simple-active-orders")]
    public class SimpleActiveOrdersController : ControllerBase
    {
        private readonly SimpleActiveOrdersChannelWrapper _activeOrdersChannelWrapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for ActiveSettlementsController.
        /// </summary>
        /// <param name="activeOrdersChannelWrapper">
        /// SimpleActiveOrdersChannelWrapper passed through DI container. 
        /// SimpleActiveOrdersChannelWrapper is wrapper around STXActiveOrdersChannel.
        /// See STXActiveOrdersChannel and SimpleActiveOrdersChannelWrapper for more details.
        /// </param>
        /// <param name="logger">Logger</param>
        public SimpleActiveOrdersController(
            SimpleActiveOrdersChannelWrapper activeOrdersChannelWrapper,
            ILogger<ActiveOrdersController> logger)
        {
            _activeOrdersChannelWrapper = activeOrdersChannelWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Method sets, starts and stops active orders channel and returns last received item.
        /// </summary>
        /// <returns>User active order data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXActiveOrders), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetActiveOrders()
        {
            await _activeOrdersChannelWrapper.SetChannelAsync();
            _activeOrdersChannelWrapper.StartAsync();

            while (_activeOrdersChannelWrapper.LastItem == null)
            {
                await Task.Delay(500);
            }

            var result = _activeOrdersChannelWrapper.LastItem;

            await _activeOrdersChannelWrapper.StopAsync();

            return Ok(result);
        }
    }
}
