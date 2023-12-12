using CommunityToolkit.Mvvm.ComponentModel;
using System.Reactive.Linq;

namespace LatokenMauiClient
{
    public partial class RewardsAndAirdropsViewModel : ObservableObject
    {
        private UserProfile userProfile;
        private LatokenRestClient restClient;
        private ICurrencyCache currencyCache;

        [ObservableProperty]
        private int rewardDuration = 10;

        [ObservableProperty]
        private bool isRewardDurationEditable = true;

        public RewardsAndAirdropsViewModel(ICurrencyCache currencyCache)
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

        public IEnumerable<TransferDto> GetTradingCompetitionRewards()
        {
            var days = this.RewardDuration;
            var toDate = DateTime.Now.AddDays(-1 * days);

            List<TransferDto> transferList = new List<TransferDto>();
            bool isFetchComplete = false;
            int page = 0;
            while (isFetchComplete == false)
            {
                var transfers = this.restClient?.GetAllTransfers(page).ToList();

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

                    transferInfo.CurrencySymbol = currencyCache.AvailableCurrencies.FirstOrDefault(c => c.Id == transferInfo.Currency)?.Symbol;
                    if (string.IsNullOrEmpty(transferInfo.CurrencySymbol))
                    {
                        //TODO: cannot resolve currency
                    }
                }

                page++;
            }

            return transferList;
        }
    }
}