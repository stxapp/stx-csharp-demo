using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using STX.Sdk.Api.Models;
using STX.Sdk.Data;
using STX.Sdk.Services;
using System.Net;
using System.Threading.Tasks;

namespace STX.Sdk.Api.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly STXOrderService _orderService;
        private readonly ILogger _logger;

        public OrderController(
            STXOrderService orderService,
            ILogger<OrderController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpPost]
        [ProducesResponseType(typeof(STXConfirmedOrder), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConfirmOrder([FromBody] ConfirmOrderModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _orderService.ConfirmOrderAsync(
                    price: model.Price, 
                    quantity: model.Quantity, 
                    marketId: model.MarketId, 
                    action: model.Action, 
                    orderType: model.OrderType);

                return Ok(result);
            }

            return BadRequest();
        }

        [HttpPut("{orderId}")]
        [ProducesResponseType(typeof(STXConfirmedOrder), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CancelOrder(string orderId)
        {
            if (ModelState.IsValid)
            {
                var result = await _orderService.CancelOrderAsync(orderId);

                return Ok(result);
            }

            return BadRequest();
        }
    }
}
