using CommunityToolkit.Mvvm.Messaging;
using System.Collections.Generic;

namespace LatokenMauiClient
{
    public partial class AirdropsPage : ContentPage
    {
        private bool isFirstVisit = true;
        private IServiceProvider serviceProvider;

        public AirdropsViewModel ViewModel { get; set; }

        public AirdropsPage(AirdropsViewModel viewModel, IServiceProvider serviceProvider)
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
            Task.Run(() => RefreshAirdrops());
        }
        public void RefreshAirdrops()
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                this.ViewModel.IsRefreshing = true;
                this.ViewModel.IsRewardDurationEditable = false;
                //BusyIndicator.IsVisible = true;
                //BusyIndicator.IsRunning = true;
                AirdropsGrid.IsEnabled = false;
                AirdropsGrid.Children.Clear();
                AirdropsGrid.RowDefinitions.Clear();
                AddHeaderRow();
            });

            if (this.ViewModel.UserProfile == null)
            {
                this.ViewModel.InitializeProfileAndRestClient();
            }

            LastUpdatedAirdrops lastUpdatedAirdrops = new LastUpdatedAirdrops();

            var lastUpdatedSettingsString = Preferences.Default.Get<string>("LastUpdatedAirdropData", string.Empty);
            if (!string.IsNullOrEmpty(lastUpdatedSettingsString))
            {
                lastUpdatedAirdrops.DeSerialize(lastUpdatedSettingsString);
            }

            var airdropsForDisplay = this.ViewModel.GetAirdrops().ToList();
            int index = 1;

            foreach (var airdrop in airdropsForDisplay)
            {
                var tickerLabel = new Label();
                tickerLabel.SetValue(Grid.RowProperty, index);
                tickerLabel.SetValue(Grid.ColumnProperty, 0);
                tickerLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                tickerLabel.SetValue(Label.TextProperty, airdrop.Ticker);


                if (lastUpdatedAirdrops != null && !lastUpdatedAirdrops.LastUpdatedAirdropData.Contains(airdrop.Ticker))
                {
                    tickerLabel.SetValue(Label.TextColorProperty, Colors.Green);
                    tickerLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
                }

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    AirdropsGrid.RowDefinitions.Add(new RowDefinition());
                    Thread.Sleep(100);
                    AirdropsGrid.Children.Add(tickerLabel);
                    //AirdropsGrid.Children.Add(usdValueLabel);
                    //AirdropsGrid.Children.Add(sellButton);
                });
                index++;
            }

            airdropsForDisplay.ForEach(a => lastUpdatedAirdrops.LastUpdatedAirdropData.Add(a.Ticker));

            Preferences.Default.Set<string>("LastUpdatedAirdropData", lastUpdatedAirdrops.Serialize()); ;

            Application.Current.Dispatcher.Dispatch(() =>
            {
                this.ViewModel.IsRewardDurationEditable = true;
                //BusyIndicator.IsVisible = false;
                //BusyIndicator.IsRunning = false;
                AirdropsGrid.IsEnabled = true;
                RefreshButton.Text = "Refresh";
                RefreshButton.IsEnabled = true;
                this.ViewModel.IsRefreshing = false;
            });
        }

        private void AddHeaderRow()
        {
            var tickerLabel = new Label();
            tickerLabel.SetValue(Grid.RowProperty, 0);
            tickerLabel.SetValue(Grid.ColumnProperty, 0);
            tickerLabel.SetValue(Label.TextProperty, " Airdrop Symbol");
            tickerLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            tickerLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            tickerLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            AirdropsGrid.RowDefinitions.Add(new RowDefinition());
            AirdropsGrid.Children.Add(tickerLabel);
        }

        private void RefreshView_Refreshing(object sender, EventArgs e)
        {
            this.OnRefresh();
        }
    }

}
