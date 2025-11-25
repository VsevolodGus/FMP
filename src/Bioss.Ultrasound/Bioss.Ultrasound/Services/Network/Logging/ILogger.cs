using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Network.Logging
{
    public interface ILogger
    {
        void Log(string message, ServerLogLevel logLevel = ServerLogLevel.Info);
        Task LogAsync(string message, ServerLogLevel logLevel = ServerLogLevel.Info);
    }
}
