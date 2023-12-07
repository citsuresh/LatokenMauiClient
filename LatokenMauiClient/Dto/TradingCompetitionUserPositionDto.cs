using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace LatokenMauiClient
{
    public class TradingCompetitionUserPositionDto : INotifyPropertyChanged
    {
        private string nickname;
        private string targetValue;
        private string rewardValue;
        private string usdValue;
        private int position;

        public string Nickname { get => nickname; set => SetProperty(ref nickname, value); }

        public string TargetValue { get => targetValue; set => SetProperty(ref targetValue, value); }

        public string RewardValue { get => rewardValue; set => SetProperty(ref rewardValue, value); }

        public string UsdValue { get => usdValue; set => SetProperty(ref usdValue, value); }

        public int Position { get => position; set => SetProperty(ref position, value); }

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
