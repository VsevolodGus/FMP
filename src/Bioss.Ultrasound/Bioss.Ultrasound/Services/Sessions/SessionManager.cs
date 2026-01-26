using System;
using System.Threading.Tasks;
using Bioss.Ultrasound.Services.Server;

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
            _currentSession ??= await StartSessionAsync();
            return _currentSession;
        }

        public async Task<SessionInfo> StartSessionAsync()
        {
            try
            {
                var tempraryToken = await _serverHttpProvider.SendAsync();
                var token = await _serverHttpProvider.SendAsync(new SessionOpenRequest()
                {
                    TemporaryToken = tempraryToken,
                    DeviceOs = DeviceInformation.DeviceOs,
                    DeviceModel = DeviceInformation.DeviceModel,
                    Version = Constants.AppVersion
                });

                return new SessionInfo
                {
                    Token = token,
                    CreatedDate = DateTime.UtcNow,
                };
            }
            catch
            {
                throw; 
            }
        }

        public async Task Exit(string token)
        {
            try
            {
                await _serverHttpProvider.SendAsync(new SessionExitRequest
                {
                    SessionToken = token,
                    SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                });
            }
            catch
            {
                throw;
            }
        }
    }
}
