using Bioss.Ultrasound.Ble.Models.Enums;
using Bioss.Ultrasound.Extensions;
using System.Collections;

namespace Bioss.Ultrasound.Ble.Models
{
    public class Status1
    {
        private BitArray _rawBits;

        public Status1(byte rawValue)
        {
            RawValue = rawValue;
            _rawBits = new BitArray(new byte[] { RawValue });
        }

        public byte RawValue { get;}

        public SignalQuality SignalQuality
        {
            get
            {
                //  bits 1-0: FHR1 signal quality
                //      01 - bad signal
                //      10 - normal signal
                //      11 - good signal
                var array = new BitArray(new bool[] { _rawBits[0], _rawBits[1], false, false, false, false, false, false });
                return (SignalQuality)array.ConvertToByte();
            }
        }

        //  bits 2:
        //      1 - means auto identify one fetal movement;
        //      0 - not
        public bool AutoFetalMovement => _rawBits[2];
    }
}
