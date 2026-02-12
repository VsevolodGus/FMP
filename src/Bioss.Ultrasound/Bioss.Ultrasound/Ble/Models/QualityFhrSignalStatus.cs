using Bioss.Ultrasound.Ble.Models.Enums;

namespace Bioss.Ultrasound.Ble.Models
{
    public struct QualityFhrSignalStatus
    {
        public QualityFhrSignalStatus(byte rawValue)
        {
            RawValue = rawValue;
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
                var quality = RawValue & 0b0000_0011;
                return (SignalQuality)quality;
            }
        }

        //  bits 2:
        //      1 - means auto identify one fetal movement;
        //      0 - not
        public bool AutoFetalMovement => (RawValue & 0b0000_0100) != 0;
    }
}
