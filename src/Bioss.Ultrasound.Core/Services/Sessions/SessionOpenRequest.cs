using Newtonsoft.Json;

namespace Bioss.Ultrasound.Core.Services.Sessions;

public class SessionOpenRequest
{
    [JsonProperty("id")]
    public long SessionId => DateTimeOffset.Now.ToUnixTimeMilliseconds();

    [JsonProperty("sid")]
    public string TemporaryToken { get; set; }

    [JsonProperty("os")]
    public string DeviceOs { get; set; }

    [JsonProperty("dev")]
    public string DeviceModel { get; set; }

    [JsonProperty("po")]
    public string Version { get; set; }
}
