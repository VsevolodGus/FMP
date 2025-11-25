using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Network
{
    public struct HttpServerConstants
    {
        public static readonly Uri Uri = new Uri("https://dev.dxnich.ru/");
        public const string JsonMediaType = "application/json";
    }

    internal class ServerHttpProvider
    {
        private readonly HttpClient _httpClient;
        public ServerHttpProvider()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = HttpServerConstants.Uri;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Bipuls.v1");
            _httpClient.DefaultRequestHeaders.Add("Host", "dev.dxnich.ru");
        }

        public async Task<string> SendAsync<T>(T requestContent) where T : class
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, HttpServerConstants.Uri);
            var json = JsonConvert.SerializeObject(requestContent);
            request.Content = new StringContent(json, Encoding.UTF8, HttpServerConstants.JsonMediaType);

            try
            {
                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Not success request. With status code({response.StatusCode}) and content({response.Content})");

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> SendAsync()
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, HttpServerConstants.Uri);

            try
            {
                using var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Not success request. With status code({response.StatusCode}) and content({response.Content})");

                return await response.Content.ReadAsStringAsync();
            }
            catch
            {
                throw;
            }
        }
    }
}
