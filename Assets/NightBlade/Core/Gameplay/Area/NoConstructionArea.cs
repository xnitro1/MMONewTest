using UnityEngine;

namespace NightBlade
{
    public class NoConstructionArea : MonoBehaviour, IUnHittable
    {
        private void Awake()
        {
            gameObject.layer = PhysicLayers.IgnoreRaycast;
        }
    }
}







