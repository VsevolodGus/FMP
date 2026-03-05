using System;

namespace Bioss.Ultrasound.Collections
{
    public class RingBuffer<T>
    {
        private readonly object _locker = new object();

        private T[] _items;
        private int _count = 0;

        public RingBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be greater than 0");

            Capacity = capacity;
            _items = new T[capacity];
        }

        // Maximum number of elements in the buffer
        public int Capacity { get; private set; }
        // Write index
        public int Head { get; private set; }
        // Read index
        public int Tail { get; private set; }

        // Number of elements in the buffer
        public int Count
        {
            get
            {
                lock (_locker)
                {
                    return _count;
                }
            }
        }

        // Available amount of elements that could be written to the buffer
        public int Available => Capacity - Count;

        // Returns true if buffer is full
        public bool IsFull => Capacity == Count;

        // Returns true if buffer is empty
        public bool IsEmpty => Head == Tail && !IsFull;


        // Push single element (override on overflow by default)
        public void Push(T value)
        {
            _items[Head] = value;
            Head = (Head + 1) % Capacity;
            AtomicCountAdd(1);
        }

        /// <summary>
        /// Push single element with specifying write behavior
        /// </summary>
        /// <param name="value">Value to write</param>
        /// <param name="drop">Skip element if buffer is full</param>
        /// <returns>1 if value was skipped, otherwise 0</returns>
        public int Push(T value, bool drop)
        {
            if (!IsFull || !drop)
            {
                Push(value);
                return 0;
            }
            return 1;
        }

        // Push multiple  elements (override on overflow by default)
        public void Push(T[] values)
        {
            for(var i = 0; i < values.Length; ++i)
                _items[(Head + i) % Capacity] = values[i];

            Head = (Head + values.Length) % Capacity;
            AtomicCountAdd(values.Length);
        }

        /// <summary>
        /// Push multiple  elements with specifying write behavior
        /// </summary>
        /// <param name="values">Values to write</param>
        /// <param name="drop">Skip element if buffer is full</param>
        /// <returns>Number of skipped elements</returns>
        public int Push(T[] values, bool drop)
        {
            if (IsFull && drop)
                return values.Length;

            var dropped = 0;
            var amount = values.Length;

            if (values.Length > Available && drop)
            {
                amount = Available;
                dropped = values.Length - amount;
            }

            for (var i = 0; i < amount; ++i)
                _items[(Head + i) % Capacity] = values[i];

            Head = (Head + amount) % Capacity;
            AtomicCountAdd(amount);

            return dropped;
        }

        // Pop single element
        public T Pop()
        {
            if (IsEmpty)
                throw new IndexOutOfRangeException("Can not pop element in empty buffer");

            var item = _items[Tail];
            Tail = (Tail + 1) % Capacity;
            AtomicCountAdd(-1);
            return item;
        }

        /// <summary>
        /// Pop multiple  elements
        /// </summary>
        /// <param name="amount">Number of elements to read</param>
        /// <returns>Array of elements or `null` if requested amount is greater than current buffer size</returns>
        public T[] Pop(int amount)
        {
            if (IsEmpty || amount > Count)
                return null;

            var values = new T[amount];
            for (var i = 0; i < amount; ++i)
                values[i] = _items[(Tail + i) % Capacity];

            Tail = (Tail + amount) % Capacity;
            AtomicCountAdd(-amount);

            return values;
        }


        private void AtomicCountAdd(int value)
        {
            lock (_locker)
            {
                if (_count + value > Capacity)
                    _count = (_count + value) % Capacity;
                else
                    _count += value;
            }
        }
    }
}
