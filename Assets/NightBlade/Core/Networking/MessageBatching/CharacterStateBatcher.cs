using LiteNetLib;
using LiteNetLib.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace NightBlade
{
    /// <summary>
    /// Specialized batcher for character state updates with delta compression
    /// </summary>
    public class CharacterStateBatcher : MonoBehaviour
    {
        [Header("Character State Batching")]
        [Tooltip("Enable character state batching")]
        [SerializeField] private bool enableCharacterStateBatching = true;

        [Tooltip("Maximum character states per batch")]
        [SerializeField] private int maxStatesPerBatch = 20;

        [Tooltip("Send interval for character state updates (seconds)")]
        [SerializeField] private float stateUpdateInterval = 0.1f; // 10 FPS

        [Tooltip("Position change threshold for sending updates")]
        [SerializeField] private float positionThreshold = 0.1f;

        [Tooltip("Rotation change threshold for sending updates (degrees)")]
        [SerializeField] private float rotationThreshold = 5f;

        // Character state tracking
        private readonly Dictionary<string, CharacterStateSnapshot> _lastStates = new Dictionary<string, CharacterStateSnapshot>();
        private readonly List<CharacterStateUpdate> _pendingUpdates = new List<CharacterStateUpdate>();
        private float _lastSendTime;

        private BaseGameNetworkManager _networkManager;

        private void Awake()
        {
            _networkManager = GetComponent<BaseGameNetworkManager>();
        }

        private void Update()
        {
            if (!enableCharacterStateBatching || _networkManager == null)
                return;

            // Check if network components are initialized
            if (_networkManager.Client == null && _networkManager.Server == null && !Application.isEditor)
                return;

            float currentTime = Time.time;
            if (currentTime - _lastSendTime >= stateUpdateInterval)
            {
                SendCharacterStateBatch();
                _lastSendTime = currentTime;
            }
        }

        /// <summary>
        /// Queue a character state update for batching
        /// </summary>
        public void QueueCharacterStateUpdate(string characterId, Vector3 position, Quaternion rotation,
                                             long connectionId = -1)
        {
            if (!enableCharacterStateBatching) return;

            var currentState = new CharacterStateSnapshot
            {
                CharacterId = characterId,
                Position = position,
                Rotation = rotation,
                Timestamp = Time.time
            };

            // Check if this is a significant change
            if (!IsSignificantChange(characterId, currentState))
                return;

            // Update last known state
            _lastStates[characterId] = currentState;

            // Create delta update
            var deltaUpdate = new CharacterStateUpdate
            {
                CharacterId = characterId,
                Position = position,
                Rotation = rotation,
                ConnectionId = connectionId
            };

            _pendingUpdates.Add(deltaUpdate);

            // Send immediately if we have too many updates queued
            if (_pendingUpdates.Count >= maxStatesPerBatch)
            {
                SendCharacterStateBatch();
            }
        }

        private bool IsSignificantChange(string characterId, CharacterStateSnapshot newState)
        {
            if (!_lastStates.TryGetValue(characterId, out var lastState))
                return true; // First update for this character

            // Check position change
            if (Vector3.Distance(newState.Position, lastState.Position) > positionThreshold)
                return true;

            // Check rotation change
            if (Quaternion.Angle(newState.Rotation, lastState.Rotation) > rotationThreshold)
                return true;

            // Check stat changes (simplified - compare key stats)
            // TODO: Implement proper stat comparison with thresholds
            // For now, we'll rely on position/rotation changes for batching decisions

            // Check buff changes (simplified - just count for now)
            // TODO: Implement proper buff comparison
            // if (newState.Buffs.Count != lastState.Buffs.Count)
            //     return true;

            // Could add more detailed buff comparison here if needed

            return false;
        }

        private void SendCharacterStateBatch()
        {
            if (_pendingUpdates.Count == 0) return;

            // Group updates by connection ID (for server to client)
            var updatesByConnection = new Dictionary<long, List<CharacterStateUpdate>>();

            foreach (var update in _pendingUpdates)
            {
                if (!updatesByConnection.ContainsKey(update.ConnectionId))
                    updatesByConnection[update.ConnectionId] = new List<CharacterStateUpdate>();
                updatesByConnection[update.ConnectionId].Add(update);
            }

            // Send batch for each connection
            foreach (var kvp in updatesByConnection)
            {
                SendCharacterStateBatchForConnection(kvp.Key, kvp.Value);
            }

            _pendingUpdates.Clear();
        }

        private void SendCharacterStateBatchForConnection(long connectionId, List<CharacterStateUpdate> updates)
        {
            var batch = new CompressedCharacterStateBatch(GameNetworkingConsts.GetNextBatchId());
            batch.Timestamp = Time.time;

            foreach (var update in updates)
            {
                // Get previous state for delta compression
                CharacterStateSnapshot? previousState = GetLastCharacterState(update.CharacterId);
                batch.AddCompressedStateUpdate(update, previousState);
            }

            // Send using the network manager's batching system
            if (_networkManager.MessageBatcher != null)
            {
                _networkManager.SendMessageBatched(connectionId, DeliveryMethod.Unreliable,
                    GameNetworkingConsts.CharacterStateBatch, batch, MessagePriority.Medium);
            }
            else
            {
                // Fallback to direct sending
                _networkManager.ServerSendPacket(connectionId, 0, DeliveryMethod.Unreliable,
                    GameNetworkingConsts.CharacterStateBatch, batch);
            }
        }

        /// <summary>
        /// Force immediate sending of all queued character state updates
        /// </summary>
        public void FlushCharacterStateUpdates()
        {
            SendCharacterStateBatch();
        }

        /// <summary>
        /// Clear state tracking for a character (e.g., when they disconnect)
        /// </summary>
        public void ClearCharacterState(string characterId)
        {
            _lastStates.Remove(characterId);
            _pendingUpdates.RemoveAll(u => u.CharacterId == characterId);
        }

        /// <summary>
        /// Get the last known state for a character
        /// </summary>
        public CharacterStateSnapshot? GetLastCharacterState(string characterId)
        {
            if (_lastStates.TryGetValue(characterId, out var state))
                return state;
            return null;
        }
    }

    /// <summary>
    /// Snapshot of a character's position and rotation state
    /// </summary>
    public struct CharacterStateSnapshot
    {
        public string CharacterId;
        public Vector3 Position;
        public Quaternion Rotation;
        public float Timestamp;
    }

    /// <summary>
    /// Individual character state update
    /// </summary>
    public struct CharacterStateUpdate
    {
        public string CharacterId;
        public Vector3 Position;
        public Quaternion Rotation;
        public long ConnectionId;
    }

    /// <summary>
    /// Batch of character state updates with delta compression
    /// </summary>
    public struct CompressedCharacterStateBatch : INetSerializable
    {
        public uint BatchId;
        public float Timestamp;
        public List<CompressedCharacterStateUpdate> Updates;

        public CompressedCharacterStateBatch(uint batchId)
        {
            BatchId = batchId;
            Timestamp = Time.time;
            Updates = new List<CompressedCharacterStateUpdate>();
        }

        public void AddCompressedStateUpdate(CharacterStateUpdate update, CharacterStateSnapshot? previousState)
        {
            var compressed = new CompressedCharacterStateUpdate
            {
                CharacterId = update.CharacterId,
                PreviousPosition = previousState?.Position ?? Vector3.zero,
                PreviousRotation = previousState?.Rotation ?? Quaternion.identity,
                NewPosition = update.Position,
                NewRotation = update.Rotation
            };
            Updates.Add(compressed);
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(BatchId);
            writer.Put(Timestamp);
            writer.PutPackedInt(Updates.Count);

            foreach (var update in Updates)
            {
                update.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            BatchId = reader.GetPackedUInt();
            Timestamp = reader.GetFloat();
            int updateCount = reader.GetPackedInt();

            Updates = new List<CompressedCharacterStateUpdate>(updateCount);
            for (int i = 0; i < updateCount; i++)
            {
                var update = new CompressedCharacterStateUpdate();
                update.Deserialize(reader);
                Updates.Add(update);
            }
        }
    }

    /// <summary>
    /// Compressed character state update using delta encoding
    /// </summary>
    public struct CompressedCharacterStateUpdate : INetSerializable
    {
        public string CharacterId;
        public Vector3 PreviousPosition;
        public Quaternion PreviousRotation;
        public Vector3 NewPosition;
        public Quaternion NewRotation;

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(CharacterId);

            // Compress position
            DeltaCompressor.SerializeCompressedPosition(writer, NewPosition, PreviousPosition);

            // Compress rotation
            DeltaCompressor.SerializeCompressedRotation(writer, NewRotation, PreviousRotation);
        }

        public void Deserialize(NetDataReader reader)
        {
            CharacterId = reader.GetString();

            // Decompress position
            NewPosition = DeltaCompressor.DeserializeCompressedPosition(reader, PreviousPosition);

            // Decompress rotation
            NewRotation = DeltaCompressor.DeserializeCompressedRotation(reader, PreviousRotation);
        }
    }
}