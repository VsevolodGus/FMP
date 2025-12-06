using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Mapping;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Services.Sessions;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Licenses
{
    public sealed class LicenseService : ILicenseService
    {
        private readonly ILogger _logger;
        private readonly ISessionManager _sessionManager;

        private readonly AppDatabase _database;
        private readonly ServerHttpProvider _serverHttpProvider;

        public LicenseService(
            ILogger logger,
            ISessionManager sessionManager,
            AppDatabase database,
            ServerHttpProvider serverHttpProvider)
        {
            _logger = logger;
            _sessionManager = sessionManager;

            _database = database;
            _serverHttpProvider = serverHttpProvider;
        }

        public async Task CheckDeviceLicenseAsync(string deviceName)
        {
            var sessionInfo = await _sessionManager.GetCurrentSessionAsync();
            var logData = new LicenseCheckRequest
            {
                SessionToken = sessionInfo.Token,
                SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Message = Regex.Replace(deviceName, @"[^a-zA-Z0-9]", string.Empty).ToLower()
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
    }
}
