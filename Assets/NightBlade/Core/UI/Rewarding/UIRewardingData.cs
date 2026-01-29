using NightBlade.UnityEditorUtils;
using System.Collections.Generic;

namespace NightBlade
{
    public struct UIRewardingData
    {
        public int rewardExp;
        public int rewardGold;
        public int rewardCash;
        [ArrayElementTitle("item")]
        public List<RewardedItem> rewardItems;
    }
}







