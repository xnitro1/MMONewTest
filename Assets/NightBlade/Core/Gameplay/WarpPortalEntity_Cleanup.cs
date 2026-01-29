namespace NightBlade
{
    public partial class WarpPortalEntity
    {
        public override void Clean()
        {
            base.Clean();
            warpSignals?.Nulling();
            warpToMapInfo = null;
        }
    }
}







