using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Models;
using STX.Sdk.Data;
using STX.Sdk.Services;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    /// <summary>
    /// Controler used for testing STX GraphQL login and confirm2FA endpoints.
    /// </summary>
    [ApiController]
    [Route("login")]
    public class LoginController : ControllerBase
    {
        private readonly STXLoginService _loginService;
        private readonly ILogger _logger;


        /// <summary>
        /// Constructor for TokenController.
        /// </summary>
        /// <param name="loginService">STXLoginService passed through DI container</param>
        /// <param name="logger">Logger</param>
        public LoginController(
            STXLoginService loginService,
            ILogger<LoginController> logger)
        {
            _loginService = loginService;
            _logger = logger;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(STXUserDataCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _loginService.LoginAsync(model.Email, model.Password);

                return Ok(result);
            }

            return BadRequest();
        }

        [HttpPost("confirm2fa")]
        [ProducesResponseType(typeof(STXUserDataCollection), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> Confirm2FA([FromBody] Confirm2FAModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _loginService.Confirm2FAAsync(model.Code);

                return Ok(result);
            }

            return BadRequest();
        }
    }
}
