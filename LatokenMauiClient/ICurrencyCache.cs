﻿namespace LatokenMauiClient
{
    public interface ICurrencyCache
    {
        IRestClient RestClient { get; set; }

        bool IsInitialized { get; set; }

        List<CurrencyDto> AvailableCurrencies { get; set; }

        List<string> AvailableCurrencyTags { get; set; }

        List<PairDto> AvailablePairs { get; set; }

        void RefreshCache();
    }
}
