using Newtonsoft.Json;

namespace LatokenMauiClient
{
    public class TickerDto
    {
        public string Symbol { get; set; }
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set; }
        public string LastPrice { get; set; }
        public string QuoteVolume24h { get; set; }
        public string BaseVolume24h { get; set; }
        public string High24h { get; set; }
        public string Low24h { get; set; }
        public string Open24h { get; set; }
        public string Close24h { get; set; }
        public string BestAsk { get; set; }
        public string BestAskSize { get; set; }
        public string BestBid { get; set; }
        public string BestBidSize { get; set; }
        public string Fluctuation { get; set; }
        public string Url { get; set; }
        public decimal Volume7d { get; set; }
        public string Change24h { get; set; }
        public string Change7d { get; set; }
    }
}
