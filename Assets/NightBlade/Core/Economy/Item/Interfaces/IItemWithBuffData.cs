namespace NightBlade
{
    public interface IItemWithBuffData : IItem
    {
        /// <summary>
        /// Buff data for this item
        /// </summary>
        Buff BuffData { get; }
    }
}







