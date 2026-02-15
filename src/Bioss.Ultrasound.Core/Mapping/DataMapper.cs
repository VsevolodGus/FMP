using Bioss.Ultrasound.Core.Data.Database.Entities;
using Bioss.Ultrasound.Core.Domain.Models;

namespace Bioss.Ultrasound.Core.Mapping;

public static class DataMapper
{
    public static FhrData ToFhr(this DataEntity entity)
    {
        return new FhrData
        {
            Id = entity.Id,
            Time = entity.Time,
            Fhr = entity.HeartRate,
            Toco = entity.Toco,
            RecordId = entity.RecordId
        };
    }

    public static DataEntity ToEntity(this FhrData model)
    {
        return new DataEntity
        {
            Id = model.Id,
            Time = model.Time,
            HeartRate = model.Fhr,
            Toco = model.Toco,
            RecordId = model.RecordId
        };
    }
}
