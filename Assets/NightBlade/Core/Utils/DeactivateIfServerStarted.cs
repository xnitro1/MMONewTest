using UnityEngine;
using NightBlade;

namespace UtilsComponents
{
    public class DeactivateIfServerStarted : MonoBehaviour
    {
        private void Update()
        {
            if (BaseGameNetworkManager.Singleton != null && BaseGameNetworkManager.Singleton.IsServer)
                gameObject.SetActive(false);
        }
    }
}







