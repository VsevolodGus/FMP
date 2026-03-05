using Newtonsoft.Json;
using System.Text;

namespace Bioss.Ultrasound.Core.Services.Server;

/// <summary>
/// Базовый класс для работы с сервером
/// Здесь определяются только правилоа работы с ним, использование каждого апи оперделяется в других классах, но вызов апи происходит через этот класс
/// </summary>
public sealed class ServerHttpProvider
{
    private readonly HttpClient _httpClient;
    public ServerHttpProvider()
    {
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = ServerHttpConstants.Uri;
        _httpClient.DefaultRequestHeaders.Add("User-Agent", ServerHttpConstants.UserAgent);
        _httpClient.DefaultRequestHeaders.Add("Host", "dev.dxnich.ru");
    }

    /// <summary>
    /// Базовая функция для обращения к серверу
    /// Сервер всегда отвечает обычной строкой, поэтому конвертация через поток не нужна
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="requestContent"></param>
    /// <returns>ответ сервера в виде строки</returns>
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
