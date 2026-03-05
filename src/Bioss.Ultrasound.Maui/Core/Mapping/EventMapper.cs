using Bioss.Ultrasound.Core.Data.Database.Entities;
using Bioss.Ultrasound.Core.Domain.Models;

namespace Bioss.Ultrasound.Core.Mapping;

public static class EventMapper
{
    public static FhrEvent ToFhrEvent(this EventEntity entity)
    {
        return new FhrEvent
        {
            Id = entity.Id,
            Time = entity.Time,
            Event = entity.Event,
            RecordId = entity.RecordId
        };
    }

    public static EventEntity ToEntity(this FhrEvent model)
    {
        return new EventEntity
        {
            Id = model.Id,
            Time = model.Time,
            Event = model.Event,
            RecordId = model.RecordId
        };
    }
}
