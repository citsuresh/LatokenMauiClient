
using CommunityToolkit.Mvvm.Messaging;

namespace LatokenMauiClient
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePageViewModel ViewModel { get; set; }
        public ProfilePage(ProfilePageViewModel viewModel)
        {

            WeakReferenceMessenger.Default.Register<ProfilePageNavigatedToMessage>(this, this.OnProfilePageNavigatedToMessage);
            //var title = Shell.Current.CurrentItem.CurrentItem.CurrentItem.Title;

            //this.ViewModel.Title = title;
            this.ViewModel = viewModel;
            this.BindingContext = this.ViewModel;
            InitializeComponent();
        }

        private void OnProfilePageNavigatedToMessage(object recipient, ProfilePageNavigatedToMessage message)
        {
            this.ViewModel.Title = message.Value;
        }
    }

}
