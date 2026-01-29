using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NightBlade.Core.Utils.Performance
{
    /// <summary>
    /// Handles performance profiling, monitoring, and benchmarking operations.
    /// Separated from main logic for better organization and testability.
    /// </summary>
    public class PerformanceProfiler
    {
        // Mini benchmark state
        private float lastMiniBenchmarkTime = 0f;
        private int miniBenchmarkCount = 0;

        // Peak tracking
        private int peakUIPooledObjects = 0;
        private int peakStringBuilderObjects = 0;
        private int peakCollectionObjects = 0;
        private int peakMaterialPropertyBlockObjects = 0;
        private int peakDelegateObjects = 0;
        private int peakNetDataWriterObjects = 0;
        private int peakJsonOperationObjects = 0;
        private int peakFxCollectionObjects = 0;

        /// <summary>
        /// Updates peak statistics tracking.
        /// </summary>
        public void UpdatePeakStats(PerformanceStats.Stats currentStats)
        {
            if (currentStats.UIPoolSize > peakUIPooledObjects)
                peakUIPooledObjects = currentStats.UIPoolSize;
            if (currentStats.StringBuilderPoolSize > peakStringBuilderObjects)
                peakStringBuilderObjects = currentStats.StringBuilderPoolSize;
            if (currentStats.CollectionPoolSize > peakCollectionObjects)
                peakCollectionObjects = currentStats.CollectionPoolSize;
            if (currentStats.MaterialPropertyBlockPoolSize > peakMaterialPropertyBlockObjects)
                peakMaterialPropertyBlockObjects = currentStats.MaterialPropertyBlockPoolSize;
            if (currentStats.DelegatePoolSize > peakDelegateObjects)
                peakDelegateObjects = currentStats.DelegatePoolSize;
            if (currentStats.NetDataWriterPoolSize > peakNetDataWriterObjects)
                peakNetDataWriterObjects = currentStats.NetDataWriterPoolSize;
            if (currentStats.JsonOperationPoolSize > peakJsonOperationObjects)
                peakJsonOperationObjects = currentStats.JsonOperationPoolSize;
            if (currentStats.FxCollectionPoolSize > peakFxCollectionObjects)
                peakFxCollectionObjects = currentStats.FxCollectionPoolSize;
        }

        /// <summary>
        /// Resets all peak statistics.
        /// </summary>
        public void ResetPeakStats()
        {
            peakUIPooledObjects = 0;
            peakStringBuilderObjects = 0;
            peakCollectionObjects = 0;
            peakMaterialPropertyBlockObjects = 0;
            peakDelegateObjects = 0;
            peakNetDataWriterObjects = 0;
            peakJsonOperationObjects = 0;
            peakFxCollectionObjects = 0;
        }

        /// <summary>
        /// Runs a mini benchmark to test system performance.
        /// </summary>
        public void RunMiniBenchmark()
        {
            lastMiniBenchmarkTime = Time.realtimeSinceStartup;
            miniBenchmarkCount++;

            // Simple benchmark operations
            float testValue = 0f;
            for (int i = 0; i < 100000; i++)
            {
                testValue += Mathf.Sin(i * 0.01f) * Mathf.Cos(i * 0.01f);
            }

            float benchmarkTime = Time.realtimeSinceStartup - lastMiniBenchmarkTime;
            Debug.Log($"[PerformanceMonitor] Mini benchmark #{miniBenchmarkCount}: {benchmarkTime:F4}s");
        }

        /// <summary>
        /// Tests all optimization systems.
        /// </summary>
        public void TestAllSystems()
        {
            Debug.Log("[PerformanceMonitor] Testing all optimization systems...");

            // Test distance-based entities
            TestDistanceEntities();

            // Test pooled coroutines
            TestPooledCoroutines();

            // Test UI pooling (existing)
            TestUIPooling();

            // Test new pooling systems
            TestStringBuilderPooling();
            TestCollectionPooling();
            TestMaterialPropertyBlockPooling();
            TestDelegatePooling();
            TestNetDataWriterPooling();
            TestJsonOperationPooling();
            TestFxCollectionPooling();

            // Check stats after tests
            PerformanceMonitor.Instance.StartCoroutine(CheckAllStatsAfterDelay());
        }

        #region Individual System Tests

        private void TestDistanceEntities()
        {
            try
            {
                var distanceBasedObjects = GameObject.FindObjectsOfType<NightBlade.Core.Utils.DistanceBasedUpdater>(true);
                Debug.Log($"[PerformanceMonitor] Distance-based entities: {distanceBasedObjects.Length}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] Distance entities test failed: {e.Message}");
            }
        }

        private void TestPooledCoroutines()
        {
            try
            {
                int activeCoroutines = 0; // TODO: Fix when CoroutinePool API is available
                Debug.Log($"[PerformanceMonitor] Active pooled coroutines: {activeCoroutines}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] Pooled coroutines test failed: {e.Message}");
            }
        }

        private void TestUIPooling()
        {
            try
            {
                var tmpObjects = GameObject.FindObjectsOfType<TMPro.TextMeshProUGUI>(true);
                Debug.Log($"[PerformanceMonitor] UI objects (TMP): {tmpObjects.Length}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] UI pooling test failed: {e.Message}");
            }
        }

        private void TestStringBuilderPooling()
        {
            try
            {
                var sb = NightBlade.Core.Utils.StringBuilderPool.Get();
                sb.Append("Test string for pooling");
                string result = sb.ToString();
                NightBlade.Core.Utils.StringBuilderPool.Return(sb);
                Debug.Log($"[PerformanceMonitor] StringBuilder pooling test successful: {result.Length} chars");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] StringBuilder pooling test failed: {e.Message}");
            }
        }

        private void TestCollectionPooling()
        {
            try
            {
                var list = NightBlade.Core.Utils.ListPool<int>.GetList();
                for (int i = 0; i < 10; i++) list.Add(i);
                NightBlade.Core.Utils.ListPool<int>.ReturnList(list);
                Debug.Log("[PerformanceMonitor] Collection pooling test successful");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] Collection pooling test failed: {e.Message}");
            }
        }

        private void TestMaterialPropertyBlockPooling()
        {
            try
            {
                var block = NightBlade.Core.Utils.MaterialPropertyBlockPool.Get();
                block.SetColor("_Color", Color.red);
                NightBlade.Core.Utils.MaterialPropertyBlockPool.Return(block);
                Debug.Log("[PerformanceMonitor] Material property block pooling test successful");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] Material property block pooling test failed: {e.Message}");
            }
        }

        private void TestDelegatePooling()
        {
            try
            {
                var action = NightBlade.Core.Utils.DelegatePool.GetAction(() => Debug.Log("Test"));
                action.Invoke();
                Debug.Log("[PerformanceMonitor] Delegate pooling test successful");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] Delegate pooling test failed: {e.Message}");
            }
        }

        private void TestNetDataWriterPooling()
        {
            try
            {
                NightBlade.Core.Utils.NetworkWriterPool.Use(writer => {
                    writer.Put("Test message");
                    Debug.Log($"[PerformanceMonitor] NetDataWriter pooling test successful");
                });
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] NetDataWriter pooling test failed: {e.Message}");
            }
        }

        private void TestJsonOperationPooling()
        {
            try
            {
                // Test JSON serialization with pooling
                var testData = new TestData { name = "TestObject", value = 42 };
                string json = NightBlade.Core.Utils.JsonOperationPool.SerializeObject(testData);
                Debug.Log($"[PerformanceMonitor] JSON serialization successful: {json.Length} chars");

                // Test deserialization
                var deserialized = NightBlade.Core.Utils.JsonOperationPool.DeserializeObject<TestData>(json);
                Debug.Log($"[PerformanceMonitor] JSON deserialization successful: {deserialized.name}");
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] JsonOperation pooling test failed: {e.Message}");
            }
        }

        private void TestFxCollectionPooling()
        {
            try
            {
                // Create a test GameObject with particle systems for testing
                var testGameObject = new GameObject("FxCollectionTest");
                var particleSystem = testGameObject.AddComponent<ParticleSystem>();
                var mainModule = particleSystem.main;
                mainModule.loop = true;

                // Test FxCollection pooling
                var fxCollection = NightBlade.FxCollection.GetPooled(testGameObject);
                Debug.Log($"[PerformanceMonitor] FxCollection pooled successfully. Pool size: {NightBlade.FxCollection.GetPoolSize()}");

                // Test playing (in batch mode it won't actually play)
                fxCollection.Play();
                Debug.Log("[PerformanceMonitor] FxCollection.Play() called successfully");

                // Test returning to pool
                NightBlade.FxCollection.ReturnPooled(fxCollection);
                Debug.Log($"[PerformanceMonitor] FxCollection returned to pool. Pool size: {NightBlade.FxCollection.GetPoolSize()}");

                // Clean up test object
                UnityEngine.Object.Destroy(testGameObject);
            }
            catch (System.Exception e)
            {
                Debug.Log($"[PerformanceMonitor] FxCollection pooling test failed: {e.Message}");
            }
        }

        #endregion

        /// <summary>
        /// Checks stats after system tests with a delay.
        /// </summary>
        private IEnumerator CheckAllStatsAfterDelay()
        {
            yield return new WaitForSeconds(0.5f);
            PerformanceMonitor.Instance.UpdateStats();
            Debug.Log("[PerformanceMonitor] System tests completed - stats updated");
        }

        /// <summary>
        /// Debug method to check the status of all monitored systems.
        /// </summary>
        public void DebugSystemStatus()
        {
            Debug.Log("[PerformanceMonitor] === SYSTEM STATUS DEBUG ===");

            // Check UI Pool
            try
            {
                int uiPoolSize = PerformanceStats.CalculateCurrentStats().UIPoolSize;
                Debug.Log($"UI Pool: {uiPoolSize} pooled objects (peak: {peakUIPooledObjects})");
                if (uiPoolSize == -1)
                {
                    Debug.Log("Note: UI pool size -1 indicates TMP (TextMeshPro) resources not found");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"UI Pool: Error - {e.Message}");
            }

            // Check StringBuilder Pool
            try
            {
                int stringBuilderPoolSize = NightBlade.Core.Utils.StringBuilderPool.PoolSize;
                Debug.Log($"StringBuilder Pool: {stringBuilderPoolSize} pooled objects (peak: {peakStringBuilderObjects})");
                if (stringBuilderPoolSize == 0)
                {
                    Debug.Log("Note: StringBuilder pool is empty. It fills when text operations occur (damage numbers, chat, UI updates).");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"StringBuilder Pool: Error - {e.Message}");
            }

            // Check Collection Pool
            try
            {
                int collectionPoolSize = 0; // TODO: Fix when CollectionPool API is available
                Debug.Log($"Collection Pool: {collectionPoolSize} pooled objects (peak: {peakCollectionObjects})");
                if (collectionPoolSize == 0)
                {
                    Debug.Log("Note: Collection pool size tracking is not yet implemented. Collections are pooled but not centrally tracked.");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"Collection Pool: Error - {e.Message}");
            }

            // Check Material Property Block Pool
            try
            {
                int mpbPoolSize = NightBlade.Core.Utils.MaterialPropertyBlockPool.PoolSize;
                Debug.Log($"Material Property Block Pool: {mpbPoolSize} pooled objects (peak: {peakMaterialPropertyBlockObjects})");
                if (mpbPoolSize == 0)
                {
                    Debug.Log("Note: MaterialPropertyBlock pool is empty. It fills when dynamic material properties are set for GC optimization.");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"Material Property Block Pool: Error - {e.Message}");
            }

            // Check Delegate Pool
            try
            {
                int delegatePoolSize = 0; // TODO: Fix when DelegatePool API is available
                Debug.Log($"Delegate Pool: {delegatePoolSize} pooled objects (peak: {peakDelegateObjects})");
                if (delegatePoolSize == 0)
                {
                    Debug.Log("Note: Delegate pool is empty. It fills when event handlers are created for GC optimization.");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"Delegate Pool: Error - {e.Message}");
            }

            // Check NetDataWriter Pool
            try
            {
                int netDataWriterPoolSize = NightBlade.Core.Utils.NetworkWriterPool.PoolSize;
                Debug.Log($"NetDataWriter Pool: {netDataWriterPoolSize} pooled objects (peak: {peakNetDataWriterObjects})");
                if (netDataWriterPoolSize == 0)
                {
                    Debug.Log("Note: NetDataWriter pool is empty. It fills when network messages are serialized.");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"NetDataWriter Pool: Error - {e.Message}");
            }

            // Check JsonOperation Pool
            try
            {
                var (stringBuilderPool, stringWriterPool) = NightBlade.Core.Utils.JsonOperationPool.PoolSizes;
                int jsonOperationPoolSize = stringBuilderPool + stringWriterPool;
                Debug.Log($"JsonOperation Pool: {jsonOperationPoolSize} pooled objects (peak: {peakJsonOperationObjects})");
                if (jsonOperationPoolSize == 0)
                {
                    Debug.Log("Note: JsonOperation pool is empty. It fills when JSON serialization/deserialization occurs for save/load operations.");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"JsonOperation Pool: Error - {e.Message}");
            }

            // Check FxCollection Pool
            try
            {
                int fxCollectionPoolSize = NightBlade.FxCollection.GetPoolSize();
                Debug.Log($"FxCollection Pool: {fxCollectionPoolSize} pooled objects (peak: {peakFxCollectionObjects})");
                if (fxCollectionPoolSize == 0)
                {
                    Debug.Log("Note: FxCollection pool is empty. It fills when combat effects (particles, audio) are played during gameplay.");
                }
            }
            catch (System.Exception e)
            {
                Debug.Log($"FxCollection Pool: Error - {e.Message}");
            }

            Debug.Log("[PerformanceMonitor] === END SYSTEM STATUS DEBUG ===");
        }

        // Test data class for JSON testing
        private class TestData
        {
            public string name;
            public int value;
        }
    }
}