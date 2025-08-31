using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Bioss.Ultrasound.DependencyExtensions
{
    public interface IPermission
    {
        Task<PermissionStatus> CheckStatusAsync();
        Task<PermissionStatus> RequestAsync();
    }
}
