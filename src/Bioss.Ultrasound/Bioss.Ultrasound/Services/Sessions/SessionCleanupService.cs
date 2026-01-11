using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
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

        public SessionCleanupService(
            ILogger serverLogger,
            ISessionManager sessionManager,
            AppDatabase database)
        {
            _serverLogger = serverLogger;
            _sessionManager = sessionManager;

            _database = database;
        }

        public async Task RemoveOldSessionsAsync()
        {
            var sessionsToClose = await _database.SessionTable.ToArrayAsync();

            var currentSession = await _sessionManager.GetCurrentSessionAsync();
            var closeTasks = sessionsToClose
                .Where(c => c.Token != currentSession.Token)
                .Select(async session => await Exit(session));

            await Task.WhenAll(closeTasks);
        }

        private async ValueTask Exit(SessionEntity session)
        {
            try
            {
                await _sessionManager.Exit(session);
            }
            catch (Exception ex)
            {
                var errorMessage = $"Not close session, request failed with error: {ex.Message}";
                _serverLogger.Log(errorMessage, ServerLogLevel.ServerError);
                await _database.Connection.DeleteAsync(session);
            }
        }
    }
}
