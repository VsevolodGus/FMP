using Bioss.Ultrasound.Core.Data.Database.Entities;
using Bioss.Ultrasound.Core.Services.Logging;
using Bioss.Ultrasound.Core.Services.Server;

namespace Bioss.Ultrasound.Core.Mapping
{
    internal static class LogMapper
    {
        internal static LogEntity ToEntity(this BaseRequest logRequest)
            => new LogEntity
            {
                Level = logRequest.Level,
                Message = logRequest.Message,
                UnixDateTimeMs = logRequest.SessionId,
            };

        internal static LogRequest ToLogRequest(this LogEntity entity, string token)
          => new LogRequest
          {
              Level = entity.Level,
              Message = entity.Message,
              SessionToken = token,
              SessionId = entity.UnixDateTimeMs,

              DeviceModel = DeviceInformation.DeviceModel,
              DeviceOs = DeviceInformation.DeviceOs,
          };
    }
}
