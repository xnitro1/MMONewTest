using UnityEngine;

namespace NightBlade
{
    public interface IBuildAimController
    {
        void Init();
        AimPosition UpdateAimControls(Vector2 aimAxes, BuildingEntity prefab);
        void FinishAimControls(bool isCancel);
    }
}







