using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LatokenMauiClient
{
    public partial class ProfileFilter : ObservableObject
    {
        [ObservableProperty]
        private Profile selectedProfile = new();

        [ObservableProperty]
        private ObservableCollection<Profile> availableProfiles = new();

        public ProfileFilter() : base()
        {
            PopulateData();
        }

        public async Task PopulateData()
        {
            // Populate the available selections
            await LoadSavedProfiles();
        }

        public async Task LoadSavedProfiles()
        {
            if (AvailableProfiles?.Count > 0)
                return;

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
            foreach (var name in names)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    var profileName = Preferences.Default.Get<string>(name + "_ProfileName", string.Empty);
                    var apiKey = Preferences.Default.Get<string>(name + "_ApiKey", string.Empty);
                    var apiSecret = Preferences.Default.Get<string>(name + "_ApiSecret", string.Empty);
                    this.AvailableProfiles?.Add(new Profile { ProfileName = profileName, ApiKey = apiKey, ApiSecret = apiSecret });
                }
            }

            SelectedProfile = AvailableProfiles?.First();
        }
    }
}
