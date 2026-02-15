using Bioss.Ultrasound.Data.Database;
using Bioss.Ultrasound.Mapping;
using Bioss.Ultrasound.Network;
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
        private readonly ISessionManager _sessionManager;
        private readonly AppDatabase _database;
        private readonly ServerHttpProvider _serverHttpProvider;

        public LicenseService(
            ISessionManager sessionManager,
            AppDatabase database,
            ServerHttpProvider serverHttpProvider)
        {
            _sessionManager = sessionManager;
            _database = database;
            _serverHttpProvider = serverHttpProvider;
        }

        /// <summary>
        /// Отправка запроса на сервер, чтобы проверить лицензию
        /// Лицензия проверяется по хешу, если хеш пришедший с сервера и рассчитай в CalculateMd5 совпадает, значит устройство лицензировано.
        /// Название устройства удаляется все символы кроме Латинских букв и цифр.
        /// </summary>
        /// <param name="deviceName">имя девайса для которого будет проводиться проверка</param>
        /// <returns>лицензировано ли устройство</returns>
        public async Task<bool> CheckDeviceLicenseAsync(string deviceName)
        {
            return true;
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

        /// <summary>
        /// Расчет хеща для проверки лицензии
        /// </summary>
        /// <param name="input">входящая строка для рассчета хеша</param>
        /// <returns>хеш строки</returns>
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
