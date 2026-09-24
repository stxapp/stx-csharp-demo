using STX.Sdk.Enums;

namespace STX.Sdk.Api.Models
{
    public class ConfirmOrderModel
    {
        // Order input is cents (56 is $0.56) and a whole number of contracts.
        public int Price { get; set; }
        public int Quantity { get; set; }
        public string MarketId { get; set; }
        public STXOrderAction Action { get; set; }
        public STXOrderType OrderType { get; set; }
    }

    public class ConfirmOrdersModel
    {
        public List<ConfirmOrderModel> ConfirmOrderModels { get; set; }
    }
}
