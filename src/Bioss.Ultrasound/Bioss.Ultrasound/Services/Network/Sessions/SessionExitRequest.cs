using Newtonsoft.Json;

namespace Bioss.Ultrasound.Services.Network.Sessions
{
    public class SessionExitRequest : BaseRequest
    {

        [JsonProperty("lvl")]
        public byte Level => 255;

        [JsonProperty("msg")]
        public string Message => "exit";
    }
}
