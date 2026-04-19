using STX.Sdk.Enums;

namespace STX.Sdk.Api.Models
{
    public class ConfirmOrderModel
    {
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string MarketId { get; set; }
        public STXOrderAction Action { get; set; }
        public STXOrderType OrderType { get; set; }
    }
}
