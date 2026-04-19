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
    [Route("profile")]
    public class ProfileController : ControllerBase
    {
        private readonly STXProfileService _profileService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for TokenController.
        /// </summary>
        /// <param name="profileService">STXProfileService passed through DI container</param>
        /// <param name="logger">Logger</param>
        public ProfileController(
            STXProfileService profileService,
            ILogger<LoginController> logger)
        {
            _profileService = profileService;
            _logger = logger;
        }

        /// <summary>
        /// Method calls STXProfileService GetProfileAsync responsible getting user profile data.
        /// </summary>
        /// <returns>User profile data</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXUserProfile), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetProfile()
        {
            var result = await _profileService.GetProfileAsync();

            return Ok(result);
        }
    }
}
