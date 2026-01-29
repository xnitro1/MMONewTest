using UnityEngine;

namespace NightBlade.CameraAndInput
{
    public interface IAimAssistAvoidanceListener
    {
        bool AvoidAimAssist(RaycastHit hitInfo);
    }
}







