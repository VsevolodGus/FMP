using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Services.Server
{
    public sealed class ServerHttpProvider
    {
        private readonly HttpClient _httpClient;
        public ServerHttpProvider()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = ServerHttpConstants.Uri;
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Bipuls.v1");
            _httpClient.DefaultRequestHeaders.Add("Host", "dev.dxnich.ru");
        }

        public async Task<string> SendAsync<T>(T requestContent) where T : class
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, ServerHttpConstants.Uri);
            var json = JsonConvert.SerializeObject(requestContent);
            request.Content = new StringContent(json, Encoding.UTF8, ServerHttpConstants.JsonMediaType);

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
            using var request = new HttpRequestMessage(HttpMethod.Post, ServerHttpConstants.Uri);

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
