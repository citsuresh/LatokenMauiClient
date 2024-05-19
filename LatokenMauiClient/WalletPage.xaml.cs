
using CommunityToolkit.Mvvm.Messaging;

namespace LatokenMauiClient
{
    public partial class WalletPage : ContentPage
    {
        private bool isFirstVisit = true;
        private IServiceProvider serviceProvider;

        public AssetPageViewModel ViewModel { get; set; }
        private List<Task> Tasks { get; set; } = new List<Task>();

        public WalletPage(AssetPageViewModel viewModel, IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            WeakReferenceMessenger.Default.Register<SelectedProfileChangedMessage>(this, this.OnSelectedProfileChangedMessage);
            InitializeComponent();
            this.ViewModel = viewModel;
        }

        private void OnSelectedProfileChangedMessage(object recipient, SelectedProfileChangedMessage message)
        {
            if (Shell.Current.CurrentPage == this && ViewModel != null && this.RefreshButton != null)
            {
                this.ViewModel.InitializeProfileAndRestClient();
                this.OnRefresh();
            }
        }


        private async void ContentPage_Loaded(object sender, EventArgs e)
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

            var walletAssets = this.ViewModel.LoadWalletAssets();
            if (walletAssets.Any())
            {
                int index = 1;
                foreach (var asset in walletAssets)
                {
                    HorizontalStackLayout nameStackLayout = new HorizontalStackLayout();
                    nameStackLayout.SetValue(Grid.RowProperty, index);
                    nameStackLayout.SetValue(Grid.ColumnProperty, 0);
                    nameStackLayout.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                    var nameLabel = new Label();
                    nameLabel.SetValue(Label.TextProperty, asset.CurrencySymbol);

                    var txLabel = new Label();
                    txLabel.SetValue(Label.MarginProperty, new Thickness(5, 0, 0, 0));
                    txLabel.SetValue(Label.TextProperty, "<->");
                    txLabel.SetValue(Label.TextDecorationsProperty, TextDecorations.Underline);
                    var tapGestureRecognizer = new TapGestureRecognizer();
                    tapGestureRecognizer.Tapped += TxLabel_Tapped;
                    tapGestureRecognizer.CommandParameter = asset;
                    txLabel.GestureRecognizers.Add(tapGestureRecognizer);

                    nameStackLayout.Children.Add(nameLabel);
                    nameStackLayout.Children.Add(txLabel);

                    var quantityLabel = new Label();
                    quantityLabel.SetValue(Grid.RowProperty, index);
                    quantityLabel.SetValue(Grid.ColumnProperty, 1);
                    quantityLabel.SetValue(Label.TextProperty, asset.Available.ToString().TrimEnd('0').TrimEnd('.'));
                    quantityLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                    quantityLabel.SetValue(Label.LineBreakModeProperty, LineBreakMode.TailTruncation);

                    var valueLabel = new Label();
                    valueLabel.SetValue(Grid.RowProperty, index);
                    valueLabel.SetValue(Grid.ColumnProperty, 2);
                    this.Tasks.Add(Task.Run(() => PopulateValueLabel(asset, valueLabel)));
                    valueLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                    valueLabel.SetValue(Label.LineBreakModeProperty, LineBreakMode.TailTruncation);

                    Application.Current.Dispatcher.Dispatch(() =>
                    {
                        AssetsGrid.RowDefinitions.Add(new RowDefinition());
                        AssetsGrid.Children.Add(nameStackLayout);
                        AssetsGrid.Children.Add(quantityLabel);
                        AssetsGrid.Children.Add(valueLabel);
                    });

                    index++;
                }
                Task.WaitAll(this.Tasks.ToArray());
                this.Tasks.Clear();

                this.SortAndRepopulateAssets(walletAssets);

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

        private void TxLabel_Tapped(object? sender, TappedEventArgs e)
        {
            var asset = e.Parameter as BalanceDto;
            if (asset != null)
            {
                var transferPage = new TransferPage(new TransferViewModel(serviceProvider));
                transferPage.Initialize(this.ViewModel.UserProfile, this.ViewModel.RestClient, this.ViewModel.CurrencyCache, asset);
                Navigation.PushAsync(transferPage);
            }
        }

        private void SortAndRepopulateAssets(IEnumerable<BalanceDto> assets)
        {
            assets = assets.OrderByDescending(a => a.AvailableValue);
            var totalValue = assets.Sum(a => a.AvailableValue);
            Application.Current.Dispatcher.Dispatch(() =>
            {
                WalletAssetsHeadingLabel.Text = "Wallet Assets ( " + totalValue.ToString("0.##") + " USDT )";
                AssetsGrid.RowDefinitions.Clear();
                AssetsGrid.Children.Clear();
                AddHeaderRow();
            });

            int index = 1;
            foreach (var asset in assets)
            {
                HorizontalStackLayout nameStackLayout = new HorizontalStackLayout();
                nameStackLayout.SetValue(Grid.RowProperty, index);
                nameStackLayout.SetValue(Grid.ColumnProperty, 0);
                nameStackLayout.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));

                var nameLabel = new Label();
                nameLabel.SetValue(Label.TextProperty, asset.CurrencySymbol);

                var txLabel = new Label();
                txLabel.SetValue(Label.MarginProperty, new Thickness(5, 0, 0, 0));
                txLabel.SetValue(Label.TextProperty, "<->");
                txLabel.SetValue(Label.TextDecorationsProperty, TextDecorations.Underline);
                var tapGestureRecognizer = new TapGestureRecognizer();
                tapGestureRecognizer.Tapped += TxLabel_Tapped;
                tapGestureRecognizer.CommandParameter = asset;
                txLabel.GestureRecognizers.Add(tapGestureRecognizer);

                nameStackLayout.Children.Add(nameLabel);
                nameStackLayout.Children.Add(txLabel);

                var quantityLabel = new Label();
                quantityLabel.SetValue(Grid.RowProperty, index);
                quantityLabel.SetValue(Grid.ColumnProperty, 1);
                quantityLabel.SetValue(Label.TextProperty, asset.Available.ToString().TrimEnd('0').TrimEnd('.'));
                quantityLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                quantityLabel.SetValue(Label.LineBreakModeProperty, LineBreakMode.TailTruncation);

                var valueLabel = new Label();
                valueLabel.SetValue(Grid.RowProperty, index);
                valueLabel.SetValue(Grid.ColumnProperty, 2);
                valueLabel.SetValue(Label.TextProperty, asset.AvailableValue.ToString().TrimEnd('0').TrimEnd('.'));
                valueLabel.SetValue(Grid.MarginProperty, new Thickness(5, 0, 0, 5));
                valueLabel.SetValue(Label.LineBreakModeProperty, LineBreakMode.TailTruncation);

                Application.Current.Dispatcher.Dispatch(() =>
                {
                    AssetsGrid.RowDefinitions.Add(new RowDefinition());
                    AssetsGrid.Children.Add(nameStackLayout);
                    AssetsGrid.Children.Add(quantityLabel);
                    AssetsGrid.Children.Add(valueLabel);
                });

                index++;
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
            valueLabel.SetValue(Label.TextProperty, "Value in USDT");
            valueLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
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
            asset.AvailableValue = assetValue;
            if (assetValue > 0)
            {
                Application.Current.Dispatcher.Dispatch(() => valueLabel.SetValue(Label.TextProperty, assetValue.ToString().TrimEnd('0').TrimEnd('.')));
            }
        }
    }

}
