using CommunityToolkit.Mvvm.ComponentModel;
using Latoken.Api.Client.Library;
using System.Reactive.Linq;

namespace LatokenMauiClient
{
    public partial class TradingCompetitionsViewModel : ObservableObject
    {
        private Profile userProfile;
        private LatokenRestClient restClient;
        private ICurrencyCache currencyCache;

        public TradingCompetitionsViewModel(ICurrencyCache currencyCache)
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
                if (profile != null && profile.ProfileName != null && profile.ApiKey != null && profile.ApiSecret != null)
                {
                    this.userProfile = profile;
                    this.restClient = new LatokenRestClient(profile.ApiKey, profile.ApiSecret);
                }
            }
        }

        public IEnumerable<TradingCompetitionData> GetTradingCompetitions()
        {
            if (this.restClient == null)
            {
                this.InitializeProfileAndRestClient();
            }

            List<TradingCompetitionData> competitionData = new List<TradingCompetitionData>();
            if (this.restClient != null && this.currencyCache != null)
            {
                try
                {
                    var competitions = this.restClient.GetTradingCompetitions().OrderBy(c => c.EndDate);
                    foreach (var competition in competitions)
                    {
                        var offset = DateTimeOffset.FromUnixTimeMilliseconds(competition.EndDate - DateTimeOffset.Now.ToUnixTimeMilliseconds());
                        var remainingTime = $"{offset.DateTime.Day - 1} Days + {offset.DateTime.Hour}:{offset.DateTime.Minute}:{offset.DateTime.Second}";

                        competitionData.Add(new TradingCompetitionData
                        {
                            Id = competition.Id,
                            Name = competition.Name.Replace("Trading competition", "").Trim(),
                            //TotalRewards = competition.Budget,
                            PlaceRewardsDisplay = string.Join(", ", competition.PlaceRewards),
                            WinnersLimit = competition.WinnersLimit,
                            CurrencyId = competition.CurrencyId,
                            StartDate = DateTimeOffset.FromUnixTimeMilliseconds(competition.StartDate).DateTime,
                            EndDate = DateTimeOffset.FromUnixTimeMilliseconds(competition.EndDate).DateTime,
                            Symbol = this.currencyCache.AvailableCurrencies.FirstOrDefault(c => c.Id == competition.CurrencyId)?.Symbol,
                            TargetDisplay = competition.Target.Replace("TRADING_COMPETITION_TARGET_", string.Empty).Replace("_PLUS_", " + "),
                            //BudgetSplitDisplay = competition.BudgetSplit?.Replace("TRADING_COMPETITION_BUDGET_SPLIT_", string.Empty),
                            StatusDisplay = competition.Status.Replace("TRADING_COMPETITION_STATUS_", string.Empty),
                            RemainingTime = remainingTime
                        });
                    }
                }
                catch (Exception ex)
                {
                }
            }

            return competitionData;
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