using Newtonsoft.Json;

namespace Bioss.Ultrasound.Core.Services.Server;

public abstract class BaseRequest
{
    [JsonProperty("sid")]
    public string SessionToken { get; set; }
    [JsonProperty("id")]
    public long SessionId { get; set; }

    [JsonProperty("lvl")]
    public virtual byte Level { get; set; }

    [JsonProperty("msg")]
    public virtual string Message { get; set; }
}
