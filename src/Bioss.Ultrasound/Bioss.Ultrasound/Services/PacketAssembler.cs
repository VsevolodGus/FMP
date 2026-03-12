using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Collections;
using System;

namespace Bioss.Ultrasound.Services
{
    public sealed class PacketAssembler
    {
        private const int bufferSize = 1024;
        private RingBuffer<byte> _buffer = new(bufferSize);

        public Package Process(BleSignal item)
        {
            if (item is null || item.Data is null || item.Data.Length == 0)
                return null;

            var chunk = item.Data;
            if (!Package.IsStart(chunk))
            {
                _buffer.Push(chunk);
                return null;
            }

            var packageData = _buffer.Pop(_buffer.Count);
            _buffer.Push(chunk);

            if (packageData is null || packageData.Length == 0)
                return null;

            var package = Package.Init(packageData.AsSpan());
            return package?.IsValid == true ? package : null;
        }

        public void Reset()
        {
            if (!_buffer.IsEmpty)
                _buffer = new(bufferSize);
        }
    }
}
