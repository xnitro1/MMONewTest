using UnityEngine;

namespace NightBlade
{
    public interface ICustomAnimationModel
    {
        void PlayCustomAnimation(int id, bool loop);
        void StopCustomAnimation();
        AnimationClip GetCustomAnimationClip(int id);
    }
}







