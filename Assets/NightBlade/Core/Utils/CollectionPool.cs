using System.Collections.Generic;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Generic collection pooling to reduce GC pressure from temporary Dictionary/List allocations.
    /// Particularly useful for data processing, serialization, and temporary collections.
    /// </summary>
    public static class CollectionPool<TKey, TValue>
    {
        private static readonly object _lock = new object();
        private static readonly Stack<Dictionary<TKey, TValue>> _pool = new Stack<Dictionary<TKey, TValue>>();
        private const int MaxPoolSize = 8;
        private const int DefaultCapacity = 16;

        /// <summary>
        /// Gets a Dictionary from the pool or creates a new one.
        /// </summary>
        /// <param name="capacity">Initial capacity for the Dictionary</param>
        /// <returns>A Dictionary ready for use</returns>
        public static Dictionary<TKey, TValue> GetDictionary(int capacity = DefaultCapacity)
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var dict = _pool.Pop();
                    dict.Clear();
                    // Note: Dictionary doesn't have a Capacity property like List does
                    // We'll just reuse it as-is for simplicity
                    return dict;
                }
            }

            return new Dictionary<TKey, TValue>(capacity);
        }

        /// <summary>
        /// Returns a Dictionary to the pool for reuse.
        /// </summary>
        /// <param name="dict">The Dictionary to return to the pool</param>
        public static void ReturnDictionary(Dictionary<TKey, TValue> dict)
        {
            if (dict == null) return;

            lock (_lock)
            {
                if (_pool.Count < MaxPoolSize)
                {
                    dict.Clear();
                    _pool.Push(dict);
                }
            }
        }
    }

    /// <summary>
    /// List pooling utilities for common types.
    /// </summary>
    public static class ListPool<T>
    {
        private static readonly object _lock = new object();
        private static readonly Stack<List<T>> _pool = new Stack<List<T>>();
        private const int MaxPoolSize = 8;
        private const int DefaultCapacity = 16;

        /// <summary>
        /// Gets a List from the pool or creates a new one.
        /// </summary>
        /// <param name="capacity">Initial capacity for the List</param>
        /// <returns>A List ready for use</returns>
        public static List<T> GetList(int capacity = DefaultCapacity)
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var list = _pool.Pop();
                    list.Clear();
                    if (list.Capacity < capacity)
                    {
                        list.Capacity = capacity;
                    }
                    return list;
                }
            }

            return new List<T>(capacity);
        }

        /// <summary>
        /// Returns a List to the pool for reuse.
        /// </summary>
        /// <param name="list">The List to return to the pool</param>
        public static void ReturnList(List<T> list)
        {
            if (list == null) return;

            lock (_lock)
            {
                if (_pool.Count < MaxPoolSize)
                {
                    list.Clear();
                    _pool.Push(list);
                }
            }
        }

        /// <summary>
        /// Gets the current pool size for debugging.
        /// </summary>
        public static int PoolSize
        {
            get
            {
                lock (_lock)
                {
                    return _pool.Count;
                }
            }
        }
    }

    /// <summary>
    /// Convenience methods for common collection types.
    /// </summary>
    public static class CollectionPools
    {
        /// <summary>
        /// String-keyed dictionary pool.
        /// </summary>
        public static class StringDictionary<TValue>
        {
            public static Dictionary<string, TValue> Get(int capacity = 16) =>
                CollectionPool<string, TValue>.GetDictionary(capacity);

            public static void Return(Dictionary<string, TValue> dict) =>
                CollectionPool<string, TValue>.ReturnDictionary(dict);
        }

        /// <summary>
        /// Integer-keyed dictionary pool.
        /// </summary>
        public static class IntDictionary<TValue>
        {
            public static Dictionary<int, TValue> Get(int capacity = 16) =>
                CollectionPool<int, TValue>.GetDictionary(capacity);

            public static void Return(Dictionary<int, TValue> dict) =>
                CollectionPool<int, TValue>.ReturnDictionary(dict);
        }
    }
}