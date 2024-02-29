using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Linq;

namespace LatokenMauiClient
{
    public partial class RewardsAndAirdropsViewModel : ObservableObject
    {
        private LatokenRestClient restClient;
        private ICurrencyCache currencyCache;

        [ObservableProperty]
        private int rewardDuration = 10;

        [ObservableProperty]
        private bool isRewardDurationEditable = true;


        [ObservableProperty]
        private Profile userProfile;

        public RewardsAndAirdropsViewModel(ICurrencyCache currencyCache)
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

        public IEnumerable<TransferDto> GetTradingCompetitionRewards()
        {
            List<TransferDto> transferList = new List<TransferDto>();
            if (this.restClient == null)
            {
                this.InitializeProfileAndRestClient();
            }
            if (this.restClient != null && this.currencyCache != null)
            {
                var days = this.RewardDuration;
                var toDate = DateTime.Now.AddDays(-1 * days);

                bool isFetchComplete = false;
                int page = 0;
                while (isFetchComplete == false)
                {
                    var transfers = this.restClient.GetAllTransfers(page).ToList();

                    if (transfers == null || !transfers.Any())
                    {
                        isFetchComplete = true;
                    }
                    else
                    {
                        foreach (var transfer in transfers)
                        {
                            if (transfer.Timestamp < toDate)
                            {
                                isFetchComplete = true;
                            }
                            else if (transfer.Type.Contains("TRADING_COMPETITION")
                                        || transfer.Type.Contains("AIRDROP")
                                        || transfer.Type.Contains("DISTRIBUTION"))
                            {
                                transferList.Add(transfer);
                            }
                        }
                    }

                    //Formatting and modifications before displaying in grid control
                    foreach (TransferDto transferInfo in transferList)
                    {
                        transferInfo.Method = transferInfo.Method.Replace("TRANSFER_METHOD_", "");
                        transferInfo.Status = transferInfo.Status.Replace("TRANSFER_STATUS_", "");
                        transferInfo.Type = transferInfo.Type.Replace("TRANSFER_TYPE_", "");

                        transferInfo.CurrencySymbol = this.currencyCache.AvailableCurrencies.FirstOrDefault(c => c.Id == transferInfo.Currency)?.Symbol;
                        if (string.IsNullOrEmpty(transferInfo.CurrencySymbol))
                        {
                            //TODO: cannot resolve currency
                        }
                    }

                    page++;
                }
            }

            return transferList;
        }
    }
}