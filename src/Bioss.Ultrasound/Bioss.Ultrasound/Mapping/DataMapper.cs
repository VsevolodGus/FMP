using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Domain.Models;

namespace Bioss.Ultrasound.Mapping
{
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
}
