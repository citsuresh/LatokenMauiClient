using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LatokenMauiClient
{
    public partial class TradingCompetitionsViewModel : ObservableObject
    {
        public UserProfile UserProfile { get; } = null;
        public LatokenRestClient RestClient { get; private set; }
        public ICurrencyCache CurrencyCache { get; internal set; }

        public TradingCompetitionsViewModel(ICurrencyCache currencyCache)
        {
            this.CurrencyCache = currencyCache;
            var profileName = Preferences.Default.Get<string>("ProfileName", string.Empty);
            var apiKey = Preferences.Default.Get<string>("ApiKey", string.Empty);
            var apiSecret = Preferences.Default.Get<string>("ApiSecret", string.Empty);

            if (!string.IsNullOrEmpty(profileName) &&
                !string.IsNullOrEmpty(apiKey) &&
                !string.IsNullOrEmpty(apiSecret))
            {
                this.UserProfile = new UserProfile { ProfileName = profileName, ApiKey = apiKey, ApiSecret = apiSecret }; 
                this.RestClient = new LatokenRestClient(this.UserProfile.ApiKey, this.UserProfile.ApiSecret);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    }
}