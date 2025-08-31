using Bioss.Ultrasound.Ble.Commands;
using Bioss.Ultrasound.Ble.Models;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Ble.Devices
{
    public class MyDeviceIos : IMyDevice
    {
        private IAdapter _adapter;
        private IDevice _device;
        private List<ICharacteristic> _subscribedToValueUpdated = new();

        public MyDeviceIos()
        {
            _adapter = CrossBluetoothLE.Current.Adapter;
            _adapter.DeviceConnected += OnConnected;
            _adapter.DeviceDisconnected += OnDisconnected;
            _adapter.DeviceConnectionLost += OnConnectionLost;
        }

        public bool IsConnected { get; private set; }
        public string Name => _device?.Name;

        public event EventHandler<bool> ConnectedChanged;
        public event EventHandler<Package> NewPackage;

        public async Task ConnectAsync(IDevice device)
        {
            try
            {
                _device = device;
                await _adapter.ConnectToDeviceAsync(device);
            }
            catch (DeviceConnectionException e)
            {
                _device = null;
            }
        }

        public async Task DisconnectAsync()
        {
            await _adapter.DisconnectDeviceAsync(_device);
        }

        private async void OnConnected(object sender, DeviceEventArgs e)
        {
            if (e.Device != _device)
                return;

            DebugWriteLine($"Connected {e.Device.Name}");

            IsConnected = true;
            ConnectedChanged?.Invoke(this, IsConnected);

            var services = await _device.GetServicesAsync();

            //  подписываемся на все характеристики
            foreach(var s in services)
            {
                DebugWriteLine($"service: {s.Id}");

                var characteristics = await s.GetCharacteristicsAsync();
                foreach(var c in characteristics)
                {
                    DebugWriteLine($"   characteristic: {c.Id}, Update: {c.CanUpdate} Write: {c.CanWrite}");

                    if (c.CanUpdate)
                    {
                        c.ValueUpdated += Ch_ValueUpdated;
                        await c.StartUpdatesAsync();
                        _subscribedToValueUpdated.Add(c);
                    }
                }
            }
        }

        private void OnDisconnected(object sender, DeviceEventArgs e)
        {
            DisconnectWork(e.Device);
        }

        private void OnConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            DisconnectWork(e.Device);
        }

        private void Ch_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            var characteristic = e.Characteristic;

            //DebugWriteLine($"ValueUpdated: {characteristic.Uuid} {characteristic.Value} {characteristic.Value.Length}");
            //DebugWriteLine($"ValueUpdated: len: {characteristic.Value.Length}, data: {BitConverter.ToString(characteristic.Value)}");

            if (Guids.CH_CUSTOM_READ == characteristic.Id)
            {
                var package = Package.Init(characteristic.Value);
                if (package is null)
                    return;
                if (!package.IsValid)
                {
                    DebugWriteLine($"ValueUpdated: Invalid package recieved");
                    return;
                }

                NewPackage?.Invoke(this, package);
            }
        }

        public async Task ResetTocoAsync()
        {
            var customService = await _device.GetServiceAsync(Guids.SR_CUSTOM);
            var writeCh = await customService.GetCharacteristicAsync(Guids.CH_CUSTOM_WRITE);

            var command = new SetupCommand(7, 10, true, 0, false);
            var result = await writeCh.WriteAsync(command.WriteData());
            //DebugWriteLine($"TOCO reset {result}");
        }

        private void DisconnectWork(IDevice device)
        {
            if (device != _device)
                return;

            IsConnected = false;
            ConnectedChanged?.Invoke(this, IsConnected);

            //  Отпишемся от старых характеристик
            foreach (var c in _subscribedToValueUpdated)
                c.ValueUpdated -= Ch_ValueUpdated;
            _subscribedToValueUpdated.Clear();
        }

        private void DebugWriteLine(string message)
        {
            Debug.WriteLine($"MyDevice ios: {message}");
        }
    }
}
