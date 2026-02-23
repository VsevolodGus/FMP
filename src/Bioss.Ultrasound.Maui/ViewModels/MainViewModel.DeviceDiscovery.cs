using System.Collections.ObjectModel;
using Acr.UserDialogs;
using Bioss.Ultrasound.Core.Ble.Devices;
using Bioss.Ultrasound.Core.Ble;
using Bioss.Ultrasound.Core.DependencyExtensions;
using Bioss.Ultrasound.Core.Repository.Abstracts;
using Bioss.Ultrasound.Core.Services.Licenses;
using Bioss.Ultrasound.Core.Services;
using Bioss.Ultrasound.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Plugin.BLE.Abstractions.Contracts;
using Bioss.Ultrasound.Core.Services.Logging.Abstracts;

namespace Bioss.Ultrasound.Maui.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private static readonly string[] DevicePrefixesFilter =
    {
        "LCeFM",
        "Doctis CTG",
        "FMP",
        "DOCTIS-CTG"
    };

    // Общие зависимости (и для поиска, и для записи) — держим здесь
    protected readonly IUserDialogs _dialogs;
    protected readonly DevicesScaner _devicesScaner;
    protected readonly IRepository _repository;
    protected readonly AppSettingsService _appSettings;
    protected readonly IMyDevice _device;
    protected readonly AudioService _audioService;
    protected readonly ISystemVolume _systemVolume;
    protected readonly ILicenseService _licenseService;
    protected readonly CatAnaService _catAnaService;
    protected readonly ILogger _logger;
    protected readonly IPcmPlayer _pcmPlayer;

    public ObservableCollection<IDevice> Devices { get; } = new();

    [ObservableProperty] private IDevice? selectedDevice;
    [ObservableProperty] private bool isConnected;

    public MainViewModel(
        IUserDialogs dialogs,
        DevicesScaner devicesScaner,
        IRepository repository,
        AppSettingsService appSettings,
        IMyDevice device,
        IPcmPlayer pcmPlayer,
        AudioService audioService,
        ISystemVolume systemVolume,
        ILicenseService licenseService,
        CatAnaService catAnaService,
        ILogger logger)
    {
        _dialogs = dialogs;
        _devicesScaner = devicesScaner;
        _repository = repository;
        _appSettings = appSettings;
        _device = device;
        _pcmPlayer = pcmPlayer;
        _audioService = audioService;
        _systemVolume = systemVolume;
        _licenseService = licenseService;
        _catAnaService = catAnaService;
        _logger = logger;

        _devicesScaner.Discovered += OnDeviceDiscovered;
        _device.ConnectedChanged += OnConnectedChanged;

        // Вторая часть сама подпишется на NewPackage и т.п.
        InitRecordingPart();

        IsConnected = _device.IsConnected;

        // Старт сканирования — можно оставить тут (или вызвать из Appearing)
        _devicesScaner.Start();
    }

    // Команды (поиск/подключение)
    [RelayCommand]
    private async Task ConnectSelectedAsync()
    {
        if (SelectedDevice is null)
            return;

        var dev = SelectedDevice;
        SelectedDevice = null;

        try
        {
            var isLicense = await _licenseService.CheckDeviceLicenseAsync(dev.Name);

            if (!isLicense)
            {
                _dialogs.Toast("Device is not licensed");
                return;
            }

            await _device.ConnectAsync(dev);
        }
        catch (Exception ex)
        {
            _logger.Log($"ConnectSelectedAsync error: {ex}");
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        try
        {
            await _device.DisconnectAsync();
        }
        catch (Exception ex)
        {
            _logger.Log($"DisconnectAsync error: {ex}");
        }
    }

    [RelayCommand]
    private async Task RestartScanAsync()
    {
        try
        {
            Devices.Clear();
            await _devicesScaner.StopAsync();
            _devicesScaner.Start();
        }
        catch (Exception ex)
        {
            _logger.Log($"RestartScanAsync error: {ex}");
        }
    }

    private void OnDeviceDiscovered(object sender, IDevice device)
    {
        if (_device.IsConnected)
            return;

        if (string.IsNullOrWhiteSpace(device.Name))
            return;

        if (!DevicePrefixesFilter.Any(p => device.Name.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            return;

        if (Devices.Any(d => d.Id == device.Id))
            return;

        Devices.Add(device);
    }

    private async void OnConnectedChanged(object? sender, bool connected)
    {
        IsConnected = connected;

        Devices.Clear();

        try
        {
            if (connected)
                await _devicesScaner.StopAsync();
            else
                _devicesScaner.Start();

            // Вся логика, связанная с записью при коннекте/дисконнекте — во 2-й части
            await OnConnectionStateChangedAsync(connected);
        }
        catch (Exception ex)
        {
            _logger.Log($"OnConnectedChanged error: {ex}");
        }
    }
}