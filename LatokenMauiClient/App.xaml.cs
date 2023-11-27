namespace LatokenMauiClient
{
    public partial class App : Application
    {
        public static IServiceProvider Services;
        public static IAlertService AlertSvc;

        public App(AppShell page, IServiceProvider provider)
        {
            InitializeComponent();

            MainPage = page;
            Services = provider;
            AlertSvc = Services.GetService<IAlertService>();
        }
    }
}
