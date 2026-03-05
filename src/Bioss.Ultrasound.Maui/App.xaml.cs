using Bioss.Ultrasound.Core.Ble.Devices;
using Bioss.Ultrasound.Core.Data.Database;
using Bioss.Ultrasound.Core.DependencyExtensions;
using Bioss.Ultrasound.Core.Network;
using Bioss.Ultrasound.Core.Services.Logging;
using Bioss.Ultrasound.Core.Services.Logging.Abstracts;
using Bioss.Ultrasound.Core.Services.Sessions;
using Bioss.Ultrasound.Services;

namespace Bioss.Ultrasound.Maui;

public partial class App : Application
{
    private readonly AppDatabase _database;
    private readonly ILogger _serverLogger;
    private readonly ISessionManager _sessionService;
    private readonly IUnsentLogDispatcher _unsentLogDispatcher;
    private readonly IPermission _permission;
    private readonly IMyDevice _myDevice;
    private readonly AutoResetTocoService _autoResetToco;
    private readonly AppSettingsService _appSettings;

    public App(
        AppDatabase database,
        ILogger serverLogger,
        ISessionManager sessionService,
        IUnsentLogDispatcher unsentLogDispatcher,
        AutoResetTocoService autoResetToco,
        AppSettingsService appSettings,
        IPermission permission,
        AppShell shell,
        IMyDevice myDevice)
    {
        InitializeComponent();

        UserAppTheme = AppTheme.Light;

        _database = database;
        _serverLogger = serverLogger;
        _sessionService = sessionService;
        _unsentLogDispatcher = unsentLogDispatcher;
        _autoResetToco = autoResetToco;
        _appSettings = appSettings;
        _permission = permission;
        _myDevice = myDevice;

        MainPage = shell;

        InitAsync();
        Connectivity.Current.ConnectivityChanged += Connectivity_ConnectivityChanged;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    private async ValueTask InitAsync()
    {
        try
        {
            if (!NetworkState.HasNetwork)
                return;

            await EnsureStartupPermissionsAsync();
            _myDevice.Init();
            await _database.ConnectAsync();

            _ = _unsentLogDispatcher.SendAllUnsentAsync(false);
            await _sessionService.GetCurrentSessionAsync();
            

            _autoResetToco.IsAutoResetToco = _appSettings.IsAutoResetToco;
        }
        catch (Exception ex)
        {
            await _serverLogger.LogAsync($"Startup error: {ex}", ServerLogLevel.FatalTerminationError);
        }
    }

    /// <summary>
    ///  Первое открытие успешно
    /// Второе открытие все повисает на начальном экране
    /// </summary>
    /// <returns></returns>
    public async Task EnsureStartupPermissionsAsync()
    {
        var status = await _permission.CheckStatusAsync();
        if (status != PermissionStatus.Granted)
            await _permission.RequestAsync();
    }
    private async void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
            await _serverLogger.LogAsync($"Fatal error: {ex}", ServerLogLevel.FatalTerminationError);
    }

    private async void Connectivity_ConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        if (NetworkState.AccessNetwork(e.NetworkAccess))
            await _unsentLogDispatcher.SendAllUnsentAsync();
    }
}
