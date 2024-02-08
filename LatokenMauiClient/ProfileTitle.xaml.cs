using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace LatokenMauiClient;

public partial class ProfileTitle : ContentView
{
    public ProfileTitle()
    {
        WeakReferenceMessenger.Default.Register<ProfileSavedMessage>(this, this.OnProfileSavedMessage);
        InitializeComponent();
        LoadAvailableAndSelectedProfiles();
    }

    private void LoadAvailableAndSelectedProfiles()
    {
        this.profileFilterInstance = App.Current.Resources["ProfileFilterInstance"] as ProfileFilter;
        if (this.profileFilterInstance != null)
        {
            this.AvailableProfiles.Clear();
            foreach (var item in profileFilterInstance.AvailableProfiles)
            {
                this.AvailableProfiles.Add(item.ProfileName);
            }

            this.SelectedProfile = profileFilterInstance.SelectedProfile?.ProfileName;
        }
    }

    private void OnProfileSavedMessage(object recipient, ProfileSavedMessage message)
    {
        this.LoadAvailableAndSelectedProfiles();
    }

    public ObservableCollection<string> AvailableProfiles { get; set; } = new ObservableCollection<string>();

    public string selectedProfile;

    private ProfileFilter? profileFilterInstance;

    public string SelectedProfile
    {
        get => selectedProfile;
        set
        {
            if (selectedProfile != value)
            {
                selectedProfile = value;
                OnPropertyChanged();
                var profileFilterInstance = App.Current.Resources["ProfileFilterInstance"] as ProfileFilter;
                if (profileFilterInstance != null && profileFilterInstance.SelectedProfile != null)
                {
                    profileFilterInstance.SelectedProfile = profileFilterInstance.AvailableProfiles.FirstOrDefault(p => p.ProfileName == selectedProfile);
                    App.Current.Resources["ProfileFilterInstance"] = profileFilterInstance;
                    WeakReferenceMessenger.Default.Send(new SelectedProfileChangedMessage(selectedProfile));
                }
            }
        }
    }
}