using LiteNetLib.Utils;

namespace NightBlade
{
    /// <summary>
    /// Network message validation extensions
    /// </summary>
    public static class NetworkMessageValidation
    {
        /// <summary>
        /// Validates RequestIncreaseAttributeAmountMessage
        /// </summary>
        public static bool IsValid(this RequestIncreaseAttributeAmountMessage message)
        {
            return DataValidation.IsValidDataId(message.dataId);
        }

        /// <summary>
        /// Validates RequestIncreaseSkillLevelMessage
        /// </summary>
        public static bool IsValid(this RequestIncreaseSkillLevelMessage message)
        {
            return DataValidation.IsValidDataId(message.dataId);
        }

        /// <summary>
        /// Validates RequestDepositUserGoldMessage
        /// </summary>
        public static bool IsValid(this RequestDepositUserGoldMessage message)
        {
            return DataValidation.IsValidGold(message.gold) && message.gold > 0;
        }

        /// <summary>
        /// Validates RequestWithdrawUserGoldMessage
        /// </summary>
        public static bool IsValid(this RequestWithdrawUserGoldMessage message)
        {
            return DataValidation.IsValidGold(message.gold) && message.gold > 0;
        }

        /// <summary>
        /// Validates RequestCashShopBuyMessage
        /// </summary>
        public static bool IsValid(this RequestCashShopBuyMessage message)
        {
            return DataValidation.IsValidDataId(message.dataId) &&
                   DataValidation.IsValidRange(message.amount, 1, 999);
        }

        /// <summary>
        /// Validates RequestIncreaseAttributeAmountMessage with additional context
        /// </summary>
        public static bool IsValidForPlayer(this RequestIncreaseAttributeAmountMessage message, IPlayerCharacterData player)
        {
            if (!message.IsValid())
                return false;

            if (player == null)
                return false;

            // Check if player has enough stat points
            if (player.StatPoint <= 0)
                return false;

            // Check if the attribute exists
            if (!GameInstance.Attributes.TryGetValue(message.dataId, out var attribute))
                return false;

            return true;
        }

        /// <summary>
        /// Validates RequestIncreaseSkillLevelMessage with additional context
        /// </summary>
        public static bool IsValidForPlayer(this RequestIncreaseSkillLevelMessage message, IPlayerCharacterData player)
        {
            if (!message.IsValid())
                return false;

            if (player == null)
                return false;

            // Check if player has enough skill points
            if (player.SkillPoint <= 0)
                return false;

            // Check if the skill exists
            if (!GameInstance.Skills.TryGetValue(message.dataId, out var skill))
                return false;

            // Check if player already has this skill and can level it up
            int skillIndex = player.IndexOfSkill(message.dataId);
            if (skillIndex < 0)
                return false; // Must learn skill first

            var existingSkill = player.Skills[skillIndex];
            if (existingSkill.level >= skill.maxLevel)
                return false; // Already max level

            return true;
        }
    }
}







