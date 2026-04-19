using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Services;
using STX.Sdk.Data;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX Phoenix channel positions.
    /// </summary>
    [ApiController]
    [Route("simple-positions")]
    public class SimplePositionsController : ControllerBase
    {
        private readonly SimplePositionsChannelWrapper _positionsChannelWrapper;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for PositionsController.
        /// </summary>
        /// <param name="positionsChannelWrapper">
        /// SimplePositionsChannelWrapper passed through DI container. 
        /// SimplePositionsChannelWrapper is wrapper around STXPositionsChannel.
        /// See STXPositionsChannel and SimplePositionsChannelWrapper for more details.
        /// </param>
        /// <param name="logger">Logger</param>
        public SimplePositionsController(
            SimplePositionsChannelWrapper positionsChannelWrapper,
            ILogger<PositionsController> logger)
        {
            _positionsChannelWrapper = positionsChannelWrapper;
            _logger = logger;
        }

        /// <summary>
        /// Method sets, starts and stops positions channel and returns last received item.
        /// </summary>
        /// <returns>User positions data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXPositions), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetPositions()
        {
            //first set channel for use
            await _positionsChannelWrapper.SetChannelAsync();
            //second start accepting data through channel
            _positionsChannelWrapper.StartAsync();

            while (_positionsChannelWrapper.LastItem == null)
            {
                await Task.Delay(500);
            }

            //prepare last received item
            var result = _positionsChannelWrapper.LastItem;

            //stop channel to not receive data
            await _positionsChannelWrapper.StopAsync();

            return Ok(result);
        }
    }
}
