using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Maui.Controls;
using System.Windows.Input;

namespace LatokenMauiClient
{
    public partial class AppShell : Shell
    {
        public ShellViewModel ViewModel { get; set; }
        public AppShell(ShellViewModel viewModel)
        {
            ViewModel = viewModel;
            InitializeComponent();
        }

        private void Shell_Loaded(object sender, EventArgs e)
        {
            if (this.ViewModel.UserProfile != null)
            {
                GoToAsync("//tradingCompetitions");
            }
            else
            {
                GoToAsync("//profile");
            }
        }

        private void Profile1ShellContent_Appearing(object sender, EventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ProfilePageNavigatedToMessage((sender as ShellContent).Title));
        }
        private void Profile2ShellContent_Appearing(object sender, EventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ProfilePageNavigatedToMessage((sender as ShellContent).Title));

        }
        private void Profile3ShellContent_Appearing(object sender, EventArgs e)
        {
            WeakReferenceMessenger.Default.Send(new ProfilePageNavigatedToMessage((sender as ShellContent).Title));
        }
    }
}
