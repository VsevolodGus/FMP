namespace Bioss.Ultrasound.Core.Services.Logging.Abstracts;

public interface ILogger
{
    void Log(string message, ServerLogLevel logLevel = ServerLogLevel.Info);
    Task LogAsync(string message, ServerLogLevel logLevel = ServerLogLevel.Info);
}
