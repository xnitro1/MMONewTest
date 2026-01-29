using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using UnityEngine;
using NightBlade.Core.Utils;

namespace NightBlade
{
    /// <summary>
    /// Modern async save system replacing deprecated BinaryFormatter.
    /// Uses JSON serialization with optional GZip compression.
    /// All I/O operations are async to avoid blocking the main thread.
    /// </summary>
    public static class SaveSystem
    {
        private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.None // Compact for smaller files
        };

        // Register custom converters for Unity types
        static SaveSystem()
        {
            JsonSettings.Converters.Add(new Vector3Converter());
            JsonSettings.Converters.Add(new QuaternionConverter());
            JsonSettings.Converters.Add(new Vec3Converter());
        }

        /// <summary>
        /// Save data asynchronously with optional compression.
        /// Uses atomic write (temp file + rename) to prevent corruption.
        /// </summary>
        public static async UniTask SaveAsync<T>(string path, T data, bool compress = true, CancellationToken ct = default)
        {
            try
            {
                string json = JsonOperationPool.SerializeObject(data, JsonSettings);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                if (compress)
                {
                    bytes = Compress(bytes);
                }

                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write to temp file first, then move (atomic operation)
                string tempPath = path + ".tmp";
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None,
                    bufferSize: 4096, useAsync: true))
                {
                    await stream.WriteAsync(bytes, 0, bytes.Length, ct);
                }

                // Atomic replace
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(tempPath, path);
            }
            catch (OperationCanceledException)
            {
                // Save was cancelled, clean up temp file if it exists
                string tempPath = path + ".tmp";
                if (File.Exists(tempPath))
                {
                    try { File.Delete(tempPath); } catch { }
                }
                throw;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to save {path}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Load data asynchronously.
        /// Returns a new instance of T if file doesn't exist or on error.
        /// </summary>
        public static async UniTask<T> LoadAsync<T>(string path, bool compressed = true, CancellationToken ct = default) where T : new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }

            try
            {
                byte[] bytes;
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: 4096, useAsync: true))
                {
                    bytes = new byte[stream.Length];
                    await stream.ReadAsync(bytes, 0, bytes.Length, ct);
                }

                if (compressed)
                {
                    bytes = Decompress(bytes);
                }

                string json = Encoding.UTF8.GetString(bytes);
                return JsonOperationPool.DeserializeObject<T>(json, JsonSettings) ?? new T();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load {path}: {ex.Message}");
                return new T();
            }
        }

        /// <summary>
        /// Synchronous save - uses synchronous I/O. Use SaveAsync when possible.
        /// </summary>
        public static void Save<T>(string path, T data, bool compress = true)
        {
            try
            {
                string json = JsonOperationPool.SerializeObject(data, JsonSettings);
                byte[] bytes = Encoding.UTF8.GetBytes(json);

                if (compress)
                {
                    bytes = Compress(bytes);
                }

                // Ensure directory exists
                string directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Write to temp file first, then move (atomic operation)
                string tempPath = path + ".tmp";
                File.WriteAllBytes(tempPath, bytes);

                // Atomic replace
                if (File.Exists(path))
                    File.Delete(path);
                File.Move(tempPath, path);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to save {path}: {ex.Message}");
            }
        }

        /// <summary>
        /// Synchronous load - uses synchronous I/O. Use LoadAsync when possible.
        /// </summary>
        public static T Load<T>(string path, bool compressed = true) where T : new()
        {
            if (!File.Exists(path))
            {
                return new T();
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(path);

                if (compressed)
                {
                    bytes = Decompress(bytes);
                }

                string json = Encoding.UTF8.GetString(bytes);
                return JsonOperationPool.DeserializeObject<T>(json, JsonSettings) ?? new T();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveSystem] Failed to load {path}: {ex.Message}");
                return new T();
            }
        }

        private static byte[] Compress(byte[] data)
        {
            using (var output = new MemoryStream())
            {
                using (var gzip = new GZipStream(output, System.IO.Compression.CompressionLevel.Fastest))
                {
                    gzip.Write(data, 0, data.Length);
                }
                return output.ToArray();
            }
        }

        private static byte[] Decompress(byte[] data)
        {
            using (var input = new MemoryStream(data))
            using (var gzip = new GZipStream(input, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                gzip.CopyTo(output);
                return output.ToArray();
            }
        }
    }

    #region JSON Converters for Unity Types

    /// <summary>
    /// JSON converter for Unity Vector3
    /// </summary>
    public class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(value.x);
            writer.WritePropertyName("y"); writer.WriteValue(value.y);
            writer.WritePropertyName("z"); writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = Newtonsoft.Json.Linq.JObject.Load(reader);
            return new Vector3(
                (float)(obj["x"] ?? 0),
                (float)(obj["y"] ?? 0),
                (float)(obj["z"] ?? 0)
            );
        }
    }

    /// <summary>
    /// JSON converter for Unity Quaternion (stored as euler angles)
    /// </summary>
    public class QuaternionConverter : JsonConverter<Quaternion>
    {
        public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
        {
            var euler = value.eulerAngles;
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(euler.x);
            writer.WritePropertyName("y"); writer.WriteValue(euler.y);
            writer.WritePropertyName("z"); writer.WriteValue(euler.z);
            writer.WriteEndObject();
        }

        public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = Newtonsoft.Json.Linq.JObject.Load(reader);
            return Quaternion.Euler(
                (float)(obj["x"] ?? 0),
                (float)(obj["y"] ?? 0),
                (float)(obj["z"] ?? 0)
            );
        }
    }

    /// <summary>
    /// JSON converter for NightBlade Vec3 type
    /// </summary>
    public class Vec3Converter : JsonConverter<Vec3>
    {
        public override void WriteJson(JsonWriter writer, Vec3 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x"); writer.WriteValue(value.x);
            writer.WritePropertyName("y"); writer.WriteValue(value.y);
            writer.WritePropertyName("z"); writer.WriteValue(value.z);
            writer.WriteEndObject();
        }

        public override Vec3 ReadJson(JsonReader reader, Type objectType, Vec3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var obj = Newtonsoft.Json.Linq.JObject.Load(reader);
            return new Vec3(
                (float)(obj["x"] ?? 0),
                (float)(obj["y"] ?? 0),
                (float)(obj["z"] ?? 0)
            );
        }
    }

    #endregion
}
