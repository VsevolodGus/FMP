using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Ble.Models.Enums;


namespace Bioss.Ultrasound.Test.Statuses;


public class BattaryStatusTests
{
    [Theory]
    [InlineData(0b00000000, BatteryLevel.Critical)]
    [InlineData(0b00000001, BatteryLevel.Bad)]
    [InlineData(0b00000010, BatteryLevel.Normal)]
    [InlineData(0b00000011, BatteryLevel.Good)]
    [InlineData(0b00000100, BatteryLevel.Excellent)]
    public void BatteryLevel_ShouldParseCorrectly(byte raw, BatteryLevel expected)
    {
        var status = new BattaryStatus(raw);

        Assert.Equal(expected, status.BatteryLevel);
    }

    [Fact]
    public void IsFHR1Include_ShouldBeTrue_WhenBit4Set()
    {
        var status = new BattaryStatus(0b00010000);

        Assert.True(status.IsFHR1Include);
    }

    [Fact]
    public void IsFHR2Include_ShouldBeTrue_WhenBit5Set()
    {
        var status = new BattaryStatus(0b00100000);

        Assert.True(status.IsFHR2Include);
    }

    [Fact]
    public void IsTocoInclude_ShouldBeTrue_WhenBit6Set()
    {
        var status = new BattaryStatus(0b01000000);

        Assert.True(status.IsTocoInclude);
    }

    [Fact]
    public void IsAFMInclude_ShouldBeTrue_WhenBit7Set()
    {
        var status = new BattaryStatus(0b10000000);

        Assert.True(status.IsAFMInclude);
    }

    [Fact]
    public void CombinedBits_ShouldParseAllFlagsCorrectly()
    {
        var raw = (byte)0b11110111;

        var status = new BattaryStatus(raw);

        Assert.Equal((BatteryLevel)(raw & 0b00000111), status.BatteryLevel);
        Assert.True(status.IsFHR1Include);
        Assert.True(status.IsFHR2Include);
        Assert.True(status.IsTocoInclude);
        Assert.True(status.IsAFMInclude);
    }
}

