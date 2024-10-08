﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace LatokenMauiClient
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            var a = Assembly.GetExecutingAssembly();
            using var stream = a.GetManifestResourceStream("LatokenMauiClient.appsettings.json");

            var config = new ConfigurationBuilder()
                        .AddJsonStream(stream)
                        .Build();


            builder.Configuration.AddConfiguration(config);
            
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<ShellViewModel>();
            builder.Services.AddTransient<WalletPage>();
            builder.Services.AddTransient<SpotPage>();
            builder.Services.AddTransient<AssetPageViewModel>();
            builder.Services.AddTransient<ProfilePage>();
            builder.Services.AddTransient<ProfilePageViewModel>();
            builder.Services.AddTransient<TradingCompetitionsPage>();
            builder.Services.AddTransient<TradingCompetitionsViewModel>();
            builder.Services.AddTransient<RewardsAndAirdropsPage>();
            builder.Services.AddTransient<RewardsAndAirdropsViewModel>();
            builder.Services.AddTransient<AirdropsPage>();
            builder.Services.AddTransient<AirdropsViewModel>();
            builder.Services.AddTransient<SellPage>();
            builder.Services.AddTransient<SellViewModel>();
            builder.Services.AddTransient<TransferPage>();
            builder.Services.AddTransient<TransferViewModel>();
            builder.Services.AddTransient<ICurrencyCache, LatokenCurrencyCache>();

            builder.Services.AddSingleton<IAlertService, AlertService>();

            return builder.Build();
        }
    }
}
