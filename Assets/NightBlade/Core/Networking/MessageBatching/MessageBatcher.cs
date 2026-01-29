using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using NightBlade.Core.Utils;

namespace NightBlade
{
    /// <summary>
    /// Handles batching of network messages for improved performance and reduced bandwidth
    /// </summary>
    public class MessageBatcher : MonoBehaviour
    {
        [Header("Batch Configuration")]
        [Tooltip("Maximum messages per batch")]
        [SerializeField] private int maxMessagesPerBatch = 50;

        [Tooltip("Maximum batch size in bytes")]
        [SerializeField] private int maxBatchSizeBytes = 4096;

        [Tooltip("Maximum time to wait before sending a batch (seconds)")]
        [SerializeField] private float maxBatchDelay = 0.1f;

        [Header("Priority Configuration")]
        [Tooltip("Send interval for critical messages (seconds)")]
        [SerializeField] private float criticalSendInterval = 0.016f; // 60 FPS

        [Tooltip("Send interval for high priority messages (seconds)")]
        [SerializeField] private float highSendInterval = 0.033f; // 30 FPS

        [Tooltip("Send interval for medium priority messages (seconds)")]
        [SerializeField] private float mediumSendInterval = 0.1f; // 10 FPS

        [Tooltip("Send interval for low priority messages (seconds)")]
        [SerializeField] private float lowSendInterval = 0.5f; // 2 FPS

        // Message queues for each priority level
        private readonly ConcurrentQueue<PendingMessage>[] _messageQueues = new ConcurrentQueue<PendingMessage>[4];
        private readonly List<PendingMessage>[] _batchBuffers = new List<PendingMessage>[4];

        // Timing tracking
        private float[] _lastSendTimes;
        private uint _nextBatchId = 1;

        // Statistics
        private int _totalMessagesBatched;
        private int _totalBatchesSent;
        private int _totalBandwidthSaved;

        private LiteNetLibManager.LiteNetLibManager _networkManager;
        private BaseGameNetworkManager _baseNetworkManager;

        private void Awake()
        {
            _networkManager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
            _baseNetworkManager = GetComponent<BaseGameNetworkManager>();

            // Initialize queues and buffers
            for (int i = 0; i < 4; i++)
            {
                _messageQueues[i] = new ConcurrentQueue<PendingMessage>();
                _batchBuffers[i] = new List<PendingMessage>();
            }

            _lastSendTimes = new float[4];
        }

        private void Update()
        {
            // Skip processing if network manager is not ready
            if (_baseNetworkManager == null)
                return;

            // Check if network components are initialized
            if (_baseNetworkManager.Client == null && _baseNetworkManager.Server == null && !Application.isEditor)
                return;

            float currentTime = Time.time;

            // Process each priority level
            for (int priority = 0; priority < 4; priority++)
            {
                float sendInterval = GetSendInterval((MessagePriority)priority);
                if (currentTime - _lastSendTimes[priority] >= sendInterval)
                {
                    ProcessBatchForPriority((MessagePriority)priority);
                    _lastSendTimes[priority] = currentTime;
                }
            }
        }

        /// <summary>
        /// Queue a message for batching
        /// </summary>
        public void QueueMessage(long connectionId, DeliveryMethod deliveryMethod, ushort messageType, INetSerializable message, MessagePriority priority = MessagePriority.Medium)
        {
            // Serialize the message immediately to avoid threading issues
            var writer = NetworkWriterPool.Get();
            message.Serialize(writer);

            var pendingMessage = new PendingMessage
            {
                ConnectionId = connectionId,
                DeliveryMethod = deliveryMethod,
                MessageType = messageType,
                Data = writer.Data,
                Priority = priority,
                Timestamp = Time.time
            };

            _messageQueues[(int)priority].Enqueue(pendingMessage);

            // Return writer to pool after copying data
            NetworkWriterPool.Return(writer);
        }

        /// <summary>
        /// Queue a raw message for batching
        /// </summary>
        public void QueueRawMessage(long connectionId, DeliveryMethod deliveryMethod, ushort messageType, byte[] data, MessagePriority priority = MessagePriority.Medium)
        {
            var pendingMessage = new PendingMessage
            {
                ConnectionId = connectionId,
                DeliveryMethod = deliveryMethod,
                MessageType = messageType,
                Data = data,
                Priority = priority,
                Timestamp = Time.time
            };

            _messageQueues[(int)priority].Enqueue(pendingMessage);
        }

        /// <summary>
        /// Force immediate sending of all queued messages for a specific priority
        /// </summary>
        public void FlushPriority(MessagePriority priority)
        {
            ProcessBatchForPriority(priority, true);
            _lastSendTimes[(int)priority] = Time.time;
        }

        /// <summary>
        /// Force immediate sending of all queued messages for all priorities
        /// </summary>
        public void FlushAll()
        {
            for (int priority = 0; priority < 4; priority++)
            {
                ProcessBatchForPriority((MessagePriority)priority, true);
                _lastSendTimes[priority] = Time.time;
            }
        }

        private void ProcessBatchForPriority(MessagePriority priority, bool forceSend = false)
        {
            var queue = _messageQueues[(int)priority];
            var buffer = _batchBuffers[(int)priority];

            // Move messages from queue to buffer
            while (queue.TryDequeue(out var message))
            {
                buffer.Add(message);
            }

            if (buffer.Count == 0)
                return;

            // Check if we should send the batch
            bool shouldSend = forceSend ||
                             buffer.Count >= maxMessagesPerBatch ||
                             ShouldSendDueToTime(buffer) ||
                             ShouldSendDueToSize(buffer);

            if (shouldSend)
            {
                SendBatch(buffer, priority);
                buffer.Clear();
            }
        }

        private bool ShouldSendDueToTime(List<PendingMessage> buffer)
        {
            if (buffer.Count == 0) return false;
            return Time.time - buffer[0].Timestamp >= maxBatchDelay;
        }

        private bool ShouldSendDueToSize(List<PendingMessage> buffer)
        {
            int totalSize = 0;
            foreach (var message in buffer)
            {
                totalSize += message.Data.Length + 8; // Rough estimate with overhead
                if (totalSize >= maxBatchSizeBytes)
                    return true;
            }
            return false;
        }

        private void SendBatch(List<PendingMessage> messages, MessagePriority priority)
        {
            if (messages.Count == 0) return;

            // Group messages by connection ID
            var messagesByConnection = new Dictionary<long, List<PendingMessage>>();
            foreach (var message in messages)
            {
                if (!messagesByConnection.ContainsKey(message.ConnectionId))
                    messagesByConnection[message.ConnectionId] = new List<PendingMessage>();
                messagesByConnection[message.ConnectionId].Add(message);
            }

            // Send a batch for each connection
            foreach (var kvp in messagesByConnection)
            {
                SendBatchForConnection(kvp.Key, kvp.Value, priority);
            }

            // Update statistics
            _totalMessagesBatched += messages.Count;
            _totalBatchesSent++;
        }

        private void SendBatchForConnection(long connectionId, List<PendingMessage> messages, MessagePriority priority)
        {
            var batch = new BatchedMessage(_nextBatchId++);
            int totalOriginalSize = 0;

            foreach (var message in messages)
            {
                batch.AddMessage(message.MessageType, message.Data, message.Priority);
                totalOriginalSize += message.Data.Length + 16; // Estimate original packet overhead
            }

            // Calculate bandwidth savings (rough estimate)
            int batchSize = NetworkWriterPool.Use(batchWriter =>
            {
                batch.Serialize(batchWriter);
                return batchWriter.Data.Length + 16; // Batch packet overhead
            });
            _totalBandwidthSaved += Mathf.Max(0, totalOriginalSize - batchSize);

            // Send the batch
            var deliveryMethod = GetDeliveryMethodForPriority(priority);
            _networkManager.ServerSendPacket(connectionId, 0, deliveryMethod, GameNetworkingConsts.BatchedMessage, batch);
        }

        private float GetSendInterval(MessagePriority priority)
        {
            switch (priority)
            {
                case MessagePriority.Critical: return criticalSendInterval;
                case MessagePriority.High: return highSendInterval;
                case MessagePriority.Medium: return mediumSendInterval;
                case MessagePriority.Low: return lowSendInterval;
                default: return mediumSendInterval;
            }
        }

        private DeliveryMethod GetDeliveryMethodForPriority(MessagePriority priority)
        {
            // Critical messages always reliable
            if (priority == MessagePriority.Critical)
                return DeliveryMethod.ReliableOrdered;

            // High priority usually reliable
            if (priority == MessagePriority.High)
                return DeliveryMethod.ReliableOrdered;

            // Medium/low can be unreliable for performance
            return DeliveryMethod.Unreliable;
        }

        /// <summary>
        /// Get batching statistics
        /// </summary>
        public MessageBatchStats GetStats()
        {
            return new MessageBatchStats
            {
                TotalMessagesBatched = _totalMessagesBatched,
                TotalBatchesSent = _totalBatchesSent,
                TotalBandwidthSaved = _totalBandwidthSaved,
                AverageMessagesPerBatch = _totalBatchesSent > 0 ? (float)_totalMessagesBatched / _totalBatchesSent : 0,
                CurrentQueueSizes = new int[]
                {
                    _messageQueues[0].Count,
                    _messageQueues[1].Count,
                    _messageQueues[2].Count,
                    _messageQueues[3].Count
                }
            };
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStats()
        {
            _totalMessagesBatched = 0;
            _totalBatchesSent = 0;
            _totalBandwidthSaved = 0;
        }
    }

    /// <summary>
    /// Pending message structure for queuing
    /// </summary>
    public struct PendingMessage
    {
        public long ConnectionId;
        public DeliveryMethod DeliveryMethod;
        public ushort MessageType;
        public byte[] Data;
        public MessagePriority Priority;
        public float Timestamp;
    }

    /// <summary>
    /// Statistics for message batching performance
    /// </summary>
    public struct MessageBatchStats
    {
        public int TotalMessagesBatched;
        public int TotalBatchesSent;
        public int TotalBandwidthSaved;
        public float AverageMessagesPerBatch;
        public int[] CurrentQueueSizes;
    }
}