namespace Bioss.Ultrasound.Ble.Models.Enums
{
    public enum SignalQuality
    {
        Unknown = 0,
        // FHR1 signal quality:  01 - bad signal
        Bad,
        // FHR1 signal quality: 10 - normal signal
        Normal,
        // FHR1 signal quality: 11 - good signal
        Good,
    }
}
