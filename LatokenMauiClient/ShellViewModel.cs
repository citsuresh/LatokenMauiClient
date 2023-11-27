using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LatokenMauiClient
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        public UserProfile UserProfile { get; set; } = null;

        public ShellViewModel()
        {
            var profileName = Preferences.Default.Get<string>("ProfileName", string.Empty);
            var apiKey = Preferences.Default.Get<string>("ApiKey", string.Empty);
            var apiSecret = Preferences.Default.Get<string>("ApiSecret", string.Empty);

            if (!string.IsNullOrEmpty(profileName) &&
                !string.IsNullOrEmpty(apiKey) &&
                !string.IsNullOrEmpty(apiSecret))
            {
                this.UserProfile = new UserProfile { ProfileName = profileName, ApiKey = apiKey, ApiSecret = apiSecret };
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}