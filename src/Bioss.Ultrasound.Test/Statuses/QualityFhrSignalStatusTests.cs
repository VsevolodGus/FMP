using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Ble.Models.Enums;

namespace Bioss.Ultrasound.Test.Statuses;


public class QualityFhrSignalStatusTests
{
    [Theory]
    [InlineData(0b00000001, SignalQuality.Bad)]
    [InlineData(0b00000010, SignalQuality.Normal)]
    [InlineData(0b00000011, SignalQuality.Good)]
    public void SignalQuality_ShouldParseCorrectly(byte raw, SignalQuality expected)
    {
        var status = new QualityFhrSignalStatus(raw);

        Assert.Equal(expected, status.SignalQuality);
    }

    [Fact]
    public void AutoFetalMovement_ShouldBeTrue_WhenBit2Set()
    {
        var status = new QualityFhrSignalStatus(0b00000100);

        Assert.True(status.AutoFetalMovement);
    }

    [Fact]
    public void AutoFetalMovement_ShouldBeFalse_WhenBit2NotSet()
    {
        var status = new QualityFhrSignalStatus(0b00000000);

        Assert.False(status.AutoFetalMovement);
    }

    [Fact]
    public void CombinedBits_ShouldParseCorrectly()
    {
        var raw = (byte)0b00000111;

        var status = new QualityFhrSignalStatus(raw);

        Assert.Equal((SignalQuality)(raw & 0b00000011), status.SignalQuality);
        Assert.True(status.AutoFetalMovement);
    }
}

