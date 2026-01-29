using LiteNetLibManager;
using UnityEngine;

namespace NightBlade
{
    public partial class DefaultClientUserContentHandlers : MonoBehaviour, IClientUserContentHandlers
    {
        public LiteNetLibManager.LiteNetLibManager Manager { get; private set; }

        private void Awake()
        {
            Manager = GetComponent<LiteNetLibManager.LiteNetLibManager>();
        }

        // NOTE: Unlockable content functionality moved to addons - providing stub implementations
        public bool RequestAvailableContents(RequestAvailableContentsMessage data, ResponseDelegate<ResponseAvailableContentsMessage> callback)
        {
            // Functionality moved to addons
            return false;
        }

        public bool RequestUnlockContent(RequestUnlockContentMessage data, ResponseDelegate<ResponseUnlockContentMessage> callback)
        {
            // Functionality moved to addons
            return false;
        }
    }
}







