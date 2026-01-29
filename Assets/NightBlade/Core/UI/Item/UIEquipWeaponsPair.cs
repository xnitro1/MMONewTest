using UnityEngine;

namespace NightBlade
{
    [System.Serializable]
    public struct UIEquipWeaponsPair
    {
        [Range(0, 15)]
        public byte equipWeaponSetIndex;
        public UICharacterItem rightHandSlot;
        public UICharacterItem leftHandSlot;
    }
}







