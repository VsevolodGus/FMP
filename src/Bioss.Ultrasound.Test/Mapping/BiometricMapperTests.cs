using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Mapping;

namespace Bioss.Ultrasound.Test.Mapping;

public class BiometricMapperTests
{
    [Fact]
    public void ToBiometric_MapsAllProperties()
    {
        var entity = new BiometricEntity
        {
            Id = 1,
            Comment = "note",
            Diastolic = 80,
            Systolic = 120,
            Temperature = 36.6,
            Pulse = 70,
            Sugar = 4.8,
            RecordId = 2
        };

        var result = entity.ToBiometric();

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.Comment, result.Comment);
        Assert.Equal(entity.Diastolic, result.Diastolic);
        Assert.Equal(entity.Systolic, result.Systolic);
        Assert.Equal(entity.Temperature, result.Temperature);
        Assert.Equal(entity.Pulse, result.Pulse);
        Assert.Equal(entity.Sugar, result.Sugar);
        Assert.Equal(entity.RecordId, result.RecordId);
    }

    [Fact]
    public void ToEntity_MapsAllProperties()
    {
        var model = new Biometric
        {
            Id = 1,
            Comment = "note",
            Diastolic = 80,
            Systolic = 120,
            Temperature = 36.6,
            Pulse = 70,
            Sugar = 4.8,
            RecordId = 2
        };

        var result = model.ToEntity();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.Comment, result.Comment);
        Assert.Equal(model.Diastolic, result.Diastolic);
        Assert.Equal(model.Systolic, result.Systolic);
        Assert.Equal(model.Temperature, result.Temperature);
        Assert.Equal(model.Pulse, result.Pulse);
        Assert.Equal(model.Sugar, result.Sugar);
        Assert.Equal(model.RecordId, result.RecordId);
    }
}
