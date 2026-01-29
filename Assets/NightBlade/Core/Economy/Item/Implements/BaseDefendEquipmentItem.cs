using NightBlade.UnityEditorUtils;
using UnityEngine;

namespace NightBlade
{
    public abstract partial class BaseDefendEquipmentItem : BaseEquipmentItem, IDefendEquipmentItem
    {
        [Category("Buff/Bonus Settings")]
        [SerializeField]
        private ArmorIncremental armorAmount = default;
        public ArmorIncremental ArmorAmount
        {
            get { return armorAmount; }
        }
    }
}







