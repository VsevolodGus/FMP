using Bioss.Ultrasound.Collections;

namespace Bioss.Ultrasound.Test.Collections;

public class RingBufferTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Ctor_InvalidCapacity_ThrowsArgumentException(int capacity)
    {
        Assert.Throws<ArgumentException>(() => new RingBuffer<int>(capacity));
    }

    [Fact]
    public void Ctor_ValidCapacity_InitialStateIsCorrect()
    {
        var buffer = new RingBuffer<int>(3);

        Assert.Equal(3, buffer.Capacity);
        Assert.Equal(0, buffer.Head);
        Assert.Equal(0, buffer.Tail);
        Assert.Equal(0, buffer.Count);
        Assert.Equal(3, buffer.Available);
        Assert.True(buffer.IsEmpty);
        Assert.False(buffer.IsFull);
    }

    [Fact]
    public void Push_And_Pop_SingleValue_Works()
    {
        var buffer = new RingBuffer<int>(3);

        buffer.Push(42);

        Assert.Equal(1, buffer.Count);
        Assert.Equal(2, buffer.Available);
        Assert.False(buffer.IsEmpty);
        Assert.False(buffer.IsFull);
        Assert.Equal(1, buffer.Head);
        Assert.Equal(0, buffer.Tail);

        var value = buffer.Pop();

        Assert.Equal(42, value);
        Assert.Equal(0, buffer.Count);
        Assert.Equal(3, buffer.Available);
        Assert.True(buffer.IsEmpty);
        Assert.False(buffer.IsFull);
        Assert.Equal(1, buffer.Head);
        Assert.Equal(1, buffer.Tail);
    }

    [Fact]
    public void Push_WithDropFalse_WhenNotFull_ReturnsZero_AndWritesValue()
    {
        var buffer = new RingBuffer<int>(2);

        var skipped = buffer.Push(10, drop: false);

        Assert.Equal(0, skipped);
        Assert.Equal(1, buffer.Count);
        Assert.Equal(10, buffer.Pop());
    }

    [Fact]
    public void Pop_FromEmpty_ThrowsIndexOutOfRangeException()
    {
        var buffer = new RingBuffer<int>(2);

        Assert.Throws<IndexOutOfRangeException>(() => buffer.Pop());
    }

    [Fact]
    public void Push_WithDrop_WhenBufferIsFull_SkipsValue()
    {
        var buffer = new RingBuffer<int>(2);
        buffer.Push(1);
        buffer.Push(2);

        var skipped = buffer.Push(3, drop: true);

        Assert.Equal(1, skipped);
        Assert.Equal(2, buffer.Count);
        Assert.True(buffer.IsFull);
        Assert.Equal(1, buffer.Pop());
        Assert.Equal(2, buffer.Pop());
    }

    [Fact]
    public void PushArray_WritesItemsInOrder()
    {
        var buffer = new RingBuffer<int>(5);

        buffer.Push(new[] { 1, 2, 3 });

        Assert.Equal(3, buffer.Count);
        Assert.Equal(2, buffer.Available);
        Assert.Equal(3, buffer.Head);
        Assert.Equal(0, buffer.Tail);

        var values = buffer.Pop(3);

        Assert.Equal(new[] { 1, 2, 3 }, values);
        Assert.True(buffer.IsEmpty);
    }

    [Fact]
    public void PushArray_WithDrop_WhenBufferAlreadyFull_SkipsAllItems()
    {
        var buffer = new RingBuffer<int>(2);
        buffer.Push(new[] { 1, 2 });

        var dropped = buffer.Push(new[] { 3, 4 }, drop: true);

        Assert.Equal(2, dropped);
        Assert.Equal(2, buffer.Count);
        Assert.Equal(new[] { 1, 2 }, buffer.Pop(2));
    }

    [Fact]
    public void PushArray_WithDrop_WritesOnlyAvailablePart_AndReturnsDroppedCount()
    {
        var buffer = new RingBuffer<int>(5);
        buffer.Push(new[] { 1, 2, 3 });

        var dropped = buffer.Push(new[] { 4, 5, 6, 7 }, drop: true);

        Assert.Equal(2, dropped);
        Assert.Equal(5, buffer.Count);
        Assert.True(buffer.IsFull);
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, buffer.Pop(5));
    }

    [Fact]
    public void Pop_Multiple_WhenAmountExceedsCount_ReturnsNull()
    {
        var buffer = new RingBuffer<int>(3);
        buffer.Push(new[] { 1, 2 });

        var values = buffer.Pop(3);

        Assert.Null(values);
        Assert.Equal(2, buffer.Count);
    }

    [Fact]
    public void Pop_Multiple_ReturnsItemsInOrder_AndAdvancesTail()
    {
        var buffer = new RingBuffer<int>(5);
        buffer.Push(new[] { 1, 2, 3, 4 });

        var first = buffer.Pop(2);
        var second = buffer.Pop(2);

        Assert.Equal(new[] { 1, 2 }, first);
        Assert.Equal(new[] { 3, 4 }, second);
        Assert.Equal(0, buffer.Count);
        Assert.True(buffer.IsEmpty);
    }

    [Fact]
    public void Buffer_WrapsAround_AfterPopAndPush()
    {
        var buffer = new RingBuffer<int>(3);
        buffer.Push(new[] { 1, 2, 3 });

        var first = buffer.Pop();
        buffer.Push(4);

        Assert.Equal(1, first);
        Assert.Equal(3, buffer.Count);
        Assert.True(buffer.IsFull);
        Assert.Equal(1, buffer.Head);
        Assert.Equal(1, buffer.Tail);
        Assert.Equal(new[] { 2, 3, 4 }, buffer.Pop(3));
    }

    [Fact]
    public void PushArray_WithoutDrop_WhenOverflow_ShouldKeepLatestItems()
    {
        var buffer = new RingBuffer<int>(3);
        buffer.Push([1, 2 ]);

        buffer.Push([3, 4, 5 ]);

        Assert.Equal(2, buffer.Count);
        Assert.False(buffer.IsFull);
        Assert.Equal([ 4, 5 ], buffer.Pop(2));
    }
}