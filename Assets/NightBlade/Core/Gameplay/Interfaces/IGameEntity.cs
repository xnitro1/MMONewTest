using LiteNetLibManager;

namespace NightBlade
{
    public interface IGameEntity : ITargetableEntity
    {
        BaseGameEntity Entity { get; }
        LiteNetLibIdentity Identity { get; }
        void PrepareRelatesData();
        EntityInfo GetInfo();
        bool IsHide();
        bool IsRevealsHide();
        bool IsBlind();
    }
}







