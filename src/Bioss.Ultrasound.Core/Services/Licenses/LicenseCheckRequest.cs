using Bioss.Ultrasound.Core.Services.Server;
using Newtonsoft.Json;

namespace Bioss.Ultrasound.Core.Services.Licenses;

public class LicenseCheckRequest : BaseRequest
{

    [JsonProperty("lvl")]
    public override byte Level => 254;

    [JsonProperty("msg")]
    public override string Message { get; set; }

    internal object ToEntity()
    {
        throw new NotImplementedException();
    }
}
