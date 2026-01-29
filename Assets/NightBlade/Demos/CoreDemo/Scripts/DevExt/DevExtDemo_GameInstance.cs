using NightBlade.DevExtension;
using UnityEngine;

namespace NightBlade
{
    public partial class GameInstance
    {
        [DevExtMethods("Awake")]
        public void DevExtAwake()
        {
            Debug.Log("[DevExt] GameInstance - Awake");
        }

        [DevExtMethods("LoadedGameData")]
        public void DevExtLoadedGameData()
        {
            Debug.Log("[DevExt] GameInstance - LoadedGameData");
        }
    }
}







