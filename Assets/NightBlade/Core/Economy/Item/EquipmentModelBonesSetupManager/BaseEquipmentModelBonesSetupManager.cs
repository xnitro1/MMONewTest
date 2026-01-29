using UnityEngine;

namespace NightBlade
{
    public abstract class BaseEquipmentModelBonesSetupManager : ScriptableObject
    {
        public abstract void Setup(BaseCharacterModel characterModel, EquipmentModel equipmentModel, GameObject instantiatedObject, BaseEquipmentEntity instantiatedEntity, EquipmentInstantiatedObjectGroup instantiatedObjectGroup, EquipmentContainer equipmentContainer);
    }
}







