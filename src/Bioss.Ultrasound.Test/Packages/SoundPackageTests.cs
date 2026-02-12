using Bioss.Ultrasound.Ble.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bioss.Ultrasound.Test.Packages;

public class SoundPackageTests
{
    internal static byte[] CreateValidSoundPacket()
    {
        var data = new byte[SoundPackage.DataLength];

        data[0] = 0x55;
        data[1] = 0xAA;
        data[2] = 0x09;

        int sum = 0;

        for (int i = 0; i < 100; i++)
        {
            data[3 + i] = (byte)(i + 1);
            sum += data[3 + i];
        }

        data[103] = (byte)(sum % 256);
        data[104] = 5;
        data[105] = 10;
        data[106] = 20;

        return data;
    }

    [Fact]
    public void Data_ShouldBeCopiedFromCorrectOffset()
    {
        var data = new byte[107];

        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)i;

        var package = SoundPackage.Init(data);

        for (int i = 0; i < 100; i++)
            Assert.Equal(data[3 + i], package.Data[i]);
    }

    [Fact]
    public void Init_WithWrongLength_ShouldReturnNull()
    {
        var data = new byte[50];

        var package = SoundPackage.Init(data);

        Assert.Null(package);
    }

    [Fact]
    public void Init_ShouldAssignAllFieldsCorrectly()
    {
        var data = new byte[107];

        data[0] = 0x55;
        data[1] = 0xAA;
        data[2] = 0x09;

        for (int i = 0; i < 100; i++)
            data[3 + i] = (byte)(i + 1);

        data[103] = 200; // Parity
        data[104] = 11;  // AdpcmIndex
        data[105] = 22;  // Low
        data[106] = 33;  // High

        var package = SoundPackage.Init(data);

        Assert.NotNull(package);

        Assert.Equal(0x55, package.Head1);
        Assert.Equal(0xAA, package.Head2);
        Assert.Equal(0x09, package.Control);

        Assert.Equal(100, package.Data.Length);

        for (int i = 0; i < 100; i++)
            Assert.Equal((byte)(i + 1), package.Data[i]);

        Assert.Equal(200, package.Parity);
        Assert.Equal(11, package.AdpcmIndex);
        Assert.Equal(22, package.AdpcmValueLow);
        Assert.Equal(33, package.AdpcmValueHigh);
    }

    [Fact]
    public void Parity_ShouldMatchSumOfData()
    {
        var data = CreateValidSoundPacket();

        var package = SoundPackage.Init(data);

        int expectedSum = 0;
        for (int i = 0; i < package.Data.Length; i++)
            expectedSum += package.Data[i];

        var expectedParity = (byte)(expectedSum % 256);

        Assert.Equal(expectedParity, package.Parity);
    }

    [Fact]
    public void IsValid_WithWrongParity_ShouldBeFalse()
    {
        var data = CreateValidSoundPacket();

        // гарантированно ломаем parity
        data[0] = 0x54;

        var package = SoundPackage.Init(data);

        Assert.NotNull(package);
        Assert.False(package.IsValid);
    }

    [Fact]
    public void IsValid_WithCorrectParity_ShouldBeTrue()
    {
        var data = CreateValidSoundPacket();

        var package = SoundPackage.Init(data);

        Assert.NotNull(package);
        Assert.True(package.IsValid);
    }

    [Fact]
    public void Data_ShouldBeCopiedFromCorrectOffset_Span()
    {
        var data = new byte[107];

        for (int i = 0; i < data.Length; i++)
            data[i] = (byte)i;

        var package = SoundPackage.Init(data.AsSpan());

        for (int i = 0; i < 100; i++)
            Assert.Equal(data[3 + i], package.Data[i]);
    }

    [Fact]
    public void Init_WithWrongLength_ShouldReturnNull_Span()
    {
        var data = new byte[50];

        var package = SoundPackage.Init(data.AsSpan());

        Assert.Null(package);
    }

    [Fact]
    public void Init_ShouldAssignAllFieldsCorrectly_Span()
    {
        var data = new byte[107];

        data[0] = 0x55;
        data[1] = 0xAA;
        data[2] = 0x09;

        for (int i = 0; i < 100; i++)
            data[3 + i] = (byte)(i + 1);

        data[103] = 200; // Parity
        data[104] = 11;  // AdpcmIndex
        data[105] = 22;  // Low
        data[106] = 33;  // High

        var package = SoundPackage.Init(data.AsSpan());

        Assert.NotNull(package);

        Assert.Equal(0x55, package.Head1);
        Assert.Equal(0xAA, package.Head2);
        Assert.Equal(0x09, package.Control);

        Assert.Equal(100, package.Data.Length);

        for (int i = 0; i < 100; i++)
            Assert.Equal((byte)(i + 1), package.Data[i]);

        Assert.Equal(200, package.Parity);
        Assert.Equal(11, package.AdpcmIndex);
        Assert.Equal(22, package.AdpcmValueLow);
        Assert.Equal(33, package.AdpcmValueHigh);
    }

    [Fact]
    public void Parity_ShouldMatchSumOfData_Span()
    {
        var data = CreateValidSoundPacket();

        var package = SoundPackage.Init(data.AsSpan());

        int expectedSum = 0;
        for (int i = 0; i < package.Data.Length; i++)
            expectedSum += package.Data[i];

        var expectedParity = (byte)(expectedSum % 256);

        Assert.Equal(expectedParity, package.Parity);
    }

    [Fact]
    public void IsValid_WithWrongParity_ShouldBeFalse_Span()
    {
        var data = CreateValidSoundPacket();

        // гарантированно ломаем parity
        data[0] = 0x54;

        var package = SoundPackage.Init(data.AsSpan());

        Assert.NotNull(package);
        Assert.False(package.IsValid);
    }

    [Fact]
    public void IsValid_WithCorrectParity_ShouldBeTrue_Span()
    {
        var data = CreateValidSoundPacket();

        var package = SoundPackage.Init(data.AsSpan());

        Assert.NotNull(package);
        Assert.True(package.IsValid);
    }
}
