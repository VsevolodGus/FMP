using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Licenses
{
    public interface ILicenseService
    {
        Task CheckDeviceLicenseAsync(string deviceName);
    }
}
