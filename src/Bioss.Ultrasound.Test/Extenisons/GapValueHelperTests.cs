using Bioss.Ultrasound.UI.Helpers;

namespace Bioss.Ultrasound.Test.Extenisons;

public class GapValueHelperTests
{
    [Fact]
    public void GetValueOrGap_WhenValueIsZero_ReturnsNaN()
    {
        var helper = new GapValueHelper();

        var result = helper.GetValueOrGap(0);

        Assert.True(double.IsNaN(result));
    }

    [Fact]
    public void GetValueOrGap_WhenFirstValueIsWithinGapStep_ReturnsValue()
    {
        var helper = new GapValueHelper();

        var result = helper.GetValueOrGap(10);

        Assert.Equal(10d, result);
    }

    [Fact]
    public void GetValueOrGap_WhenFirstValueExceedsGapStep_ReturnsNaN()
    {
        var helper = new GapValueHelper();

        var result = helper.GetValueOrGap(21);

        Assert.True(double.IsNaN(result));
    }

    [Fact]
    public void GetValueOrGap_WhenDifferenceIsLessThanGapStep_ReturnsValue()
    {
        var helper = new GapValueHelper();
        helper.GetValueOrGap(10);

        var result = helper.GetValueOrGap(25);

        Assert.Equal(25d, result);
    }

    [Fact]
    public void GetValueOrGap_WhenDifferenceIsExactlyGapStep_ReturnsValue()
    {
        var helper = new GapValueHelper();
        helper.GetValueOrGap(10);

        var result = helper.GetValueOrGap(30);

        Assert.Equal(30d, result);
    }

    [Fact]
    public void GetValueOrGap_WhenDifferenceIsGreaterThanGapStep_ReturnsNaN()
    {
        var helper = new GapValueHelper();
        helper.GetValueOrGap(10);

        var result = helper.GetValueOrGap(31);

        Assert.True(double.IsNaN(result));
    }

    [Fact]
    public void GetValueOrGap_UsesAbsoluteDifference()
    {
        var helper = new GapValueHelper();
        helper.GetValueOrGap(15);
        helper.GetValueOrGap(30);

        var result = helper.GetValueOrGap(5);

        Assert.True(double.IsNaN(result));
    }

    [Fact]
    public void GetValueOrGap_UpdatesLastValue_EvenWhenReturnedValueIsNaN()
    {
        var helper = new GapValueHelper();

        var first = helper.GetValueOrGap(50);
        var second = helper.GetValueOrGap(55);

        Assert.True(double.IsNaN(first));
        Assert.Equal(55d, second);
    }

    [Fact]
    public void GetValueOrGap_AfterZeroValue_StillUpdatesLastValueToZero()
    {
        var helper = new GapValueHelper();

        var first = helper.GetValueOrGap(0);
        var second = helper.GetValueOrGap(10);

        Assert.True(double.IsNaN(first));
        Assert.Equal(10d, second);
    }
}