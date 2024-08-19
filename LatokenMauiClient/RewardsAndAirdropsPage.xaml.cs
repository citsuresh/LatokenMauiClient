using CommunityToolkit.Mvvm.Messaging;

namespace LatokenMauiClient
{
    public partial class RewardsAndAirdropsPage : ContentPage
    {
        private bool isFirstVisit = true;
        private IServiceProvider serviceProvider;

        public RewardsAndAirdropsViewModel ViewModel { get; set; }

        public RewardsAndAirdropsPage(RewardsAndAirdropsViewModel viewModel, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            WeakReferenceMessenger.Default.Register<SelectedProfileChangedMessage>(this, this.OnSelectedProfileChangedMessage);
            this.ViewModel = viewModel;
            this.BindingContext = this.ViewModel;
            InitializeComponent();
        }

        private void OnSelectedProfileChangedMessage(object recipient, SelectedProfileChangedMessage message)
        {
            if (Shell.Current.CurrentPage == this && ViewModel != null && RefreshButton != null)
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
                this.ViewModel.IsRefreshing = true;
                this.ViewModel.IsRewardDurationEditable = false;
                //BusyIndicator.IsVisible = true;
                //BusyIndicator.IsRunning = true;
                TradingCompetitionRewardsGrid.IsEnabled = false;
                TradingCompetitionRewardsGrid.Children.Clear();
                TradingCompetitionRewardsGrid.RowDefinitions.Clear();
                AddHeaderRow();
            });

            if (this.ViewModel.UserProfile == null)
            {
                this.ViewModel.InitializeProfileAndRestClient();
            }

            var profileName = this.ViewModel.UserProfile?.ProfileName;
            LastUpdatedTimestampData lastUpdatedTimestampData = null;
            LastUpdatedTransfersTimestampData lastUpdatedTimestampDataForProfile = null;
            if (profileName != null)
            {
                var lastUpdatedSettingsString = Preferences.Default.Get<string>(profileName + "_LastUpdatedTimestampData", string.Empty); ;
                if (!string.IsNullOrEmpty(lastUpdatedSettingsString))
                {
                    lastUpdatedTimestampData = new LastUpdatedTimestampData();
                    lastUpdatedTimestampData.DeSerialize(lastUpdatedSettingsString);
                }

                if (lastUpdatedTimestampData == null)
                {
                    lastUpdatedTimestampData = new LastUpdatedTimestampData();
                }

                if (lastUpdatedTimestampData.TransfersTimestamps.ContainsKey(profileName))
                {
                    lastUpdatedTimestampDataForProfile = lastUpdatedTimestampData.TransfersTimestamps[profileName];
                }
                else
                {
                    lastUpdatedTimestampDataForProfile = new LastUpdatedTransfersTimestampData();
                }
            }

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

                var verticalLayout = new VerticalStackLayout();
                verticalLayout.SetValue(Grid.RowProperty, index);
                verticalLayout.SetValue(Grid.ColumnProperty, 3);
                verticalLayout.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                var usdValueLabel = new Label();
                //usdValueLabel.SetValue(Grid.RowProperty, index);
                //usdValueLabel.SetValue(Grid.ColumnProperty, 3);
                usdValueLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                usdValueLabel.SetValue(Label.TextProperty, competitionReward.UsdValue);

                var sellButton = new Button();
                //sellButton.SetValue(Grid.RowProperty, index);
                //sellButton.SetValue(Grid.ColumnProperty, 3);
                sellButton.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                sellButton.SetValue(Button.TextProperty, "Sell");
                //sellButton.SetValue(Button.HeightRequestProperty, 30);
                sellButton.Clicked += SellButton_Clicked;
                sellButton.SetValue(Button.BindingContextProperty, competitionReward);
                verticalLayout.Children.Add(usdValueLabel);
                verticalLayout.Children.Add(sellButton);


                if (lastUpdatedTimestampDataForProfile != null && (lastUpdatedTimestampDataForProfile.LastUpdatedRewardsAirdropsTimeStamp == null
                    || competitionReward.Timestamp > lastUpdatedTimestampDataForProfile.LastUpdatedRewardsAirdropsTimeStamp))
                {
                    timeStampLabel.SetValue(Label.TextColorProperty, Colors.Green);
                    assetLabel.SetValue(Label.TextColorProperty, Colors.Green);
                    transferringFundsLabel.SetValue(Label.TextColorProperty, Colors.Green);
                    usdValueLabel.SetValue(Label.TextColorProperty, Colors.Green);
                    timeStampLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
                    assetLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
                    transferringFundsLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
                    usdValueLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
                }

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    TradingCompetitionRewardsGrid.RowDefinitions.Add(new RowDefinition());
                    Thread.Sleep(100);
                    TradingCompetitionRewardsGrid.Children.Add(timeStampLabel);
                    TradingCompetitionRewardsGrid.Children.Add(assetLabel);
                    TradingCompetitionRewardsGrid.Children.Add(transferringFundsLabel);
                    TradingCompetitionRewardsGrid.Children.Add(verticalLayout);
                    //TradingCompetitionRewardsGrid.Children.Add(usdValueLabel);
                    //TradingCompetitionRewardsGrid.Children.Add(sellButton);
                });
                index++;
            }

            if (profileName != null)
            {
                lastUpdatedTimestampDataForProfile.LastUpdatedRewardsAirdropsTimeStamp = rewardsForDisplay.FirstOrDefault()?.Timestamp;

                lastUpdatedTimestampData.TransfersTimestamps[profileName] = lastUpdatedTimestampDataForProfile;

                Preferences.Default.Set<string>(profileName + "_LastUpdatedTimestampData", lastUpdatedTimestampData.Serialize()); ;
            }

            Application.Current.Dispatcher.Dispatch(() =>
            {
                this.ViewModel.IsRewardDurationEditable = true;
                //BusyIndicator.IsVisible = false;
                //BusyIndicator.IsRunning = false;
                TradingCompetitionRewardsGrid.IsEnabled = true;
                RefreshButton.Text = "Refresh";
                RefreshButton.IsEnabled = true;
                this.ViewModel.IsRefreshing = false;
            });
        }

        private void SellButton_Clicked(object? sender, EventArgs e)
        {
            var button = sender as Button;
            var transferDto = button?.BindingContext as TransferDto;
            if (transferDto != null)
            {
                var balanceDto = this.ViewModel.RestClient?.GetWalletBalanceByCurrency(transferDto.Currency);
                if (balanceDto != null)
                {
                    this.LaunchSellWindow(this.ViewModel.UserProfile, balanceDto);
                }
            }
        }

        private void LaunchSellWindow(Profile userProfile, BalanceDto balanceDto)
        {
            var availablePairs = this.ViewModel.CurrencyCache.AvailablePairs.Where(p => p.BaseCurrencyId == balanceDto.CurrencyId).ToList();
            if (!availablePairs.Any())
            {
                this.DisplayAlert("No Trading Pair", $"No Matching Trading pair available.", "OK", "Cancel", FlowDirection.LeftToRight);
            }
            else
            {
                if (balanceDto.CurrencySymbol == null) // This is null when launched from Transfers view
                {
                    balanceDto.CurrencySymbol = this.ViewModel.CurrencyCache.AvailableCurrencies.FirstOrDefault(c => c?.Id == balanceDto.CurrencyId)?.Symbol;
                }

                //Set the selected profile for the sell window
                var profileFilterInstance = App.Current.Resources["ProfileFilterInstance"] as ProfileFilter;
                if (profileFilterInstance != null && profileFilterInstance.SelectedProfile != null)
                {
                    profileFilterInstance.SelectedProfile = userProfile;
                }

                var sellPage = new SellPage(new SellViewModel(serviceProvider), serviceProvider);
                sellPage.Initialize(userProfile, this.ViewModel.RestClient, this.ViewModel.CurrencyCache, availablePairs.First(), balanceDto);
                Navigation.PushAsync(sellPage);
            }
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

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            this.OnRefresh();
        }
    }

}
