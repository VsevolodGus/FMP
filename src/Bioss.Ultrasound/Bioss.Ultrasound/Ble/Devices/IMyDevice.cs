using Bioss.Ultrasound.Ble.Models;
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
        event EventHandler<Package> NewPackage;

        Task ConnectAsync(IDevice device);
        Task DisconnectAsync();

        Task ResetTocoAsync();
    }
}
