using System.Runtime.InteropServices;

namespace NightBlade
{
    /// <summary>
    /// NOTE: Make this struct to be partial struct in-case other developer wants to add other options
    /// </summary>
    [System.Serializable]
    [StructLayout(LayoutKind.Auto)]
    public partial struct GuildOptions
    {
        public int iconDataId;
    }
}







