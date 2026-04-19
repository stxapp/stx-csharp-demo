using Microsoft.AspNetCore.Mvc;
using STX.Sdk.Api.Models;
using STX.Sdk.Data;
using STX.Sdk.Enums;
using STX.Sdk.Services;
using System.Net;

namespace STX.Sdk.Api.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly STXTradeService _tradeService;
        private readonly STXOrderService _orderService;
        private readonly STXSettlementService _settlementService;
        private readonly ILogger _logger;

        public OrderController(
            STXTradeService tradeService,
            STXOrderService orderService,
            STXSettlementService settlementService,
            ILogger<OrderController> logger)
        {
            _settlementService = settlementService;
            _tradeService = tradeService;
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyOrders()
        {
            var myOrders = await _orderService.GetMyOrdersAsync(new List<string> { "e6c81c35-a54d-4374-bc03-2ae73c226585" }, STXOrderStatus.FILLED);
            var myOrders1 = await _orderService.GetMyOrdersAsync(new List<string> { "e6c81c35-a54d-4374-bc03-2ae73c226585" });
            var myOrders2 = await _orderService.GetMyOrdersAsync(orderStatus: STXOrderStatus.FILLED);
            var myOrders3 = await _orderService.GetMyOrdersAsync();

            return Ok(myOrders);
        }

        [HttpGet("trades")]
        public async Task<IActionResult> GetMyTrades()
        {
            var myOrders = await _tradeService.GetMyTradesAsync(new List<string> { "4dc35a94-1371-46b6-8b44-fd710da5ebb2" }, STXTradeSettlementType.SETTLED);
            var myOrders1 = await _tradeService.GetMyTradesAsync(new List<string> { "4dc35a94-1371-46b6-8b44-fd710da5ebb2" });
            var myOrders2 = await _tradeService.GetMyTradesAsync(tradeSettlementType: STXTradeSettlementType.SETTLED);
            var myOrders3 = await _tradeService.GetMyTradesAsync();

            return Ok(myOrders);
        }

        [HttpGet("settlements")]
        public async Task<IActionResult> GetMySettlements()
        {
            var mySettlements = await _settlementService.GetMySettlementsAsync(new List<STXSettlementType> { STXSettlementType.CLOSED_LONG, STXSettlementType.CLOSED_SHORT });

            return Ok(mySettlements);
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

        [HttpPost("multiple")]
        [ProducesResponseType(typeof(STXConfirmedOrder), (int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> ConfirmOrders([FromBody] ConfirmOrdersModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _orderService.ConfirmOrdersAsync(model.ConfirmOrderModels
                    .Select(com => new STXConfirmOrderParams
                    {
                        MarketId = com.MarketId,
                        Action = com.Action,
                        OrderType = com.OrderType,
                        Price = com.Price,
                        Quantity = com.Quantity
                    })
                    .ToList());

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
