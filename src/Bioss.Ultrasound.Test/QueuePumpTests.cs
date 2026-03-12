using Bioss.Ultrasound.Services;
using System.Collections.Concurrent;

namespace Bioss.Ultrasound.Test;

public class QueuePumpTests
{
    private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    [Fact]
    public async Task StopAsync_WhenNeverStarted_DoesNotThrow()
    {
        var pump = new QueuePump<int>(_ => Task.CompletedTask, emptyDelayMs: 1);

        var ex = await Record.ExceptionAsync(() => pump.StopAsync());

        Assert.Null(ex);
    }

    [Fact]
    public async Task Enqueue_BeforeStart_IsProcessed_AfterStart()
    {
        var processed = new List<int>();
        var sync = new object();
        var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pump = new QueuePump<int>(item =>
        {
            lock (sync)
            {
                processed.Add(item);
                if (processed.Count == 2)
                    done.TrySetResult(true);
            }

            return Task.CompletedTask;
        }, emptyDelayMs: 1);

        pump.Enqueue(1);
        pump.Enqueue(2);

        pump.Start();

        await done.Task.WaitAsync(DefaultTimeout);
        await pump.StopAsync();

        Assert.Equal(new[] { 1, 2 }, processed);
    }

    [Fact]
    public async Task Processes_Items_In_Fifo_Order()
    {
        var processed = new List<int>();
        var sync = new object();
        var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pump = new QueuePump<int>(item =>
        {
            lock (sync)
            {
                processed.Add(item);
                if (processed.Count == 5)
                    done.TrySetResult(true);
            }

            return Task.CompletedTask;
        }, emptyDelayMs: 1);

        pump.Start();

        for (var i = 1; i <= 5; i++)
            pump.Enqueue(i);

        await done.Task.WaitAsync(DefaultTimeout);
        await pump.StopAsync();

        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, processed);
    }

    [Fact]
    public async Task Reset_Clears_Pending_Items_But_Does_Not_Cancel_Item_Already_In_Progress()
    {
        var processed = new ConcurrentQueue<int>();
        var firstStarted = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        var processedCount = 0;

        var pump = new QueuePump<int>(async item =>
        {
            if (item == 1)
            {
                firstStarted.TrySetResult(true);
                await releaseFirst.Task;
            }

            processed.Enqueue(item);
            if (Interlocked.Increment(ref processedCount) == 1)
            {
                // ничего
            }
        }, emptyDelayMs: 1);

        pump.Start();

        pump.Enqueue(1);
        pump.Enqueue(2);
        pump.Enqueue(3);

        await firstStarted.Task.WaitAsync(DefaultTimeout);

        pump.Reset();
        releaseFirst.TrySetResult(true);

        await WaitUntilAsync(() => Volatile.Read(ref processedCount) == 1, DefaultTimeout);
        await Task.Delay(100);

        await pump.StopAsync();

        Assert.Equal(new[] { 1 }, processed.ToArray());
    }

    [Fact]
    public async Task StopAsync_Stops_Processing_And_Items_Enqueued_After_Stop_Are_Not_Processed()
    {
        var processed = new ConcurrentQueue<int>();
        var firstProcessed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pump = new QueuePump<int>(item =>
        {
            processed.Enqueue(item);
            if (item == 1)
                firstProcessed.TrySetResult(true);

            return Task.CompletedTask;
        }, emptyDelayMs: 1);

        pump.Start();

        pump.Enqueue(1);
        await firstProcessed.Task.WaitAsync(DefaultTimeout);

        await pump.StopAsync();

        pump.Enqueue(2);
        pump.Enqueue(3);

        await Task.Delay(100);

        Assert.Equal(new[] { 1 }, processed.ToArray());
    }

    [Fact]
    public async Task Can_Restart_After_Stop()
    {
        var processed = new ConcurrentQueue<int>();
        var processedCount = 0;
        TaskCompletionSource<bool> done = new(TaskCreationOptions.RunContinuationsAsynchronously);

        var pump = new QueuePump<int>(item =>
        {
            processed.Enqueue(item);

            var count = Interlocked.Increment(ref processedCount);
            if (count == 1 || count == 2)
                done.TrySetResult(true);

            return Task.CompletedTask;
        }, emptyDelayMs: 1);

        pump.Start();
        pump.Enqueue(1);

        await done.Task.WaitAsync(DefaultTimeout);
        await pump.StopAsync();

        done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        pump.Start();
        pump.Enqueue(2);

        await WaitUntilAsync(() => Volatile.Read(ref processedCount) == 2, DefaultTimeout);
        await pump.StopAsync();

        Assert.Equal(new[] { 1, 2 }, processed.ToArray());
    }

    [Fact]
    public async Task Item_Enqueued_After_Idle_Period_Is_Still_Processed()
    {
        var done = new TaskCompletionSource<int>(TaskCreationOptions.RunContinuationsAsynchronously);

        var pump = new QueuePump<int>(item =>
        {
            done.TrySetResult(item);
            return Task.CompletedTask;
        }, emptyDelayMs: 5);

        pump.Start();

        await Task.Delay(50);
        pump.Enqueue(42);

        var result = await done.Task.WaitAsync(DefaultTimeout);
        await pump.StopAsync();

        Assert.Equal(42, result);
    }

    private static async Task WaitUntilAsync(Func<bool> predicate, TimeSpan timeout, int pollDelayMs = 10)
    {
        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < timeout)
        {
            if (predicate())
                return;

            await Task.Delay(pollDelayMs);
        }

        Assert.True(predicate(), "Condition was not met within the timeout.");
    }
}