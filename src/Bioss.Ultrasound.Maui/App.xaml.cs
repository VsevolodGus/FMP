using Bioss.Ultrasound.Core.Ble.Devices;
using Bioss.Ultrasound.Core.Data.Database;
using Bioss.Ultrasound.Core.DependencyExtensions;
using Bioss.Ultrasound.Core.Network;
using Bioss.Ultrasound.Core.Services.Logging;
using Bioss.Ultrasound.Core.Services.Logging.Abstracts;
using Bioss.Ultrasound.Core.Services.Sessions;
using Bioss.Ultrasound.Maui.Pages;
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

    private readonly Task? _connectDbTask;

    public App(
        AppDatabase database,
        ILogger serverLogger,
        ISessionManager sessionService,
        IUnsentLogDispatcher unsentLogDispatcher,
        AutoResetTocoService autoResetToco,
        AppSettingsService appSettings,
        IPermission permission,
        MenuPage menuPage, 
        IMyDevice myDevice)
    {
        InitializeComponent();

        _database = database;
        _serverLogger = serverLogger;
        _sessionService = sessionService;
        _unsentLogDispatcher = unsentLogDispatcher;
        _autoResetToco = autoResetToco;
        _appSettings = appSettings;
        _permission = permission;
        _myDevice = myDevice;

        Connectivity.Current.ConnectivityChanged += Connectivity_ConnectivityChanged;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        InitAsync();
        MainPage = new NavigationPage(menuPage);
        //MainPage = new AppShell();

    }

    protected override async void OnStart()
    {
        
    }


    private async ValueTask InitAsync()
    {
        try
        {
            if (!NetworkState.HasNetwork)
                return;

            await CheckPermissionsAsync();

            _myDevice.Init();

            if (_connectDbTask != null)
                await _connectDbTask;

            _ = _unsentLogDispatcher.SendAllUnsentAsync(false);
            await _sessionService.GetCurrentSessionAsync();
            

            _autoResetToco.IsAutoResetToco = _appSettings.IsAutoResetToco;
        }
        catch (Exception ex)
        {
            await _serverLogger.LogAsync($"Startup error: {ex}", ServerLogLevel.FatalTerminationError);
        }
    }

    private async Task<PermissionStatus> CheckPermissionsAsync()
    {
        var ble = _permission;
        var status = await ble.CheckStatusAsync();
        if (status != PermissionStatus.Granted)
            status = await ble.RequestAsync();

        return status;
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
