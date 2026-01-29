# Troubleshooting Guide

This guide helps you resolve common issues with NightBlade. NightBlade includes enhanced diagnostics and error reporting compared to conventional MMO frameworks, making it easier to identify and fix problems.

## 🔍 Diagnostic Tools

Before troubleshooting, enable NightBlade's diagnostic features:

```csharp
// Enable comprehensive logging
Debug.LogLevel = LogLevel.Verbose;
DataValidation.EnableDetailedLogging = true;
RuntimeValidation.EnablePerformanceMonitoring = true;

// Run diagnostics
NightBladeDiagnostics.RunFullDiagnostic();

// Check system status
Debug.Log($"Validation Status: {DataValidation.GetStatus()}");
Debug.Log($"Performance Status: {PerformanceProfiler.GetStatus()}");
```

## 🚫 Common Issues & Solutions

### Import/Setup Issues

#### Package Import Fails

**Symptoms:**
- Import dialog shows errors
- Missing scripts or components
- Console shows "Failed to import package"

**Solutions:**

1. **Check Unity Version**
   ```bash
   # Required: Unity 6.3 LTS or later
   ```

2. **Restart Unity**
   - Close Unity completely
   - Delete Library folder if needed
   - Reopen and retry import

3. **Check Dependencies**
   ```csharp
   // In Unity: Window → Package Manager
   // Ensure these packages are installed:
   // - TextMeshPro
   // - Unity Input System
   // - Addressables
   ```

4. **Manual Package Installation**
   ```bash
   # If automatic import fails:
   1. Extract NightBlade package manually
   2. Copy Assets/NightBlade_1.95+ to your project
   3. Run Tools → NightBlade → Setup → Fix References
   ```

#### Missing Components After Import

**Symptoms:**
- GameObjects show "Missing Script" warnings
- Components not attached to prefabs

**Solution:**
```csharp
// Run the setup tool
using UnityEditor;
using NightBlade.Editor;

public class FixMissingScripts {
    [MenuItem("NightBlade/Setup/Fix Missing Scripts")]
    static void FixScripts() {
        MissingScriptFixer.FixAll();
    }
}
```

### 🔒 Security & Validation Issues

#### Validation Errors in Console

**Symptoms:**
- Console shows validation warnings/errors
- Character creation fails
- Network messages rejected

**NightBlade Enhancement:** Unlike conventional MMO frameworks, NightBlade provides detailed validation feedback.

**Solutions:**

1. **Check Validation Level**
   ```csharp
   // In development, reduce strictness
   DataValidation.CurrentLevel = ValidationLevel.Basic;

   // In production, ensure strict validation
   DataValidation.CurrentLevel = ValidationLevel.Strict;
   ```

2. **Review Validation Logs**
   ```csharp
   // Get detailed validation report
   var report = DataValidation.GetValidationReport();
   Debug.Log($"Validation Issues: {report.TotalIssues}");

   foreach (var issue in report.GetIssues()) {
     Debug.LogWarning($"Validation: {issue.Type} - {issue.Message}");
   }
   ```

3. **Common Validation Fixes**
   ```csharp
   // Fix character stats
   if (!DataValidation.IsValidCharacterStats(character)) {
       character.Stats.Strength = Mathf.Clamp(character.Stats.Strength, 1, 100);
       character.Stats.Dexterity = Mathf.Clamp(character.Stats.Dexterity, 1, 100);
   }

   // Fix item data
   if (!DataValidation.IsValidItemData(item)) {
       item.Name = DataValidation.SanitizeItemName(item.Name);
       item.MaxStackSize = Mathf.Clamp(item.MaxStackSize, 1, 999);
   }
   ```

#### Security Warnings

**Symptoms:**
- "Exploit attempt blocked" messages
- Players unable to perform actions

**Solution:**
```csharp
// Check what's being blocked
var securityLog = SecurityMonitor.GetRecentEvents();
foreach (var event in securityLog) {
    Debug.Log($"Security: {event.Type} at {event.Timestamp}: {event.Details}");
}

// Adjust security settings if needed
SecurityConfig config = SecurityConfig.Instance;
config.MaxRequestsPerMinute = 120; // Increase if too restrictive
config.BlockSuspiciousInputs = false; // Disable for testing
```

### ⚡ Performance Issues

#### Low Frame Rate

**Symptoms:**
- Game runs below 30 FPS
- Stuttering during combat
- High CPU usage

**NightBlade Solutions:**

1. **Check Performance Metrics**
   ```csharp
   // Run performance diagnostic
   var metrics = PerformanceProfiler.GetMetrics();

   Debug.Log($"CPU Usage: {metrics.CpuUsage}%");
   Debug.Log($"Memory: {metrics.MemoryUsage}MB");
   Debug.Log($"Save Frequency: {metrics.SaveOperationsPerMinute}/min");
   Debug.Log($"Physics Rays: {metrics.RaycastsPerSecond}/sec");
   ```

2. **Auto-Save Optimization**
   ```csharp
   // Increase save interval (default 30s)
   LanRpgNetworkManager.saveInterval = 60f;

   // Enable smart saving
   LanRpgNetworkManager.smartSaveOnly = true;
   ```

3. **Physics Optimization**
   ```csharp
   // Reduce raycast frequency (default 10fps)
   CharacterAlignOnGround.updateInterval = 0.2f; // 5 updates/second

   // Disable interpolation if needed
   CharacterAlignOnGround.useInterpolation = false;
   ```

4. **Validation Performance**
   ```csharp
   // Enable fast validation
   DataValidation.UseFastStringValidation = true;

   // Reduce validation frequency
   RuntimeValidation.ValidationInterval = 0.5f; // Every 500ms
   ```

#### Memory Leaks

**Symptoms:**
- Memory usage continuously increases
- Occasional garbage collection pauses

**Solutions:**
```csharp
// Enable object pooling
ObjectPool.EnableAllPools = true;

// Monitor allocations
MemoryProfiler.StartTracking();

void Update() {
    if (Time.frameCount % 300 == 0) { // Every 10 seconds
        var stats = MemoryProfiler.GetStats();
        Debug.Log($"GC Allocations: {stats.GcAllocations}KB this frame");
    }
}

// Force cleanup if needed
Resources.UnloadUnusedAssets();
GC.Collect();
```

### 🎨 UI Pooling Issues (v2.x)

#### Template Null During Pre-warming

**Symptoms:**
- Warning: `"Cannot pre-warm pool 'DamageNumbers' - template is null"`
- Info: `"Pre-warmed 20 damage number objects. Total pooled: 0"`
- Damage numbers don't appear in combat

**Root Cause:**
- Template GameObjects were being destroyed during scene changes
- Templates weren't parented to `DontDestroyOnLoad` objects

**Solution (Fixed in v2.x):**
- Templates are now automatically parented to `UIPoolManager`
- Pool state is cleared between server instances
- Each map server maintains isolated pool state

**Manual Verification:**
```csharp
// Check pool status
var stats = UIPoolManager.Instance.GetPoolStats();
Debug.Log($"Damage Numbers Pooled: {stats.ContainsKey("DamageNumbers") ? stats["DamageNumbers"] : 0}");

// Should show: "Damage Numbers Pooled: 20"
```

#### UI Pool Not Initializing

**Symptoms:**
- No damage numbers appear
- PerformanceMonitor shows: `🎨 UI Pooled: 0 objects`
- Warning logs about pooling failures

**Solutions:**

1. **Check TMP Resources**
   ```csharp
   // Verify TextMesh Pro is imported
   Window > TextMesh Pro > Import TMP Essential Resources
   ```

2. **Force Reinitialization**
   ```csharp
   // Manually restart UI pooling
   GameInstance.Singleton.ReinitializeUIPools();
   ```

3. **Check Pool Manager**
   ```csharp
   // Verify pool manager exists
   if (UIPoolManager.Instance == null) {
       Debug.LogError("UIPoolManager not initialized!");
   }
   ```

### 🌐 Networking Issues

#### Connection Problems

**Symptoms:**
- "Failed to connect" errors
- Timeouts during gameplay
- Packet loss

**NightBlade Enhanced Networking:**

1. **Check Network Diagnostics**
   ```csharp
   var networkStats = NetworkDiagnostics.GetStats();
   Debug.Log($"Ping: {networkStats.AveragePing}ms");
   Debug.Log($"Packet Loss: {networkStats.PacketLoss}%");
   Debug.Log($"Messages/Sec: {networkStats.MessagesPerSecond}");
   ```

2. **Connection Optimization**
   ```csharp
   // Adjust network settings
   NetworkManager.MaxConnections = 100;
   NetworkManager.SendRate = 20; // 20 updates/second
   NetworkManager.CompressionEnabled = true;
   ```

3. **Firewall/AV Issues**
   - Add Unity and game to firewall exceptions
   - Disable real-time antivirus scanning during development
   - Check for VPN interference

#### Synchronization Problems

**Symptoms:**
- Character positions desync
- Combat results inconsistent
- Inventory not updating properly

**Solutions:**
```csharp
// Enable enhanced sync
NetworkSyncManager.EnableInterpolation = true;
NetworkSyncManager.ExtrapolationEnabled = true;
NetworkSyncManager.SyncRate = 10; // 10 syncs/second

// Debug sync issues
NetworkSyncManager.EnableDebugLogging = true;
```

### 🖥️ Server & Cluster Issues (v2.x)

#### Map Server Registration Problems

**Symptoms:**
- Map servers show wrong names in cluster registration
- Log shows: `"Register map server: 1_Map_GuildWar"` (when should be Map001)
- Multiple servers registering with same/incorrect identifiers

**Root Cause (Fixed in v2.x):**
- Map servers were using shared `CurrentMapInfo` instead of instance-specific `MapInfo`
- Led to cross-contamination between different server instances

**Solution:**
- MapNetworkManager now uses `MapInfo.Id` (instance-specific) instead of `CurrentMapInfo.Id`
- Each server instance maintains isolated map information
- Registration uses correct map identifiers

**Verification:**
```csharp
// Check server registration in logs
// Should show correct map names:
// Map001-Ch1: Register map server: 1_Map001
// Map002-Ch1: Register map server: 1_Map002
// GuildWar: Register map server: 1_Map_GuildWar (when scene is available)
```

#### Server Instance Isolation

**Symptoms:**
- Settings from one server affect others
- Pool states shared between different map instances
- Memory leaks from uncleared previous instances

**NightBlade Solutions:**
```csharp
// Each server instance is now properly isolated
MapNetworkManager mapManager = GetComponent<MapNetworkManager>();
mapManager.MapInfo = mapInfo; // Instance-specific

// UI pools are cleared between instances
UIPoolManager.Instance.ClearAllPools(); // Clean slate for each server
```

### 🎮 Gameplay Issues

#### Character Creation Fails

**Symptoms:**
- Character creation dialog doesn't work
- "Invalid character data" errors

**NightBlade Validation:**
```csharp
// Check character validation
string validationError = DataValidation.ValidateCharacterCreation(
    name, classType, gender);

if (!string.IsNullOrEmpty(validationError)) {
    Debug.LogError($"Character creation failed: {validationError}");
    ShowErrorToUser(validationError);
    return;
}

// If validation passes, proceed
CreateCharacter(name, classType, gender);
```

#### Combat Not Working

**Symptoms:**
- Attacks don't deal damage
- Skills don't activate
- Combat animations not playing
- "Can't attack" despite being in range

**Recent Fixes (v1.95+):**
- **Change Detection Physics Sync**: Optimized physics synchronization while maintaining 100% attack accuracy
- **Attack System Compatibility**: Immediate physics sync for combat operations using ForceSyncPhysicsTransforms()
- **Movement System Integration**: Updated entity movement to use efficient dirty flag system

**Debug Steps:**
```csharp
// Enable combat logging
CombatSystem.EnableDebugLogging = true;

// Test basic combat
var attacker = GetPlayerCharacter();
var defender = GetTargetEnemy();

Debug.Log($"Can attack: {CombatValidation.CanAttack(attacker, defender)}");
Debug.Log($"Attacker position: {attacker.Position}");
Debug.Log($"Defender position: {defender.Position}");
Debug.Log($"Distance: {Vector3.Distance(attacker.Position, defender.Position)}");

// Check attack validation
Debug.Log($"Is attacking: {attacker.IsAttacking}");
Debug.Log($"Can do actions: {attacker.CanDoActions()}");
Debug.Log($"Weapons sheathed: {attacker.IsWeaponsSheathed}");

// Physics sync debugging (v1.95+ change detection system)
var networkManager = BaseGameNetworkManager.Singleton;
if (networkManager != null) {
    Debug.Log($"3D Transforms Dirty: {networkManager._transformsDirty3D}");
    Debug.Log($"2D Transforms Dirty: {networkManager._transformsDirty2D}");
    Debug.Log($"Last 3D Sync: {Time.time - networkManager._lastSyncTime3D}s ago");
    Debug.Log($"Last 2D Sync: {Time.time - networkManager._lastSyncTime2D}s ago");
}

// Manual damage test
if (CombatValidation.CanAttack(attacker, defender)) {
    float damage = DamageCalculator.CalculatePhysicalDamage(attacker, defender, 50f);
    Debug.Log($"Calculated damage: {damage}");
    defender.TakeDamage(damage, attacker);
}
```

**Common Attack Issues:**
```csharp
// Issue: Weapons are sheathed
if (playerCharacter.IsWeaponsSheathed) {
    Debug.Log("Cannot attack - weapons are sheathed!");
    // Solution: Call playerCharacter.ToggleWeaponsSheathed();
}

// Issue: Currently attacking (attack in progress)
if (playerCharacter.IsAttacking) {
    Debug.Log("Cannot attack - already attacking!");
    // Wait for current attack to finish
}

// Issue: Cached data prevents attack
if (!playerCharacter.CachedData.DisallowAttack) {
    Debug.Log("Attack blocked by cached data!");
    // Force cache refresh: playerCharacter.MarkToMakeCaches();
}

// Issue: Physics sync timing problems (Fixed in v1.95+)
Debug.Log($"Physics auto sync: {Physics.autoSyncTransforms}");
// Change detection system ensures sync when needed for attacks
if (BaseGameNetworkManager.Singleton != null) {
    BaseGameNetworkManager.Singleton.ForceSyncPhysicsTransforms();
}
```

#### Items Not Working

**Symptoms:**
- Can't pick up items
- Inventory doesn't update
- Equipment doesn't provide bonuses

**Solutions:**
```csharp
// Check item validation
if (!DataValidation.IsValidItemData(item)) {
    Debug.LogError($"Invalid item data: {item.Id}");
    return;
}

// Test inventory operations
var inventory = playerCharacter.Inventory;
bool added = inventory.AddItem(item);

if (!added) {
    Debug.LogWarning("Failed to add item to inventory");
    Debug.Log($"Inventory slots used: {inventory.UsedSlots}/{inventory.MaxSlots}");
}
```

### 🏗️ Build Issues

#### Build Fails

**Symptoms:**
- Build process stops with errors
- Missing references in build
- Script compilation errors

**NightBlade Build Setup:**
```csharp
// Ensure correct build settings
[MenuItem("NightBlade/Build/Setup for Production")]
static void SetupProductionBuild() {
    // Enable production optimizations
    BuildSettings.OptimizeForProduction = true;

    // Set validation levels
    DataValidation.CurrentLevel = ValidationLevel.Strict;
    RuntimeValidation.Enabled = false; // Disabled in production

    // Configure performance settings
    AutoSaveConfig.Interval = 45f;
    PhysicsConfig.UpdateRate = 12f;
}
```

#### Platform-Specific Issues

**Android Build Issues:**
```csharp
// Check Android settings
PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel21;
PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel30;

// Enable ARM64 if needed
PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
```

**iOS Build Issues:**
```csharp
// iOS-specific settings
PlayerSettings.iOS.targetOSVersionString = "12.0";
PlayerSettings.iOS.cameraUsageDescription = "Camera access for gameplay";
PlayerSettings.iOS.microphoneUsageDescription = "Voice chat";
```

### 📊 Data Issues

#### Corrupted Save Files

**Symptoms:**
- Character data resets
- Progress lost
- "Invalid save data" errors

**NightBlade Recovery:**
```csharp
// Enable save validation
SaveSystem.EnableValidation = true;
SaveSystem.CreateBackups = true;

// Recover corrupted saves
if (!SaveSystem.LoadPlayerData(playerId)) {
    Debug.LogWarning("Primary save corrupted, trying backup");
    if (!SaveSystem.LoadBackup(playerId)) {
        Debug.LogError("Backup also corrupted, resetting to defaults");
        CreateDefaultCharacter();
    }
}
```

#### Database Connection Issues

**Symptoms:**
- "Database connection failed" errors
- Character data not saving
- Server startup failures

**Solutions:**
```csharp
// Test database connection
bool connected = DatabaseManager.TestConnection();
Debug.Log($"Database connected: {connected}");

if (!connected) {
    // Check connection string
    string connectionString = DatabaseConfig.GetConnectionString();
    Debug.Log($"Connection string: {connectionString}");

    // Try to reconnect
    DatabaseManager.Reconnect();
}
```

### 🔧 Advanced Troubleshooting

#### Profiling Performance Bottlenecks

```csharp
using UnityEngine.Profiling;

public class PerformanceDebugger : MonoBehaviour {
    void Update() {
        Profiler.BeginSample("NightBlade_Update");

        // Your game logic here
        UpdateGameplay();

        Profiler.EndSample();

        // Log expensive frames
        if (Time.deltaTime > 0.1f) { // Slower than 10fps
            Debug.LogWarning($"Slow frame: {Time.deltaTime}s - investigating...");
            LogPerformanceStats();
        }
    }

    void LogPerformanceStats() {
        Debug.Log($"Mono heap: {Profiler.GetMonoHeapSizeLong() / 1024 / 1024}MB");
        Debug.Log($"Allocated: {Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024}MB");
        Debug.Log($"GC allocated: {Profiler.GetAllocatedMemoryForGraphicsDriver() / 1024 / 1024}MB");
    }
}
```

#### Memory Leak Detection

```csharp
public class MemoryLeakDetector : MonoBehaviour {
    private Dictionary<string, long> memorySnapshots = new Dictionary<string, long>();

    void Start() {
        TakeMemorySnapshot("Start");
    }

    void Update() {
        if (Time.frameCount % 600 == 0) { // Every 20 seconds
            TakeMemorySnapshot($"Frame_{Time.frameCount}");

            // Check for leaks
            DetectLeaks();
        }
    }

    void TakeMemorySnapshot(string label) {
        long memory = GC.GetTotalMemory(false);
        memorySnapshots[label] = memory;
        Debug.Log($"Memory at {label}: {memory / 1024 / 1024}MB");
    }

    void DetectLeaks() {
        if (memorySnapshots.Count < 2) return;

        var keys = memorySnapshots.Keys.ToArray();
        long first = memorySnapshots[keys[0]];
        long last = memorySnapshots[keys[keys.Length - 1]];

        long difference = last - first;
        if (difference > 50 * 1024 * 1024) { // 50MB increase
            Debug.LogWarning($"Potential memory leak detected: +{difference / 1024 / 1024}MB");
            LogMemoryAnalysis();
        }
    }

    void LogMemoryAnalysis() {
        // Log top memory consumers
        var textureMemory = Resources.FindObjectsOfTypeAll<Texture>().Sum(t => Profiler.GetRuntimeMemorySizeLong(t));
        var meshMemory = Resources.FindObjectsOfTypeAll<Mesh>().Sum(m => Profiler.GetRuntimeMemorySizeLong(m));

        Debug.Log($"Texture memory: {textureMemory / 1024 / 1024}MB");
        Debug.Log($"Mesh memory: {meshMemory / 1024 / 1024}MB");
    }
}
```

#### Network Packet Analysis

```csharp
public class NetworkDebugger : MonoBehaviour {
    private Queue<NetworkMessage> messageHistory = new Queue<NetworkMessage>();

    void Start() {
        NetworkManager.OnMessageReceived += AnalyzeMessage;
    }

    void AnalyzeMessage(NetworkMessage message) {
        // Keep recent history
        messageHistory.Enqueue(message);
        if (messageHistory.Count > 100) {
            messageHistory.Dequeue();
        }

        // Analyze message patterns
        AnalyzeMessagePatterns();

        // Check for anomalies
        if (IsSuspiciousMessage(message)) {
            Debug.LogWarning($"Suspicious message: {message.Type} from {message.Sender}");
        }
    }

    void AnalyzeMessagePatterns() {
        var messageCounts = messageHistory
            .GroupBy(m => m.Type)
            .ToDictionary(g => g.Key, g => g.Count());

        Debug.Log("Message frequency (last 100 messages):");
        foreach (var kvp in messageCounts) {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }

    bool IsSuspiciousMessage(NetworkMessage message) {
        // Check for rapid-fire messages
        var recentMessages = messageHistory.Where(m =>
            m.Sender == message.Sender &&
            Time.time - m.Timestamp < 1f); // Last second

        return recentMessages.Count() > 10; // More than 10 messages/second
    }
}
```

## 📞 Getting Help

### Diagnostic Reports

Generate comprehensive diagnostic reports:

```csharp
[MenuItem("NightBlade/Help/Generate Diagnostic Report")]
static void GenerateDiagnosticReport() {
    string report = DiagnosticReporter.GenerateFullReport();
    string path = Path.Combine(Application.dataPath, "NightBlade_Diagnostics.txt");
    File.WriteAllText(path, report);
    Debug.Log($"Diagnostic report saved to: {path}");
}
```

The report includes:
- System information
- Performance metrics
- Validation status
- Network statistics
- Error logs
- Configuration settings

### Community Support

- **Documentation**: Check all `.md` files in `Assets/NightBlade_1.95+/`
- **Issue Tracker**: Report bugs with diagnostic reports attached
- **Forum**: Community discussions and solutions
- **Discord**: Real-time help and user community

### NightBlade-Specific Issues

Since NightBlade includes significant enhancements over conventional MMO frameworks, some issues are unique:

1. **Validation Too Strict**: Reduce `DataValidation.CurrentLevel` for development
2. **Performance Monitoring Overhead**: Disable `RuntimeValidation.Enabled` in production
3. **Save Optimization Conflicts**: Adjust `LanRpgNetworkManager.saveInterval` if saves are too infrequent
4. **Security Features Blocking Gameplay**: Review `SecurityConfig` settings

Remember: NightBlade's enhanced error reporting and validation will often tell you exactly what's wrong and how to fix it. Always check the console for detailed diagnostic information.

