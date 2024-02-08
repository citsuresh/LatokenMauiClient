using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace LatokenMauiClient
{
    public partial class ProfilePageViewModel : ObservableObject
    {
        public ProfilePageViewModel()
        {
        }
        partial void OnTitleChanged(string? value)
        {
            this.ProfileName = Preferences.Default.Get<string>(this.Title + "_ProfileName", string.Empty);
            this.ApiKey = Preferences.Default.Get<string>(this.Title + "_ApiKey", string.Empty);
            this.ApiSecret = Preferences.Default.Get<string>(this.Title + "_ApiSecret", string.Empty);
        }

        [ObservableProperty]
        private string title;

        [ObservableProperty]
        private string profileName;

        [ObservableProperty]
        private string apiSecret;

        [ObservableProperty]
        private string apiKey;

        [RelayCommand]
        private void Save()
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(this.ApiKey) && !string.IsNullOrWhiteSpace(this.ApiSecret))
                {
                    var restClient = new LatokenRestClient(this.ApiKey, this.ApiSecret);
                    if (restClient != null && !string.IsNullOrEmpty(restClient.GetUser()?.Id))
                    {
                        Preferences.Default.Set<string>(this.Title + "_ProfileName", this.ProfileName);
                        Preferences.Default.Set<string>(this.Title + "_ApiKey", this.ApiKey);
                        Preferences.Default.Set<string>(this.Title + "_ApiSecret", this.ApiSecret);


                        var profileNamesStr = Preferences.Default.Get<string>("ProfileNames", string.Empty);
                        List<string> names;
                        if (string.IsNullOrWhiteSpace(profileNamesStr))
                        {
                            names = new List<string>();
                        }
                        else
                        {
                            names = profileNamesStr.Split(',').ToList();
                        }

                        if (!names.Contains(this.Title))
                        {
                            names.Add(this.Title);
                            var updatedProfileNamesStr = string.Join(',', names);
                            Preferences.Default.Set<string>("ProfileNames", updatedProfileNamesStr);
                        }

                        var profileFilterInstance = App.Current.Resources["ProfileFilterInstance"] as ProfileFilter;
                        if (profileFilterInstance == null)
                        {
                            profileFilterInstance = new ProfileFilter();
                        }
                        var currentProfileObj = new Profile { ProfileName = this.ProfileName, ApiKey = this.ApiKey, ApiSecret = this.ApiSecret };
                        if (profileFilterInstance.AvailableProfiles.Any(p => p.ProfileName == this.ProfileName))
                        {
                            var profileFilter = profileFilterInstance.AvailableProfiles.First(p => p.ProfileName == this.ProfileName);
                            profileFilter.ApiKey = this.ApiKey;
                            profileFilter.ApiSecret = this.ApiSecret;
                        }
                        else
                        {
                            profileFilterInstance.AvailableProfiles.Add(currentProfileObj);
                        }

                        if(profileFilterInstance.SelectedProfile == null)
                        {
                            profileFilterInstance.SelectedProfile = currentProfileObj;
                        }

                        App.Current.Resources["ProfileFilterInstance"] = profileFilterInstance;

                        App.AlertSvc.ShowAlert("Success", "Profile has been saved locally.", "OK");

                        WeakReferenceMessenger.Default.Send(new ProfileSavedMessage(this.ProfileName));
                    }
                }
            }
            catch (Exception ex)
            {
                App.AlertSvc.ShowAlert("Error occurred", ex.Message, "OK");

            }
        }
    }
}