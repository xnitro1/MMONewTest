using UnityEngine;

namespace NightBlade
{
    public interface IMapMarker
    {
        string MapMarkerType { get; }
        string MapMarkerId { get; }
        Transform transform { get; }
        GameObject gameObject { get; }
    }
}







