using System.Collections.Generic;
using UnityEngine;

namespace LiteNetLib.Utils
{
    /// <summary>
    /// Thread-safe string interning system for network messages.
    /// Reduces memory usage and network bandwidth by reusing identical strings.
    /// </summary>
    public static class NetworkStringCache
    {
        private static readonly Dictionary<string, string> internedStrings = new Dictionary<string, string>();
        private static readonly object lockObject = new object();

        /// <summary>
        /// Interns a string, returning a cached reference if the string already exists.
        /// Thread-safe operation for multiplayer environments.
        /// </summary>
        /// <param name="value">The string to intern</param>
        /// <returns>The interned string reference</returns>
        public static string Intern(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            lock (lockObject)
            {
                if (internedStrings.TryGetValue(value, out string internedValue))
                {
                    return internedValue;
                }
                internedStrings[value] = value;
                return value;
            }
        }

        /// <summary>
        /// Pre-populates the cache with commonly used network strings.
        /// Call this during game initialization for optimal performance.
        /// </summary>
        public static void InitializeCommonStrings()
        {
            // Player-related strings
            Intern("Player");
            Intern("Character");
            Intern("Avatar");
            Intern("Hero");
            Intern("NPC");

            // System messages
            Intern("System");
            Intern("Server");
            Intern("Client");
            Intern("Connected");
            Intern("Disconnected");
            Intern("Error");
            Intern("Success");
            Intern("Loading");
            Intern("Ready");

            // Chat and communication
            Intern("Global");
            Intern("Local");
            Intern("Party");
            Intern("Guild");
            Intern("Whisper");
            Intern("Trade");

            // Game world
            Intern("World");
            Intern("Map");
            Intern("Zone");
            Intern("Area");
            Intern("Region");

            // Combat and stats
            Intern("Health");
            Intern("Mana");
            Intern("Damage");
            Intern("Attack");
            Intern("Defense");
            Intern("Level");
            Intern("Experience");

            // Items and equipment
            Intern("Item");
            Intern("Weapon");
            Intern("Armor");
            Intern("Potion");
            Intern("Gold");
            Intern("Inventory");

            Debug.Log($"[NetworkStringCache] Initialized with {internedStrings.Count} common strings");
        }

        /// <summary>
        /// Clears the cache. Use with caution as it may cause temporary memory spikes.
        /// </summary>
        public static void Clear()
        {
            lock (lockObject)
            {
                internedStrings.Clear();
                Debug.Log("[NetworkStringCache] Cache cleared");
            }
        }

        /// <summary>
        /// Gets the current cache size for debugging/monitoring.
        /// </summary>
        public static int GetCacheSize()
        {
            lock (lockObject)
            {
                return internedStrings.Count;
            }
        }
    }
}
