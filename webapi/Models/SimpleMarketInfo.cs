using System;
using System.Collections.Generic;
using STX.Sdk.Helpers;

namespace STX.Sdk.Api.Models
{
    /// <summary>
    /// A custom market projection: the SDK requests only the fields declared here.
    /// </summary>
    /// <remarks>
    /// Money fields arrive as cents. Each has a <c>String</c> companion built with
    /// <see cref="STXWireFormat"/>, the same way the SDK types do it; the companions are
    /// ignored by the SDK when it builds the query, and the API writes them in place of the cents.
    /// </remarks>
    public class SimpleMarketInfo
    {
        public DateTime? Timestamp { get; set; }
        public string HomeCategory { get; set; }
        public string Title { get; set; }
        public string RulesSpecifier { get; set; }
        public Guid MarketId { get; set; }
        public string EventType { get; set; }
        public string Status { get; set; }
        public DateTime? LastProbabilityAt { get; set; }
        public DateTime? EventStart { get; set; }
        public string EventStatus { get; set; }
        public int? LastTradedPrice { get; set; }
        public string Result { get; set; }
        public Guid EventId { get; set; }
        public List<SimpleBidOrOffer> Bids { get; set; }
        public List<SimpleBidOrOffer> Offers { get; set; }
        public string ShortTitle { get; set; }
        public string Position { get; set; }
        public List<SimplePriceRule> OrderPriceRules { get; set; }
        public string Description { get; set; }
        public int? PriceChange24h { get; set; }
        public string EventBrief { get; set; }
        public bool? ManualProbability { get; set; }
        public List<SimpleRecentTrade> RecentTrades { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string LastTradedPriceString => STXWireFormat.DollarString(LastTradedPrice);

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string PriceChange24hString => STXWireFormat.DollarString(PriceChange24h);
    }

    public class SimpleBidOrOffer
    {
        public decimal? Quantity { get; set; }
        public int? Price { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string QuantityString => STXWireFormat.QuantityString(Quantity);

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string PriceString => STXWireFormat.DollarString(Price);
    }

    public class SimplePriceRule
    {
        public int? From { get; set; }
        public int? To { get; set; }
        public int? Inc { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string FromPriceString => STXWireFormat.DollarString(From);

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string ToPriceString => STXWireFormat.DollarString(To);

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string IncPriceString => STXWireFormat.DollarString(Inc);
    }

    public class SimpleRecentTrade
    {
        public decimal? Quantity { get; set; }
        public int? Price { get; set; }
        public DateTime? Timestamp { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string QuantityString => STXWireFormat.QuantityString(Quantity);

        [Newtonsoft.Json.JsonIgnore]
        [System.Text.Json.Serialization.JsonIgnore]
        public string PriceString => STXWireFormat.DollarString(Price);
    }
}
