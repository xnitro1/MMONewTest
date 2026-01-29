namespace NightBlade
{
    /// <summary>
    /// Priority levels for network messages - determines when and how messages are batched/sent
    /// </summary>
    public enum MessagePriority
    {
        /// <summary>
        /// Critical messages that must be sent immediately (attacks, health changes, etc.)
        /// </summary>
        Critical = 0,

        /// <summary>
        /// High priority messages sent in next batch (position updates, animations)
        /// </summary>
        High = 1,

        /// <summary>
        /// Medium priority messages (stat changes, buff updates)
        /// </summary>
        Medium = 2,

        /// <summary>
        /// Low priority messages (ambient effects, idle state changes)
        /// </summary>
        Low = 3,
    }

    /// <summary>
    /// Delivery method for batched messages
    /// </summary>
    public enum BatchDeliveryMethod
    {
        /// <summary>
        /// Reliable ordered - guaranteed delivery in order
        /// </summary>
        ReliableOrdered,

        /// <summary>
        /// Reliable unordered - guaranteed delivery but order not preserved
        /// </summary>
        ReliableUnordered,

        /// <summary>
        /// Unreliable - may be lost, used for frequent updates
        /// </summary>
        Unreliable,
    }
}