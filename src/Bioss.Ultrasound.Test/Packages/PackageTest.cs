using Bioss.Ultrasound.Ble.Models;

namespace Bioss.Ultrasound.Test.Packages;

public class PackageTest
{
    [Fact]
    public void Init_WithSoundAndFhr_ShouldParseBothPackages()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();
        var fhr = FhrPackageTests.CreateValidFhrPacket();

        var combined = new byte[sound.Length + fhr.Length];
        Array.Copy(sound, 0, combined, 0, sound.Length);
        Array.Copy(fhr, 0, combined, sound.Length, fhr.Length);

        var package = Package.Init(combined);

        Assert.NotNull(package);
        Assert.NotNull(package.SoundPackage);
        Assert.NotNull(package.FHRPackage);
        Assert.True(package.IsValid);
    }

    [Fact]
    public void Init_WithOnlySound_ShouldParseSoundOnly()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();

        var package = Package.Init(sound);

        Assert.NotNull(package);
        Assert.NotNull(package.SoundPackage);
        Assert.Null(package.FHRPackage);
        Assert.True(package.IsValid);
    }

    [Fact]
    public void Init_WithInvalidLength_ShouldReturnNull()
    {
        var invalid = new byte[5];

        var package = Package.Init(invalid);

        Assert.Null(package);
    }

    [Fact]
    public void IsValid_ShouldBeFalse_WhenFhrInvalid()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();
        var fhr = FhrPackageTests.CreateValidFhrPacket();

        // ломаем parity FHR
        fhr[9] = (byte)(fhr[9] + 1);

        var combined = new byte[sound.Length + fhr.Length];
        Array.Copy(sound, 0, combined, 0, sound.Length);
        Array.Copy(fhr, 0, combined, sound.Length, fhr.Length);

        var package = Package.Init(combined);

        Assert.NotNull(package);
        Assert.False(package.IsValid);
    }

    [Fact]
    public void Init_ShouldSplitCombinedBufferCorrectly()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();
        var fhr = FhrPackageTests.CreateValidFhrPacket();

        var combined = new byte[sound.Length + fhr.Length];

        for (int i = 0; i < combined.Length; i++)
            combined[i] = (byte)i;

        var package = Package.Init(combined);

        Assert.NotNull(package);

        // Проверяем первый байт Sound
        Assert.Equal(combined[0], package.SoundPackage.Head1);

        // Проверяем первый байт FHR
        Assert.Equal(
            combined[SoundPackage.DataLength],
            package.FHRPackage.Head1);
    }


    [Fact]
    public void Init_WithSoundAndFhr_ShouldParseBothPackages_Span()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();
        var fhr = FhrPackageTests.CreateValidFhrPacket();

        var combined = new byte[sound.Length + fhr.Length];
        Array.Copy(sound, 0, combined, 0, sound.Length);
        Array.Copy(fhr, 0, combined, sound.Length, fhr.Length);

        var package = Package.Init(combined.AsSpan());

        Assert.NotNull(package);
        Assert.NotNull(package.SoundPackage);
        Assert.NotNull(package.FHRPackage);
        Assert.True(package.IsValid);
    }

    [Fact]
    public void Init_WithOnlySound_ShouldParseSoundOnly_Span()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();

        var package = Package.Init(sound.AsSpan());

        Assert.NotNull(package);
        Assert.NotNull(package.SoundPackage);
        Assert.Null(package.FHRPackage);
        Assert.True(package.IsValid);
    }

    [Fact]
    public void Init_WithInvalidLength_ShouldReturnNull_Span()
    {
        var invalid = new byte[5];

        var package = Package.Init(invalid.AsSpan());

        Assert.Null(package);
    }

    [Fact]
    public void IsValid_ShouldBeFalse_WhenFhrInvalid_Span()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();
        var fhr = FhrPackageTests.CreateValidFhrPacket();

        // ломаем parity FHR
        fhr[9] = (byte)(fhr[9] + 1);

        var combined = new byte[sound.Length + fhr.Length];
        Array.Copy(sound, 0, combined, 0, sound.Length);
        Array.Copy(fhr, 0, combined, sound.Length, fhr.Length);

        var package = Package.Init(combined.AsSpan());

        Assert.NotNull(package);
        Assert.False(package.IsValid);
    }

    [Fact]
    public void Init_ShouldSplitCombinedBufferCorrectly_Span()
    {
        var sound = SoundPackageTests.CreateValidSoundPacket();
        var fhr = FhrPackageTests.CreateValidFhrPacket();

        var combined = new byte[sound.Length + fhr.Length];

        for (int i = 0; i < combined.Length; i++)
            combined[i] = (byte)i;

        var package = Package.Init(combined.AsSpan());

        Assert.NotNull(package);

        // Проверяем первый байт Sound
        Assert.Equal(combined[0], package.SoundPackage.Head1);

        // Проверяем первый байт FHR
        Assert.Equal(
            combined[SoundPackage.DataLength],
            package.FHRPackage.Head1);
    }
}
