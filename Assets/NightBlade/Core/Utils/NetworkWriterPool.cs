using System.Collections.Generic;
using LiteNetLib.Utils;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// NetDataWriter pooling to reduce GC pressure from frequent network message serialization.
    /// Essential for high-frequency network operations in MMOs.
    /// </summary>
    public static class NetworkWriterPool
    {
        private static readonly object _lock = new object();
        private static readonly Stack<NetDataWriter> _pool = new Stack<NetDataWriter>();
        private const int MaxPoolSize = 32;

        /// <summary>
        /// Gets a NetDataWriter from the pool or creates a new one.
        /// </summary>
        /// <returns>A clean NetDataWriter ready for use</returns>
        public static NetDataWriter Get()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                {
                    var writer = _pool.Pop();
                    writer.Reset();
                    return writer;
                }
            }

            return new NetDataWriter();
        }

        /// <summary>
        /// Returns a NetDataWriter to the pool for reuse.
        /// </summary>
        /// <param name="writer">The NetDataWriter to return to the pool</param>
        public static void Return(NetDataWriter writer)
        {
            if (writer == null) return;

            lock (_lock)
            {
                if (_pool.Count < MaxPoolSize)
                {
                    _pool.Push(writer);
                }
            }
        }

        /// <summary>
        /// Gets a NetDataWriter, executes an action with it, and automatically returns it to the pool.
        /// </summary>
        /// <param name="action">Action to perform with the NetDataWriter</param>
        public static void Use(System.Action<NetDataWriter> action)
        {
            var writer = Get();
            try
            {
                action(writer);
            }
            finally
            {
                Return(writer);
            }
        }

        /// <summary>
        /// Gets a NetDataWriter, executes a function that returns a result, and returns the NetDataWriter to the pool.
        /// </summary>
        /// <typeparam name="T">Return type of the function</typeparam>
        /// <param name="func">Function to execute with the NetDataWriter</param>
        /// <returns>Result of the function</returns>
        public static T Use<T>(System.Func<NetDataWriter, T> func)
        {
            var writer = Get();
            try
            {
                return func(writer);
            }
            finally
            {
                Return(writer);
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

        /// <summary>
        /// Clears the pool (useful for testing or memory management).
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _pool.Clear();
            }
        }
    }
}