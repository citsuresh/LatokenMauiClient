using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Uri = Android.Net.Uri;

namespace LatokenMauiClient.Platforms.Android
{
    [Service]
    public class MyForegroundService : Service
    {
        public const int ServiceNotificationId = 1001;
        private const string ChannelId = "MyForegroundServiceChannel";
        private CancellationTokenSource cancellationTokenSource;
        private Timer rewardsCheckTimer;
        private const int intervalMinutes = 15; // Run every 15 minutes
        private long counter = 0;
        private ICurrencyCache currencyCache;

        public MyForegroundService()
        {
            this.currencyCache = new LatokenCurrencyCache();
        }
        public override IBinder OnBind(Intent intent) => null;

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            cancellationTokenSource = new CancellationTokenSource();
            rewardsCheckTimer = new Timer(CheckForNewRewards, null, TimeSpan.Zero, TimeSpan.FromMinutes(intervalMinutes));

            CreateNotificationChannel();
            var notification = CreateInitialNotification();

            StartForeground(ServiceNotificationId, notification);
            //var notificationManager = NotificationManagerCompat.From(this);
            //notificationManager.Notify(0, notification);



            return StartCommandResult.Sticky;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            rewardsCheckTimer.Dispose();
        }

        private void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(ChannelId, "Foreground Service", NotificationImportance.Default);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);
            }
        }

        private Notification CreateInitialNotification()
        {
            //var intent = new Intent(Intent.ActionView);
            //intent.SetData(Uri.Parse("//rewardsAndAirdrops"));

            //var intent = new Intent(this, typeof(MainActivity));
            //intent.PutExtra("tabName", "rewardsAndAirdrops"); // Replace with your tab name

            //var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notification = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle("Listening for new Rewards")
                .SetContentText("Running in the background")
                .SetSmallIcon(Resource.Drawable.searchicon)
                .SetOngoing(true)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText("Running in the background"))
                //.SetContentIntent(pendingIntent)
                .Build();

            return notification;
        }

        private Notification CreateUpdatedNotification(string text, string expandedText)
        {
            //var intent = new Intent(Intent.ActionView);
            //intent.SetData(Uri.Parse("//rewardsAndAirdrops"));

            //var intent = new Intent(this, typeof(MainActivity));
            //intent.PutExtra("tabName", "rewardsAndAirdrops"); // Replace with your tab name

            //var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notification = new NotificationCompat.Builder(this, ChannelId)
                .SetContentTitle("Listening for new Rewards")
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.searchicon)
                .SetOngoing(true)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(expandedText))
                //.SetContentIntent(pendingIntent)
                .Build();

            return notification;
        }



        private void CheckForNewRewards(object? state)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    counter++;

                    var profiles = this.GetProfiles();

                    List<Task<(Profile, IEnumerable<TransferDto>)>> tasks = new List<Task<(Profile, IEnumerable<TransferDto>)>>();
                    foreach (var profile in profiles)
                    {
                        tasks.Add(Task.Run(() => this.GetProfileRewards(profile)));
                    }

                    Task.WaitAll(tasks.ToArray());

                    var notificationMessage = string.Empty;
                    foreach (var task in tasks)
                    {
                        var profileRewards = ((Profile, IEnumerable<TransferDto>))task.Result;
                        var profile = profileRewards.Item1;
                        var rewards = profileRewards.Item2;

                        var profileName = profile.ProfileName;
                        LastUpdatedTimestampData lastUpdatedTimestampData = null;
                        LastUpdatedTransfersTimestampData lastUpdatedTimestampDataForProfile = null;

                        var lastUpdatedSettingsString = Preferences.Default.Get<string>(profileName + "_LastUpdatedTimestampData", string.Empty); ;
                        if (!string.IsNullOrEmpty(lastUpdatedSettingsString))
                        {
                            lastUpdatedTimestampData = new LastUpdatedTimestampData();
                            lastUpdatedTimestampData.DeSerialize(lastUpdatedSettingsString);
                        }

                        if (lastUpdatedTimestampData == null)
                        {
                            lastUpdatedTimestampData = new LastUpdatedTimestampData();
                        }

                        if (lastUpdatedTimestampData.TransfersTimestamps.ContainsKey(profileName))
                        {
                            lastUpdatedTimestampDataForProfile = lastUpdatedTimestampData.TransfersTimestamps[profileName];
                        }
                        else
                        {
                            lastUpdatedTimestampDataForProfile = new LastUpdatedTransfersTimestampData();
                        }

                        string rewardsString = string.Empty;
                        foreach (var competitionReward in rewards)
                        {
                            if (lastUpdatedTimestampDataForProfile != null && (lastUpdatedTimestampDataForProfile.LastUpdatedRewardsAirdropsTimeStamp == null
                            || competitionReward.Timestamp > lastUpdatedTimestampDataForProfile.LastUpdatedRewardsAirdropsTimeStamp))
                            {
                                rewardsString += $"\n\t{competitionReward.CurrencySymbol} - {competitionReward.UsdValue} USDT";
                            }
                        }

                        if (!string.IsNullOrEmpty(rewardsString))
                        {
                            notificationMessage += profile.ProfileName + " - New Rewards:" + rewardsString + "\n";
                        }
                    }

                    if (string.IsNullOrEmpty(notificationMessage))
                    {
                        string message = $"Checked rewards {counter} times\nNo new rewards since you last checked!";
                        var notification = CreateUpdatedNotification("Running in the background", message);
                        StartForeground(ServiceNotificationId, notification);
                    }
                    else
                    {
                        // Display a notification
                        string message = $"Checked rewards {counter} times\n{notificationMessage}";
                        var notification = CreateUpdatedNotification("Running in the background", message);
                        StartForeground(ServiceNotificationId, notification);
                    }
                }
                catch (Exception ex)
                {
                    string message = "Error while checking for new rewards!\n" + ex.Message;
                    var notification = CreateUpdatedNotification("Running in the background", message);
                    StartForeground(ServiceNotificationId, notification);
                }
            }
        }

        private (Profile, IEnumerable<TransferDto>) GetProfileRewards(Profile profile)
        {
            var vm = new RewardsAndAirdropsViewModel(this.currencyCache);
            vm.InitializeProfileAndRestClient(profile);
            var rewards = vm.GetTradingCompetitionRewards();
            return (profile, rewards);
        }

        private List<Profile> GetProfiles()
        {
            var profileFilterInstance = App.Current.Resources["ProfileFilterInstance"] as ProfileFilter;
            if (profileFilterInstance != null && profileFilterInstance.SelectedProfile != null)
            {
                return profileFilterInstance.AvailableProfiles.ToList();
            }

            return new List<Profile>();
        }
    }
}
