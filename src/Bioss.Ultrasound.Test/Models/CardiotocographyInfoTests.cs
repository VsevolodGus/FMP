using System.Reflection;
using Bioss.Ultrasound.Domain.Models;


namespace Bioss.Ultrasound.Test.Models;

public class CardiotocographyInfoTests
{
    [Fact]
    public void NewInstance_HasNoValidCriteria_AndOverallCriteriaIsFalse()
    {
        var info = new CardiotocographyInfo();

        Assert.Equal(0, info.CountRoodDawsonCriteriaValid());
        Assert.False(info.IsRoodDawsonCriteriaValid());
    }

    [Fact]
    public void CountRoodDawsonCriteriaValid_ReturnsCorrectCount_ForPartialSet()
    {
        var info = new CardiotocographyInfo
        {
            IsValidRecordingDuration = true,
            SignalLossValid = true,
            BasalHeartRateValid = false,
            DecelerationsMark = true,
            IsValidSyncRhythm = false,
            STVValid = true,
            MovementFrequencyValid = false,
            IsTimeDependentParameters = true
        };

        var result = info.CountRoodDawsonCriteriaValid();

        Assert.Equal(5, result);
    }

    [Fact]
    public void CountRoodDawsonCriteriaValid_ReturnsEight_WhenAllCriteriaAreValid()
    {
        var info = CreateValidInfo();

        var result = info.CountRoodDawsonCriteriaValid();

        Assert.Equal(8, result);
    }

    [Fact]
    public void IsRoodDawsonCriteriaValid_ReturnsTrue_WhenAllCriteriaAreValid()
    {
        var info = CreateValidInfo();

        var result = info.IsRoodDawsonCriteriaValid();

        Assert.True(result);
    }

    [Theory]
    [InlineData(nameof(CardiotocographyInfo.IsValidRecordingDuration))]
    [InlineData(nameof(CardiotocographyInfo.SignalLossValid))]
    [InlineData(nameof(CardiotocographyInfo.BasalHeartRateValid))]
    [InlineData(nameof(CardiotocographyInfo.DecelerationsMark))]
    [InlineData(nameof(CardiotocographyInfo.IsValidSyncRhythm))]
    [InlineData(nameof(CardiotocographyInfo.STVValid))]
    [InlineData(nameof(CardiotocographyInfo.MovementFrequencyValid))]
    [InlineData(nameof(CardiotocographyInfo.IsTimeDependentParameters))]
    public void IsRoodDawsonCriteriaValid_ReturnsFalse_WhenAnySingleCriterionIsFalse(string propertyName)
    {
        var info = CreateValidInfo();

        SetBooleanProperty(info, propertyName, false);

        var result = info.IsRoodDawsonCriteriaValid();

        Assert.False(result);
    }

    [Theory]
    [InlineData(nameof(CardiotocographyInfo.IsValidRecordingDuration))]
    [InlineData(nameof(CardiotocographyInfo.SignalLossValid))]
    [InlineData(nameof(CardiotocographyInfo.BasalHeartRateValid))]
    [InlineData(nameof(CardiotocographyInfo.DecelerationsMark))]
    [InlineData(nameof(CardiotocographyInfo.IsValidSyncRhythm))]
    [InlineData(nameof(CardiotocographyInfo.STVValid))]
    [InlineData(nameof(CardiotocographyInfo.MovementFrequencyValid))]
    [InlineData(nameof(CardiotocographyInfo.IsTimeDependentParameters))]
    public void CountRoodDawsonCriteriaValid_DecreasesByOne_WhenAnySingleCriterionIsFalse(string propertyName)
    {
        var info = CreateValidInfo();

        SetBooleanProperty(info, propertyName, false);

        var result = info.CountRoodDawsonCriteriaValid();

        Assert.Equal(7, result);
    }

    [Fact]
    public void NumericAndNullableProperties_DoNotAffectCriteriaValidation()
    {
        var info = new CardiotocographyInfo
        {
            RecordingDuration = 25.5f,
            SignalLossPercentage = 1.2f,
            BasalHeartRate = 142f,
            AccelerationsOver10 = 3,
            AccelerationsOver15 = 2,
            Decelerations = 0,
            HighVariabilityMinutes = 12,
            LowVariabilityMinutes = 1,
            SyncRhythmMinutes = 0,
            BeatLTV = 28f,
            TimeMsLTV = 35f,
            STV = 6.4f,
            OscillationFrequency = 4.2f,
            MovementFrequency = 5.1f
        };

        Assert.Equal(0, info.CountRoodDawsonCriteriaValid());
        Assert.False(info.IsRoodDawsonCriteriaValid());
    }

    private static CardiotocographyInfo CreateValidInfo()
    {
        return new CardiotocographyInfo
        {
            IsValidRecordingDuration = true,
            SignalLossValid = true,
            BasalHeartRateValid = true,
            DecelerationsMark = true,
            IsValidSyncRhythm = true,
            STVValid = true,
            MovementFrequencyValid = true,
            IsTimeDependentParameters = true
        };
    }

    private static void SetBooleanProperty(CardiotocographyInfo info, string propertyName, bool value)
    {
        var property = typeof(CardiotocographyInfo).GetProperty(
            propertyName,
            BindingFlags.Instance | BindingFlags.Public);

        Assert.NotNull(property);
        property!.SetValue(info, value);
    }
}