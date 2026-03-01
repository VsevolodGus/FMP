using Bioss.Ultrasound.Core.Domain.Collections;

namespace Bioss.Ultrasound.Maui.Helpers;

internal class LossPercentageHelper
{
    private const byte ErrorValue = 0;
    private readonly TimeQueue<byte> _queue = new(TimeSpan.FromMinutes(1));

    private int _countValues;
    private int _countZeroValues;

    public bool IsError => IsQueryFull && PercentInMin() > .25;

    public double PercentAll()
    {
        if (_countValues == 0)
            return 0;
        return (double)_countZeroValues / _countValues;
    }

    public double PercentInMin()
    {
        return _queue.Percent(ErrorValue);
    }

    public bool IsQueryFull => _queue.IsFull;

    public void Add(byte value)
    {
        _queue.Add(value);

        _countValues++;
        if (value == 0)
            _countZeroValues++;
    }

    public void Clear()
    {
        _queue.Clear();
        _countValues = 0;
        _countZeroValues = 0;
    }
}
