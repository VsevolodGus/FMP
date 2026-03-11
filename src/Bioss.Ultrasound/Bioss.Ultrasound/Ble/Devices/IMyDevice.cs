using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Ble.Devices
{
    public interface IMyDevice
    {
        bool IsConnected { get; }
        string Name { get; }

        event EventHandler<bool> ConnectedChanged;

        Task ConnectAsync(IDevice device);
        Task DisconnectAsync();
        Task ResetTocoAsync();
    }
}
