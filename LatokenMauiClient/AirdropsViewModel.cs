using CommunityToolkit.Mvvm.ComponentModel;
using Latoken.Api.Client.Library;
using System.Reactive.Linq;

namespace LatokenMauiClient
{
    public partial class AirdropsViewModel : ObservableObject
    {
        [ObservableProperty]
        private ICurrencyCache currencyCache;

        [ObservableProperty]
        private LatokenRestClient restClient;

        [ObservableProperty]
        private int rewardDuration = 10;

        [ObservableProperty]
        private bool isRewardDurationEditable = true;

        [ObservableProperty]
        private bool isRefreshing;


        [ObservableProperty]
        private Profile userProfile;

        public AirdropsViewModel(ICurrencyCache currencyCache)
        {
            this.currencyCache = currencyCache;
            //InitializeProfileAndRestClient();
        }

        internal void InitializeProfileAndRestClient()
        {
            var profileFilterInstance = App.Current.Resources["ProfileFilterInstance"] as ProfileFilter;
            if (profileFilterInstance != null && profileFilterInstance.SelectedProfile != null)
            {
                var profile = profileFilterInstance.SelectedProfile;
                this.userProfile = profile;
                this.restClient = new LatokenRestClient(profile.ApiKey, profile.ApiSecret);
            }
        }

        internal void InitializeProfileAndRestClient(Profile profile)
        {
            this.userProfile = profile;
            this.restClient = new LatokenRestClient(profile.ApiKey, profile.ApiSecret);
        }

        public IEnumerable<Airdrop> GetAirdrops()
        {
            var airdrops = this.restClient.GetAirdrops().ToList();

            return airdrops;
        }
    }
}