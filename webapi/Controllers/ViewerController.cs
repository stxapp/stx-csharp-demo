using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Data;
using STX.Sdk.Services;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX GraphQL userProfile endpoint.
    /// </summary>
    [ApiController]
    [Route("me")]
    public class ViewerController : ControllerBase
    {
        private readonly STXViewerService _viewerService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for TokenController.
        /// </summary>
        /// <param name="viewerService">STXViewerService passed through DI container</param>
        /// <param name="logger">Logger</param>
        public ViewerController(
            STXViewerService viewerService,
            ILogger<LoginController> logger)
        {
            _viewerService = viewerService;
            _logger = logger;
        }

        /// <summary>
        /// Returns who the current credentials belong to and what they may do.
        /// </summary>
        /// <returns>User id, account id, name, auth method and scope</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXViewer), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMe()
        {
            var result = await _viewerService.GetMeAsync();

            return Ok(result);
        }
    }
}
