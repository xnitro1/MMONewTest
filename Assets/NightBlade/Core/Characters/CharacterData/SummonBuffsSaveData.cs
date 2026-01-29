using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NightBlade
{
    [System.Serializable]
    public class SummonBuffsSaveData
    {
        public List<CharacterBuff> summonBuffs = new List<CharacterBuff>();

        private static string GetPath(string id)
        {
            return Path.Combine(Application.persistentDataPath, $"{id}_summon_buffs.json.gz");
        }

        /// <summary>
        /// Save summon buffs data asynchronously using modern JSON serialization.
        /// </summary>
        public async UniTask SavePersistentDataAsync(string id)
        {
            string path = GetPath(id);
            await SaveSystem.SaveAsync(path, this);
        }

        /// <summary>
        /// Load summon buffs data asynchronously.
        /// </summary>
        public async UniTask LoadPersistentDataAsync(string id)
        {
            string path = GetPath(id);
            summonBuffs.Clear();

            var loaded = await SaveSystem.LoadAsync<SummonBuffsSaveData>(path);
            if (loaded?.summonBuffs != null)
            {
                summonBuffs = loaded.summonBuffs;
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
            summonBuffs.Clear();

            var loaded = SaveSystem.Load<SummonBuffsSaveData>(path);
            if (loaded?.summonBuffs != null)
            {
                summonBuffs = loaded.summonBuffs;
            }
        }
    }
}
