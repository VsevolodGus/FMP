using Bioss.Ultrasound.Services.Server;
using Newtonsoft.Json;

namespace Bioss.Ultrasound.Services.Sessions
{
    public class SessionExitRequest : BaseRequest
    {

        [JsonProperty("lvl")]
        public byte Level => 255;

        [JsonProperty("msg")]
        public string Message => "exit";
    }
}
