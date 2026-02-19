using Bioss.Ultrasound.Core.Data.Database;
using Bioss.Ultrasound.Core.Mapping;
using Bioss.Ultrasound.Core.Services.Logging.Abstracts;
using Bioss.Ultrasound.Core.Services.Server;
using Bioss.Ultrasound.Core.Services.Sessions;

namespace Bioss.Ultrasound.Core.Services.Logging;

public class ServerLogger : ILogger, IUnsentLogDispatcher
{
    private readonly ISessionManager _sessionManager;
    private readonly ServerHttpProvider _serverHttpProvider;
    private readonly AppDatabase _database;

    public ServerLogger(
        ISessionManager sessionTokenProvider,
        ServerHttpProvider serverHttpProvider,
        AppDatabase database)
    {
        _sessionManager = sessionTokenProvider;
        _serverHttpProvider = serverHttpProvider;
        _database = database;
    }


    public async void Log(string message, ServerLogLevel logLevel = ServerLogLevel.Info)
    {
        await LogAsync(message, logLevel);
    }

    public async Task LogAsync(string message, ServerLogLevel logLevel = ServerLogLevel.Info)
    {
        var sessionInfo = await _sessionManager.GetCurrentSessionAsync();
        await LogAsync(message, sessionInfo.Token, logLevel);
    }

    private async Task LogAsync(string message, string token, ServerLogLevel logLevel = ServerLogLevel.Info)
    {
        var logData = new LogRequest
        {
            SessionToken = token,
            DeviceModel = DeviceInformation.DeviceModel,
            DeviceOs = DeviceInformation.DeviceOs,
            SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
            Level = (byte)logLevel,
            Message = message
        };

        try
        {
            await _serverHttpProvider.SendAsync(logData);
        }
        catch
        {
            await _database.Connection.InsertAsync(logData.ToEntity());
        }
    }

    /// <summary>
    /// Отправление всех логов, что до этого не могли отправить
    /// </summary>
    /// <returns></returns>
    public async Task SendAllUnsentAsync(bool sendLogCurrentSession = true)
    {
        var logToSend = await _database.LogTable.ToArrayAsync();

        if (logToSend is null || logToSend.Length == 0)
            return;

        var sessionInfo = sendLogCurrentSession
            ? await _sessionManager.GetCurrentSessionAsync()
            : await _sessionManager.StartSessionAsync();

        if (!sendLogCurrentSession)
            await LogAsync("Отправляем логи с прошлых сессий", sessionInfo.Token);

        var sendLogTasks = logToSend.Select(async logData =>
        {
            try
            {
                await _serverHttpProvider.SendAsync(logData.ToLogRequest(sessionInfo.Token));
                await _database.Connection.DeleteAsync(logData);
            }
            catch
            {
                await _database.Connection.InsertAsync(logData);
            }
        });

        await Task.WhenAll(sendLogTasks);
        if (!sendLogCurrentSession)
        {
            await LogAsync("Завершили отправку логов", sessionInfo.Token);
            await _sessionManager.Exit(sessionInfo.Token);
        }
    }
}
