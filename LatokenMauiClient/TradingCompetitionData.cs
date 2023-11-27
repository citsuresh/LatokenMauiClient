using System;

namespace LatokenMauiClient
{
    public class TradingCompetitionData
    {
        public string Id { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string CurrencyId { get; set; }

        public string Symbol { get; set; }

        public string TargetDisplay { get; set; }

        public string BudgetSplitDisplay { get; set; }

        public string StatusDisplay { get; set; }

        public string RemainingTime { get; set; }
        public string Name { get; set; }
        public string TotalRewards { get; set; }
        public int WinnersLimit { get; set; }
    }
}
