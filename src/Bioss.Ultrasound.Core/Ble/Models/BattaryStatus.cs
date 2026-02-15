using Bioss.Ultrasound.Core.Ble.Models.Enums;

namespace Bioss.Ultrasound.Core.Ble.Models;

public struct BattaryStatus
{
    public BattaryStatus(byte rawValue)
    {
        RawValue = rawValue;
    }

    public byte RawValue { get; }

    public BatteryLevel BatteryLevel
    {
        get
        {
            //  bits 2-0: Device power status
            //      000 - critical
            //      001 - bad
            //      010 - normal
            //      011 - good
            //      100 - excellent
            var level = RawValue & 0b0000_0111;
            return (BatteryLevel)level;
        }
    }

    public bool IsFHR1Include => (RawValue & 0b0001_0000) != 0;
    public bool IsFHR2Include => (RawValue & 0b0010_0000) != 0;
    public bool IsTocoInclude => (RawValue & 0b0100_0000) != 0;
    public bool IsAFMInclude => (RawValue & 0b1000_0000) != 0;
}
