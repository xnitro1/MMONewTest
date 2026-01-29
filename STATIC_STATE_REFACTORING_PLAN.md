# 🔧 Static State Refactoring Plan

## Overview
**Status:** Requires Architecture Refactoring  
**Priority:** CRITICAL for multi-server deployments  
**Estimated Effort:** 20-40 hours

## Problem Statement

Multiple server classes use **static collections** that are shared across all server instances. This causes data corruption when running multiple map servers simultaneously.

---

## Affected Files

### 1. **MMOServerGuildHandlers.cs**
**Location:** `Assets\NightBlade\MMO\MapServer\Map\MMOServerGuildHandlers.cs:12-14`

**Current (Broken):**
```csharp
public static readonly ConcurrentDictionary<int, GuildData> Guilds = new ConcurrentDictionary<int, GuildData>();
public static readonly ConcurrentDictionary<long, GuildData> UpdatingGuildMembers = new ConcurrentDictionary<long, GuildData>();
public static readonly ConcurrentHashSet<string> GuildInvitations = new ConcurrentHashSet<string>();
```

**Impact:** Multiple map servers share the same guild data → Guild corruption, wrong server sees invitations

---

### 2. **DefaultServerUserHandlers.cs**
**Location:** `Assets\NightBlade\Core\Networking\Implements\DefaultServerUserHandlers.cs:10-18`

**Current (Broken):**
```csharp
public static readonly ConcurrentDictionary<long, IPlayerCharacterData> PlayerCharacters = new ConcurrentDictionary<long, IPlayerCharacterData>();
public static readonly ConcurrentDictionary<string, IPlayerCharacterData> PlayerCharactersById = new ConcurrentDictionary<string, IPlayerCharacterData>();
public static readonly ConcurrentDictionary<long, string> ConnectionIdsByCharacterId = new ConcurrentDictionary<long, string>();
public static readonly ConcurrentDictionary<string, long> ConnectionIdsByCharacterName = new ConcurrentDictionary<string, long>();
// ... more static collections
```

**Impact:** Multiple map servers share player data → Players appear on wrong servers, duplicate login issues

---

### 3. **MapNetworkManagerDataUpdater.cs**
**Location:** `Assets\NightBlade\MMO\MapServer\Map\DataUpdater\MapNetworkManagerDataUpdater.cs:11-12`

**Current (Broken):**
```csharp
internal static readonly HashSet<BuildingDataUpdater> BuildingDataUpdaters = new HashSet<BuildingDataUpdater>();
internal static readonly HashSet<PlayerCharacterDataUpdater> PlayerCharacterDataUpdaters = new HashSet<PlayerCharacterDataUpdater>();
```

**Impact:** Updaters from different servers mix → Save conflicts, data loss

---

## Refactoring Approach

### Option 1: Instance-Based Collections (Recommended)
**Convert static collections to instance fields** and pass server instance references.

#### Step 1: Convert Static to Instance Fields

**Before:**
```csharp
public partial class MMOServerGuildHandlers
{
    public static readonly ConcurrentDictionary<int, GuildData> Guilds = ...;
}
```

**After:**
```csharp
public partial class MMOServerGuildHandlers
{
    private readonly ConcurrentDictionary<int, GuildData> _guilds;
    private readonly MapNetworkManager _serverInstance;

    public MMOServerGuildHandlers(MapNetworkManager serverInstance)
    {
        _serverInstance = serverInstance;
        _guilds = new ConcurrentDictionary<int, GuildData>();
    }
    
    public ConcurrentDictionary<int, GuildData> Guilds => _guilds;
}
```

#### Step 2: Update All Callers
Find all references to `MMOServerGuildHandlers.Guilds` and update to use instance reference.

**Before:**
```csharp
if (MMOServerGuildHandlers.Guilds.TryGetValue(guildId, out GuildData guild))
{
    // ...
}
```

**After:**
```csharp
if (ServerGuildHandlers.Guilds.TryGetValue(guildId, out GuildData guild))
{
    // Already uses instance via ServerGuildHandlers property
}
```

#### Step 3: Initialize in Server Manager
```csharp
public partial class MapNetworkManager
{
    protected override void Awake()
    {
        base.Awake();
        // Initialize handlers with instance reference
        ServerGuildHandlers = new MMOServerGuildHandlers(this);
        ServerUserHandlers = new DefaultServerUserHandlers(this);
    }
}
```

---

### Option 2: Server Instance ID Prefix (Faster but less clean)
**Keep static collections but prefix keys with server instance ID.**

**Pros:** Minimal code changes  
**Cons:** Pollution of static state, memory leaks if not cleaned up

#### Implementation:
```csharp
public partial class MMOServerGuildHandlers
{
    private static string GetKey(string serverId, int guildId)
    {
        return $"{serverId}_{guildId}";
    }
    
    public static bool TryGetGuild(string serverId, int guildId, out GuildData guild)
    {
        return Guilds.TryGetValue(GetKey(serverId, guildId), out guild);
    }
}
```

**Not recommended** - Band-aid solution that leaves technical debt.

---

## Migration Steps

### Phase 1: Preparation (2-4 hours)
1. ✅ Identify all static collection usages with grep
2. ✅ Document all affected classes and methods
3. ✅ Create branch: `refactor/static-to-instance-collections`
4. ✅ Write unit tests for existing behavior (if possible)

### Phase 2: Refactor DefaultServerUserHandlers (8-12 hours)
1. Convert static fields to instance fields
2. Add constructor that takes server instance
3. Update all 200+ usages across codebase
4. Test with single server instance
5. Test with multiple server instances

### Phase 3: Refactor MMOServerGuildHandlers (4-8 hours)
1. Convert static fields to instance fields
2. Add constructor that takes server instance
3. Update all usages
4. Test guild operations across servers

### Phase 4: Refactor MapNetworkManagerDataUpdater (2-4 hours)
1. Convert static fields to instance fields
2. Update data updater registration
3. Test save/load operations

### Phase 5: Testing & Verification (4-8 hours)
1. Run all existing tests
2. Manual testing with 2+ map servers
3. Verify no data leakage between servers
4. Performance testing

### Phase 6: Documentation & Deployment (2-4 hours)
1. Update architecture documentation
2. Update setup guides for multi-server
3. Create migration guide for addon developers
4. Deploy to staging environment

---

## Testing Strategy

### Unit Tests
```csharp
[Test]
public void TestServerInstancesHaveSeparateGuilds()
{
    var server1 = new MapNetworkManager("server1");
    var server2 = new MapNetworkManager("server2");
    
    server1.ServerGuildHandlers.Guilds[1] = new GuildData { guildName = "Guild1" };
    server2.ServerGuildHandlers.Guilds[1] = new GuildData { guildName = "Guild2" };
    
    Assert.AreEqual("Guild1", server1.ServerGuildHandlers.Guilds[1].guildName);
    Assert.AreEqual("Guild2", server2.ServerGuildHandlers.Guilds[1].guildName);
}
```

### Integration Tests
1. Start 3 map servers
2. Create guild on server 1
3. Verify guild NOT visible on servers 2 and 3
4. Player joins guild on server 1
5. Player moves to server 2
6. Verify guild data loads correctly from database, not static state

---

## Risks & Mitigation

### Risk 1: Breaking Existing Addons
**Mitigation:** 
- Provide backward compatibility layer for 1-2 versions
- Document migration path for addon developers
- Deprecation warnings in logs

### Risk 2: Performance Impact
**Mitigation:**
- Benchmark before and after
- Instance fields should have same or better performance
- No additional allocations in hot paths

### Risk 3: Incomplete Refactoring
**Mitigation:**
- Use compiler to find all static references
- Grep for all usages: `rg "MMOServerGuildHandlers\."`, `rg "DefaultServerUserHandlers\."`
- Add runtime assertions to detect static access

---

## Alternative: Quick Fix for Single Server Deployments

If multi-server is not needed immediately, add **runtime check**:

```csharp
public partial class MapNetworkManager
{
    private static int _instanceCount = 0;
    
    protected override void Awake()
    {
        base.Awake();
        _instanceCount++;
        
        if (_instanceCount > 1)
        {
            Debug.LogError("CRITICAL: Multiple MapNetworkManager instances detected! Static collections will cause data corruption. See STATIC_STATE_REFACTORING_PLAN.md");
            // Optionally disable server or force quit
        }
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        _instanceCount--;
    }
}
```

This prevents silent corruption by making the issue obvious during testing.

---

## Conclusion

**Recommended Action:** 
- For **single-server deployments**: Add runtime check (quick fix)
- For **multi-server deployments**: Full refactoring required (Option 1)

**Timeline:**
- Quick fix: 1 hour
- Full refactoring: 20-40 hours

**This issue should be prioritized** if multiple map server instances are planned for production.

---

**Next Steps:**
1. Determine if multi-server is needed in production
2. If yes, schedule refactoring sprint
3. If no, apply quick fix and create backlog item for future
