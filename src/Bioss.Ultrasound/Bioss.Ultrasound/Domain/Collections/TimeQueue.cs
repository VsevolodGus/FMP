using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Bioss.Ultrasound.Domain.Collections
{
    public class TimeQueue<T>
    {
        private readonly ConcurrentQueue<KeyValuePair<DateTime, T>> _queue = new();
        private readonly TimeSpan _span;
        
        public TimeQueue(TimeSpan span)
        {
            _span = span;
        }

        public bool IsFull { get; private set; }

        public void Add(T value)
        {
            var now = DateTime.Now;
            _queue.Enqueue(new(now, value));

            var cutTime = now.Subtract(_span);
            
            while (true)
            {
                if (!_queue.TryPeek(out var element))
                    break;

                if (element.Key >= cutTime)
                    break;

                IsFull = true;
                _queue.TryDequeue(out _);
            }
        }

        public double Percent(T value)
        {
            if (_queue.IsEmpty)
                return .0;

            var valueCount = _queue.Count(a => EqualityComparer<T>.Default.Equals(a.Value, value));
            return (double)valueCount / _queue.Count;
        }

        public void Clear()
        {
            _queue.Clear();
            IsFull = false;
        }
    }
}
