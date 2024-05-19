using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Linq;

namespace LatokenMauiClient
{
    public partial class AssetPageViewModel : ObservableObject
    {

        [ObservableProperty] 
        private Profile userProfile;

        [ObservableProperty] 
        private LatokenRestClient restClient;

        [ObservableProperty] 
        private ICurrencyCache currencyCache;

        public AssetPageViewModel(ICurrencyCache currencyCache)
        {
            this.currencyCache = currencyCache;            
            //InitializeProfileAndRestClient();
        }

        public void InitializeProfileAndRestClient()
        {
            var profileFilterInstance = App.Current.Resources["ProfileFilterInstance"] as ProfileFilter;
            if (profileFilterInstance != null && profileFilterInstance.SelectedProfile != null)
            {
                var profile = profileFilterInstance.SelectedProfile;
                this.userProfile = profile;
                this.restClient = new LatokenRestClient(profile.ApiKey, profile.ApiSecret);
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
            if (this.restClient == null)
            {
                this.InitializeProfileAndRestClient();
            }

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