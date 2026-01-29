using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NightBlade
{
    [System.Serializable]
    public class StorageSaveData
    {
        public List<StorageCharacterItem> storageItems = new List<StorageCharacterItem>();

        private static string GetPath(string id)
        {
            return Path.Combine(Application.persistentDataPath, $"{id}_storage.json.gz");
        }

        /// <summary>
        /// Save storage data asynchronously using modern JSON serialization.
        /// </summary>
        public async UniTask SavePersistentDataAsync(string id)
        {
            string path = GetPath(id);
            await SaveSystem.SaveAsync(path, this);
        }

        /// <summary>
        /// Load storage data asynchronously.
        /// </summary>
        public async UniTask LoadPersistentDataAsync(string id)
        {
            string path = GetPath(id);
            storageItems.Clear();

            var loaded = await SaveSystem.LoadAsync<StorageSaveData>(path);
            if (loaded?.storageItems != null)
            {
                storageItems = loaded.storageItems;
            }
        }

        /// <summary>
        /// Synchronous save - blocks until save is complete.
        /// Use SavePersistentDataAsync when possible.
        /// </summary>
        public void SavePersistentData(string id)
        {
            string path = GetPath(id);
            SaveSystem.Save(path, this);
        }

        /// <summary>
        /// Synchronous load - blocks until complete.
        /// Prefer LoadPersistentDataAsync when possible.
        /// </summary>
        public void LoadPersistentData(string id)
        {
            string path = GetPath(id);
            storageItems.Clear();

            var loaded = SaveSystem.Load<StorageSaveData>(path);
            if (loaded?.storageItems != null)
            {
                storageItems = loaded.storageItems;
            }
        }
    }
}
