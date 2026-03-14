using Bioss.Ultrasound.Data.Database.Entities;
using Bioss.Ultrasound.Data.Database.Entities.Enums;
using Bioss.Ultrasound.Domain.Models;
using Bioss.Ultrasound.Mapping;

namespace Bioss.Ultrasound.Test.Mapping;

public class AudioMapperTests
{
    [Fact]
    public void ToAudio_MapsProperties_AndConvertsRaw()
    {
        var raw = AudioEntity.ToBytes(new short[] { 1, 2, 300 });
        var entity = new AudioEntity
        {
            Id = 5,
            Raw = raw,
            RecordId = 9
        };

        var result = entity.ToAudio();

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.RecordId, result.RecordId);
        Assert.Equal([.. AudioEntity.ToShorts(raw)], result.Sound);
    }

    [Fact]
    public void ToAudio_WhenRawIsNull_ReturnsEmptySoundList()
    {
        var entity = new AudioEntity
        {
            Id = 5,
            Raw = null,
            RecordId = 9
        };

        var result = entity.ToAudio();

        Assert.Equal(entity.Id, result.Id);
        Assert.Equal(entity.RecordId, result.RecordId);
        Assert.NotNull(result.Sound);
        Assert.Empty(result.Sound);
    }

    [Fact]
    public void ToEntity_MapsProperties_AndConvertsSound()
    {
        var model = new Audio
        {
            Id = 5,
            RecordId = 9,
            Sound = new List<short> { 1, 2, 300 }
        };

        var result = model.ToEntity();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.RecordId, result.RecordId);
        Assert.Equal(AudioEntity.ToBytes([.. model.Sound]), result.Raw);
    }

    [Fact]
    public void ToEntity_WhenSoundIsNull_ReturnsEmptyRawArray()
    {
        var model = new Audio
        {
            Id = 5,
            RecordId = 9,
            Sound = null
        };

        var result = model.ToEntity();

        Assert.Equal(model.Id, result.Id);
        Assert.Equal(model.RecordId, result.RecordId);
        Assert.NotNull(result.Raw);
        Assert.Empty(result.Raw);
    }
}






