using Acr.UserDialogs;
using Bioss.Ultrasound.Core.Ble;
using Bioss.Ultrasound.Core.Ble.Devices;
using Bioss.Ultrasound.Core.Data.Database;
using Bioss.Ultrasound.Core.DependencyExtensions;
using Bioss.Ultrasound.Core.Repository;
using Bioss.Ultrasound.Core.Repository.Abstracts;
using Bioss.Ultrasound.Core.Services;
using Bioss.Ultrasound.Core.Services.Licenses;
using Bioss.Ultrasound.Core.Services.Logging;
using Bioss.Ultrasound.Core.Services.Logging.Abstracts;
using Bioss.Ultrasound.Core.Services.Server;
using Bioss.Ultrasound.Core.Services.Sessions;
using Bioss.Ultrasound.Maui.Navigation;
using Bioss.Ultrasound.Maui.Pages;
using Bioss.Ultrasound.Maui.Platforms.Android.Extensions;
using Bioss.Ultrasound.Maui.ViewModels;
using Bioss.Ultrasound.Services;
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Bioss.Ultrasound.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var docs = Path.Combine(FileSystem.AppDataDirectory, "Documents");
        Directory.CreateDirectory(docs);
        var builder = MauiApp.CreateBuilder();

        // TODO вынести куда либо
        builder.Services.AddSingleton<INavigationService, NavigationService>();

        builder.Services.AddSingleton<AppSettingsService>();
        builder.Services.AddSingleton<InfoSettingsService>();
        builder.Services.AddSingleton<AutoResetTocoService>();
        builder.Services.AddSingleton<ServerHttpProvider>();
        builder.Services.AddSingleton<AudioService>();
        builder.Services.AddSingleton<DevicesScaner>();
        builder.Services.AddSingleton<CatAnaService>();
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "db.sqlite");
        builder.Services.AddSingleton(new AppDatabase(dbPath));

        // выбор по  platform
        builder.Services.AddSingleton<ISystemVolume, SystemVolume>();
        builder.Services.AddSingleton<IPermission, BLEPermission>();        
        builder.Services.AddSingleton<IPcmPlayer, PcmPlayer>();        

        builder.Services.AddSingleton<IMyDevice, MyDeviceAndroid>();
        builder.Services.AddSingleton<ILogger, ServerLogger>();
        builder.Services.AddSingleton<IUnsentLogDispatcher, ServerLogger>();
        builder.Services.AddSingleton<ISessionManager, SessionManager>();
        builder.Services.AddSingleton<ILicenseService, LicenseService>();
        builder.Services.AddSingleton<IRepository, Repository>();
        builder.Services.AddSingleton<IUserDialogs>(UserDialogs.Instance);

        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<MenuPage>();
        builder.Services.AddSingleton<AboutPage>();
        builder.Services.AddSingleton<DocumentPage>();
        builder.Services.AddSingleton<SettingsPage>();
        builder.Services.AddSingleton<RecordsPage>();

        builder.Services.AddSingleton<MainViewModel>();
        builder.Services.AddSingleton<MenuViewModel>();
        builder.Services.AddSingleton<AboutViewModel>();
        builder.Services.AddSingleton<DocumentViewModel>();
        builder.Services.AddSingleton<RecordsViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>();


        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
		//builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
