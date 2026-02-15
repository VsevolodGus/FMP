namespace Bioss.Ultrasound.Core.Network;

public static class NetworkState
{
    public static bool HasNetwork => Connectivity.NetworkAccess == NetworkAccess.Internet;
    public static bool AccessNetwork(NetworkAccess networkAccess) => networkAccess == NetworkAccess.Internet;
}
