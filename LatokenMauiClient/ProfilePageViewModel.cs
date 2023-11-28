using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace LatokenMauiClient
{
    public partial class ProfilePageViewModel : ObservableObject
    {
        public ProfilePageViewModel()
        {
            this.ProfileName = Preferences.Default.Get<string>("ProfileName", string.Empty);
            this.ApiKey = Preferences.Default.Get<string>("ApiKey", string.Empty);
            this.ApiSecret = Preferences.Default.Get<string>("ApiSecret", string.Empty);
        }

        [ObservableProperty]
        private string profileName;

        [ObservableProperty]
        private string apiSecret;

        [ObservableProperty]
        private string apiKey;

        [RelayCommand]
        private void Save()
        {
            if (!string.IsNullOrWhiteSpace(this.ApiKey) && !string.IsNullOrWhiteSpace(this.ApiSecret))
            {
                var restClient = new LatokenRestClient(this.ApiKey, this.ApiSecret);
                if (restClient != null && !string.IsNullOrEmpty(restClient.GetUser()?.Id))
                {
                    Preferences.Default.Set<string>("ProfileName", this.ProfileName);
                    Preferences.Default.Set<string>("ApiKey", this.ApiKey);
                    Preferences.Default.Set<string>("ApiSecret", this.ApiSecret);

                    App.AlertSvc.ShowAlert("Success", "Profile has been saved locally.", "OK");
                }
            }
        }
    }
}