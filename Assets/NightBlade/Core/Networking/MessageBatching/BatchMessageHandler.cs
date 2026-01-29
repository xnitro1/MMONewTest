using LiteNetLib.Utils;
using LiteNetLibManager;
using System.Collections.Generic;
using UnityEngine;

namespace NightBlade
{
    /// <summary>
    /// Handles processing of incoming batched messages and routes them to individual message handlers
    /// </summary>
    public class BatchMessageHandler : MonoBehaviour
    {
        private LiteNetLibManager.LiteNetLibManager _networkManager;
        private readonly NetDataReader _reader = new NetDataReader();

        // Statistics
        private int _totalBatchesReceived;
        private int _totalMessagesProcessed;
        private int _totalProcessingTimeMs;

        private void Awake()
        {
            _networkManager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        /// <summary>
        /// Process an incoming batched message
        /// </summary>
        public void HandleBatchedMessage(long connectionId, NetDataReader reader)
        {
            var startTime = System.Diagnostics.Stopwatch.GetTimestamp();

            try
            {
                var batch = new BatchedMessage(0);
                batch.Deserialize(reader);

                _totalBatchesReceived++;

                // Process each message in the batch
                foreach (var messageEntry in batch.Messages)
                {
                    ProcessIndividualMessage(connectionId, messageEntry);
                    _totalMessagesProcessed++;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error processing batched message from connection {connectionId}: {ex.Message}");
            }

            var endTime = System.Diagnostics.Stopwatch.GetTimestamp();
            var processingTimeMs = (int)((endTime - startTime) * 1000.0 / System.Diagnostics.Stopwatch.Frequency);
            _totalProcessingTimeMs += processingTimeMs;
        }

        private void ProcessIndividualMessage(long connectionId, BatchedMessageEntry messageEntry)
        {
            try
            {
                // Set up the reader with the message data
                _reader.SetSource(messageEntry.Data);

                // Route to the appropriate message handler based on message type
                RouteMessage(connectionId, messageEntry.MessageType, _reader);
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Error processing individual message type {messageEntry.MessageType} from connection {connectionId}: {ex.Message}");
            }
        }

        private void RouteMessage(long connectionId, ushort messageType, NetDataReader reader)
        {
            // Route messages to appropriate handlers
            // For batched messages, we handle them directly here
            RouteToExistingHandlers(connectionId, messageType, reader);
        }

        private void RouteToExistingHandlers(long connectionId, ushort messageType, NetDataReader reader)
        {
            // Route based on message type to existing handlers
            // This is a simplified version - in practice you might want to use reflection
            // or a lookup table to route messages more efficiently

            switch (messageType)
            {
                case GameNetworkingConsts.GameMessage:
                    HandleGameMessage(connectionId, reader);
                    break;

                case GameNetworkingConsts.NotifyRewardExp:
                    HandleRewardExp(connectionId, reader);
                    break;

                case GameNetworkingConsts.NotifyRewardGold:
                    HandleRewardGold(connectionId, reader);
                    break;

                case GameNetworkingConsts.NotifyRewardItem:
                    HandleRewardItem(connectionId, reader);
                    break;

                case GameNetworkingConsts.NotifyRewardCurrency:
                    HandleRewardCurrency(connectionId, reader);
                    break;

                // Add more message types as needed
                default:
                    UnityEngine.Debug.LogWarning($"Unhandled batched message type: {messageType}");
                    break;
            }
        }

        // Individual message handlers that mirror the original handlers
        private void HandleGameMessage(long connectionId, NetDataReader reader)
        {
            var message = new GameMessage();
            message.Deserialize(reader);

            // For now, just log the message - in a real implementation,
            // this would route to the appropriate client systems
            UnityEngine.Debug.Log($"Received batched game message: {message.message}");
        }

        private void HandleRewardExp(long connectionId, NetDataReader reader)
        {
            var givenType = (RewardGivenType)reader.GetByte();
            var exp = reader.GetPackedInt();

            // For now, just log the reward - in a real implementation,
            // this would update the client's UI and stats
            UnityEngine.Debug.Log($"Received batched EXP reward: {exp} (type: {givenType})");
        }

        private void HandleRewardGold(long connectionId, NetDataReader reader)
        {
            var givenType = (RewardGivenType)reader.GetByte();
            var gold = reader.GetPackedInt();

            // For now, just log the reward - in a real implementation,
            // this would update the client's gold amount
            UnityEngine.Debug.Log($"Received batched gold reward: {gold} (type: {givenType})");
        }

        private void HandleRewardItem(long connectionId, NetDataReader reader)
        {
            var givenType = (RewardGivenType)reader.GetByte();
            var dataId = reader.GetPackedInt();
            var amount = reader.GetPackedInt();

            // For now, just log the reward - in a real implementation,
            // this would add items to the client's inventory
            UnityEngine.Debug.Log($"Received batched item reward: {dataId} x{amount} (type: {givenType})");
        }

        private void HandleRewardCurrency(long connectionId, NetDataReader reader)
        {
            var givenType = (RewardGivenType)reader.GetByte();
            var dataId = reader.GetPackedInt();
            var amount = reader.GetPackedInt();

            // For now, just log the reward - in a real implementation,
            // this would update the client's currency amounts
            UnityEngine.Debug.Log($"Received batched currency reward: {dataId} x{amount} (type: {givenType})");
        }

        /// <summary>
        /// Get batch processing statistics
        /// </summary>
        public BatchProcessingStats GetStats()
        {
            return new BatchProcessingStats
            {
                TotalBatchesReceived = _totalBatchesReceived,
                TotalMessagesProcessed = _totalMessagesProcessed,
                AverageProcessingTimeMs = _totalBatchesReceived > 0 ? (float)_totalProcessingTimeMs / _totalBatchesReceived : 0,
                AverageMessagesPerBatch = _totalBatchesReceived > 0 ? (float)_totalMessagesProcessed / _totalBatchesReceived : 0
            };
        }

        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStats()
        {
            _totalBatchesReceived = 0;
            _totalMessagesProcessed = 0;
            _totalProcessingTimeMs = 0;
        }
    }

    /// <summary>
    /// Statistics for batch message processing
    /// </summary>
    public struct BatchProcessingStats
    {
        public int TotalBatchesReceived;
        public int TotalMessagesProcessed;
        public float AverageProcessingTimeMs;
        public float AverageMessagesPerBatch;
    }
}