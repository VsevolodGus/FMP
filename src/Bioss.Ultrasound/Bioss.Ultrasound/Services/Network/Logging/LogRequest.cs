using Newtonsoft.Json;

namespace Bioss.Ultrasound.Services.Network.Logging
{
    public class LogRequest : BaseRequest
    {

        [JsonProperty("dev")]
        public string DeviceModel { get; set; }

        [JsonProperty("os")]
        public string DeviceOs { get; set; }

        [JsonProperty("lvl")]
        public byte Level { get; set; }

        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
