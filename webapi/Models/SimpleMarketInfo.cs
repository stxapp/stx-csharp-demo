using System;
using System.Collections.Generic;

namespace STX.Sdk.Api.Models
{
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
        public DateTime? ClosedAt { get; set; }
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
    }

    public class SimpleBidOrOffer
    {
        public int? Quantity { get; set; }
        public int? Price { get; set; }
    }

    public class SimplePriceRule
    {
        public int? From { get; set; }
        public int? To { get; set; }
        public int? Inc { get; set; }
    }

    public class SimpleRecentTrade
    {
        public int? Quantity { get; set; }
        public int? Price { get; set; }
        public DateTime? Timestamp { get; set; }
    }
}
