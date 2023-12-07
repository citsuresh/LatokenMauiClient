namespace LatokenMauiClient
{
    internal class LatokenCurrencyCache : ICurrencyCache
    {
        private IRestClient restClient;
        private List<CurrencyDto> availableCurrencies = new List<CurrencyDto>();
        private List<string> availableCurrencyTags = new List<string>();
        private List<PairDto> availablePairs = new List<PairDto>();
        private CurrencyDto usdtCurrency;

        public bool IsInitialized { get; set; } = false;

        public List<CurrencyDto> AvailableCurrencies
        {
            get
            {
                if (!IsInitialized)
                {
                    RefreshCache();
                }

                return availableCurrencies;
            }

            set
            {
                availableCurrencies = value;
            }
        }

        public CurrencyDto USDTCurrency
        {
            get
            {
                if (usdtCurrency == null)
                {
                    usdtCurrency = this.AvailableCurrencies.FirstOrDefault(c => c.Symbol == "USDT");
                }

                return usdtCurrency;
            }

            set
            {
                usdtCurrency = value;
            }
        }

        public List<string> AvailableCurrencyTags
        {
            get
            {
                if (!IsInitialized)
                {
                    RefreshCache();
                }

                return availableCurrencyTags;
            }
            set
            {
                availableCurrencyTags = value;
            }
        }

        public List<PairDto> AvailablePairs
        {
            get
            {
                if (!IsInitialized)
                {
                    RefreshCache();
                }

                return availablePairs;
            }

            set
            {
                availablePairs = value;
            }
        }


        public IRestClient RestClient
        {
            get
            {
                if (restClient == null)
                {
                    var profileName = Preferences.Default.Get<string>("ProfileName", string.Empty);
                    var apiKey = Preferences.Default.Get<string>("ApiKey", string.Empty);
                    var apiSecret = Preferences.Default.Get<string>("ApiSecret", string.Empty);
                    if (!string.IsNullOrEmpty(profileName) &&
                        !string.IsNullOrEmpty(apiKey) &&
                        !string.IsNullOrEmpty(apiSecret))
                    {
                        restClient = new LatokenRestClient(apiKey, apiSecret);
                    }
                }

                return restClient;
            }
            set
            {
                restClient = value;
            }
        }

        public CurrencyDto? GetCurrencyById(string currencyId)
        {
            return this.AvailableCurrencies.FirstOrDefault(curr => curr.Id == currencyId);
        }

        public void RefreshCache()
        {
            availableCurrencies.Clear();
            availableCurrencyTags.Clear();
            IsInitialized = false;
            if (RestClient != null)
            {
                RestClient.GetCurrencies().ToList()
                    .ForEach(c =>
                    {
                        availableCurrencyTags.Add(c.Symbol);
                        availableCurrencies.Add(c);
                    });
                IsInitialized = true;

                AvailablePairs = RestClient.GetAvailablePairs().ToList();
            }
        }
    }
}