using UnityEngine;

namespace NightBlade
{
    public interface IShooterBuildAimController : IBuildAimController
    {
        void UpdateCameraLookData(Ray centerRay, float centerOriginToCharacterDistance, Vector3 cameraForward, Vector3 cameraRight);
    }
}







