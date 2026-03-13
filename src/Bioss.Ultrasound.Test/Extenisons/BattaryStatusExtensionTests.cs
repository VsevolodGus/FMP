using Bioss.Ultrasound.Ble.Models.Enums;
using Bioss.Ultrasound.Extensions;

namespace Bioss.Ultrasound.Test.Extenisons;

public class BattaryStatusExtensionTests
{
    [Theory]
    [InlineData(BatteryLevel.Excellent, 100)]
    [InlineData(BatteryLevel.Good, 75)]
    [InlineData(BatteryLevel.Normal, 50)]
    [InlineData(BatteryLevel.Bad, 25)]
    [InlineData(BatteryLevel.Critical, 0)]
    public void GetDigitBatteryLevel_ReturnsExpectedValue(BatteryLevel level, int expected)
    {
        var result = level.GetDigitBatteryLevel();

        Assert.Equal((byte)expected, result);
    }

    [Fact]
    public void GetDigitBatteryLevel_ForUnknownValue_ReturnsZero()
    {
        var result = ((BatteryLevel)255).GetDigitBatteryLevel();

        Assert.Equal((byte)0, result);
    }
}