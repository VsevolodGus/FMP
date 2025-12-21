using Xamarin.Essentials;

namespace Bioss.Ultrasound
{
    internal static class DeviceInformation
    {
        public static readonly string DeviceModel = DeviceInfo.Model;
        public static readonly string DeviceOs = $"{DeviceInfo.Platform}: {DeviceInfo.VersionString}";
    }
}
