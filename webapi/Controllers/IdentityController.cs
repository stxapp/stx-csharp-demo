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
    public class IdentityController : ControllerBase
    {
        private readonly STXIdentityService _identityService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for IdentityController.
        /// </summary>
        /// <param name="identityService">STXIdentityService passed through DI container</param>
        /// <param name="logger">Logger</param>
        public IdentityController(
            STXIdentityService identityService,
            ILogger<IdentityController> logger)
        {
            _identityService = identityService;
            _logger = logger;
        }

        /// <summary>
        /// Returns who the current credentials belong to and what they may do.
        /// </summary>
        /// <returns>User id, account id, name, auth method and scope</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXIdentity), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMe()
        {
            var result = await _identityService.GetMeAsync();

            return Ok(result);
        }
    }
}
