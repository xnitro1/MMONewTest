using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NightBlade
{
    [System.Serializable]
    public class WorldSaveData
    {
        public List<BuildingSaveData> buildings = new List<BuildingSaveData>();

        private static string GetPath(string id, string map)
        {
            return Path.Combine(Application.persistentDataPath, $"{id}_world_{map}.json.gz");
        }

        /// <summary>
        /// Save world data asynchronously using modern JSON serialization.
        /// </summary>
        public async UniTask SavePersistentDataAsync(string id, string map)
        {
            string path = GetPath(id, map);
            await SaveSystem.SaveAsync(path, this);
        }

        /// <summary>
        /// Load world data asynchronously.
        /// </summary>
        public async UniTask LoadPersistentDataAsync(string id, string map)
        {
            string path = GetPath(id, map);
            buildings.Clear();

            var loaded = await SaveSystem.LoadAsync<WorldSaveData>(path);
            if (loaded?.buildings != null)
            {
                buildings = loaded.buildings;
            }
        }

        /// <summary>
        /// Synchronous save - blocks until save is complete.
        /// Use SavePersistentDataAsync when possible.
        /// </summary>
        public void SavePersistentData(string id, string map)
        {
            string path = GetPath(id, map);
            SaveSystem.Save(path, this);
        }

        /// <summary>
        /// Synchronous load - blocks until complete.
        /// Prefer LoadPersistentDataAsync when possible.
        /// </summary>
        public void LoadPersistentData(string id, string map)
        {
            string path = GetPath(id, map);
            buildings.Clear();

            var loaded = SaveSystem.Load<WorldSaveData>(path);
            if (loaded?.buildings != null)
            {
                buildings = loaded.buildings;
            }
        }
    }
}
