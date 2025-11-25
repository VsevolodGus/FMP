using System;
using System.Threading.Tasks;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Services.Server.Sessions;

namespace Bioss.Ultrasound.Services.Sessions
{
    internal class SessionManager : ISessionManager
    {
        private readonly ServerHttpProvider _serverHttpProvider;

        private SessionInfo _currentSession;

        public SessionManager(ServerHttpProvider serverHttpProvider)
        {
            _serverHttpProvider = serverHttpProvider;
        }

        public async ValueTask<SessionInfo> GetCurrentSessionAsync()
        {
            if (_currentSession is null)
                await StartSessionAsync();

            return _currentSession;
        }

        public async Task StartSessionAsync()
        {
            var tempraryToken = await _serverHttpProvider.SendAsync();
            var token = await _serverHttpProvider.SendAsync(new SessionOpenRequest()
            {
                TemporaryToken = tempraryToken,
                DeviceOs = DeviceInformation.DeviceOs,
                DeviceModel = DeviceInformation.DeviceModel,
            });

            _currentSession = new SessionInfo
            {
                Token = token,
                LastActivityDate = DateTimeOffset.UtcNow,
            };

            // TODO добавить сессию в БД
        }

        public async Task UpdateLastActivityAsync()
        {
            _currentSession.LastActivityDate = DateTimeOffset.UtcNow;
            await Task.Delay(100);
            // TODO обновить время в БД
        }
    }
}
