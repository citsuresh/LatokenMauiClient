namespace LatokenMauiClient
{
    public class PairDto
    {
        public int Id { get; set; }
        public string BaseCurrencyId { get; set; }
        public string QuoteCurrencyId { get; set; }
        public int QuantityDecimals { get; set; }
        public decimal PriceTick { get; set; }
    }
}