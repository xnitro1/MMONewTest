using UnityEngine;

namespace NightBlade
{
    // NOTE: PlayerIcon functionality moved to addons - this is a placeholder
    // [CreateAssetMenu(fileName = GameDataMenuConsts.PLAYER_ICON_FILE, menuName = GameDataMenuConsts.PLAYER_ICON_MENU, order = GameDataMenuConsts.PLAYER_ICON_ORDER)]
    public partial class PlayerIcon : BaseGameData, IUnlockableGameData
    {
        [SerializeField]
        private UnlockRequirement unlockRequirement;
        public UnlockRequirement UnlockRequirement
        {
            get { return unlockRequirement; }
        }
    }
}







