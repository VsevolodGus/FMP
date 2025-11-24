using Bioss.Ultrasound.Ble.Commands;
using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Ble.ProtocolGenerations;
using Bioss.Ultrasound.Collections;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Ble.Devices
{
    public class MyDeviceAndroid : IMyDevice
    {
        private IAdapter _adapter;
        private IDevice _device;
        private List<ICharacteristic> _subscribedToValueUpdated = new();
        private IGeneration _guids;

        private RingBuffer<byte> _buffer = new RingBuffer<byte>(1024);

        public MyDeviceAndroid()
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
                ConnectParameters para = new ConnectParameters(true, true);
                await _adapter.ConnectToDeviceAsync(device, para);
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

            _guids = await GuidsManager.GetGeneration(_device);

            var services = await _device.GetServicesAsync();
            //  подписываемся на все характеристики
            foreach (var s in services)
            {
                DebugWriteLine($"service: {s.Id}");

                var characteristics = await s.GetCharacteristicsAsync();
                foreach (var c in characteristics)
                {
                    DebugWriteLine($"   characteristic: {c.Id}, CanUpdate: {c.CanUpdate}");

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

            if (_guids.ChCustomRead == characteristic.Id)
            {
                var data = e.Characteristic.Value;

                var checkStartPackage = data.Length >= 3 && data[0] == 0x55 && data[1] == 0xAA && data[2] == 0x09;

                if (!checkStartPackage)
                {
                    _buffer.Push(data);
                    return;
                }
                
                var packageData = _buffer.Pop(_buffer.Count);
                _buffer.Push(data);

                if (packageData is null)
                    return;
                
                var package = Package.Init(packageData);
                if (!(package?.IsValid ?? false))
                {
                    DebugWriteLine($"ValueUpdated: Invalid package recieved");
                    return;
                }

                NewPackage?.Invoke(this, package);
            }
        }

        public async Task<bool> ResetTocoAsync()
        {
            try
            {
                var customService = await _device.GetServiceAsync(_guids.SrCustom);
                var writeCh = await customService.GetCharacteristicAsync(_guids.ChCustomWrite);

                var command = new SetupCommand(7, 1, true, 0, false);
                return await writeCh.WriteAsync(command.WriteData());
            } catch { }

            return false;
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
            //Debug.WriteLine($"MyDevice Android: {message}");
        }
    }
}
