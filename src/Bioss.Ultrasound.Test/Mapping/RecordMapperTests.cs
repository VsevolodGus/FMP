using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Mapping;
using Record = Bioss.Ultrasound.Domain.Models.Record;

namespace Bioss.Ultrasound.Test.Mapping;

public class RecordMapperTests
{
    [Fact]
    public void ToRecord_MapsWholeGraph()
    {
        var start = new DateTime(2026, 3, 14, 10, 0, 0, DateTimeKind.Utc);
        var end = start.AddMinutes(5);

        var entity = new RecordEntity
        {
            Id = 1,
            StartTime = start,
            EndTime = end,
            DeviceSerialNumber = "SN-001",
            Datas = new[]
            {
            new DataEntity
            {
                Id = 11,
                Time = start.AddSeconds(1),
                HeartRate = 140,
                Toco = 10,
                RecordId = 1
            }
        },
            Events = new[]
            {
            new EventEntity
            {
                Id = 21,
                Time = start.AddSeconds(2),
                Event = Events.FetalMovement,
                RecordId = 1
            }
        },
            Audio = new AudioEntity
            {
                Id = 31,
                Raw = AudioEntity.ToBytes(new short[] { 1, 2 }),
                RecordId = 1
            },
            Biometric = new BiometricEntity
            {
                Id = 41,
                Comment = "ok",
                Diastolic = 80,
                Systolic = 120,
                Temperature = 36.7,
                Pulse = 70,
                Sugar = 5.1,
                RecordId = 1
            }
        };

        var result = entity.ToRecord();

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.StartTime, result.StartTime);
        Assert.Equal(entity.EndTime, result.StopTime);
        Assert.Equal(entity.DeviceSerialNumber, result.DeviceSerialNumber);

        Assert.Single(result.Fhrs);
        Assert.Equal(entity.Datas[0].Id, result.Fhrs[0].Id);
        Assert.Equal(entity.Datas[0].HeartRate, result.Fhrs[0].Fhr);
        Assert.Equal(entity.Datas[0].Toco, result.Fhrs[0].Toco);

        Assert.Single(result.Events);
        Assert.Equal(entity.Events[0].Id, result.Events[0].Id);
        Assert.Equal(entity.Events[0].Event, result.Events[0].Event);

        Assert.NotNull(result.Audio);
        Assert.Equal(entity.Audio.Id, result.Audio.Id);
        Assert.Equal(entity.Audio.RecordId, result.Audio.RecordId);
        Assert.Equal(AudioEntity.ToShorts(entity.Audio.Raw).ToList(), result.Audio.Sound);

        Assert.NotNull(result.Biometric);
        Assert.Equal(entity.Biometric.Id, result.Biometric.Id);
        Assert.Equal(entity.Biometric.Comment, result.Biometric.Comment);
        Assert.Equal(entity.Biometric.RecordId, result.Biometric.RecordId);

        Assert.Null(result.CardiotocographyInfo);
    }

    [Fact]
    public void ToRecord_WhenOptionalRelationsAreNull_MapsNullsAndEmptyCollections()
    {
        var start = new DateTime(2026, 3, 14, 10, 0, 0, DateTimeKind.Utc);

        var entity = new RecordEntity
        {
            Id = 1,
            StartTime = start,
            EndTime = start.AddMinutes(1),
            DeviceSerialNumber = "SN-001",
            Datas = Array.Empty<DataEntity>(),
            Events = Array.Empty<EventEntity>(),
            Audio = null,
            Biometric = null
        };

        var result = entity.ToRecord();

        Assert.Empty(result.Fhrs);
        Assert.Empty(result.Events);
        Assert.Null(result.Audio);
        Assert.Null(result.Biometric);
        Assert.Null(result.CardiotocographyInfo);
    }

    [Fact]
    public void ToEntity_MapsWholeGraph()
    {
        var start = new DateTime(2026, 3, 14, 10, 0, 0, DateTimeKind.Utc);
        var stop = start.AddMinutes(5);

        var record = new Domain.Models.Record
        {
            Id = 1,
            StartTime = start,
            StopTime = stop,
            DeviceSerialNumber = "SN-001",
            Fhrs = new List<FhrData>
        {
            new FhrData
            {
                Id = 11,
                Time = start.AddSeconds(1),
                Fhr = 140,
                Toco = 10,
                RecordId = 1
            }
        },
            Events = new List<FhrEvent>
        {
            new FhrEvent
            {
                Id = 21,
                Time = start.AddSeconds(2),
                Event = Events.FetalMovement,
                RecordId = 1
            }
        },
            Audio = new Audio
            {
                Id = 31,
                Sound = new List<short> { 1, 2 },
                RecordId = 1
            },
            Biometric = new Biometric
            {
                Id = 41,
                Comment = "ok",
                Diastolic = 80,
                Systolic = 120,
                Temperature = 36.7,
                Pulse = 70,
                Sugar = 5.1,
                RecordId = 1
            }
        };

        var result = record.ToEntity();

        Assert.Equal(record.Id, result.Id);
        Assert.Equal(record.StartTime, result.StartTime);
        Assert.Equal(record.StopTime, result.EndTime);
        Assert.Equal(record.DeviceSerialNumber, result.DeviceSerialNumber);

        Assert.NotNull(result.Datas);
        Assert.Single(result.Datas);
        Assert.Equal(record.Fhrs[0].Id, result.Datas[0].Id);
        Assert.Equal(record.Fhrs[0].Fhr, result.Datas[0].HeartRate);
        Assert.Equal(record.Fhrs[0].Toco, result.Datas[0].Toco);

        Assert.NotNull(result.Events);
        Assert.Single(result.Events);
        Assert.Equal(record.Events[0].Id, result.Events[0].Id);
        Assert.Equal(record.Events[0].Event, result.Events[0].Event);

        Assert.NotNull(result.Audio);
        Assert.Equal(record.Audio.Id, result.Audio.Id);
        Assert.Equal(record.Audio.RecordId, result.Audio.RecordId);
        Assert.Equal(AudioEntity.ToBytes(record.Audio.Sound.ToArray()), result.Audio.Raw);

        Assert.NotNull(result.Biometric);
        Assert.Equal(record.Biometric.Id, result.Biometric.Id);
        Assert.Equal(record.Biometric.Comment, result.Biometric.Comment);
        Assert.Equal(record.Biometric.RecordId, result.Biometric.RecordId);
    }

    [Fact]
    public void ToEntity_WhenOptionalRelationsAreNull_MapsNulls()
    {
        var start = new DateTime(2026, 3, 14, 10, 0, 0, DateTimeKind.Utc);

        var record = new Record
        {
            Id = 1,
            StartTime = start,
            StopTime = start.AddMinutes(1),
            DeviceSerialNumber = "SN-001",
            Fhrs = new List<FhrData>(),
            Events = new List<FhrEvent>(),
            Audio = null,
            Biometric = null
        };

        var result = record.ToEntity();

        Assert.NotNull(result.Datas);
        Assert.Empty(result.Datas);

        Assert.NotNull(result.Events);
        Assert.Empty(result.Events);

        Assert.Null(result.Audio);
        Assert.Null(result.Biometric);
    }
}
