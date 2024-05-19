
namespace LatokenMauiClient;

public partial class SellPage : ContentPage
{
    private IServiceProvider serviceProvider;
    public SellViewModel ViewModel { get; set; }

    private Label[] priceLables = new Label[14];
    private Label[] quantityLabels = new Label[14];
    private Label[] accumulatedLabels = new Label[14];

    public SellPage(SellViewModel viewModel, IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
        this.ViewModel = viewModel;
        this.ViewModel.RefreshCallback = () => this.OnRefresh();

        this.BindingContext = ViewModel;
        InitializeComponent();
    }

    internal void Initialize(Profile userProfile, LatokenRestClient restClient, ICurrencyCache currencyCache, PairDto selectedTradingPair, BalanceDto balanceDto)
    {
        this.ViewModel.InitializeProfileAndRestClient(userProfile, restClient, currencyCache, selectedTradingPair, balanceDto);
    }
    private void ContentPage_Loaded(object sender, EventArgs e)
    {
        this.OnRefresh();
    }

    private void RefreshButton_Clicked(object sender, EventArgs e)
    {
        this.OnRefresh();
    }
    private void OnRefresh()
    {
        RefreshButton.Text = "Refreshing...";
        RefreshButton.IsEnabled = false;
        RefreshPage();
    }

    public void RefreshPage()
    {
        Application.Current.Dispatcher.Dispatch(() =>
        {
            //BusyIndicator.IsVisible = true;
            //BusyIndicator.IsRunning = true;
            OrderBookGrid.IsEnabled = false;
            OrderBookGrid.Children.Clear();
            OrderBookGrid.RowDefinitions.Clear();
            this.ViewModel.IsSellEnabled = false;
            AddHeaderRow();
        });

        this.ViewModel.RefreshBalances();
        var lastTrade = this.ViewModel.GetLastTrade();
        var orderbook = this.ViewModel.FetchOrderBook();
        if (orderbook != null)
        {
            for (int i = 0;i<14;i++)
            {
                priceLables[i] = null;
                quantityLabels[i] = null;
                accumulatedLabels[i] = null;
            }

            int index = 1;
            foreach (var ask in orderbook.Ask)
            {
                AddOrUpdateRow(ask, index++, true);
            }

            var lastTradedPriceLabel = new Label();
            lastTradedPriceLabel.SetValue(Grid.RowProperty, index++);
            lastTradedPriceLabel.SetValue(Grid.ColumnProperty, 0);
            lastTradedPriceLabel.SetValue(Label.TextProperty, lastTrade.Price + " - Last Trade");
            lastTradedPriceLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.Black : Colors.White);
            lastTradedPriceLabel.SetValue(Label.BindingContextProperty, lastTrade);
            //lastTradedPriceLabel.SetValue(Label.HorizontalOptionsProperty, LayoutOptions.Center);
            lastTradedPriceLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            lastTradedPriceLabel.SetValue(Label.MarginProperty, new Thickness(10));
            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += LastTradedPrice_Tapped;
            lastTradedPriceLabel.GestureRecognizers.Add(tapGestureRecognizer);
            Application.Current.Dispatcher.Dispatch(() =>
            {
                OrderBookGrid.RowDefinitions.Add(new RowDefinition());
                OrderBookGrid.Children.Add(lastTradedPriceLabel);
            }); ;

            foreach (var bid in orderbook.Bid)
            {
                AddOrUpdateRow(bid, index++, false);
            }

            if (this.ViewModel.SelectedPrice == 0 && this.ViewModel.Orderbook.Bid.Any())
            {
                this.SetPriceFromBid(this.ViewModel.Orderbook.Bid.First());
            }
            else
            {
                if(this.ViewModel.Orderbook != null 
                    && this.ViewModel.Orderbook.Bid.Any() 
                    && this.ViewModel.Orderbook.Bid.First().Price != this.ViewModel.SelectedPrice
                    && this.ViewModel.Orderbook.Bid.First().Quantity != this.ViewModel.SelectedQuantity)
                {
                    this.SetPriceFromBid(this.ViewModel.Orderbook.Bid.First());
                }
            }
        }

        Application.Current.Dispatcher.Dispatch(() =>
        {
            //BusyIndicator.IsVisible = false;
            //BusyIndicator.IsRunning = false;
            OrderBookGrid.IsEnabled = true;
            RefreshButton.Text = "Refresh";
            RefreshButton.IsEnabled = true;
            this.ViewModel.UpdateSellEnabled();
            this.ViewModel.IsRefreshing = false;
        });
    }

    private void AddOrUpdateRow(PriceLevelDto priceLevelDto, int rowIndex, bool isAskRow)
    {
        if (priceLables[rowIndex] == null)
        {
            Color textColor = isAskRow ? Colors.Red : Colors.Green;

            var tapGestureRecognizer = new TapGestureRecognizer();
            if (isAskRow)
            {
                tapGestureRecognizer.Tapped += Ask_Tapped;
            }
            else
            {
                tapGestureRecognizer.Tapped += Bid_Tapped;
            }
            var priceLabel = new Label();
            priceLabel.StyleId = "Price" + rowIndex;
            priceLabel.SetValue(Grid.RowProperty, rowIndex);
            priceLabel.SetValue(Grid.ColumnProperty, 0);
            priceLabel.SetValue(Label.TextProperty, priceLevelDto.Price);
            priceLabel.SetValue(Label.TextColorProperty, textColor);
            priceLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            priceLabel.SetValue(Label.BindingContextProperty, priceLevelDto);
            priceLabel.SetValue(Label.MarginProperty, new Thickness(3));
            priceLabel.GestureRecognizers.Add(tapGestureRecognizer);
            priceLables[rowIndex] = priceLabel;

            var quantityLabel = new Label();
            quantityLabel.StyleId = "Quantity" + rowIndex;
            quantityLabel.SetValue(Grid.RowProperty, rowIndex);
            quantityLabel.SetValue(Grid.ColumnProperty, 1);
            quantityLabel.SetValue(Label.TextProperty, priceLevelDto.Quantity);
            quantityLabel.SetValue(Label.TextColorProperty, textColor);
            quantityLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            quantityLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);
            quantityLabel.SetValue(Label.BindingContextProperty, priceLevelDto);
            quantityLabel.SetValue(Label.MarginProperty, new Thickness(3));
            quantityLabel.GestureRecognizers.Add(tapGestureRecognizer);

            quantityLabels[rowIndex] = quantityLabel;

            var accumulatedLabel = new Label();
            accumulatedLabel.StyleId = "Accumulated" + rowIndex;
            accumulatedLabel.SetValue(Grid.RowProperty, rowIndex);
            accumulatedLabel.SetValue(Grid.ColumnProperty, 2);
            accumulatedLabel.SetValue(Label.TextProperty, priceLevelDto.Accumulated);
            accumulatedLabel.SetValue(Label.TextColorProperty, textColor);
            accumulatedLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
            accumulatedLabel.SetValue(Label.BindingContextProperty, priceLevelDto);
            accumulatedLabel.SetValue(Label.MarginProperty, new Thickness(3));
            accumulatedLabel.GestureRecognizers.Add(tapGestureRecognizer);

            accumulatedLabels[rowIndex] = accumulatedLabel;

            Application.Current.Dispatcher.Dispatch(() =>
            {
                OrderBookGrid.RowDefinitions.Add(new RowDefinition());
                OrderBookGrid.Children.Add(priceLabel);
                OrderBookGrid.Children.Add(quantityLabel);
                OrderBookGrid.Children.Add(accumulatedLabel);
            });
        }
        else
        {
            var priceLabel = priceLables[rowIndex];
            var quantityLabel = quantityLabels[rowIndex];
            var accumulatedLabel = accumulatedLabels[rowIndex];
            priceLabel.SetValue(Label.TextProperty, priceLevelDto.Price);
            quantityLabel.SetValue(Label.TextProperty, priceLevelDto.Quantity);
            accumulatedLabel.SetValue(Label.TextProperty, priceLevelDto.Accumulated);
        }
    }

    private void LastTradedPrice_Tapped(object? sender, TappedEventArgs e)
    {
        var label = sender as Label;
        var tradeDto = label?.BindingContext as TradeDto;
        decimal priceValue;
        if (tradeDto != null && decimal.TryParse(tradeDto.Price, out priceValue))
        {
            this.ViewModel.SelectedPrice = priceValue;
        }
    }
    private void Ask_Tapped(object? sender, TappedEventArgs e)
    {
        var label = sender as Label;
        var priceLevelDto = label?.BindingContext as PriceLevelDto;
        if (priceLevelDto != null)
        {
            this.ViewModel.SelectedPrice = priceLevelDto.Price;
        }
    }

    private void Bid_Tapped(object? sender, TappedEventArgs e)
    {
        var label = sender as Label;
        var priceLevelDto = label?.BindingContext as PriceLevelDto;
        if (priceLevelDto != null)
        {
            SetPriceFromBid(priceLevelDto);
        }
    }

    public void SetPriceFromBid(PriceLevelDto priceLevelDto)
    {
        this.ViewModel.SelectedPrice = priceLevelDto.Price;
        var accumulatedQuantity = this.ViewModel.GetAccumulatedQuantityForBidPrice(priceLevelDto);
        var totalAvailable = this.ViewModel.AvailableSpotBalance + this.ViewModel.AvailableWalletBalance;
        if (accumulatedQuantity < totalAvailable)
        {
            this.ViewModel.SelectedQuantity = accumulatedQuantity;
            this.ViewModel.UpdatePercentBasedOnQuantity();
        }
        else
        {
            this.ViewModel.SelectedQuantity = totalAvailable;
            this.ViewModel.UpdatePercentBasedOnQuantity();
        }
    }

    private void AddHeaderRow()
    {
        var priceLabel = new Label();
        priceLabel.SetValue(Grid.RowProperty, 0);
        priceLabel.SetValue(Grid.ColumnProperty, 0);
        priceLabel.SetValue(Label.TextProperty, " Price");
        priceLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
        priceLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
        priceLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

        var quantityLabel = new Label();
        quantityLabel.SetValue(Grid.RowProperty, 0);
        quantityLabel.SetValue(Grid.ColumnProperty, 1);
        quantityLabel.SetValue(Label.TextProperty, "Quantity");
        quantityLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
        quantityLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
        quantityLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);
        quantityLabel.SetValue(Label.HorizontalTextAlignmentProperty, TextAlignment.Center);

        var accumulatedLabel = new Label();
        accumulatedLabel.SetValue(Grid.RowProperty, 0);
        accumulatedLabel.SetValue(Grid.ColumnProperty, 2);
        accumulatedLabel.SetValue(Label.TextProperty, "Accumulated");
        accumulatedLabel.SetValue(Label.BackgroundColorProperty, Colors.Gray);
        accumulatedLabel.SetValue(Label.TextColorProperty, App.Current.PlatformAppTheme == AppTheme.Light ? Colors.White : Colors.Black);
        accumulatedLabel.SetValue(Label.FontAttributesProperty, FontAttributes.Bold);

        OrderBookGrid.RowDefinitions.Add(new RowDefinition());
        OrderBookGrid.Children.Add(priceLabel);
        OrderBookGrid.Children.Add(quantityLabel);
        OrderBookGrid.Children.Add(accumulatedLabel);
    }

    private void Price25PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SellAmountPercent = 25;
        this.ViewModel.UpdateQuantityBasedOnPercent();
    }
    private void Price50PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SellAmountPercent = 50;
        this.ViewModel.UpdateQuantityBasedOnPercent();
    }
    private void Price75PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SellAmountPercent = 75;
        this.ViewModel.UpdateQuantityBasedOnPercent();
    }
    private void Price100PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SellAmountPercent = 100;
        this.ViewModel.UpdateQuantityBasedOnPercent();
    }

    private void Slider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        this.ViewModel.UpdateQuantityBasedOnPercent();
    }

    private void SellButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.PlaceSellOrder();
    }

    private void RefreshView_Refreshing(object sender, EventArgs e)
    {
        this.OnRefresh();
    }

    private void AvailableWalletBalance_Tapped(object sender, TappedEventArgs e)
    {
        if(this.ViewModel.AvailableWalletBalance > 0)
        {
            this.ViewModel.SelectedQuantity = this.ViewModel.AvailableWalletBalance;
        }
    }

    private void AvailableSpotBalance_Tapped(object sender, TappedEventArgs e)
    {
        if (this.ViewModel.AvailableSpotBalance > 0)
        {
            this.ViewModel.SelectedQuantity = this.ViewModel.AvailableSpotBalance;
        }
    }

    private void TransferBalance_Tapped(object sender, TappedEventArgs e)
    {
        var transferPage = new TransferPage(new TransferViewModel(serviceProvider));
        transferPage.Initialize(this.ViewModel.UserProfile, this.ViewModel.RestClient, this.ViewModel.CurrencyCache, this.ViewModel.BalanceDto);
        Navigation.PushAsync(transferPage);
    }
}