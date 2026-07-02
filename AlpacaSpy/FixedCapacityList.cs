namespace AlpacaSpy
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class FixedCapacityList<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _start;   // index of the oldest item
        private int _count;   // number of items currently stored

        public int Capacity { get; }
        public int Count => _count;

        public FixedCapacityList(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            Capacity = capacity;
            _buffer = new T[capacity];
            _start = 0;
            _count = 0;
        }

        public void Add(T item)
        {
            if (_count < Capacity)
            {
                // Write at end
                int index = (_start + _count) % Capacity;
                _buffer[index] = item;
                _count++;
            }
            else
            {
                // Overwrite oldest
                _buffer[_start] = item;
                _start = (_start + 1) % Capacity;
            }
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                int actual = (_start + index) % Capacity;
                return _buffer[actual];
            }
            set
            {
                if (index < 0 || index >= _count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                int actual = (_start + index) % Capacity;
                _buffer[actual] = value;
            }
        }

        public void Clear()
        {
            Array.Clear(_buffer, 0, _buffer.Length);
            _start = 0;
            _count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < _count; i++)
            {
                int actual = (_start + i) % Capacity;
                yield return _buffer[actual];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
