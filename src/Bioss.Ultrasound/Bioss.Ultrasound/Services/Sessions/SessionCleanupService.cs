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
        private readonly ISessionManager _sessionManager;
        private readonly AppDatabase _database;
        private readonly ServerHttpProvider _serverHttpProvider;

        public SessionCleanupService(
            ILogger serverLogger,
            ISessionManager sessionManager,
            AppDatabase database,
            ServerHttpProvider serverHttpProvider)
        {
            _serverLogger = serverLogger;
            _sessionManager = sessionManager;

            _database = database;
            _serverHttpProvider = serverHttpProvider;
        }

        public async Task RemoveOldSessionsAsync()
        {
            var sessionsToClose = await _database.SessionTable.ToArrayAsync();
            
            var currentSession = await _sessionManager.GetCurrentSessionAsync();
            var closeTasks = sessionsToClose
                .Where(c=> c.Token != currentSession.Token)
                .Select(async session =>
            {
                try
                {
                    await _sessionManager.Exit(session.Token);
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
