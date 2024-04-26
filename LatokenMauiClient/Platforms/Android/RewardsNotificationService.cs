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
        private const string NormalNotificationChannelId = "NormalNotificationChannel";
        private const string NewRewardsNotificationChannelId = "NewRewardsNotificationChannel";
        private const string ErrorNotificationChannelId = "ErrorNotificationChannel";
        private const string EXTRA_REFRESH = "refresh";
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


            CreateNotificationChannel();
            var notification = CreateInitialNotification();

            StartForeground(ServiceNotificationId, notification);
            //var notificationManager = NotificationManagerCompat.From(this);
            //notificationManager.Notify(0, notification);

            if (intent?.GetBooleanExtra(EXTRA_REFRESH, false) == true)
            {
                // The "Refresh" notification button was clicked
                CheckForNewRewards(null);
            }
            else
            {
                rewardsCheckTimer?.Dispose();
                rewardsCheckTimer = new Timer(CheckForNewRewards, null, TimeSpan.Zero, TimeSpan.FromMinutes(intervalMinutes));
            }

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
                var channel = new NotificationChannel(NormalNotificationChannelId, "Other Notifications", NotificationImportance.Max);
                var notificationManager = (NotificationManager)GetSystemService(NotificationService);
                notificationManager.CreateNotificationChannel(channel);

                var channel2 = new NotificationChannel(NewRewardsNotificationChannelId, "New Rewards", NotificationImportance.Max);
                var notificationManager2 = (NotificationManager)GetSystemService(NotificationService);
                notificationManager2.CreateNotificationChannel(channel2);

                var channel3 = new NotificationChannel(ErrorNotificationChannelId, "Error", NotificationImportance.Max);
                var notificationManager3 = (NotificationManager)GetSystemService(NotificationService);
                notificationManager3.CreateNotificationChannel(channel3);
            }
        }

        private Notification CreateInitialNotification()
        {
            //var intent = new Intent(Intent.ActionView);
            //intent.SetData(Uri.Parse("//rewardsAndAirdrops"));

            //var intent = new Intent(this, typeof(MainActivity));
            //intent.PutExtra("tabName", "rewardsAndAirdrops"); // Replace with your tab name

            //var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.UpdateCurrent);

            var notification = new NotificationCompat.Builder(this, NormalNotificationChannelId)
                .SetContentTitle("Listening for new Rewards")
                .SetContentText("Running in the background")
                .SetSmallIcon(Resource.Drawable.latoken)
                .SetOngoing(true)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText("Running in the background"))
                //.SetContentIntent(pendingIntent)
                //.AddAction(action1)
                .SetPriority(NotificationCompat.PriorityHigh)
                .Build();
            return notification;
        }

        private Notification CreateUpdatedNotification(string channelId, string text, string expandedText)
        {

            var refreshActionIntent = new Intent(this, typeof(MyForegroundService));
            refreshActionIntent.PutExtra(EXTRA_REFRESH, true);
            var refreshActionPendingIntent = PendingIntent.GetService(
                                                        this,
                                                        0,
                                                        refreshActionIntent,
                                                        PendingIntentFlags.Immutable);


            var refreshAction = new NotificationCompat.Action.Builder(Resource.Drawable.refresh_image, "Refresh", refreshActionPendingIntent).Build();

            var notification = new NotificationCompat.Builder(this, channelId)
                .SetContentTitle("Listening for new Rewards")
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.latoken)
                .SetOngoing(true)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(expandedText))
                .AddAction(refreshAction)
                .SetPriority(NotificationCompat.PriorityHigh)
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
                    var totalNewRewards = 0;
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
                                totalNewRewards++;
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
                        var notification = CreateUpdatedNotification(NormalNotificationChannelId, "Running in the background. No New Rewards!", message);
                        StartForeground(ServiceNotificationId, notification);
                    }
                    else
                    {
                        // Display a notification
                        string message = $"Checked rewards {counter} times\n{notificationMessage}";
                        var notification = CreateUpdatedNotification(NewRewardsNotificationChannelId, $"Running in the background. {totalNewRewards} New Rewards!\"", message);
                        StartForeground(ServiceNotificationId, notification);
                    }
                }
                catch (Exception ex)
                {
                    string message = "Error while checking for new rewards!\n" + ex.Message;
                    var notification = CreateUpdatedNotification(ErrorNotificationChannelId, "Running in the background. Error Occurred!", message);
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
