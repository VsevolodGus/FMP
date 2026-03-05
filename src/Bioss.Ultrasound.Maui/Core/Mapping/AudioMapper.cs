using Bioss.Ultrasound.Core.Data.Database.Entities;
using Bioss.Ultrasound.Core.Domain.Models;

namespace Bioss.Ultrasound.Core.Mapping;

public static class AudioMapper
{
    public static Audio ToAudio(this AudioEntity entity)
    {
        return new Audio
        {
            Id = entity.Id,
            Sound = entity.Raw is not null
                ? [.. AudioEntity.ToShorts(entity.Raw)]
                : new List<short>(),
            RecordId = entity.RecordId,
        };
    }

    public static AudioEntity ToEntity(this Audio model)
    {
        return new AudioEntity()
        {
            Id = model.Id,
            Raw = model.Sound is not null
                ? AudioEntity.ToBytes(model.Sound.ToArray())
                : Array.Empty<byte>(),
            RecordId = model.RecordId,
        };
    }
}
