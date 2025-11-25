using System.Threading.Tasks;
using Bioss.Ultrasound.Services.Logging;

namespace Bioss.Ultrasound.Services.Logging.Abstracts
{
    public interface ILogger
    {
        void Log(string message, ServerLogLevel logLevel = ServerLogLevel.Info);
        Task LogAsync(string message, ServerLogLevel logLevel = ServerLogLevel.Info);
    }


}
