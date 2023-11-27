using Microsoft.Extensions.Configuration;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;

namespace LatokenMauiClient
{
    public class ProfilePageViewModel : INotifyPropertyChanged
    {
        public ProfilePageViewModel()
        {
            this.ProfileName = Preferences.Default.Get<string>("ProfileName", string.Empty);
            this.ApiKey = Preferences.Default.Get<string>("ApiKey", string.Empty);
            this.ApiSecret = Preferences.Default.Get<string>("ApiSecret", string.Empty);

            SaveCommand = new Command(() => ExecuteSave(), () => true);
        }

        public string ProfileName
        {
            get
            {
                return profileName;
            }

            set
            {
                profileName = value;
                this.OnPropertyChanged();
            }
        }

        public string ApiKey
        {
            get
            {
                return apiKey;
            }

            set
            {
                apiKey = value;
                this.OnPropertyChanged();
            }
        }

        public string ApiSecret
        {
            get
            {
                return apiSecret;
            }

            set
            {
                apiSecret = value;
                this.OnPropertyChanged();
            }
        }

        private void ExecuteSave()
        {
            if (!string.IsNullOrWhiteSpace(this.ApiKey) && !string.IsNullOrWhiteSpace(this.ApiSecret))
            {
                var restClient = new LatokenRestClient(this.ApiKey, this.ApiSecret);
                if (restClient != null && !string.IsNullOrEmpty(restClient.GetUser()?.Id))
                {
                    Preferences.Default.Set<string>("ProfileName", this.ProfileName);
                    Preferences.Default.Set<string>("ApiKey", this.ApiKey);
                    Preferences.Default.Set<string>("ApiSecret", this.ApiSecret);
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private string profileName;
        private string apiSecret;
        private string apiKey;

        public ICommand SaveCommand { get; private set; }


        public void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}