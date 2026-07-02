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
        private readonly object _sync = new();

        public int Capacity { get; }
        public int Count
        {
            get { lock (_sync) return _count; }
        }

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
            lock (_sync)
            {
                if (_count < Capacity)
                {
                    // Write at end without using modulo
                    int index = _start + _count;
                    if (index >= Capacity) index -= Capacity;
                    _buffer[index] = item;
                    _count++;
                }
                else
                {
                    // Overwrite oldest and advance start without modulo
                    _buffer[_start] = item;
                    _start++;
                    if (_start >= Capacity) _start = 0;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_sync)
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    int actual = _start + index;
                    if (actual >= Capacity) actual -= Capacity;
                    return _buffer[actual];
                }
            }
            set
            {
                lock (_sync)
                {
                    if (index < 0 || index >= _count)
                        throw new ArgumentOutOfRangeException(nameof(index));

                    int actual = _start + index;
                    if (actual >= Capacity) actual -= Capacity;
                    _buffer[actual] = value;
                }
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                Array.Clear(_buffer, 0, _buffer.Length);
                _start = 0;
                _count = 0;
            }
        }

        // Struct enumerator used by foreach on the concrete type to avoid heap allocations
        public Enumerator GetEnumerator()
        {
            // Capture snapshot of state under lock so enumeration sees a consistent window of items
            lock (_sync)
            {
                return new Enumerator(_buffer, _start, _count, Capacity);
            }
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            // Fall back to boxed enumerator for interface consumers (allocates)
            return GetEnumerator().AsBoxed();
        }

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] _buffer;
            private readonly int _start;
            private readonly int _count;
            private readonly int _capacity;
            private int _index; // -1 before start, 0.._count-1 during iteration
            private T _current;

            internal Enumerator(T[] buffer, int start, int count, int capacity)
            {
                _buffer = buffer;
                _start = start;
                _count = count;
                _capacity = capacity;
                _index = -1;
                _current = default!;
            }
            public T Current => _current;

            object IEnumerator.Current => _current!;

            public bool MoveNext()
            {
                int next = _index + 1;
                if (next >= _count) return false;
                _index = next;
                int actual = _start + _index;
                if (actual >= _capacity) actual -= _capacity;
                _current = _buffer[actual];
                return true;
            }

            public void Reset()
            {
                _index = -1;
                _current = default!;
            }

            public void Dispose() { }
        }
    }

    // Helper extensions for boxing the struct enumerator when an IEnumerator<T> is required.
    internal static class FixedCapacityListExtensions
    {
            public static IEnumerator<T> AsBoxed<T>(this FixedCapacityList<T>.Enumerator enumerator)
            {
                return new BoxedEnumerator<T>(enumerator);
            }

            private class BoxedEnumerator<T> : IEnumerator<T>
            {
                private FixedCapacityList<T>.Enumerator _enumerator;

                public BoxedEnumerator(FixedCapacityList<T>.Enumerator enumerator)
                {
                    _enumerator = enumerator;
                }

                public T Current => _enumerator.Current;
            object IEnumerator.Current => _enumerator.Current!;

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => _enumerator.Reset();

            public void Dispose() { }
        }
    }
}
