using Bioss.Ultrasound.Services.Server;
using Newtonsoft.Json;

namespace Bioss.Ultrasound.Services.Licenses
{
    public class LicenseCheckRequest : BaseRequest
    {

        [JsonProperty("lvl")]
        public override byte Level => 254;

        [JsonProperty("msg")]
        public override string Message { get; set; }
    }
}
