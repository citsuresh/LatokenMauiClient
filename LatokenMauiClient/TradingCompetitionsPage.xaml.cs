using Latoken.Api.Client.Library;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace LatokenMauiClient
{
    public partial class TradingCompetitionsPage : ContentPage
    {
        private bool isFirstVisit = true;

        public TradingCompetitionsViewModel ViewModel { get; set; }
        public RowDefinition HeaderRowDefinition { get; }
        public List<Task> Tasks { get; private set; }

        public TradingCompetitionsPage(TradingCompetitionsViewModel viewModel)
        {
            InitializeComponent();
            this.ViewModel = viewModel;
        }


        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            if (this.isFirstVisit)
            {
                this.isFirstVisit = false;
                this.Tasks = new List<Task>();
                RefreshButton.Text = "Refreshing...";
                RefreshButton.IsEnabled = false;
                Task.Run(() => RefreshTradingCompetitions());
            }
        }


        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            this.Tasks = new List<Task>();
            RefreshButton.Text = "Refreshing...";
            RefreshButton.IsEnabled = false;
            Task.Run(() => RefreshTradingCompetitions());
        }

        public void RefreshTradingCompetitions()
        {
            if (this.ViewModel.RestClient != null)
            {
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    BusyIndicator.IsVisible = true;
                    BusyIndicator.IsRunning = true;
                    TradingCompetitionGrid.IsEnabled = false;
                    TradingCompetitionGrid.Children.Clear();
                    TradingCompetitionGrid.RowDefinitions.Clear();
                    AddHeaderRow();
                });

                if (ViewModel.RestClient != null && this.ViewModel.CurrencyCache != null)
                {
                    var competitions = this.ViewModel.RestClient.GetTradingCompetitions().OrderBy(c => c.EndDate).ToList();

                    List<TradingCompetitionData> competitionsForDisplay = new List<TradingCompetitionData>();
                    int index = 1; 
                    foreach (var competition in competitions)
                    {
                        TradingCompetitionData competitionForDisplay = new TradingCompetitionData();
                        competitionForDisplay.Id = competition.Id;
                        competitionForDisplay.Name = competition.Name;
                        competitionForDisplay.TotalRewards = competition.Budget;
                        competitionForDisplay.WinnersLimit = competition.WinnersLimit;
                        competitionForDisplay.CurrencyId = competition.CurrencyId;
                        competitionForDisplay.StartDate = DateTimeOffset.FromUnixTimeMilliseconds(competition.StartDate).DateTime;
                        competitionForDisplay.EndDate = DateTimeOffset.FromUnixTimeMilliseconds(competition.EndDate).DateTime;
                        competitionForDisplay.Symbol = this.ViewModel.CurrencyCache.AvailableCurrencies.FirstOrDefault(c => c.Id == competition.CurrencyId)?.Symbol;
                        competitionForDisplay.TargetDisplay = competition.Target.Replace("TRADING_COMPETITION_TARGET_", string.Empty);
                        competitionForDisplay.BudgetSplitDisplay = competition.BudgetSplit?.Replace("TRADING_COMPETITION_BUDGET_SPLIT_", string.Empty);
                        competitionForDisplay.StatusDisplay = competition.Status.Replace("TRADING_COMPETITION_STATUS_", string.Empty);
                        var offset = DateTimeOffset.FromUnixTimeMilliseconds(competition.EndDate - DateTimeOffset.Now.ToUnixTimeMilliseconds());
                        var remainingTime = $"{offset.DateTime.Day - 1} Days + {offset.DateTime.Hour}:{offset.DateTime.Minute}:{offset.DateTime.Second}";
                        competitionForDisplay.RemainingTime = remainingTime;
                        competitionsForDisplay.Add(competitionForDisplay);

                        var userPosition = this.ViewModel.RestClient.GetUserPositionForTradingCompetition(competition.Id);


                        var nameLabel = new Label();
                        nameLabel.SetValue(Grid.RowProperty, index);
                        nameLabel.SetValue(Grid.ColumnProperty, 0);
                        nameLabel.SetValue(Label.TextProperty, competitionForDisplay.Name.Replace("Trading competition", "").Trim());
                        nameLabel.SetValue(Label.BackgroundProperty, Colors.White);

                        var userPositionLabel = new Label();
                        userPositionLabel.SetValue(Grid.RowProperty, index);
                        userPositionLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);
                        userPositionLabel.SetValue(Grid.ColumnProperty, 1);
                        userPositionLabel.SetValue(Label.TextProperty, userPosition.Position);
                        userPositionLabel.SetValue(Label.BackgroundProperty, Colors.White);

                        var remainingTimeLabel = new Label();
                        remainingTimeLabel.SetValue(Grid.RowProperty, index);
                        remainingTimeLabel.SetValue(Grid.ColumnProperty, 2);
                        remainingTimeLabel.SetValue(Label.TextProperty, competitionForDisplay.RemainingTime);
                        remainingTimeLabel.SetValue(Label.BackgroundProperty, Colors.White);

                        var targetLabel = new Label();
                        targetLabel.SetValue(Grid.RowProperty, index);
                        targetLabel.SetValue(Grid.ColumnProperty, 3);
                        targetLabel.SetValue(Label.TextProperty, competitionForDisplay.TargetDisplay.Replace("_PLUS_", " + "));
                        targetLabel.SetValue(Label.BackgroundProperty, Colors.White);

                        //var startDateLabel = new Label();
                        //startDateLabel.SetValue(Grid.RowProperty, index);
                        //startDateLabel.SetValue(Grid.ColumnProperty, 4);
                        //startDateLabel.SetValue(Label.TextProperty, competitionForDisplay.StartDate);
                        //startDateLabel.SetValue(Label.BackgroundProperty, Colors.White);

                        //var endDateLabel = new Label();
                        //endDateLabel.SetValue(Grid.RowProperty, index);
                        //endDateLabel.SetValue(Grid.ColumnProperty, 5);
                        //endDateLabel.SetValue(Label.TextProperty, competitionForDisplay.EndDate);
                        //endDateLabel.SetValue(Label.BackgroundProperty, Colors.White);

                        Application.Current.Dispatcher.Dispatch(() =>
                        {
                            TradingCompetitionGrid.RowDefinitions.Add(new RowDefinition());
                            TradingCompetitionGrid.Children.Add(nameLabel);
                            TradingCompetitionGrid.Children.Add(userPositionLabel);
                            TradingCompetitionGrid.Children.Add(remainingTimeLabel);
                            TradingCompetitionGrid.Children.Add(targetLabel);
                            //TradingCompetitionGrid.Children.Add(startDateLabel);
                            //TradingCompetitionGrid.Children.Add(endDateLabel);
                        });
                        index++;
                    }
                }
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    BusyIndicator.IsVisible = false;
                    BusyIndicator.IsRunning = false;
                    TradingCompetitionGrid.IsEnabled = true; 
                    RefreshButton.Text = "Refresh";
                    RefreshButton.IsEnabled = true;
                });
            }
        }


        private void AddHeaderRow()
        {
            var nameLabel = new Label();
            nameLabel.SetValue(Grid.RowProperty, 0);
            nameLabel.SetValue(Grid.ColumnProperty, 0);
            nameLabel.SetValue(Label.TextProperty, "Name");
            nameLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            //nameLabel.SetValue(Label.BackgroundProperty, Colors.White);

            var userPositionLabel = new Label();
            userPositionLabel.SetValue(Grid.RowProperty, 0);
            userPositionLabel.SetValue(Grid.ColumnProperty, 1);
            userPositionLabel.SetValue(Label.TextProperty, "Position");
            userPositionLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            userPositionLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);
            //userPositionLabel.SetValue(Label.BackgroundProperty, Colors.White);

            var remainingTimeLabel = new Label();
            remainingTimeLabel.SetValue(Grid.RowProperty, 0);
            remainingTimeLabel.SetValue(Grid.ColumnProperty, 2);
            remainingTimeLabel.SetValue(Label.TextProperty, "Remaining");
            remainingTimeLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            //valueLabel.SetValue(Label.BackgroundProperty, Colors.White);
            
            var targetLabel = new Label();
            targetLabel.SetValue(Grid.ColumnProperty, 3);
            targetLabel.SetValue(Grid.RowProperty, 0);
            targetLabel.SetValue(Label.TextProperty, "Target");
            targetLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            //valueLabel.SetValue(Label.BackgroundProperty, Colors.White);
            
            //var startDateLabel = new Label();
            //startDateLabel.SetValue(Grid.RowProperty, 0);
            //startDateLabel.SetValue(Grid.ColumnProperty, 4);
            //startDateLabel.SetValue(Label.TextProperty, "Start Date");
            //startDateLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            ////valueLabel.SetValue(Label.BackgroundProperty, Colors.White);
            
            //var endDateLabel = new Label();
            //endDateLabel.SetValue(Grid.RowProperty, 0);
            //endDateLabel.SetValue(Grid.ColumnProperty, 5);
            //endDateLabel.SetValue(Label.TextProperty, "End Date");
            //endDateLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            ////valueLabel.SetValue(Label.BackgroundProperty, Colors.White);

            TradingCompetitionGrid.RowDefinitions.Add(new RowDefinition());
            TradingCompetitionGrid.Children.Add(nameLabel);
            TradingCompetitionGrid.Children.Add(userPositionLabel);
            TradingCompetitionGrid.Children.Add(remainingTimeLabel);
            TradingCompetitionGrid.Children.Add(targetLabel);
            //TradingCompetitionGrid.Children.Add(startDateLabel);
            //TradingCompetitionGrid.Children.Add(endDateLabel);
        }

        private void PopulateValue(LatokenRestClient restClient, string symbol, BalanceDto asset, Label valueLabel)
        {
            var strPrice = restClient.GetLastTrade(symbol, "USDT")?.Price;
            decimal price;
            if (!string.IsNullOrEmpty(strPrice) && decimal.TryParse(strPrice, out price))
            {
                Application.Current.Dispatcher.Dispatch(() => valueLabel.SetValue(Label.TextProperty, asset.Available * price));
            }
        }
    }

}
