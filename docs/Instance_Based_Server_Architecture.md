# 🏗️ Instance-Based Server Architecture Guide

**Version:** 1.0  
**Last Updated:** 2026-01-28  
**Status:** Architecture Blueprint + Migration Guide

---

## 📋 Table of Contents

1. [Overview](#overview)
2. [The Problem: Static State Antipattern](#the-problem-static-state-antipattern)
3. [Why Instance-Based Architecture](#why-instance-based-architecture)
4. [Architecture Principles](#architecture-principles)
5. [Complete Implementation Guide](#complete-implementation-guide)
6. [Migration Steps](#migration-steps)
7. [Testing Strategy](#testing-strategy)
8. [Performance Considerations](#performance-considerations)
9. [Best Practices](#best-practices)
10. [FAQ](#faq)

---

## Overview

### What is Instance-Based Server Architecture?

**Instance-based architecture** is a design pattern where each server instance maintains its own **isolated state** rather than sharing static/global state across all instances.

### Why Does This Matter?

In a **multi-server MMO deployment**, multiple map servers run simultaneously:
- **Map Server 1**: Handles Forest Zone
- **Map Server 2**: Handles Desert Zone  
- **Map Server 3**: Handles City Zone

If these servers share **static collections** (guilds, players, buildings), they will **corrupt each other's data**.

### Current Status

**⚠️ Warning Active**: NightBlade currently has **runtime detection** that warns if multiple `MapNetworkManager` instances exist. This prevents silent data corruption during development.

**🎯 Migration Required**: For production multi-server deployments, convert static collections to instance-based architecture.

---

## The Problem: Static State Antipattern

### What Are Static Collections?

```csharp
// ❌ ANTIPATTERN: Static state shared across ALL server instances
public class MMOServerGuildHandlers
{
    public static readonly ConcurrentDictionary<int, GuildData> Guilds = 
        new ConcurrentDictionary<int, GuildData>();
    
    public static readonly ConcurrentDictionary<long, GuildData> UpdatingGuildMembers = 
        new ConcurrentDictionary<long, GuildData>();
}
```

### Why Is This A Problem?

#### Scenario: Two Map Servers Running

**Map Server A** (Forest Zone):
```csharp
MMOServerGuildHandlers.Guilds[1] = new GuildData { guildName = "Forest Guild" };
```

**Map Server B** (Desert Zone):
```csharp
MMOServerGuildHandlers.Guilds[1] = new GuildData { guildName = "Desert Guild" };
```

**Result:** 🔥 **Data corruption!** Both servers share the SAME static dictionary. The guild with ID 1 flips between "Forest Guild" and "Desert Guild" randomly.

### Real-World Impact

| Issue | Impact | Severity |
|-------|--------|----------|
| **Guild Data Corruption** | Guilds appear on wrong servers, invitations lost | 🔴 CRITICAL |
| **Player Data Leakage** | Players appear on multiple servers simultaneously | 🔴 CRITICAL |
| **Save Conflicts** | Building/character saves overwrite each other | 🔴 CRITICAL |
| **Memory Leaks** | Static state never cleaned up | 🟠 HIGH |
| **Race Conditions** | Concurrent access from multiple servers | 🟠 HIGH |

### Affected Systems

1. **`MMOServerGuildHandlers.cs`**
   - `Guilds` - All guild data
   - `UpdatingGuildMembers` - Guild member updates
   - `GuildInvitations` - Pending invitations

2. **`DefaultServerUserHandlers.cs`**
   - `PlayerCharacters` - All player character data
   - `PlayerCharactersById` - Character lookup by ID
   - `ConnectionIdsByCharacterId` - Connection mapping
   - `ConnectionIdsByCharacterName` - Name-based lookup
   - **~8 static collections total**

3. **`MapNetworkManagerDataUpdater.cs`**
   - `BuildingDataUpdaters` - Building save updaters
   - `PlayerCharacterDataUpdaters` - Player save updaters

---

## Why Instance-Based Architecture

### Benefits

#### 1. **Server Isolation** 🏝️
Each server manages only its own data. Forest server has no idea Desert server exists.

```csharp
// ✅ CORRECT: Each server has its own guild collection
MapServer forestServer = new MapServer("Forest");
MapServer desertServer = new MapServer("Desert");

forestServer.GuildHandlers.Guilds[1] = new GuildData { guildName = "Forest Guild" };
desertServer.GuildHandlers.Guilds[1] = new GuildData { guildName = "Desert Guild" };

// No conflict! Each server has separate collections
```

#### 2. **Scalability** 📈
- Add more servers without data conflicts
- Scale horizontally by adding map servers
- Each server is independent

#### 3. **Testing** 🧪
- Can spin up multiple servers in unit tests
- Integration tests with realistic multi-server scenarios
- No global state pollution between tests

#### 4. **Memory Management** 💾
- Each server cleans up its own data on shutdown
- No memory leaks from orphaned static state
- Clear ownership of resources

#### 5. **Thread Safety** 🔒
- No cross-server race conditions
- Each server manages its own synchronization
- Reduced locking contention

---

## Architecture Principles

### Core Principles

#### 1. **Dependency Injection**
Pass server instance references to components instead of using static access.

```csharp
// ❌ OLD: Static access
public class GuildService
{
    public void CreateGuild(GuildData guild)
    {
        MMOServerGuildHandlers.Guilds[guild.id] = guild;  // Static!
    }
}

// ✅ NEW: Dependency injection
public class GuildService
{
    private readonly MapNetworkManager _server;
    
    public GuildService(MapNetworkManager server)
    {
        _server = server;
    }
    
    public void CreateGuild(GuildData guild)
    {
        _server.GuildHandlers.Guilds[guild.id] = guild;  // Instance!
    }
}
```

#### 2. **Composition Over Inheritance**
Server owns handler components rather than inheriting static behavior.

```csharp
public class MapNetworkManager
{
    // Server OWNS these handlers (composition)
    public MMOServerGuildHandlers GuildHandlers { get; private set; }
    public DefaultServerUserHandlers UserHandlers { get; private set; }
    
    protected override void Awake()
    {
        base.Awake();
        
        // Inject self into handlers
        GuildHandlers = new MMOServerGuildHandlers(this);
        UserHandlers = new DefaultServerUserHandlers(this);
    }
}
```

#### 3. **Explicit Ownership**
Every collection has a clear owner (the server instance).

```csharp
public class MMOServerGuildHandlers
{
    private readonly MapNetworkManager _serverInstance;
    private readonly ConcurrentDictionary<int, GuildData> _guilds;  // Instance field, not static!
    
    public MMOServerGuildHandlers(MapNetworkManager serverInstance)
    {
        _serverInstance = serverInstance;
        _guilds = new ConcurrentDictionary<int, GuildData>();
    }
    
    public ConcurrentDictionary<int, GuildData> Guilds => _guilds;
}
```

#### 4. **Lifecycle Management**
Server manages creation and cleanup of all owned components.

```csharp
protected override void Clean()
{
    base.Clean();
    
    // Server cleans up its own handlers
    GuildHandlers?.Cleanup();
    UserHandlers?.Cleanup();
}
```

---

## Complete Implementation Guide

### Step 1: Convert Static Handler to Instance-Based

#### Before: `MMOServerGuildHandlers.cs`

```csharp
namespace NightBlade.MMO
{
    public partial class MMOServerGuildHandlers
    {
        // ❌ STATIC: Shared across all server instances
        public static readonly ConcurrentDictionary<int, GuildData> Guilds = 
            new ConcurrentDictionary<int, GuildData>();
        
        public static readonly ConcurrentDictionary<long, GuildData> UpdatingGuildMembers = 
            new ConcurrentDictionary<long, GuildData>();
        
        public static readonly ConcurrentHashSet<string> GuildInvitations = 
            new ConcurrentHashSet<string>();
        
        public static bool TryGetGuild(int guildId, out GuildData guild)
        {
            return Guilds.TryGetValue(guildId, out guild);
        }
        
        public static void AddGuild(GuildData guild)
        {
            Guilds[guild.id] = guild;
        }
    }
}
```

#### After: Instance-Based Implementation

```csharp
using ConcurrentCollections;
using System.Collections.Concurrent;

namespace NightBlade.MMO
{
    /// <summary>
    /// Instance-based guild handler for map servers.
    /// Each MapNetworkManager instance has its own guild collection.
    /// </summary>
    public partial class MMOServerGuildHandlers
    {
        #region Server Instance Reference
        
        private readonly MapNetworkManager _serverInstance;
        
        /// <summary>
        /// Constructor with server instance injection.
        /// </summary>
        /// <param name="serverInstance">The owning map server instance</param>
        public MMOServerGuildHandlers(MapNetworkManager serverInstance)
        {
            _serverInstance = serverInstance ?? throw new System.ArgumentNullException(nameof(serverInstance));
            
            // Initialize instance collections
            _guilds = new ConcurrentDictionary<int, GuildData>();
            _updatingGuildMembers = new ConcurrentDictionary<long, GuildData>();
            _guildInvitations = new ConcurrentHashSet<string>();
        }
        
        #endregion
        
        #region Instance Collections (formerly static)
        
        // ✅ INSTANCE: Each server has its own collections
        private readonly ConcurrentDictionary<int, GuildData> _guilds;
        private readonly ConcurrentDictionary<long, GuildData> _updatingGuildMembers;
        private readonly ConcurrentHashSet<string> _guildInvitations;
        
        /// <summary>
        /// Guild collection for this server instance.
        /// </summary>
        public ConcurrentDictionary<int, GuildData> Guilds => _guilds;
        
        /// <summary>
        /// Guild members currently being updated on this server.
        /// </summary>
        public ConcurrentDictionary<long, GuildData> UpdatingGuildMembers => _updatingGuildMembers;
        
        /// <summary>
        /// Pending guild invitations on this server.
        /// </summary>
        public ConcurrentHashSet<string> GuildInvitations => _guildInvitations;
        
        #endregion
        
        #region Instance Methods (formerly static)
        
        /// <summary>
        /// Try to get guild data for this server instance.
        /// </summary>
        public bool TryGetGuild(int guildId, out GuildData guild)
        {
            return _guilds.TryGetValue(guildId, out guild);
        }
        
        /// <summary>
        /// Add or update guild on this server instance.
        /// </summary>
        public void AddGuild(GuildData guild)
        {
            if (guild == null)
                throw new System.ArgumentNullException(nameof(guild));
            
            _guilds[guild.id] = guild;
            
            Logging.Log($"[Guild] Added/Updated guild {guild.id} ({guild.guildName}) on server {_serverInstance.RefId}");
        }
        
        /// <summary>
        /// Remove guild from this server instance.
        /// </summary>
        public bool RemoveGuild(int guildId)
        {
            bool removed = _guilds.TryRemove(guildId, out GuildData guild);
            
            if (removed)
            {
                Logging.Log($"[Guild] Removed guild {guildId} from server {_serverInstance.RefId}");
            }
            
            return removed;
        }
        
        #endregion
        
        #region Lifecycle Management
        
        /// <summary>
        /// Clean up all guild data for this server instance.
        /// Called when server shuts down.
        /// </summary>
        public void Cleanup()
        {
            Logging.Log($"[Guild] Cleaning up {_guilds.Count} guilds on server {_serverInstance.RefId}");
            
            _guilds.Clear();
            _updatingGuildMembers.Clear();
            _guildInvitations.Clear();
        }
        
        #endregion
        
        #region Server Instance Access
        
        /// <summary>
        /// Access the owning server instance (for advanced operations).
        /// </summary>
        public MapNetworkManager Server => _serverInstance;
        
        #endregion
    }
}
```

### Step 2: Update MapNetworkManager

```csharp
public partial class MapNetworkManager : BaseGameNetworkManager, IAppServer
{
    #region Handler Instances
    
    /// <summary>
    /// Guild management handler for this map server.
    /// Each server has its own guild collection.
    /// </summary>
    public MMOServerGuildHandlers GuildHandlers { get; private set; }
    
    /// <summary>
    /// User/player management handler for this map server.
    /// Each server has its own player collection.
    /// </summary>
    public DefaultServerUserHandlers UserHandlers { get; private set; }
    
    /// <summary>
    /// Data update handler for this map server.
    /// Manages save operations for this server's entities.
    /// </summary>
    public MapNetworkManagerDataUpdater DataUpdater { get; private set; }
    
    #endregion
    
    protected override void Awake()
    {
        base.Awake();
        
        // Initialize instance-based handlers with dependency injection
        GuildHandlers = new MMOServerGuildHandlers(this);
        UserHandlers = new DefaultServerUserHandlers(this);
        DataUpdater = new MapNetworkManagerDataUpdater(this);
        
        Logging.Log($"✓ MapNetworkManager handlers initialized for server: {RefId}");
    }
    
    protected override void Clean()
    {
        base.Clean();
        
        // Clean up handlers
        GuildHandlers?.Cleanup();
        UserHandlers?.Cleanup();
        DataUpdater?.Cleanup();
        
        Logging.Log($"✓ MapNetworkManager cleaned up for server: {RefId}");
    }
}
```

### Step 3: Update All Call Sites

#### Before: Static Access

```csharp
public void CreateGuild(CreateGuildReq request)
{
    GuildData guild = new GuildData
    {
        id = request.guildId,
        guildName = request.guildName
    };
    
    // ❌ OLD: Static access
    MMOServerGuildHandlers.Guilds[guild.id] = guild;
}
```

#### After: Instance Access

```csharp
public void CreateGuild(CreateGuildReq request)
{
    GuildData guild = new GuildData
    {
        id = request.guildId,
        guildName = request.guildName
    };
    
    // ✅ NEW: Instance access via server property
    // "this" is MapNetworkManager or a component with ServerGuildHandlers property
    ServerGuildHandlers.AddGuild(guild);
}
```

### Step 4: Provide Backward Compatibility (Optional)

For a gradual migration, provide static shim:

```csharp
public partial class MMOServerGuildHandlers
{
    // Temporary: Static shim for backward compatibility
    // TODO: Remove after all call sites updated
    [System.Obsolete("Use instance-based access via MapNetworkManager.GuildHandlers instead. This will be removed in v5.0.0")]
    public static ConcurrentDictionary<int, GuildData> Guilds
    {
        get
        {
            // Find active server instance (for single-server deployments only!)
            var server = UnityEngine.Object.FindObjectOfType<MapNetworkManager>();
            if (server == null)
            {
                Logging.LogError("[Guild] No MapNetworkManager found! Static access is deprecated.");
                return new ConcurrentDictionary<int, GuildData>();
            }
            
            if (Application.isEditor)
            {
                Logging.LogWarning("[Guild] Using deprecated static Guilds property. Update to instance-based access.");
            }
            
            return server.GuildHandlers.Guilds;
        }
    }
}
```

---

## Migration Steps

### Phase 1: Preparation (2-4 hours)

#### 1.1 Audit Static Usage

```bash
# Find all static handler usages
rg "MMOServerGuildHandlers\." --type cs > guild_static_usage.txt
rg "DefaultServerUserHandlers\." --type cs > user_static_usage.txt
rg "MapNetworkManagerDataUpdater\." --type cs > updater_static_usage.txt
```

#### 1.2 Create Migration Branch

```bash
git checkout -b refactor/instance-based-server-architecture
```

#### 1.3 Write Tests

```csharp
[Test]
public void TestMultipleServersHaveIsolatedGuilds()
{
    // Create two map servers
    GameObject server1Obj = new GameObject("MapServer1");
    GameObject server2Obj = new GameObject("MapServer2");
    
    MapNetworkManager server1 = server1Obj.AddComponent<MapNetworkManager>();
    MapNetworkManager server2 = server2Obj.AddComponent<MapNetworkManager>();
    
    // Create guilds with same ID on different servers
    GuildData guild1 = new GuildData { id = 1, guildName = "Server1 Guild" };
    GuildData guild2 = new GuildData { id = 1, guildName = "Server2 Guild" };
    
    server1.GuildHandlers.AddGuild(guild1);
    server2.GuildHandlers.AddGuild(guild2);
    
    // Verify isolation
    Assert.IsTrue(server1.GuildHandlers.TryGetGuild(1, out GuildData result1));
    Assert.AreEqual("Server1 Guild", result1.guildName);
    
    Assert.IsTrue(server2.GuildHandlers.TryGetGuild(1, out GuildData result2));
    Assert.AreEqual("Server2 Guild", result2.guildName);
    
    // Cleanup
    Object.DestroyImmediate(server1Obj);
    Object.DestroyImmediate(server2Obj);
}
```

### Phase 2: Refactor MMOServerGuildHandlers (4-8 hours)

#### 2.1 Convert Static to Instance
- Remove `static` keyword from all collections
- Make collections private with public properties
- Add constructor with server instance parameter

#### 2.2 Update All Usages
Using the audit from 1.1, update ~50-100 call sites:

```bash
# Example replacements
# Before: MMOServerGuildHandlers.Guilds[guildId]
# After: ServerGuildHandlers.Guilds[guildId]
```

#### 2.3 Add Cleanup Method
```csharp
public void Cleanup()
{
    _guilds.Clear();
    _updatingGuildMembers.Clear();
    _guildInvitations.Clear();
}
```

#### 2.4 Test Guild Operations
- Create guild on server 1
- Verify NOT visible on server 2
- Test guild invitations isolation
- Test guild member updates

### Phase 3: Refactor DefaultServerUserHandlers (8-12 hours)

#### 3.1 Convert 8+ Static Collections
- `PlayerCharacters`
- `PlayerCharactersById`
- `ConnectionIdsByCharacterId`
- `ConnectionIdsByCharacterName`
- Plus 4 more...

#### 3.2 Update 200+ Call Sites
This is the biggest part. Use IDE refactoring tools:

1. Add instance property to MapNetworkManager
2. Use "Find All References" on static properties
3. Replace with instance access one by one
4. Compile and fix errors iteratively

#### 3.3 Test Player Operations
- Player login on server 1
- Verify player NOT on server 2
- Test connection mapping isolation
- Test player data lookup

### Phase 4: Refactor MapNetworkManagerDataUpdater (2-4 hours)

#### 4.1 Convert Static Updater Collections
```csharp
// Before: Static
internal static readonly HashSet<BuildingDataUpdater> BuildingDataUpdaters = new HashSet<BuildingDataUpdater>();

// After: Instance
private readonly HashSet<BuildingDataUpdater> _buildingDataUpdaters = new HashSet<BuildingDataUpdater>();
public HashSet<BuildingDataUpdater> BuildingDataUpdaters => _buildingDataUpdaters;
```

#### 4.2 Update Registration
```csharp
public void RegisterUpdater(BuildingDataUpdater updater)
{
    _buildingDataUpdaters.Add(updater);
    Logging.Log($"[DataUpdater] Registered building updater on server {_server.RefId}");
}
```

### Phase 5: Integration Testing (4-8 hours)

#### 5.1 Multi-Server Testing
```csharp
[IntegrationTest]
public async Task TestThreeMapServersIsolation()
{
    // Start 3 map servers
    var forest = await StartMapServer("Forest");
    var desert = await StartMapServer("Desert");
    var city = await StartMapServer("City");
    
    // Create guilds on each
    forest.GuildHandlers.AddGuild(new GuildData { id = 1, guildName = "Forest Guild" });
    desert.GuildHandlers.AddGuild(new GuildData { id = 1, guildName = "Desert Guild" });
    city.GuildHandlers.AddGuild(new GuildData { id = 1, guildName = "City Guild" });
    
    // Verify isolation
    Assert.AreEqual("Forest Guild", forest.GuildHandlers.Guilds[1].guildName);
    Assert.AreEqual("Desert Guild", desert.GuildHandlers.Guilds[1].guildName);
    Assert.AreEqual("City Guild", city.GuildHandlers.Guilds[1].guildName);
    
    // Cleanup
    await StopAllServers();
}
```

#### 5.2 Player Migration Testing
Test players moving between servers:
1. Player joins server 1
2. Player saved to database
3. Player disconnects from server 1
4. Player connects to server 2
5. Player loads from database (not from server 1's memory)

### Phase 6: Performance Testing (2-4 hours)

#### 6.1 Benchmark Before/After
```csharp
[PerformanceTest]
public void BenchmarkGuildLookup()
{
    Measure.Method(() =>
    {
        server.GuildHandlers.TryGetGuild(1, out var guild);
    })
    .WarmupCount(100)
    .MeasurementCount(1000)
    .Run();
}
```

#### 6.2 Memory Profiling
- Check for memory leaks
- Verify cleanup on server shutdown
- Monitor GC allocations

### Phase 7: Documentation & Deployment (2-4 hours)

#### 7.1 Update Docs
- Architecture diagrams
- API documentation
- Migration guide for addon developers

#### 7.2 Deprecation Warnings
Add warnings for old static access:
```csharp
#if UNITY_EDITOR
[System.Obsolete("Static access deprecated. Use instance via MapNetworkManager.GuildHandlers")]
#endif
public static ConcurrentDictionary<int, GuildData> Guilds { get; }
```

#### 7.3 Release Notes
Document breaking changes and migration path.

---

## Testing Strategy

### Unit Tests

```csharp
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public class InstanceBasedArchitectureTests
{
    [Test]
    public void TestServerInstancesHaveIsolatedCollections()
    {
        // Arrange
        var server1 = CreateMapServer("Server1");
        var server2 = CreateMapServer("Server2");
        
        // Act
        server1.GuildHandlers.AddGuild(new GuildData { id = 1, guildName = "Guild1" });
        server2.GuildHandlers.AddGuild(new GuildData { id = 1, guildName = "Guild2" });
        
        // Assert
        Assert.AreEqual("Guild1", server1.GuildHandlers.Guilds[1].guildName);
        Assert.AreEqual("Guild2", server2.GuildHandlers.Guilds[1].guildName);
        
        // Different instances, different data!
        Assert.AreNotSame(server1.GuildHandlers.Guilds, server2.GuildHandlers.Guilds);
    }
    
    [Test]
    public void TestServerCleanupClearsOnlyOwnData()
    {
        // Arrange
        var server1 = CreateMapServer("Server1");
        var server2 = CreateMapServer("Server2");
        
        server1.GuildHandlers.AddGuild(new GuildData { id = 1, guildName = "Guild1" });
        server2.GuildHandlers.AddGuild(new GuildData { id = 2, guildName = "Guild2" });
        
        // Act
        server1.Clean();
        
        // Assert
        Assert.AreEqual(0, server1.GuildHandlers.Guilds.Count);  // Server 1 cleaned
        Assert.AreEqual(1, server2.GuildHandlers.Guilds.Count);  // Server 2 unaffected!
    }
    
    private MapNetworkManager CreateMapServer(string name)
    {
        GameObject go = new GameObject(name);
        return go.AddComponent<MapNetworkManager>();
    }
}
```

### Integration Tests

```csharp
[IntegrationTest]
public class MultiServerIntegrationTests
{
    [Test]
    public async Task TestPlayerMovementBetweenServers()
    {
        // Start two map servers
        var forestServer = await StartMapServer("Forest", "forest_scene");
        var cityServer = await StartMapServer("City", "city_scene");
        
        // Player joins forest server
        var player = CreateTestPlayer("TestPlayer");
        await forestServer.PlayerJoin(player);
        
        Assert.IsTrue(forestServer.UserHandlers.PlayerCharacters.ContainsKey(player.Id));
        Assert.IsFalse(cityServer.UserHandlers.PlayerCharacters.ContainsKey(player.Id));
        
        // Player saves and disconnects
        await forestServer.SavePlayer(player);
        await forestServer.PlayerDisconnect(player);
        
        // Player joins city server
        await cityServer.PlayerJoin(player);
        
        Assert.IsFalse(forestServer.UserHandlers.PlayerCharacters.ContainsKey(player.Id));
        Assert.IsTrue(cityServer.UserHandlers.PlayerCharacters.ContainsKey(player.Id));
        
        // Cleanup
        await StopAllServers();
    }
}
```

### Load Tests

```csharp
[PerformanceTest]
public class LoadTests
{
    [Test]
    public async Task TestTenThousandPlayersAcrossMultipleServers()
    {
        var servers = new List<MapNetworkManager>();
        
        // Start 10 map servers
        for (int i = 0; i < 10; i++)
        {
            servers.Add(await StartMapServer($"Server{i}"));
        }
        
        // Add 1000 players to each server
        for (int serverId = 0; serverId < 10; serverId++)
        {
            for (int playerId = 0; playerId < 1000; playerId++)
            {
                var player = CreateTestPlayer($"Player{serverId}_{playerId}");
                await servers[serverId].PlayerJoin(player);
            }
        }
        
        // Verify each server has exactly 1000 players
        foreach (var server in servers)
        {
            Assert.AreEqual(1000, server.UserHandlers.PlayerCharacters.Count);
        }
        
        // Verify total across all servers
        int totalPlayers = servers.Sum(s => s.UserHandlers.PlayerCharacters.Count);
        Assert.AreEqual(10000, totalPlayers);
    }
}
```

---

## Performance Considerations

### Memory Impact

| Metric | Static | Instance-Based | Change |
|--------|--------|----------------|---------|
| Per-Server Collections | 1 (shared) | N (one per server) | +N overhead |
| Memory per Server | ~100KB | ~100KB | Same per instance |
| Total Memory (3 servers) | ~100KB | ~300KB | +200KB |
| Memory Leaks | High risk | Low risk | ✅ Safer |

**Verdict**: Minimal memory increase (200KB for 3 servers) is negligible compared to safety benefits.

### Performance Impact

| Operation | Static | Instance-Based | Change |
|-----------|--------|----------------|---------|
| Guild Lookup | O(1) | O(1) | Same |
| Player Lookup | O(1) | O(1) | Same |
| Collection Access | Direct | One indirection | +1 pointer dereference |
| Lock Contention | High (shared) | Low (isolated) | ✅ Faster |

**Verdict**: Performance is **equivalent or better** due to reduced locking contention.

### Benchmarks

```csharp
// Static access: ~5ns per lookup (but thread contention!)
MMOServerGuildHandlers.Guilds.TryGetValue(guildId, out guild);

// Instance access: ~6ns per lookup (no contention!)
server.GuildHandlers.Guilds.TryGetValue(guildId, out guild);
```

**1ns difference is negligible.** Thread safety benefits far outweigh the tiny overhead.

---

## Best Practices

### 1. Always Inject Server Instance

```csharp
// ✅ GOOD: Constructor injection
public class CustomHandler
{
    private readonly MapNetworkManager _server;
    
    public CustomHandler(MapNetworkManager server)
    {
        _server = server;
    }
}

// ❌ BAD: Static access or FindObjectOfType
public class CustomHandler
{
    public void DoSomething()
    {
        var server = FindObjectOfType<MapNetworkManager>();  // Slow + fragile!
    }
}
```

### 2. Use Properties for Collections

```csharp
// ✅ GOOD: Property access (encapsulation)
public ConcurrentDictionary<int, GuildData> Guilds => _guilds;

// ❌ BAD: Public field (no encapsulation)
public ConcurrentDictionary<int, GuildData> Guilds;
```

### 3. Always Cleanup in Clean()

```csharp
public void Cleanup()
{
    _guilds.Clear();
    _updatingGuildMembers.Clear();
    _guildInvitations.Clear();
    
    Logging.Log($"[Guild] Cleaned up {_guilds.Count} guilds on server {_server.RefId}");
}
```

### 4. Add Logging for Debugging

```csharp
public void AddGuild(GuildData guild)
{
    _guilds[guild.id] = guild;
    
    Logging.Log($"[Guild] Added guild {guild.id} to server {_server.RefId}");
}
```

### 5. Validate Server Instance

```csharp
public CustomHandler(MapNetworkManager server)
{
    _server = server ?? throw new ArgumentNullException(nameof(server));
}
```

---

## FAQ

### Q: Will this break my existing code?

**A:** Yes, if you access handlers statically. Migration required.

**Solution:** Use search-and-replace to update call sites:
- `MMOServerGuildHandlers.Guilds` → `ServerGuildHandlers.Guilds`
- `DefaultServerUserHandlers.PlayerCharacters` → `ServerUserHandlers.PlayerCharacters`

### Q: Can I keep static for single-server deployments?

**A:** Technically yes, but **not recommended**. Benefits of instance-based:
- Easier testing (multiple instances in tests)
- Future-proof for scaling
- Clearer ownership and lifecycle
- No global state pollution

### Q: What about performance?

**A:** Instance-based is **equal or better** performance:
- Same O(1) lookup time
- Less lock contention (isolated collections)
- Negligible memory overhead (~100KB per server)

### Q: How long will migration take?

**A:** Depends on codebase size:
- **Small (1-2 servers)**: 8-16 hours
- **Medium (3-5 servers)**: 20-40 hours  
- **Large (complex handlers)**: 40-80 hours

### Q: Can I migrate incrementally?

**A:** Yes! Use backward compatibility shim:
1. Add instance-based architecture
2. Keep static properties as shims (with deprecation warnings)
3. Update call sites gradually
4. Remove static shims in next major version

### Q: What if I have addons using static access?

**A:** Provide migration guide:
1. Document breaking changes
2. Provide code examples
3. Keep backward compat for 1-2 versions
4. Add deprecation warnings

### Q: Is there a tool to automate migration?

**A:** Not yet, but regex helps:
```bash
# Find all static access
rg "MMOServerGuildHandlers\.[A-Z]" --type cs

# Replace with instance access (manual verification needed)
sed -i 's/MMOServerGuildHandlers\./ServerGuildHandlers\./g' *.cs
```

---

## Conclusion

**Instance-based server architecture** is essential for:
- ✅ Multi-server deployments
- ✅ Horizontal scaling
- ✅ Data integrity
- ✅ Memory safety
- ✅ Testing

**Migration is required** for production multi-server MMO.

**Quick fix applied:** Runtime warning prevents silent corruption during development.

**Full migration:** See `STATIC_STATE_REFACTORING_PLAN.md` for project-specific implementation steps.

---

**Next Steps:**
1. Review this architecture guide
2. Decide on migration timeline
3. Create migration tickets
4. Start with MMOServerGuildHandlers (easiest)
5. Test thoroughly with multiple servers
6. Deploy incrementally

**Questions?** See [Troubleshooting Guide](./troubleshooting.md) or create a GitHub issue.
