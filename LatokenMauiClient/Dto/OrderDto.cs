using Newtonsoft.Json;

namespace LatokenMauiClient
{
    public class OrderDto
    {
        public string Id { get; set; }

        public string Status { get; set; }

        public string Side { get; set; }

        public string Condition { get; set; }

        public string Type { get; set; }

        public string BaseCurrency { get; set; }

        public string QuoteCurrency { get; set; }

        public string ClientOrderId { get; set; }

        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public decimal? Cost { get; set; }

        public decimal Filled { get; set; }

        public string Trader { get; set; }

        public DateTime Timestamp { get; set; }



        public string BaseCurrencySymbol { get; set; }

        public string QuoteCurrencySymbol { get; set; }

    }
}
