using Bioss.Ultrasound.UI.Helpers;

namespace Bioss.Ultrasound.Test.Extenisons;

public class RecordTimePassedHelperTests
{
    [Fact]
    public void Init_SetsAllProperties()
    {
        var helper = new RecordTimePassedHelper();
        var startTime = new DateTime(2026, 3, 13, 10, 0, 0);

        helper.Init(true, 120, startTime);

        Assert.True(helper.IsAutoRecord);
        Assert.Equal(120, helper.TotalAutoRecordSeconds);
        Assert.Equal(startTime, helper.StartTime);
    }

    [Fact]
    public void CurrentRecordTime_ReturnsElapsedTime()
    {
        var helper = new RecordTimePassedHelper
        {
            StartTime = DateTime.Now - TimeSpan.FromSeconds(10)
        };

        var result = helper.CurrentRecordTime;

        Assert.InRange(result.TotalSeconds, 9, 11);
    }

    [Fact]
    public void IsTimeEnd_ReturnsFalse_WhenAutoRecordIsDisabled()
    {
        var helper = new RecordTimePassedHelper
        {
            IsAutoRecord = false,
            TotalAutoRecordSeconds = 1,
            StartTime = DateTime.Now - TimeSpan.FromSeconds(10)
        };

        Assert.False(helper.IsTimeEnd);
    }

    [Fact]
    public void IsTimeEnd_ReturnsFalse_WhenAutoRecordTimeIsNotReached()
    {
        var helper = new RecordTimePassedHelper
        {
            IsAutoRecord = true,
            TotalAutoRecordSeconds = 10,
            StartTime = DateTime.Now - TimeSpan.FromSeconds(5)
        };

        Assert.False(helper.IsTimeEnd);
    }

    [Fact]
    public void IsTimeEnd_ReturnsTrue_WhenAutoRecordTimeIsExceeded()
    {
        var helper = new RecordTimePassedHelper
        {
            IsAutoRecord = true,
            TotalAutoRecordSeconds = 10,
            StartTime = DateTime.Now - TimeSpan.FromSeconds(11)
        };

        Assert.True(helper.IsTimeEnd);
    }

    [Fact]
    public void DisplayTimePassed_WhenManualRecord_ReturnsElapsedTimeInMmSs()
    {
        var helper = new RecordTimePassedHelper
        {
            IsAutoRecord = false,
            StartTime = DateTime.Now - TimeSpan.FromSeconds(65.1)
        };

        var result = helper.DisplayTimePassed();

        Assert.Equal("01:05", result);
    }

    [Fact]
    public void DisplayTimePassed_WhenAutoRecord_ReturnsRemainingTimeInMmSs()
    {
        var helper = new RecordTimePassedHelper
        {
            IsAutoRecord = true,
            TotalAutoRecordSeconds = 125,
            StartTime = DateTime.Now - TimeSpan.FromSeconds(5.1)
        };

        var result = helper.DisplayTimePassed();

        Assert.Equal("01:59", result);
    }
}