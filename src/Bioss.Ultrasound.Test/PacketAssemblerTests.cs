using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Services;

namespace Bioss.Ultrasound.Test;

public class PacketAssemblerTests
{
    [Fact]
    public void Process_ReturnsNull_WhenChunkIsNull()
    {
        var assembler = new PacketAssembler();

        var result = assembler.Process(new BleSignal
        {
            Data = null
        });

        Assert.Null(result);
    }

    [Fact]
    public void Process_ReturnsNull_WhenBleSignalNull()
    {
        var assembler = new PacketAssembler();
        var result = assembler.Process(null);
        Assert.Null(result);
    }

    [Fact]
    public void Process_ReturnsNull_WhenChunkIsEmpty()
    {
        var assembler = new PacketAssembler();

        var result = assembler.Process(new BleSignal
        {
            Data = Array.Empty<byte>()
        });

        Assert.Null(result);
    }

    [Fact]
    public void Process_ReturnsSoundPackage_WhenNextStartArrives()
    {
        var assembler = new PacketAssembler();
        var soundPacket = BuildValidSoundPacket();

        var firstChunk = soundPacket.Take(10).ToArray();
        var secondChunk = soundPacket.Skip(10).ToArray();
        var nextStartChunk = BuildStartChunkOnly();

        Assert.Null(assembler.Process(Signal(firstChunk)));
        Assert.Null(assembler.Process(Signal(secondChunk)));

        var result = assembler.Process(Signal(nextStartChunk));

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.NotNull(result.SoundPackage);
        Assert.Null(result.FHRPackage);

        Assert.Equal(0x55, result.SoundPackage.Head1);
        Assert.Equal(0xAA, result.SoundPackage.Head2);
        Assert.Equal(0x09, result.SoundPackage.Control);

        // Проверяем, что полезные данные тоже доехали
        Assert.Equal(0x11, result.SoundPackage.Data[0]);
        Assert.Equal(0x22, result.SoundPackage.Data[1]);
        Assert.Equal(0x33, result.SoundPackage.AdpcmIndex);
        Assert.Equal(0x44, result.SoundPackage.AdpcmValueLow);
        Assert.Equal(0x55, result.SoundPackage.AdpcmValueHigh);
    }

    [Fact]
    public void Process_ReturnsCombinedPackage_WhenNextStartArrives()
    {
        var assembler = new PacketAssembler();
        var combinedPacket = BuildValidCombinedPacket();

        var firstChunk = combinedPacket.Take(15).ToArray();
        var secondChunk = combinedPacket.Skip(15).ToArray();
        var nextStartChunk = BuildStartChunkOnly();

        Assert.Null(assembler.Process(Signal(firstChunk)));
        Assert.Null(assembler.Process(Signal(secondChunk)));

        var result = assembler.Process(Signal(nextStartChunk));

        Assert.NotNull(result);
        Assert.True(result.IsValid);
        Assert.NotNull(result.SoundPackage);
        Assert.NotNull(result.FHRPackage);

        Assert.Equal(120, result.FHRPackage.Fhr1);
        Assert.Equal(0, result.FHRPackage.Fhr2);
        Assert.Equal(20, result.FHRPackage.Toco);
        Assert.True(result.FHRPackage.IsValid);
    }

    [Fact]
    public void Process_ReturnsNull_WhenBufferedPacketLengthIsInvalid()
    {
        var assembler = new PacketAssembler();
        var partialPacket = BuildValidSoundPacket().Take(25).ToArray();

        Assert.Null(assembler.Process(Signal(partialPacket)));

        var result = assembler.Process(Signal(BuildStartChunkOnly()));

        Assert.Null(result);
    }

    [Fact]
    public void Reset_ClearsBufferedData()
    {
        var assembler = new PacketAssembler();
        var soundPacket = BuildValidSoundPacket();

        Assert.Null(assembler.Process(Signal(soundPacket.Take(10).ToArray())));
        Assert.Null(assembler.Process(Signal(soundPacket.Skip(10).ToArray())));

        assembler.Reset();

        var result = assembler.Process(Signal(BuildStartChunkOnly()));

        Assert.Null(result);
    }

    private static BleSignal Signal(byte[] data) => new BleSignal
    {
        Data = data
    };

    private static byte[] BuildStartChunkOnly()
    {
        return new byte[] { 0x55, 0xAA, 0x09 };
    }

    private static byte[] BuildValidSoundPacket()
    {
        var data = new byte[SoundPackage.DataLength];

        data[0] = 0x55;
        data[1] = 0xAA;
        data[2] = 0x09;

        // Полезные байты звука
        data[3] = 0x11;
        data[4] = 0x22;

        // parity не проверяется в SoundPackage.IsValid, но пусть будет задан
        data[103] = 0x99;

        // ADPCM extended bytes
        data[104] = 0x33;
        data[105] = 0x44;
        data[106] = 0x55;

        return data;
    }

    private static byte[] BuildValidCombinedPacket()
    {
        var sound = BuildValidSoundPacket();
        var fhr = BuildValidFhrPacket();

        return sound.Concat(fhr).ToArray();
    }

    private static byte[] BuildValidFhrPacket()
    {
        var fhr1 = (byte)120;
        var fhr2 = (byte)0;
        var toco = (byte)20;
        var afm = (byte)0;
        var quality = (byte)0;
        var status2 = (byte)0;

        var parity = (byte)((fhr1 + fhr2 + toco + afm + quality + status2) % 256);

        return new[]
        {
            (byte)0x55,
            (byte)0xAA,
            (byte)0x03,
            fhr1,
            fhr2,
            toco,
            afm,
            quality,
            status2,
            parity
        };
    }
}