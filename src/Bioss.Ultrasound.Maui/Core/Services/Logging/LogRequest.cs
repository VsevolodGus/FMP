using Bioss.Ultrasound.Core.Services.Server;
using Newtonsoft.Json;

namespace Bioss.Ultrasound.Core.Services.Logging;

public class LogRequest : BaseRequest
{

    [JsonProperty("dev")]
    public string DeviceModel { get; set; }

    [JsonProperty("os")]
    public string DeviceOs { get; set; }
}
