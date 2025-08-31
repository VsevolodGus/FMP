using System.Linq;
using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Domain.Models;

namespace Bioss.Ultrasound.Mapping
{
    public static class AudioMapper
    {
        public static Audio ToAudio(this AudioEntity entity)
        {
            return new Audio
            {
                Id = entity.Id,
                Sound = AudioEntity.ToShorts(entity.Raw).ToList(),
                RecordId = entity.RecordId,
            };
        }

        public static AudioEntity ToEntity(this Audio model)
        {
            return new AudioEntity()
            {
                Id = model.Id,
                Raw = AudioEntity.ToBytes(model.Sound.ToArray()),
                RecordId = model.RecordId,
            };
        }
    }
}
