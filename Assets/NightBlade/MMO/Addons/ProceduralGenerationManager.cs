using UnityEngine;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace NightBlade.Addons
{
    public class ProceduralGenerationManager : BaseGameNetworkManagerComponent
    {
        public static ProceduralGenerationManager Singleton { get; private set; }

        [SerializeField] private int _currentInstanceSeed = 0;
        public int CurrentInstanceSeed => _currentInstanceSeed;

        // Dieses Event informiert jeden, der zuhört, wenn ein neuer Seed da ist
        public event System.Action<int> OnWorldSeedReceived;

        private void Awake()
        {
            Singleton = this;
        }

        public override void OnServerOnlineSceneLoaded(BaseGameNetworkManager manager)
        {
            if (manager.IsServer)
            {
                _currentInstanceSeed = System.Guid.NewGuid().GetHashCode();
                OnWorldSeedReceived?.Invoke(_currentInstanceSeed);
            }
        }

        public override void WriteMapInfoExtra(BaseGameNetworkManager manager, NetDataWriter writer)
        {
            writer.Put(_currentInstanceSeed);
        }

        public override void ReadMapInfoExtra(BaseGameNetworkManager manager, NetDataReader reader)
        {
            _currentInstanceSeed = reader.GetInt();
            Debug.Log($"[Seed-Sync] Seed empfangen: {_currentInstanceSeed}");
            OnWorldSeedReceived?.Invoke(_currentInstanceSeed);
        }
    }
}