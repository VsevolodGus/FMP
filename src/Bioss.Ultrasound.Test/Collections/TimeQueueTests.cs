using Bioss.Ultrasound.Domain.Collections;

namespace Bioss.Ultrasound.Test.Collections;

public class TimeQueueTests
{
    [Fact]
    public void NewQueue_IsNotFull_AndPercentIsZero()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromSeconds(1));

        Assert.False(queue.IsFull);
        Assert.Equal(0d, queue.Percent(1));
    }

    [Fact]
    public void Add_SingleValue_PercentBecomesOne()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromSeconds(1));

        queue.Add(10);

        Assert.False(queue.IsFull);
        Assert.Equal(1d, queue.Percent(10));
        Assert.Equal(0d, queue.Percent(20));
    }

    [Fact]
    public void Percent_ReturnsCorrectRatio_ForSeveralValues()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromSeconds(1));

        queue.Add(1);
        queue.Add(1);
        queue.Add(2);

        Assert.Equal(2d / 3d, queue.Percent(1), 6);
        Assert.Equal(1d / 3d, queue.Percent(2), 6);
        Assert.Equal(0d, queue.Percent(3));
    }

    [Fact]
    public void Add_RemovesExpiredItems_WhenTheyAreOutOfWindow()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromMilliseconds(30));

        queue.Add(1);
        Thread.Sleep(80);
        queue.Add(2);

        Assert.True(queue.IsFull);
        Assert.Equal(0d, queue.Percent(1));
        Assert.Equal(1d, queue.Percent(2));
    }

    [Fact]
    public void Add_RemovesSeveralExpiredItems()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromMilliseconds(40));

        queue.Add(1);
        queue.Add(1);
        Thread.Sleep(90);
        queue.Add(2);
        queue.Add(2);

        Assert.True(queue.IsFull);
        Assert.Equal(0d, queue.Percent(1));
        Assert.Equal(1d, queue.Percent(2));
    }

    [Fact]
    public void Clear_EmptiesQueue_AndResetsIsFull()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromMilliseconds(30));

        queue.Add(1);
        Thread.Sleep(80);
        queue.Add(2);

        Assert.True(queue.IsFull);

        queue.Clear();

        Assert.False(queue.IsFull);
        Assert.Equal(0d, queue.Percent(1));
        Assert.Equal(0d, queue.Percent(2));
    }

    [Fact]
    public void IsFull_StaysFalse_WhileNothingExpires()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromSeconds(1));

        queue.Add(1);
        queue.Add(2);
        queue.Add(3);

        Assert.False(queue.IsFull);
    }

    [Fact]
    public void IsFull_BecomesTrue_AfterFirstEviction_AndDoesNotResetUntilClear()
    {
        var queue = new TimeQueue<int>(TimeSpan.FromMilliseconds(30));

        queue.Add(1);
        Thread.Sleep(80);
        queue.Add(2);

        Assert.True(queue.IsFull);

        queue.Add(3);

        Assert.True(queue.IsFull);
    }
}