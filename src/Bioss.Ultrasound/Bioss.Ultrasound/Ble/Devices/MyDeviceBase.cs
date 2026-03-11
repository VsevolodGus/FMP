using Bioss.Ultrasound.Ble.ProtocolGenerations;
using Bioss.Ultrasound.Services;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Plugin.BLE;
using Plugin.BLE.Abstractions;
using Plugin.BLE.Abstractions.Contracts;

namespace Bioss.Ultrasound.Ble.Devices
{
    /// <summary>
    /// Эту модель лучше дополнить при адаптации под ios
    /// </summary>
    public abstract class MyDeviceBase
    {
        protected static readonly ConnectParameters connectParameters = new ConnectParameters(true, true);

        protected readonly ILogger _logger;
        protected readonly IAdapter _adapter;
        protected readonly DeviceStreamProcessor _streamProcessor;

        protected IDevice _device;
        protected IGeneration _guids;

        public bool IsConnected { get; protected set; }
        public string Name => _device?.Name;

        protected MyDeviceBase(
            ILogger logger, 
            DeviceStreamProcessor streamProcessor)
        {
            _logger = logger;
            _adapter = CrossBluetoothLE.Current.Adapter;
            _streamProcessor = streamProcessor;
        }
    }
}
