using System.Collections.Generic;
using UnityEngine;
using Unity.Profiling;

namespace NightBlade.Core.Utils.Performance
{
    /// <summary>
    /// Performance statistics data structures and calculations.
    /// Contains all data definitions and computation logic.
    /// </summary>
    public static class PerformanceStats
    {
        /// <summary>
        /// Main performance statistics structure containing all metrics.
        /// </summary>
        public struct Stats
        {
            public float FrameTime;
            public float FPS;
            public long GCAllocationsThisFrame;
            public long TotalMemoryUsed;
            public int NetworkStringsCached;
            public int UIPoolSize;
            public int DistanceBasedEntities;
            public int PooledCoroutinesActive;
            public int StringBuilderPoolSize;
            public int CollectionPoolSize;
            public int MaterialPropertyBlockPoolSize;
            public int DelegatePoolSize;
            public int NetDataWriterPoolSize;
            public int JsonOperationPoolSize;
            public int FxCollectionPoolSize;
        }

        // Profiler markers for performance tracking
        public static readonly ProfilerMarker NetworkUpdateMarker = new ProfilerMarker("Network.Update");
        public static readonly ProfilerMarker UIRenderMarker = new ProfilerMarker("UI.Render");
        public static readonly ProfilerMarker DistanceUpdateMarker = new ProfilerMarker("Distance.Update");
        public static readonly ProfilerMarker CoroutinePoolMarker = new ProfilerMarker("Coroutine.Pool");
        public static readonly ProfilerMarker StringBuilderPoolMarker = new ProfilerMarker("StringBuilder.Pool");
        public static readonly ProfilerMarker CollectionPoolMarker = new ProfilerMarker("Collection.Pool");
        public static readonly ProfilerMarker MaterialPropertyBlockPoolMarker = new ProfilerMarker("MaterialPropertyBlock.Pool");
        public static readonly ProfilerMarker DelegatePoolMarker = new ProfilerMarker("Delegate.Pool");

        /// <summary>
        /// Calculates current performance statistics.
        /// </summary>
        public static Stats CalculateCurrentStats()
        {
            var stats = new Stats
            {
                FrameTime = Time.deltaTime,
                FPS = 1f / Time.deltaTime,
                GCAllocationsThisFrame = 0, // TODO: Re-enable Profiler API when compilation is fixed
                TotalMemoryUsed = 0, // TODO: Re-enable Profiler API when compilation is fixed
                NetworkStringsCached = GetNetworkStringsCached(),
                UIPoolSize = GetUIPoolSize(),
                DistanceBasedEntities = GetDistanceBasedEntities(),
                PooledCoroutinesActive = GetPooledCoroutinesActive(),
                StringBuilderPoolSize = GetStringBuilderPoolSize(),
                CollectionPoolSize = GetCollectionPoolSize(),
                MaterialPropertyBlockPoolSize = GetMaterialPropertyBlockPoolSize(),
                DelegatePoolSize = GetDelegatePoolSize(),
                NetDataWriterPoolSize = GetNetDataWriterPoolSize(),
                JsonOperationPoolSize = GetJsonOperationPoolSize(),
                FxCollectionPoolSize = GetFxCollectionPoolSize()
            };

            return stats;
        }

        #region Pool Size Getters

        private static int GetNetworkStringsCached()
        {
            try
            {
                return LiteNetLib.Utils.NetworkStringCache.GetCacheSize();
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        private static int GetUIPoolSize()
        {
            try
            {
                // Check if TMP is available and get pooled objects count
                var textMeshProObjects = GameObject.FindObjectsOfType<TMPro.TextMeshProUGUI>(true);
                return textMeshProObjects.Length;
            }
            catch (System.Exception)
            {
                return -1; // TMP not available
            }
        }

        private static int GetDistanceBasedEntities()
        {
            try
            {
                var distanceBasedObjects = GameObject.FindObjectsOfType<NightBlade.Core.Utils.DistanceBasedUpdater>(true);
                return distanceBasedObjects.Length;
            }
            catch (System.Exception)
            {
                return 0;
            }
        }

        // Pool size getters - return actual sizes or 0 on error
        private static int GetPooledCoroutinesActive()
        {
            try { return NightBlade.Core.Utils.CoroutinePool.ActiveCoroutineCount; }
            catch { return 0; }
        }
        private static int GetStringBuilderPoolSize() 
        {
            try { return NightBlade.Core.Utils.StringBuilderPool.PoolSize; } 
            catch { return 0; }
        }
        private static int GetCollectionPoolSize()
        {
            try 
            { 
                // ListPool<T> is generic, so we can't get a total across all types
                // Return 0 for now as tracking would require tracking all instantiated generic types
                return 0; 
            }
            catch { return 0; }
        }
        private static int GetMaterialPropertyBlockPoolSize() 
        {
            try { return NightBlade.Core.Utils.MaterialPropertyBlockPool.PoolSize; } 
            catch { return 0; }
        }
        private static int GetDelegatePoolSize()
        {
            try 
            { 
                var sizes = NightBlade.Core.Utils.DelegatePool.Advanced.PoolSizes;
                return sizes.actionPool + sizes.actionObjPool;
            }
            catch { return 0; }
        }
        private static int GetNetDataWriterPoolSize() 
        {
            try { return NightBlade.Core.Utils.NetworkWriterPool.PoolSize; } 
            catch { return 0; }
        }
        private static int GetJsonOperationPoolSize()
        {
            try 
            { 
                var sizes = NightBlade.Core.Utils.JsonOperationPool.PoolSizes;
                return sizes.stringBuilderPool + sizes.stringWriterPool;
            }
            catch { return 0; }
        }
        private static int GetFxCollectionPoolSize()
        {
            try 
            { 
                return NightBlade.FxCollection.GetPoolSize();
            }
            catch { return 0; }
        }

        #endregion
    }
}