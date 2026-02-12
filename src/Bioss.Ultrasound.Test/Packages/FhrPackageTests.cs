using Bioss.Ultrasound.Ble.Models;

namespace Bioss.Ultrasound.Test.Packages;

public class FhrPackageTests
{
    internal static byte[] CreateValidFhrPacket()
    {
        var data = new byte[10];

        data[0] = 0x55;
        data[1] = 0xaa;
        data[2] = 0x03;
        data[3] = 100;
        data[4] = 120;
        data[5] = 30;
        data[6] = 1;
        data[7] = 2;
        data[8] = 3;

        var sum = (100 + 120 + 30 + 1 + 2 + 3) % 256;
        data[9] = (byte)sum;

        return data;
    }

    [Fact]
    public void Init_WithValidData_ShouldBeValid()
    {
        var data = CreateValidFhrPacket();

        var package = FHRPackage.Init(data);

        Assert.NotNull(package);
        Assert.True(package.IsValid);
        Assert.Equal(0x55, package.Head1);
        Assert.Equal(0xaa, package.Head2);
        Assert.Equal(0x03, package.Control);
    }

    [Fact]
    public void Init_WithInvalidLength_ShouldReturnNull()
    {
        var data = new byte[5];

        var package = FHRPackage.Init(data);

        Assert.Null(package);
    }

    [Fact]
    public void IsValid_WithWrongParity_ShouldBeFalse()
    {
        var data = CreateValidFhrPacket();
        data[9] = (byte)(data[9] + 1);

        var package = FHRPackage.Init(data);

        Assert.False(package.IsValid);
    }

    [Fact]
    public void Parity_ShouldBeCalculatedCorrectly()
    {
        var data = CreateValidFhrPacket();
        var package = FHRPackage.Init(data);

        var expected =
            (package.Fhr1 +
             package.Fhr2 +
             package.Toco +
             package.Afm +
             package.Status1.RawValue +
             package.Status2.RawValue) % 256;

        Assert.Equal(expected, package.Parity);
        Assert.True(package.IsValid);
    }

    [Fact]
    public void Init_ShouldAssignAllFieldsCorrectly()
    {
        // Arrange
        var data = new byte[10]
        {
            0x55, // Head1
            0xAA, // Head2
            0x03, // Control
            101,  // Fhr1
            102,  // Fhr2
            50,   // Toco
            7,    // Afm
            8,    // Status1 raw
            9,    // Status2 raw
            0     // Parity (заполним ниже)
        };

        data[9] = (byte)(
            (data[3] + data[4] + data[5] +
             data[6] + data[7] + data[8]) % 256);

        var package = FHRPackage.Init(data);

        Assert.NotNull(package);

        Assert.Equal(0x55, package.Head1);
        Assert.Equal(0xAA, package.Head2);
        Assert.Equal(0x03, package.Control);

        Assert.Equal(101, package.Fhr1);
        Assert.Equal(102, package.Fhr2);
        Assert.Equal(50, package.Toco);
        Assert.Equal(7, package.Afm);

        Assert.Equal(8, package.Status1.RawValue);
        Assert.Equal(9, package.Status2.RawValue);

        Assert.Equal(data[9], package.Parity);

        Assert.True(package.IsValid);
    }
}
