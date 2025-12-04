using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Sessions
{
    public interface ISessionManager
    {
        ValueTask<SessionInfo> GetCurrentSessionAsync();

        Task StartSessionAsync();
    }
}
