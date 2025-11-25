using Bioss.Ultrasound.Services.Network.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Network.Sessions
{
    internal class SessionCleanupService
    {
        // TODO читать из БД
        private ICollection<SessionInfo> _sessions = Array.Empty<SessionInfo>();


        private readonly ServerHttpProvider _serverHttpProvider;
        private readonly ILogger _serverLogger;

        private SessionInfo _currentSession;

        public SessionCleanupService(
            ILogger serverLogger,
            ServerHttpProvider serverHttpProvider)
        {
            _serverLogger = serverLogger;
            _serverHttpProvider = serverHttpProvider;
        }

        
        public async Task RemoveOldSessionsAsync()
        {
            var now = DateTimeOffset.Now;
            var sessionsToClose = _sessions.Where(c => c.LastActivityDate.AddMonths(1) < now).ToArray();

            var closeTasks = sessionsToClose.Select(async session =>
            {
                try
                {
                    _sessions.Remove(session);
                    await _serverHttpProvider.SendAsync(new SessionExitRequest
                    {
                        SessionToken = session.Token,
                    });
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
