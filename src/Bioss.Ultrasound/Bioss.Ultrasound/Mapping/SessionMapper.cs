using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Services.Sessions;

namespace Bioss.Ultrasound.Mapping
{
    internal static class SessionMapper
    {

        internal static SessionEntity ToEntity(this SessionInfo sessionInfo)
                => new SessionEntity
                {
                    Token = sessionInfo.Token,
                    CreatedDate = sessionInfo.CreatedDate
                };

        internal static SessionInfo ToInfo(this SessionEntity entity)
          => new SessionInfo
          {
              Token = entity.Token,
              CreatedDate = entity.CreatedDate
          };
    }
}
