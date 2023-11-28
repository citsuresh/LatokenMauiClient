namespace LatokenMauiClient
{
    public partial class SpotPage : ContentPage
    {
        private bool isFirstVisit = true;

        public AssetPageViewModel ViewModel { get; set; }
        private List<Task> Tasks { get; set; } = new List<Task>();

        public SpotPage(AssetPageViewModel viewModel)
        {
            InitializeComponent();
            this.ViewModel = viewModel;
        }


        private void ContentPage_Loaded(object sender, EventArgs e)
        {
            if (this.isFirstVisit)
            {
                this.isFirstVisit = false;
                this.Tasks.Clear();
                RefreshButton.Text = "Refreshing...";
                RefreshButton.IsEnabled = false;
                Task.Run(() => PopulateAssets());
            }
        }


        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            this.Tasks.Clear();
            RefreshButton.Text = "Refreshing...";
            RefreshButton.IsEnabled = false;
            Task.Run(() => PopulateAssets());
        }

        public void PopulateAssets()
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                BusyIndicator.IsVisible = true;
                BusyIndicator.IsRunning = true;
                AssetsGrid.IsEnabled = false;
                AssetsGrid.Children.Clear();
                AssetsGrid.RowDefinitions.Clear();
                AddHeaderRow();
            });

            var spotAssets = this.ViewModel.LoadSpotAssets();
            if (spotAssets.Any())
            {
                int index = 1;
                foreach (var asset in spotAssets)
                {
                    var nameLabel = new Label();
                    nameLabel.SetValue(Grid.RowProperty, index);
                    nameLabel.SetValue(Grid.ColumnProperty, 0);
                    nameLabel.SetValue(Label.TextProperty, asset.CurrencySymbol);
                    nameLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                    var quantityLabel = new Label();
                    quantityLabel.SetValue(Grid.RowProperty, index);
                    quantityLabel.SetValue(Grid.ColumnProperty, 1);
                    quantityLabel.SetValue(Label.TextProperty, asset.Available);
                    quantityLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                    var valueLabel = new Label();
                    valueLabel.SetValue(Grid.RowProperty, index);
                    valueLabel.SetValue(Grid.ColumnProperty, 2);
                    this.Tasks.Add(Task.Run(() => PopulateValueLabel(asset, valueLabel)));
                    valueLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                    Application.Current.Dispatcher.Dispatch(() =>
                    {
                        AssetsGrid.RowDefinitions.Add(new RowDefinition());
                        AssetsGrid.Children.Add(nameLabel);
                        AssetsGrid.Children.Add(quantityLabel);
                        AssetsGrid.Children.Add(valueLabel);
                    });

                    index++;
                }
                Task.WaitAll(this.Tasks.ToArray());
                this.Tasks.Clear();
                Application.Current.Dispatcher.Dispatch(() =>
                {
                    BusyIndicator.IsVisible = false;
                    BusyIndicator.IsRunning = false;
                    RefreshButton.Text = "Refresh";
                    RefreshButton.IsEnabled = true;
                    AssetsGrid.IsEnabled = true;
                });
            }
        }

        private void AddHeaderRow()
        {
            var nameLabel = new Label();
            nameLabel.SetValue(Grid.RowProperty, 0);
            nameLabel.SetValue(Grid.ColumnProperty, 0);
            nameLabel.SetValue(Label.TextProperty, " Name");
            nameLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            nameLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);

            var quantityLabel = new Label();
            quantityLabel.SetValue(Grid.RowProperty, 0);
            quantityLabel.SetValue(Grid.ColumnProperty, 1);
            quantityLabel.SetValue(Label.TextProperty, "Available");
            quantityLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            quantityLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);

            var valueLabel = new Label();
            valueLabel.SetValue(Grid.RowProperty, 0);
            valueLabel.SetValue(Grid.ColumnProperty, 2);
            valueLabel.SetValue(Label.TextProperty, "Value");
            valueLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            valueLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);

            AssetsGrid.RowDefinitions.Add(new RowDefinition());
            AssetsGrid.Children.Add(nameLabel);
            AssetsGrid.Children.Add(quantityLabel);
            AssetsGrid.Children.Add(valueLabel);
        }

        private void PopulateValueLabel(BalanceDto asset, Label valueLabel)
        {
            decimal assetValue = this.ViewModel.GetAssetValue(asset);
            if (assetValue > 0)
            {
                Application.Current.Dispatcher.Dispatch(() => valueLabel.SetValue(Label.TextProperty, assetValue));
            }
        }
    }

}
