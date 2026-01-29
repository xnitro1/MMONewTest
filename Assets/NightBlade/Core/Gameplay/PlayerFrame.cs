using UnityEngine;

namespace NightBlade
{
    // NOTE: PlayerFrame functionality moved to addons - this is a placeholder
    // [CreateAssetMenu(fileName = GameDataMenuConsts.PLAYER_FRAME_FILE, menuName = GameDataMenuConsts.PLAYER_FRAME_MENU, order = GameDataMenuConsts.PLAYER_FRAME_ORDER)]
    public partial class PlayerFrame : BaseGameData, IUnlockableGameData
    {
        [SerializeField]
        private UnlockRequirement unlockRequirement;
        public UnlockRequirement UnlockRequirement
        {
            get { return unlockRequirement; }
        }
    }
}







