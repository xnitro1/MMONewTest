using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using System.Text;

namespace NightBlade.Core.Utils
{
    /// <summary>
    /// JSON operation pooling to reduce GC pressure from frequent JSON serialization/deserialization.
    /// Critical for save/load operations and data persistence in MMOs.
    /// </summary>
    public static class JsonOperationPool
    {
        private static readonly object _lock = new object();
        private static readonly Stack<StringBuilder> _stringBuilderPool = new Stack<StringBuilder>();
        private const int MaxPoolSize = 8;

        /// <summary>
        /// Gets a pooled StringBuilder for JSON operations.
        /// </summary>
        public static StringBuilder GetStringBuilder()
        {
            lock (_lock)
            {
                if (_stringBuilderPool.Count > 0)
                {
                    var sb = _stringBuilderPool.Pop();
                    sb.Clear();
                    return sb;
                }
            }
            return new StringBuilder();
        }

        /// <summary>
        /// Returns a StringBuilder to the pool.
        /// </summary>
        public static void ReturnStringBuilder(StringBuilder sb)
        {
            if (sb == null) return;

            lock (_lock)
            {
                if (_stringBuilderPool.Count < MaxPoolSize)
                {
                    _stringBuilderPool.Push(sb);
                }
            }
        }


        /// <summary>
        /// Serializes an object to JSON using pooled StringBuilder.
        /// </summary>
        public static string SerializeObject<T>(T obj, JsonSerializerSettings settings = null)
        {
            var stringBuilder = GetStringBuilder();
            try
            {
                using (var stringWriter = new StringWriter(stringBuilder))
                using (var jsonWriter = new JsonTextWriter(stringWriter))
                {
                    jsonWriter.Formatting = Formatting.None; // Minimize output
                    var serializer = JsonSerializer.CreateDefault(settings);
                    serializer.Serialize(jsonWriter, obj);
                }
                return stringBuilder.ToString();
            }
            finally
            {
                ReturnStringBuilder(stringBuilder);
            }
        }

        /// <summary>
        /// Deserializes JSON to an object using pooled resources.
        /// </summary>
        public static T DeserializeObject<T>(string json, JsonSerializerSettings settings = null)
        {
            var stringReader = new StringReader(json);
            try
            {
                using (var jsonReader = new JsonTextReader(stringReader))
                {
                    var serializer = JsonSerializer.CreateDefault(settings);
                    return serializer.Deserialize<T>(jsonReader);
                }
            }
            finally
            {
                stringReader.Dispose();
            }
        }

        /// <summary>
        /// Gets the current pool sizes for debugging.
        /// </summary>
        public static (int stringBuilderPool, int stringWriterPool) PoolSizes
        {
            get
            {
                lock (_lock)
                {
                    return (_stringBuilderPool.Count, 0); // Only StringBuilder pool now
                }
            }
        }

        /// <summary>
        /// Clears all pools (useful for testing or memory management).
        /// </summary>
        public static void Clear()
        {
            lock (_lock)
            {
                _stringBuilderPool.Clear();
            }
        }
    }
}