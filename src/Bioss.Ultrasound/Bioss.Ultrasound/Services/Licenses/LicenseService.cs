using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Mapping;
using Bioss.Ultrasound.Network;
using Bioss.Ultrasound.Services.Logging.Abstracts;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Services.Sessions;
using System;
using System.Security.Cryptography;
using System.Text;
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

        public async Task<bool> CheckDeviceLicenseAsync(string deviceName)
        {
            var sessionInfo = await _sessionManager.GetCurrentSessionAsync();
            var requestData = new LicenseCheckRequest
            {
                SessionToken = sessionInfo.Token,
                SessionId = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                Message = Regex.Replace(deviceName, @"[^a-zA-Z0-9]", string.Empty).ToLower()
            };

            try
            {
                var hashServer =  await _serverHttpProvider.SendAsync(requestData);
                var hashLocal = CalculateMd5($"{ServerHttpConstants.UserAgent}{requestData.Message}{requestData.SessionId + 1}");
                return hashLocal.Equals(hashServer, StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                if(NetworkState.HasNetwork)
                {
                    await _database.Connection.InsertAsync(requestData.ToEntity());
                    return true;
                }
                
                return false;
            }
        }

        private string CalculateMd5(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);
            using MD5 md5 = MD5.Create();
            var hashBytes = md5.ComputeHash(inputBytes);
            
            var sb = new StringBuilder(32);
            for (int i = 0; i < hashBytes.Length; i++)
                sb.Append(hashBytes[i].ToString("x2"));

            return sb.ToString();
        }
    }
}
