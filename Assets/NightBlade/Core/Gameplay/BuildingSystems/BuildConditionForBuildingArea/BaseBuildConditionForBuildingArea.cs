using UnityEngine;

namespace NightBlade
{
    public abstract class BaseBuildConditionForBuildingArea : MonoBehaviour
    {
        public abstract bool AllowToBuild(BuildingArea sourceArea, BuildingEntity newBuilding);
    }
}







