using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Data;
using STX.Sdk.Services;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX GraphQL newToken endpoint.
    /// </summary>
    [ApiController]
    [Route("token")]
    public class TokenController : ControllerBase
    {
        private readonly STXTokenService _tokenService;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for TokenController.
        /// </summary>
        /// <param name="loginService">STXLoginService passed through DI container</param>
        /// <param name="logger">Logger</param>
        public TokenController(
            STXTokenService loginService,
            ILogger<TokenController> logger)
        {
            _tokenService = loginService;
            _logger = logger;
        }

        /// <summary>
        /// Method calls STXTokenService for token pair.
        /// </summary>
        /// <returns>Token pair</returns>
        [HttpGet]
        [ProducesResponseType(typeof(STXTokens), (int)HttpStatusCode.OK)]
        public IActionResult GetTokens()
        {
            var result = _tokenService.Tokens;

            return Ok(result);
        }

        /// <summary>
        /// Method calls STXTokenService RefreshTokenAsync responsible for refreshing token pair.
        /// </summary>
        /// <returns>User data: Token, RefreshToken, UserId, UserUid, SessionId, CurrentLoginAt</returns>
        [HttpPost]
        [ProducesResponseType(typeof(STXUserDataCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> RefreshTokenAsync()
        {
            var result = await _tokenService.RefreshTokenAsync();

            return Ok(result);
        }
    }
}