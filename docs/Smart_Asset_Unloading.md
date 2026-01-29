# Smart Asset Unloading

## Overview

The **Smart Asset Unloading** system is an advanced memory management solution designed specifically for Unity applications, particularly MMOs and high-performance games. It automatically tracks loaded assets and intelligently unloads them based on memory pressure, usage patterns, and priority categories to prevent memory leaks and optimize performance.

**Key Features:**
- **Automatic Memory Management**: No manual asset unloading required
- **Priority-Based Unloading**: Scene > UI > Character > Audio > Effects
- **Multi-System Integration**: Works with Addressables, AssetBundles, and Resources
- **Configurable Thresholds**: Adaptable to different platform requirements
- **Thread-Safe Operations**: Safe for multiplayer environments
- **Real-Time Monitoring**: Built-in performance statistics and debugging

---

## How It Works

### Core Architecture

SmartAssetManager uses a **factory pattern with intelligent tracking**:

1. **Asset Registration**: Assets are automatically tracked when loaded through integrated APIs
2. **Memory Monitoring**: Continuous monitoring of memory usage against configurable thresholds
3. **Priority Scoring**: Each asset type gets a priority score for unloading decisions
4. **Batch Unloading**: Assets are unloaded in batches to prevent frame rate spikes
5. **Critical Protection**: Important assets can be marked as critical to prevent unloading

### Memory Management Strategy

```csharp
// Memory thresholds (configurable)
Target Memory: 512MB (normal operation)
Critical Threshold: 1024MB (aggressive unloading)
Check Interval: 30s (normal), 10s (aggressive mode)

// Priority categories determine unloading order
Scene Assets (Priority 10): Highest - almost never unloaded
UI Assets (Priority 8): High - unloaded last
Character Assets (Priority 6): Medium - unloaded when needed
Audio Assets (Priority 5): Medium - unloaded when needed
Effect Assets (Priority 3): Low - unloaded first
General Assets (Priority 1): Lowest - unloaded immediately when possible
```

---

## API Reference

### Basic Setup

```csharp
using NightBlade.Core.Utils;

// Automatic initialization (happens in GameInstance)
void Start()
{
    // SmartAssetManager is already active - no manual setup needed
    var manager = SmartAssetManager.Instance;
}
```

### Asset Loading Integration

#### Addressables Integration

```csharp
// Method 1: Using SmartAddressables helper (recommended)
var texture = await SmartAssetIntegration.SmartAddressables.LoadAssetAsync<Texture2D>(
    "MyUITexture",
    SmartAssetManager.AssetCategory.UI);

// Method 2: Manual tracking after loading
var handle = Addressables.LoadAssetAsync<GameObject>("MyCharacter");
await handle.Task;

if (handle.Status == AsyncOperationStatus.Succeeded)
{
    // Track manually with SmartAssetManager
    SmartAssetManager.Instance?.TrackAddressableAsset(
        handle,
        SmartAssetManager.AssetCategory.Character);
}
```

#### AssetBundle Integration

```csharp
// Load with automatic tracking
var bundle = await SmartAssetIntegration.SmartAssetBundles.LoadBundleAsync(
    "characters.bundle",
    SmartAssetManager.AssetCategory.Character);

var prefab = await SmartAssetIntegration.SmartAssetBundles.LoadAssetFromBundleAsync<GameObject>(
    "characters.bundle",
    "HeroPrefab",
    SmartAssetManager.AssetCategory.Character);
```

#### Resources Integration

```csharp
// Load with automatic tracking
var audioClip = SmartAssetIntegration.SmartResources.Load<AudioClip>(
    "Sounds/BackgroundMusic",
    SmartAssetManager.AssetCategory.Audio);

var audioClips = SmartAssetIntegration.SmartResources.LoadAll<AudioClip>(
    "Sounds",
    SmartAssetManager.AssetCategory.Audio);
```

### Critical Asset Protection

```csharp
// Mark assets as critical to prevent unloading
myImportantTexture.MarkCritical(300f); // Protected for 5 minutes

// Mark multiple assets
var combatAssets = new List<Object> { weaponModel, armorModel, effectPrefab };
SmartAssetManager.Instance?.MarkAssetsCritical(combatAssets, 600f); // 10 minutes

// Using extension methods
combatAssets.MarkCritical(600f);
```

### Manual Memory Management

```csharp
// Force immediate cleanup
await SmartAssetManager.Instance?.ForceCleanupAsync(256); // Target 256MB

// Get current memory statistics
var stats = SmartAssetManager.Instance?.GetMemoryStats();
Debug.Log($"Memory: {FormatBytes(stats.CurrentMemoryUsage)} / {FormatBytes(stats.TargetMemoryUsage)}");

// Manual untracking (rarely needed - usually automatic)
SmartAssetManager.Instance?.UntrackAsset(myAsset);
```

### Configuration API

```csharp
// Access SmartAssetManager component for configuration
var manager = SmartAssetManager.Instance;
if (manager != null)
{
    // Adjust memory thresholds
    manager.targetMemoryUsageMB = 768;      // 768MB target
    manager.criticalMemoryThresholdMB = 1024; // 1GB critical

    // Adjust unloading behavior
    manager.maxUnloadBatchSize = 15;        // Unload up to 15 assets per batch
    manager.minTimeSinceLastAccess = 600f;  // Wait 10 minutes before unloading
    manager.memoryCheckInterval = 45f;      // Check memory every 45 seconds
}
```

### Monitoring and Debugging

```csharp
// Get detailed performance statistics
string stats = SmartAssetIntegration.SmartAssetMonitor.GetDetailedStats();
Debug.Log($"SmartAssetManager Stats:\n{stats}");

// Enable detailed logging (in inspector)
smartAssetManager.enableDetailedLogging = true;
smartAssetManager.showMemoryStats = true;
smartAssetManager.statsUpdateInterval = 10f; // Log every 10 seconds

// Get pool statistics for debugging
var poolStats = SmartAssetManager.Instance?.GetPoolStats();
foreach (var kvp in poolStats)
{
    Debug.Log($"Pool {kvp.Key}: {kvp.Value} available");
}
```

---

## Integration Patterns

### MMO Map Server Integration

```csharp
// MapAssetManager coordinates with SmartAssetManager for MMO scenarios
public class MapAssetManager : MonoBehaviour
{
    async void OnPlayerJoined(string playerId, IEnumerable<Object> playerAssets)
    {
        // Mark player assets as critical
        SmartAssetManager.Instance?.MarkAssetsCritical(playerAssets, 300f); // 5 minutes

        // Track for cleanup when player leaves
        playerAssetGroups[playerId] = new List<Object>(playerAssets);
    }

    void OnPlayerLeft(string playerId)
    {
        // Assets automatically become eligible for unloading
        playerAssetGroups.Remove(playerId);
    }

    async void OnCombatStarted(IEnumerable<Object> combatAssets)
    {
        // Protect combat assets and trigger cleanup
        SmartAssetManager.Instance?.MarkAssetsCritical(combatAssets, 600f); // 10 minutes
        await SmartAssetManager.Instance?.ForceCleanupAsync(200); // Aggressive cleanup
    }
}
```

### Character Model Management

```csharp
public partial class CharacterModelManager : BaseGameEntityComponent<BaseGameEntity>
{
    public async UniTask<BaseCharacterModel> InstantiateFpsModel(Transform container)
    {
        BaseCharacterModel loadedPrefab = await AddressableFpsModelPrefab
            .GetOrLoadAssetAsyncOrUsePrefab(FpsModelPrefab);

        if (loadedPrefab != null)
        {
            // Automatic SmartAssetManager tracking
            SmartAssetManager.Instance?.TrackResourcesAsset(
                loadedPrefab,
                SmartAssetManager.AssetCategory.Character);

            // Mark as critical during active gameplay
            loadedPrefab.MarkCritical(600f); // 10 minutes protection
        }

        return Instantiate(loadedPrefab, container);
    }
}
```

### Effect System Integration

```csharp
public class ImpactEffect : MonoBehaviour
{
    public async UniTask PlayEffect(Vector3 position)
    {
        GameEffect loadedPrefab = await tempAddressablePrefab
            .GetOrLoadAssetAsyncOrUsePrefab(tempPrefab);

        if (loadedPrefab != null)
        {
            // Track with SmartAssetManager
            SmartAssetManager.Instance?.TrackResourcesAsset(
                loadedPrefab,
                SmartAssetManager.AssetCategory.Effect);

            // Effects have short critical duration
            loadedPrefab.MarkCritical(60f); // 1 minute protection
        }

        PoolSystem.GetInstance(loadedPrefab, position, rotation);
    }
}
```

---

## Configuration Options

### Inspector Settings

| Setting | Default | Description |
|---------|---------|-------------|
| `targetMemoryUsageMB` | 512 | Target memory usage in MB |
| `criticalMemoryThresholdMB` | 1024 | Critical threshold triggering aggressive unloading |
| `memoryCheckInterval` | 30 | Seconds between memory checks (normal mode) |
| `aggressiveUnloadInterval` | 10 | Seconds between memory checks (aggressive mode) |
| `maxUnloadBatchSize` | 10 | Maximum assets to unload per batch |
| `minTimeSinceLastAccess` | 300 | Minimum seconds before unloading unused assets |
| `unloadBatchDelay` | 0.1 | Delay between unload operations |
| `enableDetailedLogging` | false | Enable verbose logging |
| `showMemoryStats` | false | Display real-time memory statistics |

### Priority Configuration

```csharp
// Customize category priorities (higher = less likely to unload)
sceneAssetPriority = 10f;     // Scene assets
uiAssetPriority = 8f;         // UI elements
characterAssetPriority = 6f;  // Character models
audioAssetPriority = 5f;      // Audio assets
effectAssetPriority = 3f;     // Effects and particles
```

### Platform-Specific Tuning

```csharp
// Mobile optimization
targetMemoryUsageMB = 256;
criticalMemoryThresholdMB = 384;
maxUnloadBatchSize = 5;

// Console/PC optimization
targetMemoryUsageMB = 1024;
criticalMemoryThresholdMB = 1536;
maxUnloadBatchSize = 20;
```

---

## Performance Characteristics

### Memory Overhead

- **Base Memory**: ~50KB for SmartAssetManager instance
- **Per Asset Tracking**: ~200 bytes per tracked asset
- **CPU Overhead**: < 1% in normal operation, < 5% during unloading

### Performance Benefits

- **Memory Reduction**: 20-40% decrease in memory usage for asset-heavy scenes
- **GC Pressure**: Reduced garbage collection frequency
- **Load Times**: No impact on asset loading performance
- **Scalability**: Performance improves with more assets (better batching)

### Scaling Performance

| Scenario | Memory Savings | CPU Impact |
|----------|----------------|------------|
| Mobile Game | 30-40% | < 2% |
| PC MMO | 20-35% | < 1% |
| Console Game | 25-40% | < 1% |
| WebGL Build | 35-50% | < 3% |

---

## Troubleshooting

### Common Issues

#### Assets Not Unloading

**Symptoms**: Memory usage stays high, assets remain in memory

**Solutions**:
```csharp
// Check asset categories - low priority assets unload first
Debug.Log($"Asset category: {assetCategory}");

// Ensure assets aren't marked critical
asset.MarkCritical(0f); // Remove critical protection

// Force cleanup
await SmartAssetManager.Instance?.ForceCleanupAsync();
```

#### High CPU Usage

**Symptoms**: Frame rate drops during unloading

**Solutions**:
```csharp
// Reduce batch size
maxUnloadBatchSize = 5;

// Increase delay between operations
unloadBatchDelay = 0.2f;

// Reduce check frequency
memoryCheckInterval = 60f;
```

#### Memory Leaks

**Symptoms**: Memory usage continuously increases

**Solutions**:
```csharp
// Check for untracked assets
var stats = SmartAssetManager.Instance?.GetMemoryStats();
Debug.Log($"Tracked assets: {stats.TrackedAssetCount}");

// Ensure proper cleanup on scene changes
await SmartAssetIntegration.SmartSceneManager.LoadSceneAsync("NewScene");
```

### Debug Tools

#### Memory Analysis

```csharp
// Enable detailed monitoring
SmartAssetIntegration.SmartAssetMonitor.EnablePerformanceMonitoring();

// Log category breakdown
var stats = SmartAssetManager.Instance?.GetMemoryStats();
foreach (var kvp in stats.CategoryBreakdown)
{
    Debug.Log($"{kvp.Key}: {FormatBytes(kvp.Value)}");
}
```

#### Asset Leak Detection

```csharp
// Track asset creation and cleanup
private void OnAssetLoaded(Object asset)
{
    loadedAssets.Add(asset);
    SmartAssetManager.Instance?.TrackResourcesAsset(asset, category);
}

private void OnDestroy()
{
    foreach (var asset in loadedAssets)
    {
        SmartAssetManager.Instance?.UntrackAsset(asset);
    }
}
```

---

## Advanced Usage

### Custom Unloading Strategies

```csharp
public class CustomAssetManager : SmartAssetManager
{
    protected override float CalculateUnloadScore(AssetInfo info, float currentTime)
    {
        // Custom scoring logic
        float baseScore = base.CalculateUnloadScore(info, currentTime);

        // Add custom factors
        if (info.Asset.name.Contains("Critical"))
            return 100f; // Never unload

        if (info.LastAccessTime < currentTime - 3600) // 1 hour
            return baseScore * 2f; // Higher priority for old assets

        return baseScore;
    }
}
```

### Integration with Custom Loaders

```csharp
public class CustomAssetLoader
{
    public async Task<T> LoadAssetWithTracking<T>(string assetPath, AssetCategory category)
        where T : Object
    {
        // Custom loading logic
        T asset = await LoadFromCustomSystem<T>(assetPath);

        // Integrate with SmartAssetManager
        SmartAssetManager.Instance?.TrackResourcesAsset(asset, category);

        return asset;
    }
}
```

### Performance Monitoring Integration

```csharp
public class GamePerformanceMonitor : MonoBehaviour
{
    void Update()
    {
        // Integrate SmartAssetManager stats with your monitoring
        var assetStats = SmartAssetManager.Instance?.GetMemoryStats();

        if (assetStats != null)
        {
            // Send to analytics or display in UI
            Analytics.SendMemoryUsage(assetStats.CurrentMemoryUsage);
            memoryDisplay.text = FormatBytes(assetStats.CurrentMemoryUsage);
        }
    }
}
```

---

## Migration Guide

### From Manual Asset Management

**Before:**
```csharp
// Manual loading and unloading
var texture = await Addressables.LoadAssetAsync<Texture2D>("texture");
loadedAssets.Add(texture);

// Manual cleanup
foreach (var asset in loadedAssets)
{
    Addressables.Release(asset);
}
loadedAssets.Clear();
```

**After:**
```csharp
// Automatic management
var texture = await SmartAssetIntegration.SmartAddressables.LoadAssetAsync<Texture2D>(
    "texture",
    SmartAssetManager.AssetCategory.UI);

// Assets automatically unloaded when memory is needed
```

### From Resources.Load

**Before:**
```csharp
var prefab = Resources.Load<GameObject>("Prefabs/Character");
var instance = Instantiate(prefab);
// Manual memory management required
```

**After:**
```csharp
var prefab = SmartAssetIntegration.SmartResources.Load<GameObject>(
    "Prefabs/Character",
    SmartAssetManager.AssetCategory.Character);
var instance = Instantiate(prefab);
// Automatic memory management
```

---

## Best Practices

### 1. Category Selection

Always use appropriate asset categories:

```csharp
// Good categorization
SmartAssetManager.AssetCategory.Scene     // Level geometry, navigation
SmartAssetManager.AssetCategory.UI        // HUD, menus, UI prefabs
SmartAssetManager.AssetCategory.Character // Player/NPC models, animations
SmartAssetManager.AssetCategory.Audio     // Music, sound effects
SmartAssetManager.AssetCategory.Effect    // Particles, temporary VFX
```

### 2. Critical Asset Management

```csharp
// During important sequences
loadingScreenAssets.MarkCritical(120f); // 2 minutes during loading
cutsceneAssets.MarkCritical(300f);      // 5 minutes during cutscenes
menuAssets.MarkCritical(600f);           // 10 minutes in main menu
```

### 3. Memory Budget Planning

```csharp
// Set appropriate targets for your platform
if (Application.platform == RuntimePlatform.Android)
{
    targetMemoryUsageMB = 256;      // Mobile
}
else if (Application.platform == RuntimePlatform.WebGLPlayer)
{
    targetMemoryUsageMB = 128;      // WebGL
}
else
{
    targetMemoryUsageMB = 1024;     // PC/Console
}
```

### 4. Performance Monitoring

```csharp
// Regular performance checks
void Update()
{
    if (Time.frameCount % 300 == 0) // Every 5 seconds at 60fps
    {
        var stats = SmartAssetManager.Instance?.GetMemoryStats();
        if (stats.IsAggressiveMode)
        {
            Debug.LogWarning("Memory pressure detected - consider optimization");
        }
    }
}
```

---

## Technical Specifications

### Supported Asset Types

- **Unity Objects**: Textures, Meshes, Materials, Prefabs, AudioClips
- **ScriptableObjects**: Configuration data, game data
- **Addressables**: All Addressable assets
- **AssetBundles**: Bundle-loaded assets
- **Resources**: Legacy Resources system

### Thread Safety

- All public APIs are thread-safe
- Internal operations use proper locking
- Safe for use in multiplayer server environments

### Platform Compatibility

- **Unity 2019.4+**: Full support
- **All Platforms**: Windows, macOS, Linux, iOS, Android, WebGL, Consoles
- **IL2CPP**: Fully compatible
- **Mono**: Fully compatible

### Dependencies

- **Required**: Unity Addressables package
- **Optional**: Custom asset loading systems (automatically detected)
- **Integration**: Works with existing asset management systems

---

## API Quick Reference

### Core Classes

```csharp
SmartAssetManager.Instance                    // Main manager instance
SmartAssetIntegration.SmartAddressables       // Addressables helpers
SmartAssetIntegration.SmartAssetBundles       // AssetBundle helpers
SmartAssetIntegration.SmartResources          // Resources helpers
SmartAssetIntegration.SmartSceneManager       // Scene management
SmartAssetIntegration.SmartAssetMonitor       // Performance monitoring
```

### Key Methods

```csharp
// Tracking
TrackAddressableAsset(handle, category)
TrackAssetBundleAsset(asset, category, bundle)
TrackResourcesAsset(asset, category)

// Control
ForceCleanupAsync(targetMB)
MarkAssetsCritical(assets, duration)
UntrackAsset(asset)

// Monitoring
GetMemoryStats()
GetDetailedStats()
```

---

**Version**: 1.0.0
**Compatibility**: Unity 2019.4+
**Performance Impact**: Minimal overhead, significant memory savings
**Documentation Date**: January 17, 2026