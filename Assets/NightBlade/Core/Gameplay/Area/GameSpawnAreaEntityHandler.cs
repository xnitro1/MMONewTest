using LiteNetLibManager;
using UnityEngine;

namespace NightBlade
{
    [DisallowMultipleComponent]
    public class GameSpawnAreaEntityHandler : MonoBehaviour
    {
        public GameSpawnAreaSubscribeHandler Handler { get; set; }
        public LiteNetLibBehaviour Entity { get; set; }

        private void OnDestroy()
        {
            if (Handler == null)
                return;
            Handler.OnEntityHandlerDestroy(this);
            Handler = null;
            Entity = null;
        }
    }
}







