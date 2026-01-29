using UnityEngine;

namespace NightBlade
{
    public interface IZoomWeaponAbilityController : IWeaponAbilityController
    {
        void InitialZoomCrosshair();
        void SetZoomCrosshairSprite(Sprite sprite);
        bool ShowZoomCrosshair { get; set; }
    }
}







