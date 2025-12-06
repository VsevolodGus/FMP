using System;
using System.Threading.Tasks;
using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Mapping;
using Bioss.Ultrasound.Services.Server;

namespace Bioss.Ultrasound.Services.Sessions
{
    internal class SessionManager : ISessionManager
    {
        private readonly AppDatabase _database;
        private readonly ServerHttpProvider _serverHttpProvider;

        private SessionInfo _currentSession;

        public SessionManager(AppDatabase database,
            ServerHttpProvider serverHttpProvider)
        {
            _database = database;
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
            try
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
                    CreatedDate = DateTime.UtcNow,
                };

                await _database.Connection.InsertAsync(_currentSession.ToEntity());
            }
            catch
            {
                throw;
            }
        }

        public async Task Exit(string token = null)
        {
            try
            {
                await _serverHttpProvider.SendAsync(new SessionExitRequest
                {
                    SessionToken = token ?? _currentSession.Token,
                    SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                });

                await _database.Connection.DeleteAsync(_currentSession.ToEntity());
            }
            catch
            {
                throw;
            }
        }
    }
}
