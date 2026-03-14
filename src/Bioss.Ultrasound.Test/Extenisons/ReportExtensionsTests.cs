using Bioss.Ultrasound.Services.Extensions;

namespace Bioss.Ultrasound.Test.Extenisons;

public class ReportExtensionsTests
{
    [Theory]
    [InlineData(1, 10, 1)]
    [InlineData(5, 5, 1)]
    [InlineData(10, 5, 2)]
    [InlineData(11, 5, 3)]
    [InlineData(59, 30, 2)]
    [InlineData(60, 30, 2)]
    [InlineData(61, 30, 3)]
    public void CalculateCountPages_ReturnsExpectedPageCount(
        int totalMinutes,
        int minutesInPage,
        int expected)
    {
        var result = ReportExtensions.CalculateCountPages(totalMinutes, minutesInPage);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateCountPages_WhenTotalMinutesIsZero_ReturnsOne_WithCurrentImplementation()
    {
        var result = ReportExtensions.CalculateCountPages(0, 10);

        Assert.Equal(1, result);
    }

    [Fact]
    public void CalculateCountPages_WhenMinutesInPageIsZero_ThrowsDivideByZeroException()
    {
        Assert.Throws<DivideByZeroException>(() =>
            ReportExtensions.CalculateCountPages(10, 0));
    }

    [Theory]
    [InlineData(12.0, 3, 4.0)]
    [InlineData(10.0, 4, 2.5)]
    [InlineData(0.0, 5, 0.0)]
    [InlineData(7.5, 2, 3.75)]
    public void CalculateMinuteInPage_ReturnsExpectedValue(
        double sizeDisplayCentimeter,
        int centimetersInMinute,
        double expected)
    {
        var result = ReportExtensions.CalculateMinuteInPage(
            sizeDisplayCentimeter,
            centimetersInMinute);

        Assert.Equal(expected, result, 6);
    }

    [Fact]
    public void CalculateMinuteInPage_WhenCentimetersInMinuteIsZero_ReturnsPositiveInfinity()
    {
        var result = ReportExtensions.CalculateMinuteInPage(10.0, 0);

        Assert.True(double.IsPositiveInfinity(result));
    }

    [Fact]
    public void CalculateMinuteInPage_WhenBothArgumentsAreZero_ReturnsNaN()
    {
        var result = ReportExtensions.CalculateMinuteInPage(0.0, 0);

        Assert.True(double.IsNaN(result));
    }
}