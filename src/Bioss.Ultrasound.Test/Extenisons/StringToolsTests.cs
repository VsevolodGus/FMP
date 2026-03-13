using Bioss.Ultrasound.Tools;
using System.Globalization;

namespace Bioss.Ultrasound.Test.Extenisons;

public class StringToolsTests
{
    [Fact]
    public void Int_WhenZero_ReturnsEmptyString()
    {
        var result = 0.ToStringOrEmptyString();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Int_WhenZero_ReturnsCustomDefaultValue()
    {
        var result = 0.ToStringOrEmptyString("-");

        Assert.Equal("-", result);
    }

    [Fact]
    public void Int_WhenNotZero_AndFormatIsNull_ReturnsValueToString()
    {
        var result = 12.ToStringOrEmptyString();

        Assert.Equal("12", result);
    }

    [Fact]
    public void Int_WhenNotZero_AndFormatIsSpecified_ReturnsFormattedValue()
    {
        var result = 12.ToStringOrEmptyString(format: "0000");

        Assert.Equal("0012", result);
    }

    [Fact]
    public void Double_WhenZero_ReturnsEmptyString()
    {
        var result = 0d.ToStringOrEmptyString();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void Double_WhenZero_ReturnsCustomDefaultValue_EvenIfFormatSpecified()
    {
        var result = 0d.ToStringOrEmptyString("-", "0.00");

        Assert.Equal("-", result);
    }

    [Fact]
    public void Double_WhenNotZero_AndFormatIsNull_ReturnsValueToString()
    {
        using var _ = new CultureScope("en-US");

        var result = 12.5d.ToStringOrEmptyString();

        Assert.Equal("12.5", result);
    }

    [Fact]
    public void Double_WhenNotZero_AndFormatIsSpecified_ReturnsFormattedValue()
    {
        using var _ = new CultureScope("en-US");

        var result = 12.5d.ToStringOrEmptyString(format: "0.00");

        Assert.Equal("12.50", result);
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUiCulture;

        public CultureScope(string cultureName)
        {
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUiCulture = CultureInfo.CurrentUICulture;

            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUiCulture;
        }
    }
}