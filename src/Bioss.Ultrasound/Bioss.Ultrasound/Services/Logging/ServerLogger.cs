using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Mapping;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Services.Sessions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Logging
{
    internal class ServerLogger : ILogger, IUnsentLogDispatcher
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
            var logData = new LogRequest
            {
                SessionToken = sessionInfo.Token,
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

        public async Task SendAllUnsentAsync()
        {
            var logToSend = await _database.LogTable.ToArrayAsync();
            
            var sendLogTasks = logToSend.Select(async logData =>
            {
                try
                {
                    await _serverHttpProvider.SendAsync(logData.ToLogRequest());
                    await _database.Connection.DeleteAsync(logData);
                }
                catch
                {
                    await _database.Connection.InsertAsync(logData);
                }
            });

            await Task.WhenAll(sendLogTasks);
        }
    }
}
