using Bioss.Ultrasound.Ble.Models.Enums;
using Bioss.Ultrasound.Extensions;
using System.Collections;

namespace Bioss.Ultrasound.Ble.Models
{
    public class Status2
    {
        private BitArray _rawBits;

        public Status2(byte rawValue)
        {
            RawValue = rawValue;
            _rawBits = new BitArray(new byte[] { RawValue });
        }

        public byte RawValue { get; }

        public BatteryLevel BatteryLevel
        {
            get
            {
                // TODO подумать чтобы избавить от этого двойного выделения массивов
                //  bits 2-0: Device power status
                //      000 - critical
                //      001 - bad
                //      010 - normal
                //      011 - good
                //      100 - excellent
                var array = new BitArray(new bool[] { _rawBits[0], _rawBits[1], _rawBits[2], false, false, false, false, false });
                return (BatteryLevel)array.ConvertToByte();
            }
        }

        public bool IsFHR1Include => _rawBits[4];
        public bool IsFHR2Include => _rawBits[5];
        public bool IsTocoInclude => _rawBits[6];
        public bool IsAFMInclude => _rawBits[7];
    }
}
