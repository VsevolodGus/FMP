using Android;
using Bioss.Ultrasound.DependencyExtensions;
using System.Collections.Generic;
using Xamarin.Essentials;

namespace Bioss.Ultrasound.Droid.Extensions
{
    public class BLEPermission : Permissions.BasePlatformPermission, IPermission
    {
        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                var permissions = new List<(string, bool)>(4)
                {
                    (Manifest.Permission.AccessFineLocation, true),
                    (Manifest.Permission.AccessCoarseLocation, true)
                };

                if (DeviceInfo.Version.Major >= 12)
                {
                    // Для Android 12 и выше
                    permissions.Add((Manifest.Permission.BluetoothScan, true));
                    permissions.Add((Manifest.Permission.BluetoothConnect, true));
                }
                else
                {
                    // Для версий ниже Android 12
                    permissions.Add((Manifest.Permission.Bluetooth, false));
                    permissions.Add((Manifest.Permission.BluetoothAdmin, false));
                }
                return permissions.ToArray();
            }
        }
    }
}