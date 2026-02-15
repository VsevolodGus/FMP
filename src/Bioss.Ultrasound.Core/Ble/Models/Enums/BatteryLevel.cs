namespace Bioss.Ultrasound.Core.Ble.Models.Enums;

public enum BatteryLevel
{
    // Device power status: 000 (0)
    Critical = 0,
    // Device power status: 001 (1)
    Bad,
    // Device power status: 010 (2)
    Normal,
    // Device power status: 011 (3)
    Good,
    // Device power status: 100 (4)
    Excellent
}
