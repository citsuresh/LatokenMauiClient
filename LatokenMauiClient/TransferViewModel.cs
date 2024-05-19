using CommunityToolkit.Mvvm.ComponentModel;
using Latoken.Api.Client.Library.Commands;
using Latoken.Api.Client.Library;
using Microsoft.Maui.Controls.Internals;
using System.Reactive.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace LatokenMauiClient
{
    public partial class TransferViewModel : ObservableObject
    {
        public TransferViewModel(IServiceProvider serviceProvider)
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
        private decimal walletToSpotTransferAmountPercent = 100;

        [ObservableProperty]
        private decimal spotToWalletTransferAmountPercent = 100;

        [ObservableProperty]
        private decimal selectedWalletToSpotQuantity;

        [ObservableProperty]
        private decimal selectedSpotToWalletQuantity;

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

        public void InitializeProfileAndRestClient(Profile userProfile, LatokenRestClient restClient, ICurrencyCache currencyCache, BalanceDto balanceDto)
        {
            this.UserProfile = userProfile;
            this.RestClient = restClient;
            this.CurrencyCache = currencyCache;
            this.BalanceDto = balanceDto;
            this.OnPropertyChanged(nameof(AvailableWalletBalance));
            this.OnWalletToSpotTransferAmountPercentChanged(this.WalletToSpotTransferAmountPercent);
        }

        public void UpdateWalletToSpotQuantityBasedOnPercent()
        {
            if (BalanceDto != null)
            {
                var newQty = this.AvailableWalletBalance * ((decimal)(this.WalletToSpotTransferAmountPercent / 100));
                if (newQty != this.SelectedWalletToSpotQuantity)
                {
                    this.SelectedWalletToSpotQuantity = newQty;
                }
            }
        }

        public void UpdateSpotToWalletQuantityBasedOnPercent()
        {
            if (BalanceDto != null)
            {
                var newQty = this.AvailableSpotBalance * ((decimal)(this.SpotToWalletTransferAmountPercent / 100));
                if (newQty != this.SelectedSpotToWalletQuantity)
                {
                    this.SelectedSpotToWalletQuantity = newQty;
                }
            }
        }

        public void UpdatePercentBasedOnQuantity()
        {
            //var newPercent = (this.SelectedQuantity / this.BalanceDto.Available) * 100;
            ////if (newPercent != this.WalletToSpotAmountPercent)
            //{
            //    this.WalletToSpotAmountPercent = newPercent;
            //}
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

            this.SelectedWalletToSpotQuantity = this.AvailableWalletBalance;
            this.SelectedSpotToWalletQuantity = this.AvailableSpotBalance;
            this.WalletToSpotTransferAmountPercent = 100;
            this.SpotToWalletTransferAmountPercent = 100;
        }

        internal void TransferFromWalletToSpot()
        {
            var cmd = new Latoken.Api.Client.Library.LA.Commands.SpotTransferCommand();
            cmd.Currency = this.BalanceDto.CurrencyId;
            cmd.Value = SelectedWalletToSpotQuantity.ToString();
            var result = this.RestClient?.ClientInstance?.SpotDeposit(cmd).Result;
        }

        internal void TransferFromSpotToWallet()
        {
            var cmd = new Latoken.Api.Client.Library.LA.Commands.SpotTransferCommand();
            cmd.Currency = this.BalanceDto.CurrencyId;
            cmd.Value = SelectedSpotToWalletQuantity.ToString();
            var result = this.RestClient?.ClientInstance?.SpotWithdraw(cmd).Result;
        }
    }
}