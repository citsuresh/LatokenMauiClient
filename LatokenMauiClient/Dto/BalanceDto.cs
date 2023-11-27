using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LatokenMauiClient
{
    public class BalanceDto : INotifyPropertyChanged
    {
        private string id;
        private string status;
        private string type;
        private string currencyId;
        private decimal available;
        private decimal blocked;
        private DateTime timestamp;
        private string currencySymbol;
        private decimal lastTradedPrice;
        private decimal availableValue;

        public string Id { get => id; set => SetProperty(ref id, value); }

        public string Status { get => status; set => SetProperty(ref status, value); }

        public string Type { get => type; set => SetProperty(ref type, value); }

        public string CurrencyId { get => currencyId; set => SetProperty(ref currencyId, value); }

        public decimal Available { get => available; set => SetProperty(ref available, value); }

        public decimal Blocked { get => blocked; set => SetProperty(ref blocked, value); }

        public DateTime Timestamp { get => timestamp; set => SetProperty(ref timestamp, value); }

        public string CurrencySymbol { get => currencySymbol; set => SetProperty(ref currencySymbol, value); }

        public decimal LastTradedPrice { get => lastTradedPrice; set => SetProperty(ref lastTradedPrice, value); }

        public decimal AvailableValue { get => availableValue; set => SetProperty(ref availableValue, value); }


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


