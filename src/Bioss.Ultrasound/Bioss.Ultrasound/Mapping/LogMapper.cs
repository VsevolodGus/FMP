using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Services.Logging;

namespace Bioss.Ultrasound.Mapping
{
    internal static class LogMapper
    {
        internal static LogEntity ToEntity(this LogRequest logRequest)
            => new LogEntity
            {
                Level = logRequest.Level,
                Message = logRequest.Message,
                SessionToken = logRequest.SessionToken,
            };

        internal static LogRequest ToRequest(this LogEntity entity)
          => new LogRequest
          {
              Level = entity.Level,
              Message = entity.Message,
              SessionToken = entity.SessionToken,
              DeviceModel = DeviceInformation.DeviceModel,
              DeviceOs = DeviceInformation.DeviceOs,
          };
    }
}
