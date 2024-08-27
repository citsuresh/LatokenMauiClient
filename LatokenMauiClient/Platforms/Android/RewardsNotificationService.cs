using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Latoken.Api.Client.Library;
using System;
using Uri = Android.Net.Uri;

namespace LatokenMauiClient.Platforms.Android
{
    [Service]
    public class MyForegroundService : Service
    {
        public const int ServiceNotificationId = 1001;
        private const string NormalNotificationChannelId = "NormalNotificationChannel";
        private const string NewRewardsNotificationChannelId = "NewRewardsNotificationChannel";
        private const string NewTradingCompetitionsNotificationChannelId = "NewTradingCompetitionsNotificationChannel";
        private const string EndingTradingCompetitionsNotificationChannelId = "EndingTradingCompetitionsNotificationChannelId";
        private const string ErrorNotificationChannelId = "ErrorNotificationChannel";
        private const string EXTRA_REFRESH = "refresh";
        private CancellationTokenSource cancellationTokenSource;
        private Timer rewardsCheckTimer;
        private const int intervalMinutes = 15; // Run every 15 minutes
        private long counter = 0;
        private ICurrencyCache currencyCache;
        private int consecutiveErrorCount = 0;

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
                CheckForNewRewardsAndCompetitions(null);
            }
            else
            {
                rewardsCheckTimer?.Dispose();
                rewardsCheckTimer = new Timer(CheckForNewRewardsAndCompetitions, null, TimeSpan.Zero, TimeSpan.FromMinutes(intervalMinutes));
            }

            return StartCommandResult.Sticky;
        }

        private static bool CheckNewBitMartVoteListing()
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = client.GetAsync("https://www.bitmart.com/voting/en-US").Result;
                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    if (content.Contains("Upcoming"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return false;
        }

        private static bool CheckNewBitMartLaunchpad()
        {
            var client = new HttpClient();
            try
            {
                HttpResponseMessage response = client.GetAsync("https://www.bitmart.com/launchpad/en-US").Result;
                if (response.IsSuccessStatusCode)
                {
                    string content = response.Content.ReadAsStringAsync().Result;
                    if (content.Contains("Upcoming"))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return false;
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

                var channel3 = new NotificationChannel(NewTradingCompetitionsNotificationChannelId, "New Trading Competitions", NotificationImportance.Max);
                var notificationManager3 = (NotificationManager)GetSystemService(NotificationService);
                notificationManager3.CreateNotificationChannel(channel3);

                var channel4 = new NotificationChannel(EndingTradingCompetitionsNotificationChannelId, "Ending Trading Competitions", NotificationImportance.Max);
                var notificationManager4 = (NotificationManager)GetSystemService(NotificationService);
                notificationManager4.CreateNotificationChannel(channel4);

                var channel5 = new NotificationChannel(ErrorNotificationChannelId, "Error", NotificationImportance.Max);
                var notificationManager5 = (NotificationManager)GetSystemService(NotificationService);
                notificationManager5.CreateNotificationChannel(channel5);
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
                .SetContentTitle("Listening for new Rewards and Competitions")
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
                .SetContentTitle("Listening for new Rewards and Competitions")
                .SetContentText(text)
                .SetSmallIcon(Resource.Drawable.latoken)
                .SetOngoing(true)
                .SetStyle(new NotificationCompat.BigTextStyle().BigText(expandedText))
                .AddAction(refreshAction)
                .SetPriority(NotificationCompat.PriorityHigh)
                .Build();

            return notification;
        }



        private void CheckForNewRewardsAndCompetitions(object? state)
        {
            if (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    counter++;

                    var tradingCompetitions = this.GetTradingCompetitions();

                    var previousUpdatedTradingCompetitionsString = Preferences.Default.Get<string>("LastUpdatedTradingCompetitions", string.Empty);
                    var previousUpdatedTradingCompetitions = previousUpdatedTradingCompetitionsString.Split(",");

                    var totalNewCompetitions = 0;
                    var newTradingCompetitions = tradingCompetitions.Where(c => !previousUpdatedTradingCompetitions.Contains(c.Name));

                    var newCompetitionsNotificationMessage = string.Empty;
                    if (newTradingCompetitions.Any())
                    {
                        totalNewCompetitions = newTradingCompetitions.Count();
                        newCompetitionsNotificationMessage = "New Trading Competitions : " + string.Join(", ", newTradingCompetitions.Select(c => c.Name));
                    }

                    var totalEndingCompetitions = 0;
                    //Select the trading competitions which will within 2 hours
                    var endingTradingCompetitions = tradingCompetitions.Where(comp => comp.EndDate.ToLocalTime().AddHours(-2) <= DateTime.Now);

                    var endingCompetitionsNotificationMessage = string.Empty;
                    if (endingTradingCompetitions.Any())
                    {
                        totalEndingCompetitions = endingTradingCompetitions.Count();
                        endingCompetitionsNotificationMessage = "Ending Trading Competitions : \n" + string.Join(",\n", endingTradingCompetitions.Select(c => c.Name + " - " + c.EndDate.ToLocalTime().ToShortDateString() + " " + c.EndDate.ToLocalTime().ToLongTimeString()));
                    }

                    var airdrops = this.GetAirdrops();

                    var previouslyUpdatedAirdropData = Preferences.Default.Get<string>("LastUpdatedAirdropData", string.Empty);
                    LastUpdatedAirdrops lastUpdatedAirdrops = new LastUpdatedAirdrops();
                    lastUpdatedAirdrops.DeSerialize(previouslyUpdatedAirdropData);

                    var totalNewAirdrops = 0;
                    var newAirdrops = airdrops.Where(a => !lastUpdatedAirdrops.LastUpdatedAirdropData.Contains(a.Ticker));

                    var newAirdropsNotificationMessage = string.Empty;
                    if (newAirdrops.Any())
                    {
                        totalNewAirdrops = newAirdrops.Count();
                        newAirdropsNotificationMessage = "\nNew Airdrops : " + string.Join(", ", newAirdrops.Select(c => c.Ticker));
                    }
                    newCompetitionsNotificationMessage += newAirdropsNotificationMessage;

                    var profiles = this.GetProfiles();

                    List<Task<(Profile, IEnumerable<TransferDto>)>> tasks = new List<Task<(Profile, IEnumerable<TransferDto>)>>();
                    foreach (var profile in profiles)
                    {
                        tasks.Add(Task.Run(() => this.GetProfileRewards(profile)));
                    }

                    Task.WaitAll(tasks.ToArray());

                    var rewardsNotificationMessage = string.Empty;
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
                            rewardsNotificationMessage += profile.ProfileName + " - New Rewards:" + rewardsString + "\n";
                        }
                    }

                    var bitMartMessage = string.Empty;
                    //if (CheckNewBitMartVoteListing())
                    //{
                    //    bitMartMessage = "New BitMart Vote.\n";
                    //}

                    //if(CheckNewBitMartLaunchpad())
                    //{
                    //    bitMartMessage += "New BitMart Launchpad.\n";
                    //}

                    if (string.IsNullOrEmpty(rewardsNotificationMessage)
                        && string.IsNullOrEmpty(newCompetitionsNotificationMessage)
                        && string.IsNullOrEmpty(endingCompetitionsNotificationMessage)
                        && string.IsNullOrEmpty(bitMartMessage))
                    {
                        string message = $"Checked rewards, competitions & airdrops {counter} times\nNo new Rewards, Competitions & Airdrops since you last checked!";
                        var notification = CreateUpdatedNotification(NormalNotificationChannelId, $"Running in the background. {bitMartMessage} No New Rewards, Competitions & Airdrops!", message);
                        StartForeground(ServiceNotificationId, notification);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(rewardsNotificationMessage))
                        {
                            // Display a notification
                            string message = $"Checked rewards and competitions {counter} times\n{bitMartMessage}{rewardsNotificationMessage}";
                            var notification = CreateUpdatedNotification(NewRewardsNotificationChannelId, $"Running in the background. {bitMartMessage} {totalNewRewards} New Rewards!\"", message);
                            StartForeground(ServiceNotificationId, notification);
                        }

                        if (!string.IsNullOrEmpty(newCompetitionsNotificationMessage))
                        {
                            // Display a notification
                            string message = $"Checked rewards and competitions {counter} times\n{bitMartMessage}{newCompetitionsNotificationMessage}";
                            if (totalNewCompetitions > 0)
                            {
                                var notification = CreateUpdatedNotification(NewTradingCompetitionsNotificationChannelId, $"Running in the background. {bitMartMessage}{totalNewCompetitions} New Competitions, {totalNewAirdrops} New Airdrops!\"", message);
                                StartForeground(ServiceNotificationId, notification);
                            }
                            else
                            {
                                var notification = CreateUpdatedNotification(NewTradingCompetitionsNotificationChannelId, $"Running in the background. {bitMartMessage}{totalNewAirdrops} New Airdrops!\"", message);
                                StartForeground(ServiceNotificationId, notification);
                            }

                        }

                        if (!string.IsNullOrEmpty(endingCompetitionsNotificationMessage))
                        {
                            // Display a notification
                            string message = $"Checked rewards and competitions {counter} times\n{bitMartMessage}{endingCompetitionsNotificationMessage}";
                            var notification = CreateUpdatedNotification(EndingTradingCompetitionsNotificationChannelId, $"Running in the background. {bitMartMessage}{totalEndingCompetitions} Ending Competitions!\"", message);
                            StartForeground(ServiceNotificationId, notification);
                        }

                        if (string.IsNullOrEmpty(rewardsNotificationMessage)
                            && string.IsNullOrEmpty(newCompetitionsNotificationMessage)
                            && string.IsNullOrEmpty(endingCompetitionsNotificationMessage)
                            && !string.IsNullOrEmpty(bitMartMessage))
                        {
                            // Display a notification
                            string message = $"Checked rewards and competitions {counter} times\n{bitMartMessage}";
                            var notification = CreateUpdatedNotification(NewTradingCompetitionsNotificationChannelId, $"Running in the background. {bitMartMessage}!\"", message);
                            StartForeground(ServiceNotificationId, notification);
                        }
                    }

                    consecutiveErrorCount = 0;
                }
                catch (Exception ex)
                {
                    consecutiveErrorCount++;
                    string message = "Error while checking for new rewards!\n" + ex.Message;
                    var notification = CreateUpdatedNotification(ErrorNotificationChannelId, "Running in the background. Error Occurred!", message);
                    StartForeground(ServiceNotificationId, notification);
                    if (consecutiveErrorCount < 3)
                    {
                        CheckForNewRewardsAndCompetitions(state);
                    }
                    else
                    {
                        consecutiveErrorCount = 0;
                    }
                }
            }
        }

        private IEnumerable<TradingCompetitionData> GetTradingCompetitions()
        {
            var vm = new TradingCompetitionsViewModel(this.currencyCache);
            vm.InitializeProfileAndRestClient();
            var competitions = vm.GetTradingCompetitions();
            return competitions;
        }

        private IEnumerable<Airdrop> GetAirdrops()
        {
            var vm = new AirdropsViewModel(this.currencyCache);
            vm.InitializeProfileAndRestClient();
            var airdrops = vm.GetAirdrops();
            return airdrops;
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
