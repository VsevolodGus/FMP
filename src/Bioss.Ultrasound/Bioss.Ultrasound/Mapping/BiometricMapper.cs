using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Domain.Models;

namespace Bioss.Ultrasound.Mapping
{
    public static class BiometricMapper
    {
        public static Biometric ToBiometric(this BiometricEntity entity)
        {
            return new Biometric
            {
                Id = entity.Id,
                Comment = entity.Comment,
                Diastolic = entity.Diastolic,
                Systolic = entity.Systolic,
                Temperature = entity.Temperature,
                Pulse = entity.Pulse,
                Sugar = entity.Sugar,
                RecordId = entity.RecordId,
            };
        }

        public static BiometricEntity ToEntity(this Biometric model)
        {
            return new BiometricEntity()
            {
                Id = model.Id,
                Comment = model.Comment,
                Diastolic = model.Diastolic,
                Systolic = model.Systolic,
                Pulse = model.Pulse,
                Sugar = model.Sugar,
                Temperature = model.Temperature,
                RecordId = model.RecordId,
            };
        }
    }
}
