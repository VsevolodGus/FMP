using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Services.Server;
using Bioss.Ultrasound.Mapping;

namespace Bioss.Ultrasound.Test.Mapping;

public class LogMapperTests
{
    private sealed class TestRequest : BaseRequest
    {
    }

    [Fact]
    public void ToEntity_MapsBaseRequestToLogEntity()
    {
        var request = new TestRequest
        {
            Level = 2,
            Message = "message",
            SessionId = 123456
        };

        var result = request.ToEntity();

        Assert.Equal(request.Level, result.Level);
        Assert.Equal(request.Message, result.Message);
        Assert.Equal(request.SessionId, result.UnixDateTimeMs);
    }

    [Fact]
    public void ToLogRequest_MapsEntityTokenAndDeviceInformation()
    {
        var entity = new LogEntity
        {
            Level = 3,
            Message = "server-log",
            UnixDateTimeMs = 987654,    
        };

        var result = entity.ToLogRequest("token-1", "deviceModel", "deviceOs");

        Assert.Equal(entity.Level, result.Level);
        Assert.Equal(entity.Message, result.Message);
        Assert.Equal(entity.UnixDateTimeMs, result.SessionId);
        Assert.Equal("token-1", result.SessionToken);
        Assert.Equal("deviceModel", result.DeviceModel);
        Assert.Equal("deviceOs", result.DeviceOs);
    }
}
