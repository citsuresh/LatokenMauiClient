﻿namespace LatokenMauiClient
{
    public partial class TradingCompetitionsPage : ContentPage
    {
        private bool isFirstVisit = true;

        public TradingCompetitionsViewModel ViewModel { get; set; }

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
                RefreshButton.Text = "Refreshing...";
                RefreshButton.IsEnabled = false;
                Task.Run(() => RefreshTradingCompetitions());
            }
        }


        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            RefreshButton.Text = "Refreshing...";
            RefreshButton.IsEnabled = false;
            Task.Run(() => RefreshTradingCompetitions());
        }

        public void RefreshTradingCompetitions()
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

            var tradingCompetitionForDisplay = this.ViewModel.GetTradingCompetitions();
            int index = 1;
            foreach (var competition in tradingCompetitionForDisplay)
            {
                var userPosition = this.ViewModel.GetUserPositionForTradingCompetition(competition);

                var nameLabel = new Label();
                nameLabel.SetValue(Grid.RowProperty, index);
                nameLabel.SetValue(Grid.ColumnProperty, 0);
                nameLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                nameLabel.SetValue(Label.TextProperty, competition.Name);

                var userPositionLabel = new Label();
                userPositionLabel.SetValue(Grid.RowProperty, index);
                userPositionLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);
                userPositionLabel.SetValue(Grid.ColumnProperty, 1);
                userPositionLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                userPositionLabel.SetValue(Label.TextProperty, userPosition?.Position);

                var remainingTimeLabel = new Label();
                remainingTimeLabel.SetValue(Grid.RowProperty, index);
                remainingTimeLabel.SetValue(Grid.ColumnProperty, 2);
                remainingTimeLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                remainingTimeLabel.SetValue(Label.TextProperty, competition.RemainingTime);

                var targetLabel = new Label();
                targetLabel.SetValue(Grid.RowProperty, index);
                targetLabel.SetValue(Grid.ColumnProperty, 3);
                targetLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                targetLabel.SetValue(Label.TextProperty, competition.TargetDisplay);

                //var startDateLabel = new Label();
                //startDateLabel.SetValue(Grid.RowProperty, index);
                //startDateLabel.SetValue(Grid.ColumnProperty, 4);
                //startDateLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                //startDateLabel.SetValue(Label.TextProperty, competitionForDisplay.StartDate);

                //var endDateLabel = new Label();
                //endDateLabel.SetValue(Grid.RowProperty, index);
                //endDateLabel.SetValue(Grid.ColumnProperty, 5);
                //endDateLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                //endDateLabel.SetValue(Label.TextProperty, competitionForDisplay.EndDate);

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

            Application.Current.Dispatcher.Dispatch(() =>
            {
                BusyIndicator.IsVisible = false;
                BusyIndicator.IsRunning = false;
                TradingCompetitionGrid.IsEnabled = true;
                RefreshButton.Text = "Refresh";
                RefreshButton.IsEnabled = true;
            });
        }

        private void AddHeaderRow()
        {
            var nameLabel = new Label();
            nameLabel.SetValue(Grid.RowProperty, 0);
            nameLabel.SetValue(Grid.ColumnProperty, 0);
            nameLabel.SetValue(Label.TextProperty, " Name");
            nameLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            nameLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            nameLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            var userPositionLabel = new Label();
            userPositionLabel.SetValue(Grid.RowProperty, 0);
            userPositionLabel.SetValue(Grid.ColumnProperty, 1);
            userPositionLabel.SetValue(Label.TextProperty, "Position");
            userPositionLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            userPositionLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            userPositionLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            userPositionLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);

            var remainingTimeLabel = new Label();
            remainingTimeLabel.SetValue(Grid.RowProperty, 0);
            remainingTimeLabel.SetValue(Grid.ColumnProperty, 2);
            remainingTimeLabel.SetValue(Label.TextProperty, "Remaining");
            remainingTimeLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            remainingTimeLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            remainingTimeLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            var targetLabel = new Label();
            targetLabel.SetValue(Grid.ColumnProperty, 3);
            targetLabel.SetValue(Grid.RowProperty, 0);
            targetLabel.SetValue(Label.TextProperty, "Target");
            targetLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            targetLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            targetLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            //var startDateLabel = new Label();
            //startDateLabel.SetValue(Grid.RowProperty, 0);
            //startDateLabel.SetValue(Grid.ColumnProperty, 4);
            //startDateLabel.SetValue(Label.TextProperty, "Start Date");
            //startDateLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            //startDateLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            //startDateLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            //var endDateLabel = new Label();
            //endDateLabel.SetValue(Grid.RowProperty, 0);
            //endDateLabel.SetValue(Grid.ColumnProperty, 5);
            //endDateLabel.SetValue(Label.TextProperty, "End Date");
            //endDateLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            //endDateLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
            //endDateLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

            TradingCompetitionGrid.RowDefinitions.Add(new RowDefinition());
            TradingCompetitionGrid.Children.Add(nameLabel);
            TradingCompetitionGrid.Children.Add(userPositionLabel);
            TradingCompetitionGrid.Children.Add(remainingTimeLabel);
            TradingCompetitionGrid.Children.Add(targetLabel);
            //TradingCompetitionGrid.Children.Add(startDateLabel);
            //TradingCompetitionGrid.Children.Add(endDateLabel);
        }
    }

}
