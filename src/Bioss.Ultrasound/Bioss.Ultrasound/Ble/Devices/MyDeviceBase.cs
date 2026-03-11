using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Collections;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Ble.Devices
{
    /// <summary>
    /// Вообще лучше сделать отдельную модель с очередью
    /// </summary>
    public abstract class MyDeviceBase
    {
        protected const int bufferSize = 1024;

        protected readonly ILogger _logger;
        protected readonly IAdapter _adapter;
        
        protected readonly ConcurrentQueue<BleSignal> _incomingQueue = new();
        protected RingBuffer<byte> _buffer = new RingBuffer<byte>(bufferSize);


        private CancellationTokenSource _consumerCts;
        private Task _consumerTask = Task.CompletedTask;


        public event EventHandler<Package> NewPackage;

        protected MyDeviceBase(ILogger logger, IAdapter adapter)
        {
            _logger = logger;
            _adapter = adapter;
        }

        protected void StartConsumer()
        {
            if (_consumerTask != null && !_consumerTask.IsCompleted)
                return;

            ResetConsumerState();

            _consumerCts?.Dispose();
            _consumerCts = new CancellationTokenSource();

            var token = _consumerCts.Token;
            _consumerTask = Task.Run(async () => await ConsumeLoopAsync(token), token);
        }

        protected async Task StopConsumerAsync()
        {         
            var cts = _consumerCts;
            var consumerTask = _consumerTask;
            _consumerCts = null;
            _consumerTask = Task.CompletedTask;
            

            if (cts != null)
            {
                try
                {
                    cts.Cancel();
                }
                catch
                {
                }

                try
                {
                    if (consumerTask != null)
                        await consumerTask.ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                    _logger.Log($"BLE consumer stop error: {ex}", ServerLogLevel.BluetoothError);
                }
                finally
                {
                    cts.Dispose();
                }
            }

            ResetConsumerState();
        }

        private async Task ConsumeLoopAsync(CancellationToken token)
        {
            try
            {
                var iterationCount = 0;
                while (!token.IsCancellationRequested)
                {
                    if (_incomingQueue.TryDequeue(out var signal))
                        ProcessChunk(signal);
                    else
                        await Task.Delay(10, token).ConfigureAwait(false);


                    iterationCount++;
                    if (iterationCount % 200 == 0)
                        _logger.Log($"Size ble_queue: {_incomingQueue.Count}");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.Log($"BLE consumer loop error: {ex}", ServerLogLevel.BluetoothError);
            }
        }

        private void ProcessChunk(BleSignal signal)
        {
            // TODO ускорить меденно
            var data = signal.Data;
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

            var package = Package.Init(packageData.AsSpan());
            if (!(package?.IsValid ?? false))
            {
                return;
            }

            try
            {
                NewPackage?.Invoke(this, package);
            }
            catch (Exception ex)
            {
                _logger.Log($"Proccess NewPackage with error: {ex}", ServerLogLevel.Warn);
            }
        }

        public void ResetConsumerState()
        {
            _incomingQueue.Clear();

            if (!_buffer.IsEmpty )
                _buffer = new RingBuffer<byte>(bufferSize);
        }
    }
}
