namespace Bioss.Ultrasound.Core.DependencyExtensions;

public interface IPermission
{
    Task<PermissionStatus> CheckStatusAsync();
    Task<PermissionStatus> RequestAsync();
}
