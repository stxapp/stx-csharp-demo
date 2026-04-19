using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Services;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    [ApiController]
    [Route("worker")]
    public class WorkerController : ControllerBase
    {
        private readonly STXWorker _stxWorker;
        private readonly ILogger _logger;

        public WorkerController(
            STXWorker stxWorker,
            ILogger<WorkerController> logger)
        {
            _stxWorker = stxWorker;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetSomethingFromWorker()
        {
            _stxWorker.RunAsync().Wait();
            return Ok(true);
        }
    }
}
