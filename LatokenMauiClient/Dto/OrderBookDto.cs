
namespace LatokenMauiClient
{

    public class OrderBookDto
    {
        public List<PriceLevelDto> Ask { get; set; }

        public List<PriceLevelDto> Bid { get; set; }

        public decimal? TotalAsk { get; set; }

        public decimal? TotalBid { get; set; }
    }

    public class PriceLevelDto
    {
        private string openOrderActionText;

        public bool IsBuyOrder { get; set; }
        public decimal Price { get; set; }

        public decimal Quantity { get; set; }

        public decimal Cost { get; set; }

        public decimal Accumulated { get; set; }
        public string Count { get; set; }
        public bool HighlightRow { get; set; }

        public string OpenOrderActionText
        {
            get
            {
                openOrderActionText = HighlightRow ? "Cancel" : string.Empty;
                return openOrderActionText;
            }
            set => openOrderActionText = value;
        }
    }
}