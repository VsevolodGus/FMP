using Bioss.Ultrasound.Ble.Models;
using Bioss.Ultrasound.Services;

namespace Bioss.Ultrasound.Test;

public class DeviceStreamProcessorTests
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan ShortDelay = TimeSpan.FromMilliseconds(200);

    [Fact]
    public async Task Start_WhenValidPacketSequenceArrives_RaisesPackageReady()
    {
        var assembler = new PacketAssembler();
        var processor = new DeviceStreamProcessor(assembler, logger: null);

        var tcs = new TaskCompletionSource<Package>(TaskCreationOptions.RunContinuationsAsynchronously);
        processor.PackageReady = package =>
        {
            tcs.TrySetResult(package);
            return Task.CompletedTask;
        };

        processor.Start();

        processor.OnSignal(Signal(BuildValidSoundPacket()));
        processor.OnSignal(Signal(BuildStartChunkOnly()));

        var package = await WaitOrThrowAsync(tcs.Task, Timeout);

        await processor.StopAsync();

        Assert.NotNull(package);
        Assert.True(package.IsValid);
        Assert.NotNull(package.SoundPackage);
        Assert.Null(package.FHRPackage);
    }

    [Fact]
    public async Task Reset_ClearsBufferedPartialPacket()
    {
        var assembler = new PacketAssembler();
        var processor = new DeviceStreamProcessor(assembler, logger: null);

        var called = false;
        processor.PackageReady = package =>
        {
            called = true;
            return Task.CompletedTask;
        };

        processor.Start();

        var partial = BuildValidSoundPacket().Take(20).ToArray();
        processor.OnSignal(Signal(partial));

        await Task.Delay(ShortDelay);

        processor.Reset();

        processor.OnSignal(Signal(BuildStartChunkOnly()));

        await Task.Delay(ShortDelay);
        await processor.StopAsync();

        Assert.False(called);
    }

    [Fact]
    public async Task StopAsync_ClearsState_AndOldPartialPacketIsNotUsedAfterRestart()
    {
        var assembler = new PacketAssembler();
        var processor = new DeviceStreamProcessor(assembler, logger: null);

        var called = false;
        processor.PackageReady = package =>
        {
            called = true;
            return Task.CompletedTask;
        };

        processor.Start();

        var partial = BuildValidSoundPacket().Take(25).ToArray();
        processor.OnSignal(Signal(partial));

        await Task.Delay(ShortDelay);

        await processor.StopAsync();

        processor.Start();
        processor.OnSignal(Signal(BuildStartChunkOnly()));

        await Task.Delay(ShortDelay);
        await processor.StopAsync();

        Assert.False(called);
    }

    [Fact]
    public async Task Start_DropsSignalsThatWereQueuedBeforeStart()
    {
        var assembler = new PacketAssembler();
        var processor = new DeviceStreamProcessor(assembler, logger: null);

        var called = false;
        processor.PackageReady = package =>
        {
            called = true;
            return Task.CompletedTask;
        };

        // кладем сигналы до запуска
        processor.OnSignal(Signal(BuildValidSoundPacket()));
        processor.OnSignal(Signal(BuildStartChunkOnly()));

        // Start делает Reset queue + Reset assembler
        processor.Start();

        await Task.Delay(ShortDelay);
        await processor.StopAsync();

        Assert.False(called);
    }

    private static BleSignal Signal(byte[] data)
        => new BleSignal { Data = data };

    private static async Task<T> WaitOrThrowAsync<T>(Task<T> task, TimeSpan timeout)
    {
        var completed = await Task.WhenAny(task, Task.Delay(timeout));
        if (completed != task)
            throw new TimeoutException("Expected task was not completed in time.");

        return await task;
    }

    private static byte[] BuildStartChunkOnly()
        => new byte[] { 0x55, 0xAA, 0x09 };

    private static byte[] BuildValidSoundPacket()
    {
        var data = new byte[SoundPackage.DataLength];

        data[0] = 0x55;
        data[1] = 0xAA;
        data[2] = 0x09;

        data[3] = 0x11;
        data[4] = 0x22;

        // parity/служебный байт
        data[103] = 0x99;

        // ADPCM extra bytes
        data[104] = 0x33;
        data[105] = 0x44;
        data[106] = 0x55;

        return data;
    }
}