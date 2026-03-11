using Bioss.Ultrasound.Ble.Commands;
using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Ble.Devices
{
    public class MyDeviceAndroid : MyDeviceBase, IMyDevice
    {

        private readonly HashSet<ICharacteristic> _subscribedToValueUpdated = new();
        private bool _disconnecting;

        public event EventHandler<bool> ConnectedChanged;

        public MyDeviceAndroid(ILogger logger, DeviceStreamProcessor deviceStreamProcessor) : base(logger, deviceStreamProcessor)
        {
            _adapter.DeviceConnected += OnConnected;
            _adapter.DeviceDisconnected += OnDisconnected;
            _adapter.DeviceConnectionLost += OnConnectionLost;
        }

       

        #region public
        public async Task ConnectAsync(IDevice device)
        {
            _device = device;
            try
            {
                await _adapter.ConnectToDeviceAsync(device, connectParameters);
            }
            catch (DeviceConnectionException e)
            {
                _device = null;
                _logger.Log($"Connect error: {e}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected ||  _device is null)
                return;

            var device = _device;
            try
            {
                await _adapter.DisconnectDeviceAsync(device);
            }
            finally
            {
                await DisconnectWorkAsync(device);
            }
        }

        public async Task ResetTocoAsync()
        {
            if (!IsConnected || _device == null)
                return;

            try
            {
                var customService = await _device.GetServiceAsync(_guids.SrCustom);
                var writeCh = await customService.GetCharacteristicAsync(_guids.ChCustomWrite);

                var command = new SetupCommand(7, 1, true, 0, false);
                await writeCh.WriteAsync(command.WriteData());
            }
            catch { }
        }
        #endregion

        #region Events
        private async void OnConnected(object sender, DeviceEventArgs e)
        {
            try
            {
                if (_disconnecting || e.Device != _device)
                    return;

                IsConnected = true;
                ConnectedChanged?.Invoke(this, IsConnected);

                _guids = await GuidsManager.GetGeneration(_device);
                _streamProcessor.Start();
                var services = await _device.GetServicesAsync();
                //  подписываемся на все характеристики
                foreach (var s in services)
                {

                    var characteristics = await s.GetCharacteristicsAsync();
                    foreach (var c in characteristics)
                    {

                        if (c.CanUpdate && c.Id == _guids.ChCustomRead && _subscribedToValueUpdated.Add(c))
                        {
                            c.ValueUpdated += Ch_ValueUpdated;

                            try
                            {
                                await c.StartUpdatesAsync();
                            }
                            catch (Exception ex)
                            {
                                _logger.Log($"Exception BLU OnConnected {nameof(characteristics)}Item.{nameof(c.StartUpdatesAsync)}: {ex}");
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                _logger.Log($"Error log in {nameof(OnConnected)}: {ex}", ServerLogLevel.BluetoothError);
            }
        }

        private async void OnDisconnected(object sender, DeviceEventArgs e)
        {
            try
            {
                await DisconnectWorkAsync(e.Device);
            }
            catch(Exception ex)
            {
                _logger.Log($"Error log in {nameof(OnDisconnected)}: {ex}", ServerLogLevel.BluetoothError);
            }
        }

        private async void OnConnectionLost(object sender, DeviceErrorEventArgs e)
        {
            try
            {
                await DisconnectWorkAsync(e.Device);
            }
            catch (Exception ex)
            {
                _logger.Log($"Error log in {nameof(OnConnectionLost)}: {ex}", ServerLogLevel.BluetoothError);
            }
        }

        private void Ch_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            if (_guids == null || !IsConnected)
                return;

            var characteristic = e.Characteristic;
            if (_guids.ChCustomRead != characteristic.Id)
                return;

            var source = characteristic.Value;
            if (source is null || source.Length == 0)
                return;

            var data = new byte[source.Length];
            Buffer.BlockCopy(source, 0, data, 0, source.Length);

            _streamProcessor.OnSignal(new BleSignal()
            {
                Data = data,
            });
        }
        #endregion

        #region private
        private async Task DisconnectWorkAsync(IDevice device)
        {

            if (_disconnecting)
                return;

            _disconnecting = true;
            try
            {
                if (_device is null || device != _device)
                    return;
                
                if (IsConnected)
                {
                    IsConnected = false;
                    ConnectedChanged?.Invoke(this, false);
                }

                var oldSubs = _subscribedToValueUpdated.ToList();
                _subscribedToValueUpdated.Clear();

                foreach (var c in oldSubs)
                {
                    try
                    {
                        c.ValueUpdated -= Ch_ValueUpdated;
                        if (device.State == DeviceState.Connected)
                            await c.StopUpdatesAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"BLE StopUpdates ignored: {ex.Message}", ServerLogLevel.Debug);
                    }
                }

                await _streamProcessor.StopAsync().ConfigureAwait(false);
            }
            finally
            {
                if (device == _device)
                    _device = null;

                _disconnecting = false;
            }
        }
        #endregion
    }
}
