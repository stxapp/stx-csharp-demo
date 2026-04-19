using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.ChannelWrappers;
using STX.Sdk.Data;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX Phoenix channel active settlements.
    /// </summary>
    [ApiController]
    [Route("active-settlements")]
    public class ActiveSettlementsController : ControllerBase
    {
        private readonly STXActiveSettlementsChannelWrapper _activeSettlementsChannelWrapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for ActiveSettlementsController.
        /// </summary>
        /// <param name="activeSettlementsChannelWrapper">
        /// STXActiveSettlementsChannelWrapper passed through DI container. 
        /// STXActiveSettlementsChannelWrapper is wrapper around STXActiveSettlementsChannel.
        /// See STXActiveSettlementsChannel and STXActiveSettlementsChannelWrapper for more details.
        /// </param>
        /// <param name="logger">Logger</param>
        public ActiveSettlementsController(
            STXActiveSettlementsChannelWrapper activeSettlementsChannelWrapper,
            ILogger<ActiveSettlementsController> logger)
        {
            _activeSettlementsChannelWrapper = activeSettlementsChannelWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Method sets, starts and stops active settlements channel and returns last received item.
        /// </summary>
        /// <returns>User active settlements data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXActiveSettlements), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetActiveSettlements()
        {
            //first set channel for use
            await _activeSettlementsChannelWrapper.SetChannelAsync();
            //second start accepting data through channel
            _activeSettlementsChannelWrapper.StartAsync();

            while (_activeSettlementsChannelWrapper.LastItem == null)
            {
                await Task.Delay(500);
            }

            //prepare last received item
            var result = _activeSettlementsChannelWrapper.LastItem;

            //stop channel to not receive data
            await _activeSettlementsChannelWrapper.StopAsync();

            return Ok(result);
        }
    }
}
