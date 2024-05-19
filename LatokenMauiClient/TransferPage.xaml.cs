
using Microsoft.Maui.Controls.Internals;

namespace LatokenMauiClient;

public partial class TransferPage : ContentPage
{
    public TransferViewModel ViewModel { get; set; }

    private Label[] priceLables = new Label[14];
    private Label[] quantityLabels = new Label[14];
    private Label[] accumulatedLabels = new Label[14];

    public TransferPage(TransferViewModel viewModel)
    {
        this.ViewModel = viewModel;
        this.ViewModel.RefreshCallback = () => this.OnRefresh();

        this.BindingContext = ViewModel;
        InitializeComponent();
    }

    internal void Initialize(Profile userProfile, LatokenRestClient restClient, ICurrencyCache currencyCache, BalanceDto balanceDto)
    {
        this.ViewModel.InitializeProfileAndRestClient(userProfile, restClient, currencyCache, balanceDto);
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
        });

        this.ViewModel.RefreshBalances();

        Application.Current.Dispatcher.Dispatch(() =>
        {
            //BusyIndicator.IsVisible = false;
            //BusyIndicator.IsRunning = false;
            RefreshButton.Text = "Refresh";
            RefreshButton.IsEnabled = true;
            this.ViewModel.IsRefreshing = false;
        });
    }

    private void WalletToSpot25PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.WalletToSpotTransferAmountPercent = 25;
        this.ViewModel.UpdateWalletToSpotQuantityBasedOnPercent();
    }
    private void WalletToSpot50PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.WalletToSpotTransferAmountPercent = 50;
        this.ViewModel.UpdateWalletToSpotQuantityBasedOnPercent();
    }
    private void WalletToSpot75PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.WalletToSpotTransferAmountPercent = 75;
        this.ViewModel.UpdateWalletToSpotQuantityBasedOnPercent();
    }
    private void WalletToSpot100PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.WalletToSpotTransferAmountPercent = 100;
        this.ViewModel.UpdateWalletToSpotQuantityBasedOnPercent();
    }

    private void WalletToSpotAmountSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        this.ViewModel.UpdateWalletToSpotQuantityBasedOnPercent();
    }

    private void SpotToWallet25PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SpotToWalletTransferAmountPercent = 25;
        this.ViewModel.UpdateSpotToWalletQuantityBasedOnPercent();
    }
    private void SpotToWallet50PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SpotToWalletTransferAmountPercent = 50;
        this.ViewModel.UpdateSpotToWalletQuantityBasedOnPercent();
    }
    private void SpotToWallet75PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SpotToWalletTransferAmountPercent = 75;
        this.ViewModel.UpdateSpotToWalletQuantityBasedOnPercent();
    }
    private void SpotToWallet100PercentButton_Clicked(object sender, EventArgs e)
    {
        this.ViewModel.SpotToWalletTransferAmountPercent = 100;
        this.ViewModel.UpdateSpotToWalletQuantityBasedOnPercent();
    }

    private void SpotToWalletAmountSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        this.ViewModel.UpdateSpotToWalletQuantityBasedOnPercent();
    }

    private void RefreshView_Refreshing(object sender, EventArgs e)
    {
        this.OnRefresh();
    }

    private void AvailableWalletBalance_Tapped(object sender, TappedEventArgs e)
    {
        if(this.ViewModel.AvailableWalletBalance > 0)
        {
            this.ViewModel.SelectedWalletToSpotQuantity = this.ViewModel.AvailableWalletBalance;
            this.ViewModel.WalletToSpotTransferAmountPercent = 100;
        }
    }

    private void AvailableSpotBalance_Tapped(object sender, TappedEventArgs e)
    {
        if (this.ViewModel.AvailableSpotBalance > 0)
        {
            this.ViewModel.SelectedSpotToWalletQuantity = this.ViewModel.AvailableSpotBalance;
            this.ViewModel.SpotToWalletTransferAmountPercent = 100;
        }
    }

    private void TransferFromWalletToSpotButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            this.ViewModel.TransferFromWalletToSpot();
            App.AlertSvc.ShowAlert("Success", "Transfer From Wallet To Spot Successful.", "OK");
            this.OnRefresh();
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                App.AlertSvc.ShowAlert("Error", $"Error during Transfer. {ex.Message}", "OK");
            });
        }
    }

    private void TransferFromSpotToWalletButton_Clicked(object sender, EventArgs e)
    {
        try
        {
            this.ViewModel.TransferFromSpotToWallet();
            App.AlertSvc.ShowAlert("Success", "Transfer From Spot to Wallet Successful.", "OK");
            this.OnRefresh();
        }
        catch (Exception ex)
        {
            Application.Current.Dispatcher.Dispatch(() =>
            {
                App.AlertSvc.ShowAlert("Error", $"Error during Transfer. {ex.Message}", "OK");
            });
        }
    }
}