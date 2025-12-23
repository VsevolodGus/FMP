using Xamarin.Essentials;

namespace Bioss.Ultrasound
{
    internal static class DeviceInformation
    {
        /// <summary>
        /// Модель устройства
        /// </summary>
        public static readonly string DeviceModel = DeviceInfo.Model;
        /// <summary>
        /// Версия OC устройства
        /// </summary>
        public static readonly string DeviceOs = $"{DeviceInfo.Platform}: {DeviceInfo.VersionString}";
    }
}
