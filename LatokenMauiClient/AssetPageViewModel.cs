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
        public UserProfile UserProfile { get; } = null;
        public LatokenRestClient RestClient { get; private set; }

        [ObservableProperty]
        private string assetType;

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
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        internal IEnumerable<BalanceDto> LoadWalletAssets()
        {
            if(this.UserProfile != null)
            {
                this.RestClient = new LatokenRestClient(this.UserProfile.ApiKey, this.UserProfile.ApiSecret);
                var balances = this.RestClient.GetBalances(false);
                return balances;
            }

            return new BalanceDto[0];
        }
    }
}