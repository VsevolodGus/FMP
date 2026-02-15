using Bioss.Ultrasound.Core.Ble.Models;
using Plugin.BLE.Abstractions.Contracts;

namespace Bioss.Ultrasound.Core.Ble.Devices;

public interface IMyDevice
{
    bool IsConnected { get; }
    string Name { get; }

    event EventHandler<bool> ConnectedChanged;
    event EventHandler<Package> NewPackage;

    Task ConnectAsync(IDevice device);
    Task DisconnectAsync();

    Task ResetTocoAsync();
}
