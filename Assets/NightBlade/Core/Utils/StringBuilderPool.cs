using System.Collections.Generic;
using System.Text;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// Simple StringBuilder pooling to reduce GC pressure.
    /// </summary>
    public static class StringBuilderPool
    {
        private static readonly Stack<StringBuilder> _pool = new Stack<StringBuilder>();
        private const int MaxPoolSize = 10;

        /// <summary>
        /// Gets a StringBuilder from the pool or creates a new one.
        /// </summary>
        public static StringBuilder Get()
        {
            if (_pool.Count > 0)
            {
                var sb = _pool.Pop();
                sb.Clear();
                return sb;
            }
            return new StringBuilder();
        }

        /// <summary>
        /// Returns a StringBuilder to the pool.
        /// </summary>
        public static void Return(StringBuilder sb)
        {
            if (sb != null && _pool.Count < MaxPoolSize)
            {
                _pool.Push(sb);
            }
        }

        /// <summary>
        /// Uses a pooled StringBuilder and automatically returns it.
        /// </summary>
        public static string Use(System.Func<StringBuilder, string> func)
        {
            var sb = Get();
            try
            {
                return func(sb);
            }
            finally
            {
                Return(sb);
            }
        }

        /// <summary>
        /// Gets the current pool size for debugging.
        /// </summary>
        public static int PoolSize => _pool.Count;
    }
}