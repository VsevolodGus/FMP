using Bioss.Ultrasound.DependencyExtensions;
using CoreBluetooth;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Bioss.Ultrasound.iOS.Extensions
{
    public partial class BLEPermission : Permissions.BasePlatformPermission, IPermission
    {
        protected override Func<IEnumerable<string>> RequiredInfoPlistKeys =>
                () => new string[] { "NSBluetoothAlwaysUsageDescription" };

        public override Task<PermissionStatus> CheckStatusAsync()
        {
            EnsureDeclared();

            return Task.FromResult(CheckPermissionsStatus());
        }

        public override async Task<PermissionStatus> RequestAsync()
        {
            EnsureDeclared();

            var status = CheckPermissionsStatus();
            if (status == PermissionStatus.Granted)
                return status;

            EnsureMainThread();

            return await RequestPermissionAsync();
        }

        internal void EnsureMainThread()
        {
            if (!MainThread.IsMainThread)
                throw new PermissionException("Permission request must be invoked on main thread.");
        }

        internal PermissionStatus CheckPermissionsStatus()
        {
            var status = CBManager.Authorization;
            switch (CBCentralManager.Authorization)
            {
                case CBManagerAuthorization.AllowedAlways:
                    return PermissionStatus.Granted;
                case CBManagerAuthorization.Restricted:
                    return PermissionStatus.Restricted;
                case CBManagerAuthorization.NotDetermined:
                    return PermissionStatus.Unknown;
                default:
                    return PermissionStatus.Denied;
            }
        }

        internal async Task<PermissionStatus> RequestPermissionAsync()
        {
            // Initializing CBCentralManager will present the Bluetooth permission dialog.
            var manager = new CBCentralManager();
            PermissionStatus status;
            try
            {
                do
                {
                    status = CheckPermissionsStatus();
                    await Task.Delay(200);
                } while (status == PermissionStatus.Unknown);
                manager.Dispose();
                return status;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Unable to get Bluetooth LE permission: " + ex);
                return PermissionStatus.Unknown;
            }
        }
    }
}