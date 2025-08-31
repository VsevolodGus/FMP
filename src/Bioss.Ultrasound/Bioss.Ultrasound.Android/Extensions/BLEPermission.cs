using Android;
using Bioss.Ultrasound.DependencyExtensions;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Bioss.Ultrasound.Droid.Extensions
{
    public class BLEPermission : Permissions.BasePlatformPermission, IPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
            new (string, bool)[]
            {
                (Manifest.Permission.AccessCoarseLocation, true),
                (Manifest.Permission.AccessFineLocation, true),
                (Manifest.Permission.Bluetooth, false),
                (Manifest.Permission.BluetoothAdmin, false)
            };

        public override Task<PermissionStatus> CheckStatusAsync()
        {
            return base.CheckStatusAsync();
        }
    }
}