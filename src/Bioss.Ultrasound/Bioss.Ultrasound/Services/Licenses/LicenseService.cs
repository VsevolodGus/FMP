using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Services.Sessions;
using System;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Licenses
{
    public sealed class LicenseService : ILicenseService
    {
        private readonly ILogger _logger;
        private readonly ISessionManager _sessionManager;
        private readonly ServerHttpProvider _serverHttpProvider;

        public LicenseService(
            ILogger logger,
            ISessionManager sessionManager,
            ServerHttpProvider serverHttpProvider)
        {
            _logger = logger;
            _sessionManager = sessionManager;
            _serverHttpProvider = serverHttpProvider;
        }

        public async Task<bool> CheckDeviceLicenseAsync(string deviceName)
        {
            try
            {
                var sessionInfo = await _sessionManager.GetCurrentSessionAsync();
                var result = await _serverHttpProvider.SendAsync(new LicenseCheckRequest
                {
                    SessionToken = sessionInfo.Token,
                    Message = deviceName,
                });

                return !string.IsNullOrEmpty(result);
            }
            catch(Exception ex)
            {
                _logger.Log($"Error in checking license device({deviceName}) by cause: {ex.Message}", ServerLogLevel.ServerError);
                return false;
            }
        }

    }
}
