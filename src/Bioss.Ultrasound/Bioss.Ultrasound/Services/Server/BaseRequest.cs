using Newtonsoft.Json;
using System;

namespace Bioss.Ultrasound.Services.Server
{
    public abstract class BaseRequest
    {
        [JsonProperty("sid")]
        public string SessionToken { get; set; }
        [JsonProperty("id")]
        public long SessionId => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    }
}
