using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using System;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services
{
    public sealed class DeviceStreamProcessor
    {
        private readonly QueuePump<BleSignal> _queuePump;
        private readonly PacketAssembler _assembler;

        public Func<Package, Task> PackageReady { get; set; }

        public DeviceStreamProcessor(PacketAssembler assembler, ILogger logger)
        {
            _assembler = assembler;
            _queuePump = new QueuePump<BleSignal>(ConsumeAsync, logger);
        }

        public void Start()
        {
            _queuePump.Reset();
            _assembler.Reset();
            _queuePump.Start();
        }

        public void OnSignal(BleSignal data)
        {
            _queuePump.Enqueue(data);
        }

        public Task StopAsync()
        {
            _assembler.Reset();
            return _queuePump.StopAsync();
        }

        public void Reset()
        {
            _assembler.Reset();
            _queuePump.Reset();
        }


        private async Task ConsumeAsync(BleSignal data)
        {
            var package = _assembler.Process(data);
            if (PackageReady != null && package != null)
                await PackageReady(package);
        }
    }
}
