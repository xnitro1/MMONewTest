using UnityEngine;

namespace NightBlade
{
    public interface IEntityMovementFactory
    {
        string Name { get; }
        bool ValidateSourceObject(GameObject obj);
        IEntityMovementComponent Setup(GameObject obj, ref Bounds bounds);
    }
}







