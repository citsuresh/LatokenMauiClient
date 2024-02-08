using CommunityToolkit.Mvvm.Messaging;

namespace LatokenMauiClient
{
    public partial class RewardsAndAirdropsPage : ContentPage
    {
        private bool isFirstVisit = true;

        public RewardsAndAirdropsViewModel ViewModel { get; set; }

        public RewardsAndAirdropsPage(RewardsAndAirdropsViewModel viewModel)
        {
            WeakReferenceMessenger.Default.Register<SelectedProfileChangedMessage>(this, this.OnSelectedProfileChangedMessage);
            this.ViewModel = viewModel;
            this.BindingContext = this.ViewModel;
            InitializeComponent();
        }

        private void OnSelectedProfileChangedMessage(object recipient, SelectedProfileChangedMessage message)
        {
            if (Shell.Current.CurrentPage == this && ViewModel != null && RefreshButton != null )
            {
                this.ViewModel.InitializeProfileAndRestClient();
                this.OnRefresh();
            }
        }

        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            if (this.isFirstVisit)
            {
                this.isFirstVisit = false;
                this.OnRefresh();
            }
        }


        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            this.OnRefresh();
        }
        private void OnRefresh()
        {
            RefreshButton.Text = "Refreshing...";
            RefreshButton.IsEnabled = false;
            Task.Run(() => RefreshTradingCompetitionRewards());
        }
        public void RefreshTradingCompetitionRewards()
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                this.ViewModel.IsRewardDurationEditable = false;
                BusyIndicator.IsVisible = true;
                BusyIndicator.IsRunning = true;
                TradingCompetitionRewardsGrid.IsEnabled = false;
                TradingCompetitionRewardsGrid.Children.Clear();
                TradingCompetitionRewardsGrid.RowDefinitions.Clear();
                AddHeaderRow();
            });

            var rewardsForDisplay = this.ViewModel.GetTradingCompetitionRewards();
            int index = 1;
            
            foreach (var competitionReward in rewardsForDisplay)
            {
                var timeStampLabel = new Label();
                timeStampLabel.SetValue(Grid.RowProperty, index);
                timeStampLabel.SetValue(Grid.ColumnProperty, 0);
                timeStampLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                timeStampLabel.SetValue(Label.TextProperty, competitionReward.Timestamp);

                var assetLabel = new Label();
                assetLabel.SetValue(Grid.RowProperty, index);
                assetLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);
                assetLabel.SetValue(Grid.ColumnProperty, 1);
                assetLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                assetLabel.SetValue(Label.TextProperty, competitionReward.CurrencySymbol);

                var transferringFundsLabel = new Label();
                transferringFundsLabel.SetValue(Grid.RowProperty, index);
                transferringFundsLabel.SetValue(Grid.ColumnProperty, 2);
                transferringFundsLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                transferringFundsLabel.SetValue(Label.TextProperty, competitionReward.TransferringFunds);

                var usdValueLabel = new Label();
                usdValueLabel.SetValue(Grid.RowProperty, index);
                usdValueLabel.SetValue(Grid.ColumnProperty, 3);
                usdValueLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                usdValueLabel.SetValue(Label.TextProperty, competitionReward.UsdValue);

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    TradingCompetitionRewardsGrid.RowDefinitions.Add(new RowDefinition());
                    TradingCompetitionRewardsGrid.Children.Add(timeStampLabel);
                    TradingCompetitionRewardsGrid.Children.Add(assetLabel);
                    TradingCompetitionRewardsGrid.Children.Add(transferringFundsLabel);
                    TradingCompetitionRewardsGrid.Children.Add(usdValueLabel);
                });
                index++;
            }

            Application.Current.Dispatcher.Dispatch(() =>
            {
                this.ViewModel.IsRewardDurationEditable = true;
                BusyIndicator.IsVisible = false;
                BusyIndicator.IsRunning = false;
                TradingCompetitionRewardsGrid.IsEnabled = true;
                RefreshButton.Text = "Refresh";
                RefreshButton.IsEnabled = true;
            });
        }

        private void AddHeaderRow()
        {
            var timeStampLabel = new Label();
            timeStampLabel.SetValue(Grid.RowProperty, 0);
            timeStampLabel.SetValue(Grid.ColumnProperty, 0);
            timeStampLabel.SetValue(Label.TextProperty, " Time");
            timeStampLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            timeStampLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            timeStampLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            var assetLabel = new Label();
            assetLabel.SetValue(Grid.RowProperty, 0);
            assetLabel.SetValue(Grid.ColumnProperty, 1);
            assetLabel.SetValue(Label.TextProperty, "Asset");
            assetLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            assetLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            assetLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            assetLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);

            var transferringFundsLabel = new Label();
            transferringFundsLabel.SetValue(Grid.RowProperty, 0);
            transferringFundsLabel.SetValue(Grid.ColumnProperty, 2);
            transferringFundsLabel.SetValue(Label.TextProperty, "Amount");
            transferringFundsLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            transferringFundsLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            transferringFundsLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            var usdValueLabel = new Label();
            usdValueLabel.SetValue(Grid.ColumnProperty, 3);
            usdValueLabel.SetValue(Grid.RowProperty, 0);
            usdValueLabel.SetValue(Label.TextProperty, "USD Value");
            usdValueLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            usdValueLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            usdValueLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            TradingCompetitionRewardsGrid.RowDefinitions.Add(new RowDefinition());
            TradingCompetitionRewardsGrid.Children.Add(timeStampLabel);
            TradingCompetitionRewardsGrid.Children.Add(assetLabel);
            TradingCompetitionRewardsGrid.Children.Add(transferringFundsLabel);
            TradingCompetitionRewardsGrid.Children.Add(usdValueLabel);
        }
    }

}
