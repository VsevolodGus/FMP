using Bioss.Ultrasound.Services.Server;
using Newtonsoft.Json;

namespace Bioss.Ultrasound.Services.Sessions
{
    public class SessionExitRequest : BaseRequest
    {
        [JsonProperty("lvl")]
        public override byte Level => 255;

        [JsonProperty("msg")]
        public override string Message => "exit";
    }
}
