using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Licenses
{
    public interface ILicenseService
    {
        /// <summary>
        /// Проверка лицензиии устройства
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        Task<bool> CheckDeviceLicenseAsync(string deviceName);
    }
}
