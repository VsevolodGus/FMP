using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Mapping;

namespace Bioss.Ultrasound.Test.Mapping;

public class EventMapperTests
{
    [Fact]
    public void ToFhrEvent_MapsAllProperties()
    {
        var time = new DateTime(2026, 3, 14, 10, 0, 0, DateTimeKind.Utc);
        var entity = new EventEntity
        {
            Id = 3,
            Time = time,
            Event = Events.FetalMovement,
            RecordId = 7
        };

        var result = entity.ToFhrEvent();

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Time, result.Time);
        Assert.Equal(entity.Event, result.Event);
        Assert.Equal(entity.RecordId, result.RecordId);
    }

    [Fact]
    public void ToEntity_MapsAllProperties()
    {
        var time = new DateTime(2026, 3, 14, 10, 0, 0, DateTimeKind.Utc);
        var model = new FhrEvent
        {
            Id = 3,
            Time = time,
            Event = Events.FetalMovement,
            RecordId = 7
        };

        var result = model.ToEntity();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.Time, result.Time);
        Assert.Equal(model.Event, result.Event);
        Assert.Equal(model.RecordId, result.RecordId);
    }
}
