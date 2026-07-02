namespace AlpacaSpy
{
    public class FixedSizeConcurrentQueue<T>
    {
        private readonly System.Collections.Concurrent.ConcurrentQueue<T> _queue = new();
        private readonly int _maxSize;

        public FixedSizeConcurrentQueue(int maxSize)
        {
            if (maxSize <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxSize));

            _maxSize = maxSize;
        }

        public void Add(T item)
        {
            _queue.Enqueue(item);

            // Trim old items if we exceed the limit
            while (_queue.Count > _maxSize)
                _queue.TryDequeue(out _);
        }

        public int Count => _queue.Count;

        public T[] ToArray() => _queue.ToArray();
    }
}
