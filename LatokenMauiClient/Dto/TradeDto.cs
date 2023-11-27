using Newtonsoft.Json;

namespace LatokenMauiClient
{
    public class TradeDto
    {
        public string Id { get; set; }
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set; }
        public string Amount { get; set; }
        public DateTime OrderTime { get; set; }
        public string Price { get; set; }
        public string Count { get; set; }
        public string Type { get; set; }
    }
}
