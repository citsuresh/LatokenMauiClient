using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LatokenMauiClient
{
    public partial class AssetPageViewModel : ObservableObject
    {
        private UserProfile UserProfile { get; } = null;
        private LatokenRestClient RestClient { get; set; }

        private IEnumerable<CurrencyDto> currencies = new CurrencyDto[0];

        public AssetPageViewModel()
        {
            var profileName = Preferences.Default.Get<string>("ProfileName", string.Empty);
            var apiKey = Preferences.Default.Get<string>("ApiKey", string.Empty);
            var apiSecret = Preferences.Default.Get<string>("ApiSecret", string.Empty);

            if (!string.IsNullOrEmpty(profileName) &&
                !string.IsNullOrEmpty(apiKey) &&
                !string.IsNullOrEmpty(apiSecret))
            {
                this.UserProfile = new UserProfile { ProfileName = profileName, ApiKey = apiKey, ApiSecret = apiSecret };
                this.RestClient = new LatokenRestClient(this.UserProfile.ApiKey, this.UserProfile.ApiSecret);
                this.currencies = this.GetCurrencies();

            }
        }

        public IEnumerable<BalanceDto> LoadWalletAssets()
        {
            return this.LoadAssets("WALLET");
        }

        internal IEnumerable<BalanceDto> LoadSpotAssets()
        {
            return this.LoadAssets("SPOT");
        }

        public IEnumerable<BalanceDto> LoadAssets(string assetType)
        {
            if (this.RestClient != null)
            {
                var allBalances = this.RestClient.GetBalances(false);
                var assetTypeBalances = allBalances.Where(b => b.Type == assetType).ToList();
                //Populate the CurrencySymbol Property
                foreach (var asset in assetTypeBalances)
                {
                    var currency = currencies.FirstOrDefault(a => a.Id == asset.CurrencyId);

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

        public IEnumerable<CurrencyDto> GetCurrencies()
        {
            if (this.RestClient != null)
            {
                return this.RestClient.GetCurrencies();
            }

            return new CurrencyDto[0];
        }

        internal decimal GetAssetValue(BalanceDto asset)
        {
            if (asset != null)
            {
                if (!string.IsNullOrEmpty(asset.CurrencySymbol))
                {
                    if (asset.CurrencySymbol != "USDT")
                    {

                        var strPrice = this.RestClient.GetLastTrade(asset.CurrencySymbol, "USDT")?.Price;
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