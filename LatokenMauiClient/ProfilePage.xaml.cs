
namespace LatokenMauiClient
{
    public partial class ProfilePage : ContentPage
    {
        public ProfilePageViewModel ViewModel { get; set; }
        public ProfilePage(ProfilePageViewModel viewModel)
        {
            this.ViewModel = viewModel;
            this.BindingContext = this.ViewModel;
            InitializeComponent();
        }
    }

}
