using CommunityToolkit.Mvvm.ComponentModel;
using Latoken.Api.Client.Library;
using System.Reactive.Linq;

namespace LatokenMauiClient
{
    public partial class TradingCompetitionsViewModel : ObservableObject
    {
        private UserProfile userProfile;
        private LatokenRestClient restClient;
        private ICurrencyCache currencyCache;

        public TradingCompetitionsViewModel(ICurrencyCache currencyCache)
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

        public IEnumerable<TradingCompetitionData> GetTradingCompetitions()
        {
            if (this.restClient != null && this.currencyCache != null)
            {
                var competitions = this.restClient.GetTradingCompetitions().OrderBy(c => c.EndDate);

                foreach (var competition in competitions)
                {
                    var offset = DateTimeOffset.FromUnixTimeMilliseconds(competition.EndDate - DateTimeOffset.Now.ToUnixTimeMilliseconds());
                    var remainingTime = $"{offset.DateTime.Day - 1} Days + {offset.DateTime.Hour}:{offset.DateTime.Minute}:{offset.DateTime.Second}";

                    yield return new TradingCompetitionData
                    {
                        Id = competition.Id,
                        Name = competition.Name.Replace("Trading competition", "").Trim(),
                        TotalRewards = competition.Budget,
                        WinnersLimit = competition.WinnersLimit,
                        CurrencyId = competition.CurrencyId,
                        StartDate = DateTimeOffset.FromUnixTimeMilliseconds(competition.StartDate).DateTime,
                        EndDate = DateTimeOffset.FromUnixTimeMilliseconds(competition.EndDate).DateTime,
                        Symbol = this.currencyCache.AvailableCurrencies.FirstOrDefault(c => c.Id == competition.CurrencyId)?.Symbol,
                        TargetDisplay = competition.Target.Replace("TRADING_COMPETITION_TARGET_", string.Empty).Replace("_PLUS_", " + "),
                        BudgetSplitDisplay = competition.BudgetSplit?.Replace("TRADING_COMPETITION_BUDGET_SPLIT_", string.Empty),
                        StatusDisplay = competition.Status.Replace("TRADING_COMPETITION_STATUS_", string.Empty),
                        RemainingTime = remainingTime
                    };
                }
            }
        }

        public TradingCompetitionUserPositionDto GetUserPositionForTradingCompetition(TradingCompetitionData competition)
        {
            if (restClient != null)
            {
                var userPosition = this.restClient.GetUserPositionForTradingCompetition(competition.Id);
                if (userPosition != null)
                {
                    var currency = this.currencyCache.GetCurrencyById(competition.CurrencyId); 
                    var lastTrade = this.restClient.GetLastTrade(currency?.Symbol, this.currencyCache.USDTCurrency.Symbol);

                    userPosition.UsdValue = (decimal.Parse(userPosition.RewardValue) * decimal.Parse(lastTrade.Price)).ToString("0.##");
                }

                return userPosition;
            }

            return null;
        }
    }
}