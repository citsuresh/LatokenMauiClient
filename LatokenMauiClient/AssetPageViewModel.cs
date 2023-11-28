using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Linq;

namespace LatokenMauiClient
{
    public partial class AssetPageViewModel : ObservableObject
    {
        private UserProfile userProfile;
        private LatokenRestClient restClient;
        private ICurrencyCache currencyCache;

        public AssetPageViewModel(ICurrencyCache currencyCache)
        {
            this.currencyCache = currencyCache;
            var profileName = Preferences.Default.Get<string>("ProfileName", string.Empty);
            var apiKey = Preferences.Default.Get<string>("ApiKey", string.Empty);
            var apiSecret = Preferences.Default.Get<string>("ApiSecret", string.Empty);

            if (!string.IsNullOrEmpty(profileName) &&
                !string.IsNullOrEmpty(apiKey) &&
                !string.IsNullOrEmpty(apiSecret))
            {
                this.userProfile = new UserProfile { ProfileName = profileName, ApiKey = apiKey, ApiSecret = apiSecret };
                this.restClient = new LatokenRestClient(this.userProfile.ApiKey, this.userProfile.ApiSecret);
            }
        }

        public IEnumerable<BalanceDto> LoadWalletAssets()
        {
            return this.LoadAssets("WALLET");
        }

        public IEnumerable<BalanceDto> LoadSpotAssets()
        {
            return this.LoadAssets("SPOT");
        }

        private IEnumerable<BalanceDto> LoadAssets(string assetType)
        {
            if (this.restClient != null && this.currencyCache != null)
            {
                var allBalances = this.restClient.GetBalances(false);
                var assetTypeBalances = allBalances.Where(b => b.Type == assetType).ToList();

                //Populate the CurrencySymbol Property
                foreach (var asset in assetTypeBalances)
                {
                    var currency = this.currencyCache.AvailableCurrencies.FirstOrDefault(a => a.Id == asset.CurrencyId);

                    if (currency != null)
                    {
                        asset.CurrencySymbol = currency.Symbol;
                    }
                    else
                    {

                    }
                }
                return assetTypeBalances;
            }

            return new BalanceDto[0];
        }

        public decimal GetAssetValue(BalanceDto asset)
        {
            if (asset != null)
            {
                if (!string.IsNullOrEmpty(asset.CurrencySymbol))
                {
                    if (asset.CurrencySymbol != "USDT")
                    {

                        var strPrice = this.restClient.GetLastTrade(asset.CurrencySymbol, "USDT")?.Price;
                        decimal price;
                        if (!string.IsNullOrEmpty(strPrice) && decimal.TryParse(strPrice, out price))
                        {
                            return asset.Available * price;
                        }
                    }
                    else
                    {
                        // Return the available value, if the asset is USDT
                        return asset.Available;
                    }
                }
                else
                {
                    //not required as the CurrencySymbol is already populated and not empty
                }
            }

            return 0;
        }
    }
}