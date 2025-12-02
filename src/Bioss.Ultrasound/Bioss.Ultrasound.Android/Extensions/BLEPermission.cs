using Android;
using Bioss.Ultrasound.DependencyExtensions;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Bioss.Ultrasound.Droid.Extensions
{
    public class BLEPermission : Permissions.BasePlatformPermission, IPermission
    {
        public override Task<PermissionStatus> CheckStatusAsync()
        {
            return base.CheckStatusAsync();
        }
    }
}