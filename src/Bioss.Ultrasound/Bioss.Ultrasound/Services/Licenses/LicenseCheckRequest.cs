using Bioss.Ultrasound.Services.Server;
using Newtonsoft.Json;

namespace Bioss.Ultrasound.Services.Licenses
{
    public class LicenseCheckRequest : BaseRequest
    {

        [JsonProperty("lvl")]
        public byte Level => 254;

        [JsonProperty("msg")]
        public string Message { get; set; }
    }
}
