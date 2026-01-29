using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;

namespace NightBlade
{
    /// <summary>
    /// Performance monitoring for message batching system
    /// </summary>
    public class MessageBatchingProfiler : MonoBehaviour
    {
        [Header("Profiling Settings")]
        [Tooltip("Enable performance monitoring")]
        [SerializeField] private bool enableProfiling = true;

        [Tooltip("Log interval in seconds")]
        [SerializeField] private float logInterval = 30f;

        [Tooltip("Show GUI overlay with stats")]
        [SerializeField] private bool showGUIStats = false;

        // Performance tracking
        private Stopwatch _sessionTimer = new Stopwatch();
        private float _lastLogTime;

        // Message statistics
        private int _totalMessagesSentWithoutBatching;
        private int _totalMessagesSentWithBatching;
        private int _totalBatchesSent;
        private int _totalBandwidthWithoutBatching;
        private int _totalBandwidthWithBatching;

        // Performance metrics
        private float _averageBatchLatency;
        private float _maxBatchLatency;
        private readonly List<float> _batchLatencies = new List<float>();

        // Component references
        private MessageBatcher _messageBatcher;
        private BatchMessageHandler _batchMessageHandler;
        private CharacterStateBatcher _characterStateBatcher;

        private void Awake()
        {
            _messageBatcher = GetComponent<MessageBatcher>();
            _batchMessageHandler = GetComponent<BatchMessageHandler>();
            _characterStateBatcher = GetComponent<CharacterStateBatcher>();

            if (enableProfiling)
            {
                _sessionTimer.Start();
            }
        }

        private void Update()
        {
            if (!enableProfiling) return;

            float currentTime = Time.time;
            if (currentTime - _lastLogTime >= logInterval)
            {
                LogPerformanceStats();
                _lastLogTime = currentTime;
            }
        }

        private void OnGUI()
        {
            if (!showGUIStats || !enableProfiling) return;

            GUI.Label(new Rect(10, 10, 400, 200), GetGUIStatsText());
        }

        /// <summary>
        /// Record a message sent without batching
        /// </summary>
        public void RecordMessageWithoutBatching(int messageSizeBytes)
        {
            if (!enableProfiling) return;
            _totalMessagesSentWithoutBatching++;
            _totalBandwidthWithoutBatching += messageSizeBytes;
        }

        /// <summary>
        /// Record a batch sent
        /// </summary>
        public void RecordBatchSent(int messagesInBatch, int batchSizeBytes, float latency)
        {
            if (!enableProfiling) return;

            _totalMessagesSentWithBatching += messagesInBatch;
            _totalBatchesSent++;
            _totalBandwidthWithBatching += batchSizeBytes;

            _batchLatencies.Add(latency);
            if (_batchLatencies.Count > 100) // Keep last 100 samples
            {
                _batchLatencies.RemoveAt(0);
            }

            // Update averages
            _averageBatchLatency = GetAverageLatency();
            _maxBatchLatency = Mathf.Max(_maxBatchLatency, latency);
        }

        /// <summary>
        /// Get current performance statistics
        /// </summary>
        public MessageBatchingPerformanceStats GetStats()
        {
            if (_messageBatcher == null || _batchMessageHandler == null)
                return new MessageBatchingPerformanceStats();

            var batcherStats = _messageBatcher.GetStats();
            var handlerStats = _batchMessageHandler.GetStats();

            return new MessageBatchingPerformanceStats
            {
                SessionDuration = _sessionTimer.Elapsed.TotalSeconds,
                TotalMessagesWithoutBatching = _totalMessagesSentWithoutBatching,
                TotalMessagesWithBatching = _totalMessagesSentWithBatching,
                TotalBatchesSent = _totalBatchesSent,
                BandwidthWithoutBatching = _totalBandwidthWithoutBatching,
                BandwidthWithBatching = _totalBandwidthWithBatching,
                BandwidthSavingsPercent = CalculateBandwidthSavings(),
                AverageMessagesPerBatch = batcherStats.AverageMessagesPerBatch,
                AverageBatchLatency = _averageBatchLatency,
                MaxBatchLatency = _maxBatchLatency,
                CurrentQueueSizes = batcherStats.CurrentQueueSizes,
                AverageProcessingTime = handlerStats.AverageProcessingTimeMs,
                BatchesProcessedPerSecond = CalculateBatchesPerSecond()
            };
        }

        /// <summary>
        /// Log performance statistics to console
        /// </summary>
        public void LogPerformanceStats()
        {
            var stats = GetStats();

            UnityEngine.Debug.Log($"=== Message Batching Performance Report ===");
            UnityEngine.Debug.Log($"Session Duration: {stats.SessionDuration:F1}s");
            UnityEngine.Debug.Log($"Messages: {_totalMessagesSentWithoutBatching} → {_totalMessagesSentWithBatching} ({stats.TotalBatchesSent} batches)");
            UnityEngine.Debug.Log($"Bandwidth: {_totalBandwidthWithoutBatching} → {_totalBandwidthWithBatching} bytes ({stats.BandwidthSavingsPercent:F1}% savings)");
            UnityEngine.Debug.Log($"Avg Messages/Batch: {stats.AverageMessagesPerBatch:F1}");
            UnityEngine.Debug.Log($"Batch Latency: {stats.AverageBatchLatency:F3}ms (max: {stats.MaxBatchLatency:F3}ms)");
            UnityEngine.Debug.Log($"Processing: {stats.AverageProcessingTime:F3}ms avg, {stats.BatchesProcessedPerSecond:F1} batches/sec");
            UnityEngine.Debug.Log($"Queue Sizes: Critical={stats.CurrentQueueSizes[0]}, High={stats.CurrentQueueSizes[1]}, Medium={stats.CurrentQueueSizes[2]}, Low={stats.CurrentQueueSizes[3]}");
        }

        /// <summary>
        /// Reset all performance statistics
        /// </summary>
        public void ResetStats()
        {
            _totalMessagesSentWithoutBatching = 0;
            _totalMessagesSentWithBatching = 0;
            _totalBatchesSent = 0;
            _totalBandwidthWithoutBatching = 0;
            _totalBandwidthWithBatching = 0;
            _averageBatchLatency = 0;
            _maxBatchLatency = 0;
            _batchLatencies.Clear();

            if (_messageBatcher != null) _messageBatcher.ResetStats();
            if (_batchMessageHandler != null) _batchMessageHandler.ResetStats();
        }

        private float CalculateBandwidthSavings()
        {
            if (_totalBandwidthWithoutBatching == 0) return 0;
            int savings = _totalBandwidthWithoutBatching - _totalBandwidthWithBatching;
            return (savings / (float)_totalBandwidthWithoutBatching) * 100f;
        }

        private float GetAverageLatency()
        {
            if (_batchLatencies.Count == 0) return 0;
            float sum = 0;
            foreach (var latency in _batchLatencies) sum += latency;
            return sum / _batchLatencies.Count;
        }

        private float CalculateBatchesPerSecond()
        {
            if (_sessionTimer.Elapsed.TotalSeconds == 0) return 0;
            return _totalBatchesSent / (float)_sessionTimer.Elapsed.TotalSeconds;
        }

        private string GetGUIStatsText()
        {
            var stats = GetStats();
            return $"Message Batching Stats:\n" +
                   $"Messages: {_totalMessagesSentWithBatching} ({stats.AverageMessagesPerBatch:F1}/batch)\n" +
                   $"Bandwidth: {_totalBandwidthWithBatching} bytes ({stats.BandwidthSavingsPercent:F1}% saved)\n" +
                   $"Latency: {stats.AverageBatchLatency:F1}ms\n" +
                   $"Queues: C={stats.CurrentQueueSizes[0]} H={stats.CurrentQueueSizes[1]} M={stats.CurrentQueueSizes[2]} L={stats.CurrentQueueSizes[3]}";
        }
    }

    /// <summary>
    /// Comprehensive performance statistics for message batching
    /// </summary>
    public struct MessageBatchingPerformanceStats
    {
        public double SessionDuration;
        public int TotalMessagesWithoutBatching;
        public int TotalMessagesWithBatching;
        public int TotalBatchesSent;
        public int BandwidthWithoutBatching;
        public int BandwidthWithBatching;
        public float BandwidthSavingsPercent;
        public float AverageMessagesPerBatch;
        public float AverageBatchLatency;
        public float MaxBatchLatency;
        public int[] CurrentQueueSizes;
        public float AverageProcessingTime;
        public float BatchesProcessedPerSecond;

        public override string ToString()
        {
            return $"Duration: {SessionDuration:F1}s, Messages: {TotalMessagesWithBatching}, " +
                   $"Bandwidth: {BandwidthWithBatching} bytes ({BandwidthSavingsPercent:F1}% saved), " +
                   $"Avg Latency: {AverageBatchLatency:F1}ms";
        }
    }
}