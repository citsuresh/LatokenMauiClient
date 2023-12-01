using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LatokenMauiClient
{
    public class TransferDto : INotifyPropertyChanged
    {
        private string id;
        private string status;
        private string type;
        private string fromAccount;
        private string toAccount;
        private decimal transferringFunds;
        private decimal usdValue;
        private string rejectReason;
        private DateTime timestamp;
        private string direction;
        private string fromUser;
        private string toUser;
        private string method;
        private string recipient;
        private string sender;
        private string currency;
        private bool codeRequired;
        private decimal fee;
        private string currencySymbol;

        public string Id { get => id; set => SetProperty(ref id, value); }

        public string Status { get => status; set => SetProperty(ref status, value); }

        public string Type { get => type; set => SetProperty(ref type, value); }

        public string FromAccount { get => fromAccount; set => SetProperty(ref fromAccount, value); }

        public string ToAccount { get => toAccount; set => SetProperty(ref toAccount, value); }

        public decimal TransferringFunds { get => transferringFunds; set => SetProperty(ref transferringFunds, value); }

        public decimal UsdValue { get => usdValue; set => SetProperty(ref usdValue, value); }

        public string RejectReason { get => rejectReason; set => SetProperty(ref rejectReason, value); }

        public DateTime Timestamp { get => timestamp; set => SetProperty(ref timestamp, value); }

        public string Direction { get => direction; set => SetProperty(ref direction, value); }

        public string Method { get => method; set => SetProperty(ref method, value); }

        public string Recipient { get => recipient; set => SetProperty(ref recipient, value); }

        public string Sender { get => sender; set => SetProperty(ref sender, value); }

        public string Currency { get => currency; set => SetProperty(ref currency, value); }

        public bool CodeRequired { get => codeRequired; set => SetProperty(ref codeRequired, value); }

        public string FromUser { get => fromUser; set => SetProperty(ref fromUser, value); }

        public string ToUser { get => toUser; set => SetProperty(ref toUser, value); }

        public decimal Fee { get => fee; set => SetProperty(ref fee, value); }

        public string CurrencySymbol { get => currencySymbol; set => SetProperty(ref currencySymbol, value); }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                return true;
            }

            return false;
        }
        #endregion
    }
}


