using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Network.Sessions
{
    public interface ISessionManager
    {
        ValueTask<SessionInfo> GetCurrentSessionAsync();

        Task StartSessionAsync();

        Task UpdateLastActivityAsync();
    }
}
