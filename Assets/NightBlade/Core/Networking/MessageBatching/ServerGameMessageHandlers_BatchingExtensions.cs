using LiteNetLib;
using LiteNetLib.Utils;

namespace NightBlade
{
    /// <summary>
    /// Extensions for ServerGameMessageHandlers that use message batching for improved performance
    /// </summary>
    public static class ServerGameMessageHandlers_BatchingExtensions
    {
        /// <summary>
        /// Send game message using batching system
        /// </summary>
        public static void SendGameMessageBatched(this IServerGameMessageHandlers handler, long connectionId, UITextKeys message, MessagePriority priority = MessagePriority.Medium)
        {
            var gameMessage = new GameMessage { message = message };
            GetNetworkManager()?.SendMessageBatched(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.GameMessage, gameMessage, priority);
        }

        /// <summary>
        /// Send reward experience notification using batching system
        /// </summary>
        public static void NotifyRewardExpBatched(this IServerGameMessageHandlers handler, long connectionId, RewardGivenType givenType, int exp, MessagePriority priority = MessagePriority.High)
        {
            if (exp <= 0) return;

            // Create the data that would be sent in the original method
            var writer = new NetDataWriter();
            writer.Put((byte)givenType);
            writer.PutPackedInt(exp);

            GetNetworkManager()?.SendRawMessageBatched(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardExp, writer.Data, priority);
        }

        /// <summary>
        /// Send reward gold notification using batching system
        /// </summary>
        public static void NotifyRewardGoldBatched(this IServerGameMessageHandlers handler, long connectionId, RewardGivenType givenType, int gold, MessagePriority priority = MessagePriority.High)
        {
            if (gold <= 0) return;

            var writer = new NetDataWriter();
            writer.Put((byte)givenType);
            writer.PutPackedInt(gold);

            GetNetworkManager()?.SendRawMessageBatched(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardGold, writer.Data, priority);
        }

        /// <summary>
        /// Send reward item notification using batching system
        /// </summary>
        public static void NotifyRewardItemBatched(this IServerGameMessageHandlers handler, long connectionId, RewardGivenType givenType, int dataId, int amount, MessagePriority priority = MessagePriority.High)
        {
            if (amount <= 0) return;

            var writer = new NetDataWriter();
            writer.Put((byte)givenType);
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(amount);

            GetNetworkManager()?.SendRawMessageBatched(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardItem, writer.Data, priority);
        }

        /// <summary>
        /// Send reward currency notification using batching system
        /// </summary>
        public static void NotifyRewardCurrencyBatched(this IServerGameMessageHandlers handler, long connectionId, RewardGivenType givenType, int dataId, int amount, MessagePriority priority = MessagePriority.High)
        {
            if (amount <= 0) return;

            var writer = new NetDataWriter();
            writer.Put((byte)givenType);
            writer.PutPackedInt(dataId);
            writer.PutPackedInt(amount);

            GetNetworkManager()?.SendRawMessageBatched(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.NotifyRewardCurrency, writer.Data, priority);
        }

        /// <summary>
        /// Send formatted game message using batching system
        /// </summary>
        public static void SendFormattedGameMessageBatched(this IServerGameMessageHandlers handler, long connectionId, UIFormatKeys format, string[] args, MessagePriority priority = MessagePriority.Medium)
        {
            var formattedMessage = new FormattedGameMessage
            {
                format = format,
                args = args
            };
            GetNetworkManager()?.SendMessageBatched(connectionId, DeliveryMethod.ReliableOrdered, GameNetworkingConsts.FormattedGameMessage, formattedMessage, priority);
        }

        /// <summary>
        /// Send multiple reward notifications in a batch (combines multiple rewards into one batch)
        /// </summary>
        public static void SendRewardBundleBatched(this IServerGameMessageHandlers handler, long connectionId, RewardBundle rewards, MessagePriority priority = MessagePriority.High)
        {
            var networkManager = GetNetworkManager();
            if (networkManager == null) return;

            // Send all rewards in sequence - they will be batched together
            if (rewards.Exp > 0)
                handler.NotifyRewardExpBatched(connectionId, rewards.ExpGivenType, rewards.Exp, priority);

            if (rewards.Gold > 0)
                handler.NotifyRewardGoldBatched(connectionId, rewards.GoldGivenType, rewards.Gold, priority);

            foreach (var itemReward in rewards.ItemRewards)
            {
                if (itemReward.amount > 0)
                    handler.NotifyRewardItemBatched(connectionId, itemReward.givenType, itemReward.dataId, itemReward.amount, priority);
            }

            foreach (var currencyReward in rewards.CurrencyRewards)
            {
                if (currencyReward.amount > 0)
                    handler.NotifyRewardCurrencyBatched(connectionId, currencyReward.givenType, currencyReward.dataId, currencyReward.amount, priority);
            }
        }

        /// <summary>
        /// Get the current network manager instance
        /// </summary>
        private static BaseGameNetworkManager GetNetworkManager()
        {
            return BaseGameNetworkManager.Singleton;
        }
    }

    /// <summary>
    /// Structure for bundling multiple reward types together
    /// </summary>
    public struct RewardBundle
    {
        public int Exp;
        public RewardGivenType ExpGivenType;
        public int Gold;
        public RewardGivenType GoldGivenType;
        public ItemRewardData[] ItemRewards;
        public CurrencyRewardData[] CurrencyRewards;

        public RewardBundle(int exp = 0, RewardGivenType expGivenType = RewardGivenType.None,
                           int gold = 0, RewardGivenType goldGivenType = RewardGivenType.None,
                           ItemRewardData[] itemRewards = null, CurrencyRewardData[] currencyRewards = null)
        {
            Exp = exp;
            ExpGivenType = expGivenType;
            Gold = gold;
            GoldGivenType = goldGivenType;
            ItemRewards = itemRewards ?? new ItemRewardData[0];
            CurrencyRewards = currencyRewards ?? new CurrencyRewardData[0];
        }
    }

    /// <summary>
    /// Individual item reward data
    /// </summary>
    public struct ItemRewardData
    {
        public int dataId;
        public int amount;
        public RewardGivenType givenType;

        public ItemRewardData(int dataId, int amount, RewardGivenType givenType = RewardGivenType.None)
        {
            this.dataId = dataId;
            this.amount = amount;
            this.givenType = givenType;
        }
    }

    /// <summary>
    /// Individual currency reward data
    /// </summary>
    public struct CurrencyRewardData
    {
        public int dataId;
        public int amount;
        public RewardGivenType givenType;

        public CurrencyRewardData(int dataId, int amount, RewardGivenType givenType = RewardGivenType.None)
        {
            this.dataId = dataId;
            this.amount = amount;
            this.givenType = givenType;
        }
    }
}