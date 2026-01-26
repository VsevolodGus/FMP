using System.Linq;
using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Domain.Models;

namespace Bioss.Ultrasound.Mapping
{
    public static class RecordMapper
    {
        public static Record ToRecord(this RecordEntity entity)
        {
            return new Record
            {
                Id = entity.Id,
                StartTime = entity.StartTime,
                StopTime = entity.EndTime,
                DeviceSerialNumber = entity.DeviceSerialNumber,
                Fhrs = entity.Datas.Select(a => a.ToFhr()).ToList(),
                Audio = entity.Audio?.ToAudio(),
                Biometric = entity.Biometric?.ToBiometric(),
                Events = entity.Events.Select(a => a.ToFhrEvent()).ToList(),
                CardiotocographyInfo = null,
            };
        }

        public static RecordEntity ToEntity(this Record record)
        {
            return new RecordEntity
            {
                Id = record.Id,
                StartTime = record.StartTime,
                EndTime = record.StopTime,
                DeviceSerialNumber = record.DeviceSerialNumber,
                Audio = record.Audio.ToEntity(),
                Datas = record.Fhrs.Select(a => a.ToEntity()).ToList(),
                Events = record.Events.Select(a => a.ToEntity()).ToList(),
                Biometric = record.Biometric?.ToEntity()
            };
        }
    }
}
