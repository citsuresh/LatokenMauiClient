
namespace LatokenMauiClient
{
    public partial class WalletPage : ContentPage
    {
        private bool isFirstVisit = true;

        public AssetPageViewModel ViewModel { get; set; }
        public RowDefinition HeaderRowDefinition { get; }
        public List<Task> Tasks { get; private set; }

        public WalletPage(AssetPageViewModel viewModel)
        {
            InitializeComponent();
            this.ViewModel = viewModel;
            this.ViewModel.AssetType = "WALLET";
        }


        private async void ContentPage_Loaded(object sender, EventArgs e)
        {
            if (this.isFirstVisit)
            {
                this.isFirstVisit = false; 
                this.Tasks = new List<Task>();
                RefreshButton.Text = "Refreshing...";
                RefreshButton.IsEnabled = false;
                Task.Run(() => PopulateAssets());
            }
        }


        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            this.Tasks = new List<Task>();
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

            var walletAssets = this.ViewModel.LoadWalletAssets();
            if (walletAssets.Any() && this.ViewModel.RestClient != null)
            {
                var currencies = this.ViewModel.RestClient.GetCurrencies();
                int index = 1;
                foreach (var asset in walletAssets)
                {
                    if (asset.Type != "WALLET")
                        continue;

                    var currency = currencies.FirstOrDefault(a => a.Id == asset.CurrencyId);
                    if (currency != null)
                    {

                        var nameLabel = new Label();
                        nameLabel.SetValue(Grid.RowProperty, index);
                        nameLabel.SetValue(Grid.ColumnProperty, 0);
                        nameLabel.SetValue(Label.TextProperty, currency.Symbol);
                        nameLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                        var quantityLabel = new Label();
                        quantityLabel.SetValue(Grid.RowProperty, index);
                        quantityLabel.SetValue(Grid.ColumnProperty, 1);
                        quantityLabel.SetValue(Label.TextProperty, asset.Available);
                        quantityLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                        var valueLabel = new Label();
                        valueLabel.SetValue(Grid.RowProperty, index);
                        valueLabel.SetValue(Grid.ColumnProperty, 2);
                        if (currency.Symbol != "USDT")
                        {
                            this.Tasks.Add(Task.Run(() => PopulateValue(this.ViewModel.RestClient, currency.Symbol, asset, valueLabel)));
                        }
                        else
                        {
                            valueLabel.SetValue(Label.TextProperty, asset.Available);
                        }
                        valueLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                        Application.Current.Dispatcher.Dispatch(() =>
                        {
                            AssetsGrid.RowDefinitions.Add(new RowDefinition());
                            AssetsGrid.Children.Add(nameLabel);
                            AssetsGrid.Children.Add(quantityLabel);
                            AssetsGrid.Children.Add(valueLabel);
                        });
                    }
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
            nameLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            nameLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            nameLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);

            var quantityLabel = new Label();
            quantityLabel.SetValue(Grid.RowProperty, 0);
            quantityLabel.SetValue(Grid.ColumnProperty, 1);
            quantityLabel.SetValue(Label.TextProperty, "Quantity");
            quantityLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            quantityLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            quantityLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);

            var valueLabel = new Label();
            valueLabel.SetValue(Grid.RowProperty, 0);
            valueLabel.SetValue(Grid.ColumnProperty, 2);
            valueLabel.SetValue(Label.TextProperty, "Value");
            valueLabel.SetValue(Label.FontAttributesProperty,  FontAttributes.Bold);
            valueLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
            valueLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);

            AssetsGrid.RowDefinitions.Add(new RowDefinition());
            AssetsGrid.Children.Add(nameLabel);
            AssetsGrid.Children.Add(quantityLabel);
            AssetsGrid.Children.Add(valueLabel);
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
