using LiteNetLib;
using LiteNetLib.Utils;
using LiteNetLibManager;
using UnityEngine;

namespace NightBlade
{
    /// <summary>
    /// Extension to BaseGameNetworkManager that adds message batching capabilities
    /// </summary>
    public partial class BaseGameNetworkManager
    {
        [Header("Message Batching")]
        [Tooltip("Enable message batching for improved performance")]
        [SerializeField] private bool enableMessageBatching = true;

        [Tooltip("Automatically add batching components if not present")]
        [SerializeField] private bool autoSetupBatching = true;

        // Batching components
        private MessageBatcher _messageBatcher;
        private BatchMessageHandler _batchMessageHandler;
        private CharacterStateBatcher _characterStateBatcher;
        private MessageBatchingProfiler _messageBatchingProfiler;

        /// <summary>
        /// Get the message batcher component
        /// </summary>
        public MessageBatcher MessageBatcher => _messageBatcher;

        /// <summary>
        /// Get the batch message handler component
        /// </summary>
        public BatchMessageHandler BatchMessageHandler => _batchMessageHandler;

        /// <summary>
        /// Get the character state batcher component
        /// </summary>
        public CharacterStateBatcher CharacterStateBatcher => _characterStateBatcher;

        /// <summary>
        /// Get the message batching profiler component
        /// </summary>
        public MessageBatchingProfiler MessageBatchingProfiler => _messageBatchingProfiler;

        /// <summary>
        /// Override IsNetworkActive to prevent null reference exceptions
        /// </summary>
        public new bool IsNetworkActive
        {
            get
            {
                // Safe check to prevent null reference exceptions
                if (Client == null && Server == null)
                    return false;

                try
                {
                    // Call base implementation safely
                    return base.IsNetworkActive;
                }
                catch (System.NullReferenceException)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Override IsClientConnected to prevent null reference exceptions
        /// </summary>
        public new bool IsClientConnected
        {
            get
            {
                // Safe check to prevent null reference exceptions
                if (Client == null)
                    return false;

                try
                {
                    // Call base implementation safely
                    return base.IsClientConnected;
                }
                catch (System.NullReferenceException)
                {
                    return false;
                }
            }
        }


        // Initialize components and register messages after network initialization
        protected new virtual void Start()
        {
            // Ensure network transport is initialized before any network operations
            EnsureNetworkInitialized();

            // Set up components if auto-setup is enabled
            if (enableMessageBatching && autoSetupBatching)
            {
                SetupBatchingComponents();
            }

            // Register batching messages after network manager is fully initialized
            if (enableMessageBatching)
            {
                // Delay registration to ensure network manager is ready
                StartCoroutine(DelayedRegisterBatchingMessages());
            }
        }

        /// <summary>
        /// Ensure network transport handlers are initialized
        /// </summary>
        private void EnsureNetworkInitialized()
        {
            // Check if network components are initialized
            if (Client == null || Server == null)
            {
                try
                {
                    // Try to initialize network transport if not already done
                    // This uses reflection to call the private InitTransportAndHandlers method
                    var initMethod = typeof(LiteNetLibManager.LiteNetLibManager).GetMethod("InitTransportAndHandlers",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (initMethod != null)
                    {
                        initMethod.Invoke(this, null);
                        UnityEngine.Debug.Log("Network transport initialized via reflection");
                    }
                }
                catch (System.Exception ex)
                {
                    UnityEngine.Debug.LogWarning($"Failed to initialize network transport: {ex.Message}");
                }
            }
        }

        private System.Collections.IEnumerator DelayedRegisterBatchingMessages()
        {
            // Wait one frame to ensure everything is initialized
            yield return null;

            // Double-check that components exist
            if (_batchMessageHandler == null)
            {
                _batchMessageHandler = GetComponent<BatchMessageHandler>();
            }

            RegisterBatchingMessages();
        }


        private void SetupBatchingComponents()
        {
            // Get or add message batcher
            _messageBatcher = GetComponent<MessageBatcher>();
            if (_messageBatcher == null)
            {
                _messageBatcher = gameObject.AddComponent<MessageBatcher>();
                UnityEngine.Debug.Log("MessageBatcher component added automatically");
            }

            // Get or add batch message handler
            _batchMessageHandler = GetComponent<BatchMessageHandler>();
            if (_batchMessageHandler == null)
            {
                _batchMessageHandler = gameObject.AddComponent<BatchMessageHandler>();
                UnityEngine.Debug.Log("BatchMessageHandler component added automatically");
            }

            // Get or add character state batcher
            _characterStateBatcher = GetComponent<CharacterStateBatcher>();
            if (_characterStateBatcher == null)
            {
                _characterStateBatcher = gameObject.AddComponent<CharacterStateBatcher>();
                UnityEngine.Debug.Log("CharacterStateBatcher component added automatically");
            }

            // Get or add message batching profiler
            _messageBatchingProfiler = GetComponent<MessageBatchingProfiler>();
            if (_messageBatchingProfiler == null)
            {
                _messageBatchingProfiler = gameObject.AddComponent<MessageBatchingProfiler>();
                UnityEngine.Debug.Log("MessageBatchingProfiler component added automatically");
            }
        }

        private void RegisterBatchingMessages()
        {
            try
            {
                // Only register if the network manager is properly initialized
                if (Client == null || Server == null)
                {
                    UnityEngine.Debug.LogWarning("Cannot register batching messages - network manager not initialized yet");
                    return;
                }

                // Ensure components exist
                if (_batchMessageHandler == null)
                {
                    _batchMessageHandler = GetComponent<BatchMessageHandler>();
                }

                // Register batch message handlers
                if (_batchMessageHandler != null)
                {
                    RegisterClientMessage(GameNetworkingConsts.BatchedMessage, HandleBatchedMessageAtClient);
                    RegisterServerMessage(GameNetworkingConsts.BatchedMessage, HandleBatchedMessageAtServer);
                }

                // Register character state batch handler
                RegisterClientMessage(GameNetworkingConsts.CharacterStateBatch, HandleCharacterStateBatchAtClient);
                RegisterServerMessage(GameNetworkingConsts.CharacterStateBatch, HandleCharacterStateBatchAtServer);

                UnityEngine.Debug.Log("Message batching handlers registered successfully");
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogError($"Failed to register batching messages: {ex.Message}");
            }
        }

        /// <summary>
        /// Send a message using the batching system (if enabled)
        /// </summary>
        public void SendMessageBatched(long connectionId, DeliveryMethod deliveryMethod, ushort messageType, INetSerializable message, MessagePriority priority = MessagePriority.Medium)
        {
            try
            {
                if (enableMessageBatching && _messageBatcher != null && (Client != null || Server != null))
                {
                    _messageBatcher.QueueMessage(connectionId, deliveryMethod, messageType, message, priority);
                }
                else
                {
                    // Fallback to direct sending
                    ServerSendPacket(connectionId, 0, deliveryMethod, messageType, message);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to send batched message: {ex.Message}");
                // Fallback to direct sending
                try
                {
                    ServerSendPacket(connectionId, 0, deliveryMethod, messageType, message);
                }
                catch (System.Exception ex2)
                {
                    UnityEngine.Debug.LogError($"Failed to send message even with fallback: {ex2.Message}");
                }
            }
        }

        /// <summary>
        /// Send a raw message using the batching system (if enabled)
        /// </summary>
        public void SendRawMessageBatched(long connectionId, DeliveryMethod deliveryMethod, ushort messageType, byte[] data, MessagePriority priority = MessagePriority.Medium)
        {
            try
            {
                if (enableMessageBatching && _messageBatcher != null && (Client != null || Server != null))
                {
                    _messageBatcher.QueueRawMessage(connectionId, deliveryMethod, messageType, data, priority);
                }
                else
                {
                    // Fallback to direct sending
                    ServerSendPacket(connectionId, 0, deliveryMethod, messageType, (writer) => writer.Put(data));
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to send raw batched message: {ex.Message}");
                // Fallback to direct sending
                try
                {
                    ServerSendPacket(connectionId, 0, deliveryMethod, messageType, (writer) => writer.Put(data));
                }
                catch (System.Exception ex2)
                {
                    UnityEngine.Debug.LogError($"Failed to send raw message even with fallback: {ex2.Message}");
                }
            }
        }

        /// <summary>
        /// Force immediate sending of all batched messages for a specific priority
        /// </summary>
        public void FlushMessageBatch(MessagePriority priority)
        {
            try
            {
                if (_messageBatcher != null && (Client != null || Server != null))
                {
                    _messageBatcher.FlushPriority(priority);
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to flush message batch: {ex.Message}");
            }
        }

        /// <summary>
        /// Force immediate sending of all batched messages
        /// </summary>
        public void FlushAllMessageBatches()
        {
            try
            {
                if (_messageBatcher != null && (Client != null || Server != null))
                {
                    _messageBatcher.FlushAll();
                }
            }
            catch (System.Exception ex)
            {
                UnityEngine.Debug.LogWarning($"Failed to flush all message batches: {ex.Message}");
            }
        }

        // Message handlers for batched messages
        private void HandleBatchedMessageAtClient(MessageHandlerData messageHandler)
        {
            if (_batchMessageHandler != null)
            {
                // For client, we don't need connectionId as it's always from server
                _batchMessageHandler.HandleBatchedMessage(-1, messageHandler.Reader);
            }
        }

        private void HandleBatchedMessageAtServer(MessageHandlerData messageHandler)
        {
            if (_batchMessageHandler != null)
            {
                _batchMessageHandler.HandleBatchedMessage(messageHandler.ConnectionId, messageHandler.Reader);
            }
        }

        private void HandleCharacterStateBatchAtClient(MessageHandlerData messageHandler)
        {
            var batch = new CompressedCharacterStateBatch(0);
            batch.Deserialize(messageHandler.Reader);

            // Process each character state update in the batch
            foreach (var update in batch.Updates)
            {
                ProcessCharacterStateUpdate(update);
            }
        }

        private void HandleCharacterStateBatchAtServer(MessageHandlerData messageHandler)
        {
            var batch = new CompressedCharacterStateBatch(0);
            batch.Deserialize(messageHandler.Reader);

            // Process each character state update in the batch
            foreach (var update in batch.Updates)
            {
                ProcessCharacterStateUpdate(update);
            }
        }

        private void ProcessCharacterStateUpdate(CompressedCharacterStateUpdate update)
        {
            // This would be implemented to update the character's visual state
            // For now, just log for debugging
            UnityEngine.Debug.Log($"Processing character state update for {update.CharacterId} at position {update.NewPosition}");
        }

        /// <summary>
        /// Get message batching statistics
        /// </summary>
        public MessageBatchStats GetMessageBatchStats()
        {
            if (_messageBatcher != null)
            {
                return _messageBatcher.GetStats();
            }

            return new MessageBatchStats();
        }

        /// <summary>
        /// Get batch processing statistics
        /// </summary>
        public BatchProcessingStats GetBatchProcessingStats()
        {
            if (_batchMessageHandler != null)
            {
                return _batchMessageHandler.GetStats();
            }

            return new BatchProcessingStats();
        }

        /// <summary>
        /// Reset all batching statistics
        /// </summary>
        public void ResetBatchStats()
        {
            if (_messageBatcher != null)
            {
                _messageBatcher.ResetStats();
            }

            if (_batchMessageHandler != null)
            {
                _batchMessageHandler.ResetStats();
            }
        }
    }
}