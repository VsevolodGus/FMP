using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Server;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Sessions
{
    internal class SessionCleanupService
    {
        private readonly ILogger _serverLogger;
        private readonly AppDatabase _database;
        private readonly ServerHttpProvider _serverHttpProvider;
        

        private SessionInfo _currentSession;

        public SessionCleanupService(
            ILogger serverLogger,
            ServerHttpProvider serverHttpProvider,
            AppDatabase database)
        {
            _database = database;
            _serverLogger = serverLogger;
            _serverHttpProvider = serverHttpProvider;
        }

        public async Task RemoveOldSessionsAsync()
        {
            var sessionsToClose = await _database.SessionTable.ToArrayAsync();

            var closeTasks = sessionsToClose.Select(async session =>
            {
                try
                {
                    await _serverHttpProvider.SendAsync(new SessionExitRequest
                    {
                        SessionToken = session.Token,
                        SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                    });
                    await _database.Connection.DeleteAsync(session);
                }
                catch (Exception ex)
                {
                    var errorMessage = $"Not close session, request failed with error: {ex.Message}";
                    _serverLogger.Log(errorMessage, ServerLogLevel.CriticalFunctionalityError);
                }
            });

            await Task.WhenAll(closeTasks);
        }
    }
}
