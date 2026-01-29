using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NightBlade
{
    /// <summary>
    /// Handles delta compression for various data types to reduce network bandwidth
    /// </summary>
    public static class DeltaCompressor
    {
        /// <summary>
        /// Compress position data using delta encoding
        /// </summary>
        public static Vector3 CompressPosition(Vector3 current, Vector3 previous, float threshold = 0.01f)
        {
            Vector3 delta = current - previous;

            // Only send significant changes
            if (Mathf.Abs(delta.x) < threshold) delta.x = 0;
            if (Mathf.Abs(delta.y) < threshold) delta.y = 0;
            if (Mathf.Abs(delta.z) < threshold) delta.z = 0;

            return delta;
        }

        /// <summary>
        /// Compress rotation data using delta encoding
        /// </summary>
        public static Quaternion CompressRotation(Quaternion current, Quaternion previous, float thresholdDegrees = 1f)
        {
            float angle = Quaternion.Angle(current, previous);

            // Only send significant changes
            if (angle < thresholdDegrees)
            {
                return Quaternion.identity; // No change
            }

            return current;
        }

        /// <summary>
        /// Compress float values using delta encoding
        /// </summary>
        public static float CompressFloat(float current, float previous, float threshold = 0.01f)
        {
            float delta = current - previous;
            return Mathf.Abs(delta) < threshold ? 0 : delta;
        }

        /// <summary>
        /// Compress integer values using delta encoding
        /// </summary>
        public static int CompressInt(int current, int previous, int threshold = 0)
        {
            int delta = current - previous;
            return Mathf.Abs(delta) <= threshold ? 0 : delta;
        }

        /// <summary>
        /// Serialize compressed position data
        /// </summary>
        public static void SerializeCompressedPosition(NetDataWriter writer, Vector3 position, Vector3? previousPosition = null)
        {
            if (previousPosition.HasValue)
            {
                Vector3 delta = CompressPosition(position, previousPosition.Value);
                writer.Put((byte)CompressionType.Delta);
                writer.Put(delta.x);
                writer.Put(delta.y);
                writer.Put(delta.z);
            }
            else
            {
                writer.Put((byte)CompressionType.Absolute);
                writer.PutVector3(position);
            }
        }

        /// <summary>
        /// Deserialize compressed position data
        /// </summary>
        public static Vector3 DeserializeCompressedPosition(NetDataReader reader, Vector3 basePosition)
        {
            CompressionType type = (CompressionType)reader.GetByte();
            if (type == CompressionType.Delta)
            {
                Vector3 delta = new Vector3(reader.GetFloat(), reader.GetFloat(), reader.GetFloat());
                return basePosition + delta;
            }
            else
            {
                return reader.GetVector3();
            }
        }

        /// <summary>
        /// Serialize compressed rotation data
        /// </summary>
        public static void SerializeCompressedRotation(NetDataWriter writer, Quaternion rotation, Quaternion? previousRotation = null)
        {
            if (previousRotation.HasValue)
            {
                Quaternion compressed = CompressRotation(rotation, previousRotation.Value);
                if (compressed == Quaternion.identity)
                {
                    writer.Put((byte)CompressionType.NoChange);
                }
                else
                {
                    writer.Put((byte)CompressionType.Delta);
                    writer.PutQuaternion(rotation);
                }
            }
            else
            {
                writer.Put((byte)CompressionType.Absolute);
                writer.PutQuaternion(rotation);
            }
        }

        /// <summary>
        /// Deserialize compressed rotation data
        /// </summary>
        public static Quaternion DeserializeCompressedRotation(NetDataReader reader, Quaternion baseRotation)
        {
            CompressionType type = (CompressionType)reader.GetByte();
            if (type == CompressionType.NoChange)
            {
                return baseRotation;
            }
            else
            {
                return reader.GetQuaternion();
            }
        }

        /// <summary>
        /// Compress a list by only sending changes
        /// </summary>
        public static List<T> CompressListChanges<T>(List<T> current, List<T> previous, System.Func<T, T, bool> equalityComparer)
        {
            var changes = new List<T>();

            // Find additions
            foreach (var item in current)
            {
                if (!previous.Exists(p => equalityComparer(item, p)))
                {
                    changes.Add(item);
                }
            }

            // Find removals (marked as default/null)
            // This is a simplified approach - in practice you might want a more sophisticated diff

            return changes;
        }

        /// <summary>
        /// Calculate compression ratio for debugging
        /// </summary>
        public static float CalculateCompressionRatio(int originalBytes, int compressedBytes)
        {
            if (originalBytes == 0) return 1f;
            return (float)compressedBytes / originalBytes;
        }
    }

    /// <summary>
    /// Types of compression used
    /// </summary>
    public enum CompressionType : byte
    {
        Absolute = 0,   // Send full value
        Delta = 1,      // Send difference from previous
        NoChange = 2,   // No change from previous
        Reference = 3,  // Reference to previously sent value
    }

    /// <summary>
    /// Statistics for compression performance
    /// </summary>
    public struct CompressionStats
    {
        public int TotalOriginalBytes;
        public int TotalCompressedBytes;
        public int MessagesCompressed;
        public float AverageCompressionRatio => TotalOriginalBytes > 0 ? (float)TotalCompressedBytes / TotalOriginalBytes : 1f;
        public int BandwidthSaved => TotalOriginalBytes - TotalCompressedBytes;
    }
}