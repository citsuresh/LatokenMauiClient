using CommunityToolkit.Mvvm.ComponentModel;
using Latoken.Api.Client.Library.Commands;
using Latoken.Api.Client.Library;
using Microsoft.Maui.Controls.Internals;
using System.Reactive.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace LatokenMauiClient
{
    public partial class SellViewModel : ObservableObject
    {
        public SellViewModel(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        [ObservableProperty]
        private LatokenRestClient restClient;

        [ObservableProperty]
        private Profile userProfile;

        [ObservableProperty]
        private ICurrencyCache currencyCache;

        [ObservableProperty]
        private BalanceDto balanceDto;

        [ObservableProperty]
        private BalanceDto spotBalance;

        [ObservableProperty]
        private BalanceDto walletBalance;

        [ObservableProperty]
        private PairDto selectedTradingPair;

        [ObservableProperty]
        private decimal sellAmountPercent = 75;

        [ObservableProperty]
        private decimal selectedPrice;

        private decimal selectedQuantity;

        public decimal SelectedQuantity
        {
            get => selectedQuantity;
            set
            {
                var trimmedQuantity = this.TrimOrderQuantityForTradingPair((decimal)value);
                SetProperty(ref selectedQuantity, trimmedQuantity);
                UpdateTotalUsdtValue();
                UpdateSellEnabled();
            }
        }

        [ObservableProperty]
        private decimal totalUsdtValue;

        [ObservableProperty]
        private OrderBookDto? orderbook;

        [ObservableProperty]
        private bool isSellEnabled;

        [ObservableProperty]
        private bool isRefreshing;


        private decimal availableWalletBalance;
        private decimal availableSpotBalance;

        private IServiceProvider serviceProvider;
        public Action RefreshCallback;
        private bool isSellInProgress;

        public decimal AvailableWalletBalance
        {
            get
            {
                if (WalletBalance != null)
                {
                    availableWalletBalance = WalletBalance.Available;
                }
                else
                {
                    availableWalletBalance = 0;
                }

                return availableWalletBalance;
            }

            set => SetProperty(ref availableWalletBalance, value);
        }

        public decimal AvailableSpotBalance
        {
            get
            {
                if (SpotBalance != null)
                {
                    availableSpotBalance = SpotBalance.Available;
                }
                else
                {
                    availableSpotBalance = 0;
                }

                return availableSpotBalance;
            }

            set => SetProperty(ref availableSpotBalance, value);
        }


        [RelayCommand]
        private void RefreshPage()
        {
            this.RefreshCallback();
        }

        public void InitializeProfileAndRestClient(Profile userProfile, LatokenRestClient restClient, ICurrencyCache currencyCache, PairDto selectedTradingPair, BalanceDto balanceDto)
        {
            this.UserProfile = userProfile;
            this.RestClient = restClient;
            this.CurrencyCache = currencyCache;
            this.SelectedTradingPair = selectedTradingPair;
            this.BalanceDto = balanceDto;
            this.OnPropertyChanged(nameof(AvailableWalletBalance));
            this.OnSellAmountPercentChanged(this.SellAmountPercent);
        }

        private decimal TrimOrderQuantityForTradingPair(decimal orderQuantity)
        {
            if (this.SelectedTradingPair != null)
            {
                var allowedDecimalDigits = this.SelectedTradingPair.QuantityDecimals;
                if (allowedDecimalDigits >= 0)
                {
                    var trimmedValue = (decimal)(Math.Round(((double)orderQuantity) * Math.Pow(10, allowedDecimalDigits), 0) / (Math.Pow(10, allowedDecimalDigits)));
                    if (trimmedValue > orderQuantity)
                    {
                        // Round down if the trimmed value is greater than actual balance
                        trimmedValue = (decimal)(Math.Floor(((double)orderQuantity) * Math.Pow(10, allowedDecimalDigits)) / (Math.Pow(10, allowedDecimalDigits)));
                    }
                    return trimmedValue;
                }
                else
                {
                    return (decimal)(Math.Floor(((double)orderQuantity) * Math.Pow(10, allowedDecimalDigits)) / (Math.Pow(10, allowedDecimalDigits)));
                }
            }
            return 0;
        }

        internal TradeDto GetLastTrade()
        {
            var lastTrade = this.RestClient.GetLastTrade(this.CurrencyCache.GetCurrencyById(this.SelectedTradingPair.BaseCurrencyId).Symbol, this.CurrencyCache.GetCurrencyById(this.SelectedTradingPair.QuoteCurrencyId).Symbol);
            return lastTrade;
        }

        internal OrderBookDto? FetchOrderBook()
        {
            this.Orderbook = this.RestClient.GetOrderBook(this.CurrencyCache.GetCurrencyById(this.SelectedTradingPair.BaseCurrencyId), this.CurrencyCache.GetCurrencyById(this.SelectedTradingPair.QuoteCurrencyId), 5);
            return this.Orderbook;
        }

        public void UpdateQuantityBasedOnPercent()
        {
            if (BalanceDto != null)
            {
                var newQty = BalanceDto.Available * ((decimal)(this.SellAmountPercent / 100));
                if (newQty != this.SelectedQuantity)
                {
                    this.SelectedQuantity = newQty;
                }
            }
        }

        public void UpdatePercentBasedOnQuantity()
        {
            //var newPercent = (this.SelectedQuantity / this.BalanceDto.Available) * 100;
            ////if (newPercent != this.SellAmountPercent)
            //{
            //    this.SellAmountPercent = newPercent;
            //}
        }

        public void UpdateSellEnabled()
        {
            this.IsSellEnabled = !this.isSellInProgress && this.SelectedQuantity > 0 && this.SelectedPrice > 0;
        }

        private void UpdateTotalUsdtValue()
        {
            this.TotalUsdtValue = this.SelectedQuantity * this.SelectedPrice;
        }

        partial void OnSelectedPriceChanged(decimal value)
        {
            UpdateTotalUsdtValue();
            UpdateSellEnabled();
        }
        internal decimal GetAccumulatedQuantityForBidPrice(PriceLevelDto priceLevelDto)
        {
            decimal accumulatedQuantity = 0;
            var matchedBid = this.Orderbook.Bid.FirstOrDefault(bid => bid.Price == priceLevelDto.Price && bid.Quantity == priceLevelDto.Quantity && bid.Accumulated == priceLevelDto.Accumulated);
            if (this.Orderbook != null && matchedBid != null)
            {
                foreach (var bid in this.Orderbook.Bid)
                {
                    accumulatedQuantity += bid.Quantity;
                    if (bid == matchedBid)
                    {
                        break;
                    }
                }
            }

            return accumulatedQuantity;
        }

        internal void PlaceSellOrder()
        {
            this.isSellInProgress = true;
            this.UpdateSellEnabled();
            var alertService = this.serviceProvider.GetService<IAlertService>();
            if (this.SelectedPrice > 0 && this.SelectedQuantity > 0)
            {
                if (this.SpotBalance == null || this.WalletBalance == null)
                {
                    alertService.ShowAlert("Error", $"Unable to get Spot and Wallet Balance values");
                    this.isSellInProgress = false;
                    this.UpdateSellEnabled();
                    return;
                }

                if (this.SelectedQuantity > this.SpotBalance.Available)
                {
                    if (this.SpotBalance.Available + this.WalletBalance.Available >= this.SelectedQuantity)
                    {
                        //Transfer from wallet
                        var cmd = new Latoken.Api.Client.Library.LA.Commands.SpotTransferCommand();
                        cmd.Currency = this.BalanceDto.CurrencyId;
                        cmd.Value = (this.SelectedQuantity - this.SpotBalance.Available).ToString();
                        var result = this.RestClient?.ClientInstance?.SpotDeposit(cmd).Result;
                        Thread.Sleep(300);
                        this.RefreshBalances();
                        this.PlaceSellOrder();
                        this.isSellInProgress = false;
                        this.UpdateSellEnabled();
                        return;
                    }
                    else
                    {
                        alertService.ShowAlert("Error", $"Not enough quantity of {this.BalanceDto.CurrencySymbol} available for sell.\nSpot Balance = {this.SpotBalance.Available}\nWallet Balance = {this.WalletBalance.Available}");

                        this.isSellInProgress = false;
                        this.UpdateSellEnabled();
                        return;
                    }
                }
                else
                {
                    string errorMessage = string.Empty;
                    if (this.PlaceOrder(SelectedTradingPair, false, this.SelectedPrice, this.SelectedQuantity, ref errorMessage))
                    {
                        alertService.ShowAlert("Success", "Sell Order placed successfully!");
                    }
                    else
                    {
                        alertService.ShowAlert("Error", errorMessage);
                    }

                    this.isSellInProgress = false;
                    this.UpdateSellEnabled();
                    this.RefreshCallback();
                }
            }

            this.isSellInProgress = false;
            this.UpdateSellEnabled();
        }
        private bool PlaceOrder(PairDto selectedTradingPair, bool isBuyOrder, decimal price, decimal quantity, ref string errorMessage)
        {
            OrderCommand cmd = new OrderCommand();
            cmd.BaseCurrency = selectedTradingPair.BaseCurrencyId;
            cmd.QuoteCurrency = selectedTradingPair.QuoteCurrencyId;
            cmd.Side = isBuyOrder ? "BUY" : "SELL";
            cmd.Condition = "GOOD_TILL_CANCELLED";
            cmd.Type = "LIMIT";
            cmd.ClientOrderId = Guid.NewGuid().ToString();
            cmd.Price = price.ToString();
            cmd.Quantity = quantity.ToString();
            cmd.Timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            bool isSuccess = true;
            errorMessage = string.Empty;
            OrderResponse resp = this.RestClient?.PlaceOrder(cmd);
            if (string.IsNullOrEmpty(resp.Error) && resp.Errors == null)
            {
                isSuccess = true;
            }
            else
            {

                var msg = $"[{DateTime.Now}]: Status:{resp.Status}, Message:{resp.Message}, Error:{resp.Error}, Errors:";
                if (resp.Errors != null && resp.Errors.Any())
                {
                    resp.Errors.ToList().ForEach(kv => msg += $"{kv.Key}:{kv.Value} ");
                }
                errorMessage = msg;
                isSuccess = false;
            }

            return isSuccess;
        }

        internal void RefreshBalances()
        {
            var task1 = Task.Run<BalanceDto>(() => this.RestClient?.GetSpotBalanceByCurrency(balanceDto.CurrencyId));
            var task2 = Task.Run<BalanceDto>(() => this.RestClient?.GetWalletBalanceByCurrency(balanceDto.CurrencyId));

            Task.WaitAll(task1, task2);


            this.SpotBalance = task1.Result;
            this.WalletBalance = task2.Result;

            this.OnPropertyChanged(nameof(AvailableSpotBalance));
            this.OnPropertyChanged(nameof(AvailableWalletBalance));
        }
    }
}