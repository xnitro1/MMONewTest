using LiteNetLib.Utils;
using System.Collections.Generic;

namespace NightBlade
{
    /// <summary>
    /// A batched message containing multiple individual messages for efficient network transmission
    /// </summary>
    public struct BatchedMessage : INetSerializable
    {
        /// <summary>
        /// The batch ID for tracking and debugging
        /// </summary>
        public uint BatchId;

        /// <summary>
        /// Timestamp when the batch was created
        /// </summary>
        public float Timestamp;

        /// <summary>
        /// List of individual messages in this batch
        /// </summary>
        public List<BatchedMessageEntry> Messages;

        public BatchedMessage(uint batchId)
        {
            BatchId = batchId;
            Timestamp = UnityEngine.Time.time;
            Messages = new List<BatchedMessageEntry>();
        }

        public void AddMessage(ushort messageType, byte[] data, MessagePriority priority = MessagePriority.Medium)
        {
            Messages.Add(new BatchedMessageEntry
            {
                MessageType = messageType,
                Data = data,
                Priority = priority
            });
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUInt(BatchId);
            writer.Put(Timestamp);
            writer.PutPackedInt(Messages.Count);

            foreach (var message in Messages)
            {
                message.Serialize(writer);
            }
        }

        public void Deserialize(NetDataReader reader)
        {
            BatchId = reader.GetPackedUInt();
            Timestamp = reader.GetFloat();
            int messageCount = reader.GetPackedInt();

            Messages = new List<BatchedMessageEntry>(messageCount);
            for (int i = 0; i < messageCount; i++)
            {
                var message = new BatchedMessageEntry();
                message.Deserialize(reader);
                Messages.Add(message);
            }
        }
    }

    /// <summary>
    /// Individual message entry within a batch
    /// </summary>
    public struct BatchedMessageEntry : INetSerializable
    {
        /// <summary>
        /// The original message type constant
        /// </summary>
        public ushort MessageType;

        /// <summary>
        /// Serialized message data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Priority of this message within the batch
        /// </summary>
        public MessagePriority Priority;

        public void Serialize(NetDataWriter writer)
        {
            writer.PutPackedUShort(MessageType);
            writer.PutBytesWithLength(Data);
            writer.Put((byte)Priority);
        }

        public void Deserialize(NetDataReader reader)
        {
            MessageType = reader.GetPackedUShort();
            Data = reader.GetBytesWithLength();
            Priority = (MessagePriority)reader.GetByte();
        }
    }
}