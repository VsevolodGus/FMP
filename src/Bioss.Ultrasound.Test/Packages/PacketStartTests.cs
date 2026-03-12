using Bioss.Ultrasound.Ble.Models;

namespace Bioss.Ultrasound.Test.Packages;

public class PacketStartTests
{
    [Fact]
    public void IsStart_ReturnsTrue_WhenHeaderMatches()
    {
        var data = new byte[] { 0x55, 0xAA, 0x09 };

        var result = Package.IsStart(data);

        Assert.True(result);
    }

    [Fact]
    public void IsStart_ReturnsTrue_WhenHeaderMatches_WithExtraBytes()
    {
        var data = new byte[] { 0x55, 0xAA, 0x09, 0x10, 0x20 };

        var result = Package.IsStart(data);

        Assert.True(result);
    }

    [Fact]
    public void IsStart_ReturnsFalse_WhenArrayTooShort()
    {
        var data = new byte[] { 0x55, 0xAA };

        var result = Package.IsStart(data);

        Assert.False(result);
    }

    [Fact]
    public void IsStart_ReturnsFalse_WhenHeaderIncorrect()
    {
        var data = new byte[] { 0x55, 0xAA, 0x08 };

        var result = Package.IsStart(data);

        Assert.False(result);
    }

    [Fact]
    public void IsStart_ReturnsFalse_WhenFirstByteIncorrect()
    {
        var data = new byte[] { 0x54, 0xAA, 0x09 };

        var result = Package.IsStart(data);

        Assert.False(result);
    }

    [Fact]
    public void IsStart_ReturnsFalse_WhenSecondByteIncorrect()
    {
        var data = new byte[] { 0x55, 0xAB, 0x09 };

        var result = Package.IsStart(data);

        Assert.False(result);
    }
}