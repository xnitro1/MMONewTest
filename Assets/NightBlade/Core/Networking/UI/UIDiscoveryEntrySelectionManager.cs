using UnityEngine.Events;

namespace NightBlade
{
    [System.Serializable]
    public class DiscoveryDataEvent : UnityEvent<DiscoveryData> { }

    [System.Serializable]
    public class UIDiscoveryEntryEvent : UnityEvent<UIDiscoveryEntry> { }

    public class UIDiscoveryEntrySelectionManager : UISelectionManager<DiscoveryData, UIDiscoveryEntry, DiscoveryDataEvent, UIDiscoveryEntryEvent>
    {
    }
}







