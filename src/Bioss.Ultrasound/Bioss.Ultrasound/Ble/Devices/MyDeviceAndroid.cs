using Bioss.Ultrasound.Ble.Commands;
using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Ble.ProtocolGenerations;
using Bioss.Ultrasound.Collections;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Plugin.BLE;
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
    public class MyDeviceAndroid : IMyDevice
    {
        private const int sizeBuffer = 1024;
        private static readonly ConnectParameters connectParameters = new ConnectParameters(true, true);

        private readonly ILogger _logger;
        private readonly IAdapter _adapter;
        private IDevice _device;
        private readonly HashSet<ICharacteristic> _subscribedToValueUpdated = new();
        private IGeneration _guids;
        private bool _disconnecting;

        private RingBuffer<byte> _buffer = new RingBuffer<byte>(sizeBuffer);

        public MyDeviceAndroid(ILogger logger)
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

        private async void OnConnected(object sender, DeviceEventArgs e)
        {
            try
            {
                if (_disconnecting || e.Device != _device)
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

        private readonly static int SoundLen = SoundPackage.DataLength;              // 107
        private readonly static int FhrLen = FHRPackage.DataLength;                // 10
        private readonly static int FullLen = SoundPackage.DataLength + FHRPackage.DataLength; // 117

        private void Ch_ValueUpdated(object sender, CharacteristicUpdatedEventArgs e)
        {
            if (_guids == null || !IsConnected)
                return;

            if (e.Characteristic.Id != _guids.ChCustomRead)
                return;

            var data = e.Characteristic.Value;
            if (data == null || data.Length == 0)
                return;

            // всегда в буфер, чтобы не ломать порядок
            _buffer.Push(data);

            // парсим буфер последовательно
            ParseBuffer();
        }

        private void ParseBuffer()
        {
            // рабочий буфер на стеке, без GC
            Span<byte> scratch = stackalloc byte[FullLen];

            while (_buffer.Count >= 3)
            {
                // 1) синхронизация по заголовку sound: 55 AA 09
                if (!IsSoundHeaderAtBufferHead())
                {
                    _buffer.Pop(); // выкидываем 1 байт и ищем дальше
                    continue;
                }

                // 2) ждём минимум soundLen
                if (_buffer.Count < SoundLen)
                    return;

                // 3) определяем, есть ли прицепленный FHR сразу после sound
                var len = SoundLen;

                if (_buffer.Count >= FullLen && IsFhrHeaderAtOffset(SoundLen))
                    len = FullLen;

                if (_buffer.Count < len)
                    return;

                // 4) снимаем ровно один кадр без аллокаций
                if (!_buffer.TryPopInto(scratch[..len], len))
                    return;

                // 5) парсим и отдаём строго по порядку
                ProcessFrame(scratch[..len]);
            }
        }

        // TODO вынести отсюда
        private bool IsSoundHeaderAtBufferHead()
        {
            return _buffer.Peek(0) == 0x55
                && _buffer.Peek(1) == 0xAA
                && _buffer.Peek(2) == 0x09;
        }

        // TODO вынести отсюда
        private bool IsFhrHeaderAtOffset(int offset)
        {
            return _buffer.Peek(offset + 0) == 0x55
                && _buffer.Peek(offset + 1) == 0xAA
                && _buffer.Peek(offset + 2) == 0x03;
        }

        private void ProcessFrame(ReadOnlySpan<byte> frame)
        {
            var package = Package.Init(frame);
            if (package?.IsValid != true)
                return;

            try
            {
                NewPackage?.Invoke(this, package);
            }
            catch (Exception ex)
            {
                _logger.Log($"Proccess NewPackage with error: {ex}", ServerLogLevel.Warn);
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
            } catch { }
        }

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
                _buffer = new RingBuffer<byte>(sizeBuffer);

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
            }
            finally
            {
                if (device == _device)
                    _device = null;

                _disconnecting = false;
            }
        }

        private void DebugWriteLine(string message)
        {
            //Debug.WriteLine($"MyDevice Android: {message}");
        }
    }
}
