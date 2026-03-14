using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Services.Logging;
using Bioss.Ultrasound.Services.Server;

namespace Bioss.Ultrasound.Mapping
{
    public static class LogMapper
    {
        public static LogEntity ToEntity(this BaseRequest logRequest)
            => new LogEntity
            {
                Level = logRequest.Level,
                Message = logRequest.Message,
                UnixDateTimeMs = logRequest.SessionId,
            };

        public static LogRequest ToLogRequest(this LogEntity entity, string token, string deviceModel = null, string deviceOs = null)
          => new LogRequest
          {
              Level = entity.Level,
              Message = entity.Message,
              SessionToken = token,
              SessionId = entity.UnixDateTimeMs,

              DeviceModel = deviceModel ?? DeviceInformation.DeviceModel,
              DeviceOs = deviceOs ??  DeviceInformation.DeviceOs,
          };
    }
}
