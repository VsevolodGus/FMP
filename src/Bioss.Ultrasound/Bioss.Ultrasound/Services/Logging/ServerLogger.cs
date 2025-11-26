using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Services.Sessions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Logging
{
    internal class ServerLogger : ILogger, IUnsentLogDispatcher
    {
        private readonly ISessionManager _sessionManager;
        private readonly ServerHttpProvider _serverHttpProvider;

        // TODO заменить на БД
        private ICollection<LogRequest> unsentLogs = new List<LogRequest>();

        public ServerLogger(
            ISessionManager sessionTokenProvider,
            ServerHttpProvider serverHttpProvider)
        {
            _sessionManager = sessionTokenProvider;
            _serverHttpProvider = serverHttpProvider;
        }


        public async void Log(string message, ServerLogLevel logLevel = ServerLogLevel.Info)
        {
            await LogAsync(message, logLevel);
        }

        public async Task LogAsync(string message, ServerLogLevel logLevel = ServerLogLevel.Info)
        {
            var sessionInfo = await _sessionManager.GetCurrentSessionAsync();
            var logData = new LogRequest
            {
                SessionToken = sessionInfo.Token,
                DeviceModel = DeviceInformation.DeviceModel,
                DeviceOs = DeviceInformation.DeviceOs,
                Level = (byte)logLevel,
                Message = message
            };

            try
            {
                await _serverHttpProvider.SendAsync(logData);
            }
            catch
            {
                unsentLogs.Add(logData);
            }
            await _sessionManager.UpdateLastActivityAsync();
        }

        public async Task SendAllUnsentAsync()
        {
            var logsToSend = unsentLogs.ToArray();
            unsentLogs.Clear();
            var sendLogTasks = logsToSend.Select(async logData =>
            {
                try
                {
                    await _serverHttpProvider.SendAsync(logData);
                }
                catch
                {
                    unsentLogs.Add(logData);
                }
            });

            await Task.WhenAll(sendLogTasks);
        }
    }
}
