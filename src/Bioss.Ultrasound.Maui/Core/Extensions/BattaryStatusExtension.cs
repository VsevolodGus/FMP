using Bioss.Ultrasound.Core.Ble.Models.Enums;

namespace Bioss.Ultrasound.Core.Extensions;

internal static class BattaryStatusExtension
{
    internal static byte GetDigitBatteryLevel(this BatteryLevel batteryLevel)
        => batteryLevel switch
        {
            BatteryLevel.Excellent => 100,
            BatteryLevel.Good => 75,
            BatteryLevel.Normal => 50,
            BatteryLevel.Bad => 25,
            BatteryLevel.Critical => 0,
            _ => 0,
        };
}
