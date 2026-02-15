using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;

namespace Bioss.Ultrasound.Core.Ble;

public class DevicesScaner
{
    private readonly IAdapter _adapter;

    public DevicesScaner()
    {
        _adapter = CrossBluetoothLE.Current.Adapter;

        _adapter.ScanMode = ScanMode.Balanced;
        _adapter.ScanMatchMode = ScanMatchMode.STICKY;
        _adapter.DeviceDiscovered += OnDeviceDiscoveredAsync;
        _adapter.ScanTimeoutElapsed += OnScanTimeoutElapsed;
    }

    public event EventHandler<IDevice> Discovered;

    public void Start()
    {
        StartScan();
    }

    public async Task StopAsync()
    {
        await _adapter.StopScanningForDevicesAsync();
    }

    private void StartScan()
    {
        if (_adapter.IsScanning)
            return;

        //var guids = new Guid[] { Guids.SR_THERMOMETER };
        //_adapter.ScanMode = ScanMode.Balanced;
        //_adapter.StartScanningForDevicesAsync(guids);
        _adapter.StartScanningForDevicesAsync();
    }

    private void OnDeviceDiscoveredAsync(object sender, DeviceEventArgs e)
    {
        Discovered?.Invoke(this, e.Device);
    }

    private void OnScanTimeoutElapsed(object sender, EventArgs e)
    {
        //  auto restart scan
        StartScan();
    }
}
