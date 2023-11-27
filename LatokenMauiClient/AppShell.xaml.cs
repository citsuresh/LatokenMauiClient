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
                GoToAsync("//pages/tradingCompetitions");
            }
            else
            {
                GoToAsync("//pages/profile");
            }
        }
    }
}
