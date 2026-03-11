using Bioss.Ultrasound.Ble.Commands;
using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Ble.ProtocolGenerations;
using Bioss.Ultrasound.Services.Logging.Abstracts;
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
        private readonly ILogger _logger;
        private readonly IAdapter _adapter;
        private IDevice _device;
        private readonly List<ICharacteristic> _subscribedToValueUpdated = new();
        private IGeneration _guids;

        public MyDeviceIos(ILogger logger)
        {
            _logger = logger;

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
                await Task.Delay(600); // время чтобы подключение полнотсью синхронизировалось были ошибки
            }
            catch (DeviceConnectionException e)
            {
                _device = null;
                _logger.Log($"Connect error: {e}");
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

            _guids = await GuidsManager.GetGeneration(_device);
            var services = await _device.GetServicesAsync();
            //  подписываемся на все характеристики
            foreach (var s in services)
            {
                DebugWriteLine($"service: {s.Id}");

                var characteristics = await s.GetCharacteristicsAsync();
                foreach(var c in characteristics)
                {
                    DebugWriteLine($"   characteristic: {c.Id}, Update: {c.CanUpdate} Write: {c.CanWrite}");

                    if (c.CanUpdate)
                    {
                        c.ValueUpdated += Ch_ValueUpdated;
                        try
                        {
                            await c.StartUpdatesAsync();
                        }
                        catch (Exception ex)
                        {
                            _logger.Log($"Exception BLU OnConnected: {ex}");
                        }
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

            if (_guids.ChCustomRead == characteristic.Id)
            {
                var package = Package.Init(characteristic.Value.AsSpan());
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
            try
            {
                var customService = await _device.GetServiceAsync(_guids.SrCustom);
                var writeCh = await customService.GetCharacteristicAsync(_guids.ChCustomWrite);

                var command = new SetupCommand(7, 1, true, 0, false);
                await writeCh.WriteAsync(command.WriteData());
            } catch { }
        }

        private void DisconnectWork(IDevice device)
        {
            if (device != _device)
                return;

            IsConnected = false;
            ConnectedChanged?.Invoke(this, IsConnected);

            _logger.Log($"Связь с датчиком {device.Name} разорвона");

            //  Отпишемся от старых характеристик
            foreach (var c in _subscribedToValueUpdated)
                c.ValueUpdated -= Ch_ValueUpdated;
            _subscribedToValueUpdated.Clear();
        }

        private void DebugWriteLine(string message)
        {
            Debug.WriteLine($"MyDevice ios: {message}");
        }

        public void ResetConsumerState()
        { }
    }
}
