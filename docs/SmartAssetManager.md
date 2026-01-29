# 🧠 SmartAssetManager - Intelligent Memory Management

## **Overview**

The **SmartAssetManager** is NightBlade's advanced asset memory management system that provides intelligent, automatic memory optimization for MMO performance. It uses priority-based unloading, real-time monitoring, and adaptive strategies to maintain optimal memory usage without manual intervention.

**Type:** Component (MonoBehaviour)  
**Purpose:** Automatic asset memory management and performance optimization  
**Integration:** Works with Addressables, AssetBundles, and Resources  

---

## 📋 **Quick Start**

1. **Add to Scene**: Attach `SmartAssetManager` to a GameObject (preferably one that persists)
2. **Configure Memory Targets**: Set target memory usage and critical thresholds
3. **Set Priorities**: Configure asset type priorities based on gameplay needs
4. **Monitor**: Enable stats display to see real-time memory usage

```csharp
// Basic setup
GameObject managerObj = new GameObject("SmartAssetManager");
SmartAssetManager manager = managerObj.AddComponent<SmartAssetManager>();

// Configure for your game
manager.targetMemoryUsageMB = 512;  // 512MB target
manager.criticalMemoryThresholdMB = 1024;  // 1GB critical
```

---

## 📊 **Memory Management Architecture**

### **Core Concepts**

#### **Asset Categories**
Assets are automatically categorized for priority-based management:
- **Scene**: Level geometry, navigation, critical environment
- **UI**: Interface elements, menus, HUD components
- **Character**: Player/NPC models, animations, equipment
- **Audio**: Sound effects, music, voice clips
- **Effect**: Particles, visual effects, temporary graphics
- **General**: Miscellaneous assets

#### **Priority System**
Each category has a priority score (0-20) determining unload likelihood:
- **High Priority (15-20)**: Rarely unloaded, essential for gameplay
- **Medium Priority (5-14)**: Balanced unloading based on memory pressure
- **Low Priority (0-4)**: Frequently unloaded, nice-to-have assets

#### **Memory States**
- **Normal**: Memory below target → Standard monitoring intervals
- **High Pressure**: Memory above target → Accelerated unloading
- **Critical**: Memory above threshold → Aggressive batch unloading

---

## ⚙️ **Configuration Sections**

### **Memory Management**

#### **Memory Targets**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Target Memory** | 128-4096 MB | 512 MB | Normal memory usage goal |
| **Critical Threshold** | 256-8192 MB | 1024 MB | Triggers aggressive unloading |
| **Check Interval** | 10-300 sec | 30 sec | Normal monitoring frequency |
| **Aggressive Interval** | 5-60 sec | 10 sec | High-pressure monitoring |

**Best Practice:** Set target memory to 60-80% of your target platform's available RAM.

#### **Memory Monitoring**
```
Normal Mode:    Memory < Target     → Check every 30 seconds
High Pressure:  Target < Memory      → Check every 10 seconds
Critical Mode:  Memory > Threshold   → Immediate batch unloading
```

---

### **Unloading Strategy**

#### **Batch Processing**
| Setting | Range | Default | Impact |
|---------|-------|---------|--------|
| **Max Batch Size** | 1-50 | 10 | Assets unloaded per operation |
| **Batch Delay** | 0.01-1.0 sec | 0.1 sec | Time between unload operations |
| **Min Access Time** | 60-3600 sec | 300 sec | Minimum time before unloading |

#### **Unload Timing Logic**
```csharp
// Asset eligible for unloading if:
LastAccessTime > MinTimeSinceLastAccess AND
MemoryPressure > TargetMemory AND
AssetPriority < UnloadThreshold
```

---

### **Priority System**

#### **Asset Type Priorities**
Configure priority scores for each asset category:

| Category | Default | Recommended Range | Gameplay Role |
|----------|---------|------------------|---------------|
| **Scene** | 10.0 | 12-15 | Critical navigation and level stability |
| **UI** | 8.0 | 10-12 | Essential user interface elements |
| **Character** | 6.0 | 6-9 | Player/NPC models and animations |
| **Audio** | 5.0 | 4-7 | Sound effects and audio assets |
| **Effect** | 3.0 | 2-5 | Particles and visual effects |
| **General** | 1.0 | 0-3 | Miscellaneous assets |

#### **Priority Calculation**
```csharp
UnloadScore = BasePriority + TimeSinceLastAccess + MemoryPressureBonus
Higher Score = More Likely to Unload
```

---

### **Debug & Monitoring**

#### **Logging Options**
| Setting | Description | Performance Impact |
|---------|-------------|-------------------|
| **Detailed Logging** | Log all asset operations | Low (development only) |
| **Memory Stats Display** | Real-time GUI overlay | Minimal |
| **Stats Update Interval** | GUI refresh rate | Very low |

#### **Runtime Statistics**
Available during play mode:
- **Current Memory Usage**: Real-time memory consumption
- **Target vs Critical**: Visual status indicators
- **Tracked Asset Count**: Total managed assets
- **Category Breakdown**: Memory usage by asset type
- **Aggressive Mode**: High-pressure status indicator

---

## 🎮 **Usage Examples**

### **Basic Setup**
```csharp
public class GameManager : MonoBehaviour
{
    void Start()
    {
        var assetManager = GetComponent<SmartAssetManager>();

        // Configure for mobile game
        assetManager.targetMemoryUsageMB = 256;
        assetManager.criticalMemoryThresholdMB = 384;

        // Prioritize UI and characters for mobile
        assetManager.uiAssetPriority = 12f;
        assetManager.characterAssetPriority = 10f;
        assetManager.sceneAssetPriority = 8f;
    }
}
```

### **Asset Tracking**
```csharp
// Track Addressables asset
async void LoadCharacterModel()
{
    var handle = Addressables.LoadAssetAsync<GameObject>("CharacterModel");
    await handle.Task;

    if (SmartAssetManager.Instance != null)
    {
        SmartAssetManager.Instance.TrackAddressableAsset(handle, SmartAssetManager.AssetCategory.Character);
    }
}

// Track Resources asset
void LoadEffect()
{
    GameObject effect = Resources.Load<GameObject>("ExplosionEffect");
    if (SmartAssetManager.Instance != null)
    {
        SmartAssetManager.Instance.TrackResourcesAsset(effect, SmartAssetManager.AssetCategory.Effect);
    }
}
```

### **Manual Cleanup**
```csharp
async void ForceMemoryCleanup()
{
    if (SmartAssetManager.Instance != null)
    {
        // Force cleanup to 256MB
        await SmartAssetManager.Instance.ForceCleanupAsync(256);
    }
}
```

---

## 📊 **Performance Optimization**

### **Memory Target Guidelines**

| Platform | Recommended Target | Critical Threshold | Rationale |
|----------|-------------------|-------------------|-----------|
| **Mobile Low-End** | 128-256 MB | 192-384 MB | Limited RAM, aggressive unloading |
| **Mobile High-End** | 256-512 MB | 384-768 MB | More RAM, balanced approach |
| **PC/WebGL** | 512-1024 MB | 768-1536 MB | Generous RAM, conservative unloading |
| **Console** | 1024-2048 MB | 1536-3072 MB | Large RAM, minimal unloading |

### **Priority Tuning Examples**

#### **Fast-Paced Action Game**
```
Scene Priority: 12 (Keep levels loaded)
Character Priority: 10 (Frequent model switching)
Effect Priority: 8 (Combat particles critical)
UI Priority: 6 (Menus less critical than action)
Audio Priority: 4 (Sound effects nice-to-have)
```

#### **Exploration RPG**
```
Scene Priority: 15 (Large world navigation)
Character Priority: 12 (NPC interactions)
UI Priority: 10 (Always accessible interface)
Audio Priority: 8 (Atmospheric music)
Effect Priority: 3 (Environmental effects)
```

#### **Strategy Game**
```
UI Priority: 15 (Complex interface critical)
Scene Priority: 10 (Strategic overview)
Character Priority: 8 (Unit models)
Audio Priority: 6 (UI feedback sounds)
Effect Priority: 2 (Particle effects optional)
```

---

## 🔍 **Runtime Monitoring**

### **Memory Dashboard**
The editor provides real-time memory monitoring:
- **Visual Memory Bar**: Color-coded usage indicator
- **Status Indicators**: Green (good), Yellow (warning), Red (critical)
- **Category Breakdown**: Memory usage by asset type
- **Aggressive Mode Alert**: High-pressure warnings

### **Performance Metrics**
```
Current Usage: 387.2 MB
Target: 512 MB | Critical: 1024 MB
Assets Tracked: 1,247
Categories:
  Scene: 156.7 MB
  UI: 89.3 MB
  Character: 67.8 MB
  Audio: 34.1 MB
  Effect: 39.3 MB
```

---

## 🚨 **Common Issues & Solutions**

### **"Memory usage keeps growing"**
**Cause:** Priorities too high or unload intervals too long
**Solution:**
- Lower priority scores for non-essential assets
- Reduce `minTimeSinceLastAccess` for faster unloading
- Increase monitoring frequency

### **"Frequent stuttering during gameplay"**
**Cause:** Large unload batches causing frame drops
**Solution:**
- Reduce `maxUnloadBatchSize` (try 5-10)
- Increase `unloadBatchDelay` (try 0.2-0.5 seconds)
- Lower critical threshold to trigger unloading earlier

### **"Assets unloading too aggressively"**
**Cause:** Priorities too low or memory targets too conservative
**Solution:**
- Increase priority scores for important assets
- Raise target memory usage
- Increase `minTimeSinceLastAccess`

### **"UI elements disappearing"**
**Cause:** UI assets being unloaded during critical memory
**Solution:**
- Increase `uiAssetPriority` (recommended: 10+)
- Lower critical threshold to trigger unloading earlier
- Ensure UI assets are properly categorized

---

## 📊 **Advanced Configuration**

### **Custom Asset Categories**
```csharp
public enum CustomAssetCategory
{
    CriticalGameplay,
    OptionalContent,
    StreamingAssets
}

// Extend SmartAssetManager
public class CustomAssetManager : SmartAssetManager
{
    public float criticalGameplayPriority = 15f;
    public float optionalContentPriority = 2f;
    public float streamingAssetsPriority = 8f;

    // Override priority calculation
    protected override float GetAssetPriority(AssetCategory category)
    {
        switch (category)
        {
            case CustomAssetCategory.CriticalGameplay:
                return criticalGameplayPriority;
            case CustomAssetCategory.OptionalContent:
                return optionalContentPriority;
            case CustomAssetCategory.StreamingAssets:
                return streamingAssetsPriority;
            default:
                return base.GetAssetPriority(category);
        }
    }
}
```

### **Memory Pressure Callbacks**
```csharp
public class MemoryAwareManager : MonoBehaviour
{
    void Start()
    {
        if (SmartAssetManager.Instance != null)
        {
            // Register for memory pressure events
            SmartAssetManager.Instance.OnMemoryPressure += HandleMemoryPressure;
        }
    }

    private void HandleMemoryPressure(bool isCritical)
    {
        if (isCritical)
        {
            // Reduce quality settings
            QualitySettings.SetQualityLevel(1, true);
            // Disable non-essential systems
            DisableParticleSystems();
        }
        else
        {
            // Restore quality
            QualitySettings.SetQualityLevel(3, true);
        }
    }
}
```

---

## 🔗 **Integration Points**

### **Addressables Integration**
```csharp
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesLoader : MonoBehaviour
{
    async void LoadAssetAsync(string address, SmartAssetManager.AssetCategory category)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(address);
        await handle.Task;

        if (SmartAssetManager.Instance != null)
        {
            SmartAssetManager.Instance.TrackAddressableAsset(handle, category);
        }
    }
}
```

### **AssetBundle Integration**
```csharp
public class AssetBundleLoader : MonoBehaviour
{
    void LoadFromBundle(AssetBundle bundle, string assetName, SmartAssetManager.AssetCategory category)
    {
        var asset = bundle.LoadAsset<GameObject>(assetName);

        if (SmartAssetManager.Instance != null)
        {
            SmartAssetManager.Instance.TrackAssetBundleAsset(asset, category, bundle);
        }
    }
}
```

### **Unity Events Integration**
```csharp
public class SceneLoader : MonoBehaviour
{
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (SmartAssetManager.Instance != null)
        {
            // Mark current scene assets as high priority
            SmartAssetManager.Instance.MarkSceneAssetsCritical(scene);
        }
    }
}
```

---

## 📈 **Performance Metrics**

### **Memory Efficiency**
- **Target Achievement**: Percentage of time spent under target memory
- **Critical Avoidance**: Time spent in critical memory state
- **Unload Efficiency**: Assets unloaded per second during cleanup

### **System Performance**
- **Frame Rate Impact**: Average FPS drop during unloading operations
- **Load Time Savings**: Time saved by reusing cached assets
- **Memory Stability**: Memory usage variance over time

### **Asset Management**
- **Cache Hit Rate**: Percentage of asset requests served from cache
- **Unload Frequency**: Average time between asset unloads
- **Category Balance**: Memory distribution across asset categories

---

## 🛠️ **Development Tools**

### **Editor Integration**
- **Memory Dashboard**: Real-time usage visualization
- **Configuration Validation**: Automatic setting verification
- **Performance Recommendations**: Optimization suggestions
- **Debug Logging**: Detailed operation tracking

### **Profiling Integration**
```csharp
// Unity Profiler integration
void OnEnable()
{
    UnityEngine.Profiling.Profiler.BeginSample("SmartAssetManager.Update");
}

void OnDisable()
{
    UnityEngine.Profiling.Profiler.EndSample();
}
```

---

## 🎯 **Best Practices**

### **1. Memory Target Setting**
- **Test on target hardware** - Different devices have different memory characteristics
- **Monitor real gameplay** - Use the memory dashboard during actual play sessions
- **Adjust for content density** - More assets = lower memory targets

### **2. Priority Configuration**
- **UI > Scene > Character** for most games - Interface is always critical
- **Test extreme scenarios** - Full party + all effects + complex scenes
- **Iterate based on crashes** - Memory-related crashes indicate priority issues

### **3. Performance Monitoring**
- **Enable detailed logging** during development and testing
- **Monitor memory dashboard** during gameplay sessions
- **Profile unload operations** to identify performance bottlenecks

### **4. Asset Organization**
- **Use consistent categories** across your asset naming conventions
- **Track assets immediately** after loading, not before unloading
- **Test with memory warnings** enabled to catch edge cases

---

## 📋 **Testing Checklist**

### **Configuration Testing**
- [ ] Memory targets appropriate for target platform
- [ ] Priority values create clear hierarchy
- [ ] Unload intervals balance performance vs memory
- [ ] Batch sizes prevent frame rate drops

### **Runtime Testing**
- [ ] Memory usage stays under critical threshold during normal play
- [ ] No asset unloading during critical gameplay moments
- [ ] UI elements remain loaded during memory pressure
- [ ] Character models reload quickly when needed

### **Performance Testing**
- [ ] Frame rate stable during memory operations
- [ ] Loading times acceptable after asset unloading
- [ ] No texture corruption or missing assets
- [ ] Memory usage patterns predictable

### **Edge Case Testing**
- [ ] Scene transitions don't cause massive unloading
- [ ] Combat sequences maintain all critical assets
- [ ] Menu navigation keeps UI assets loaded
- [ ] Network interruptions don't break asset management

---

## 📞 **API Reference**

### **Core Properties**
```csharp
public long targetMemoryUsageMB = 512;
public long criticalMemoryThresholdMB = 1024;
public float memoryCheckInterval = 30f;
public float aggressiveUnloadInterval = 10f;
public int maxUnloadBatchSize = 10;
public float minTimeSinceLastAccess = 300f;
public float sceneAssetPriority = 10f;
public float uiAssetPriority = 8f;
public float characterAssetPriority = 6f;
public float audioAssetPriority = 5f;
public float effectAssetPriority = 3f;
```

### **Public Methods**
```csharp
// Asset tracking
void TrackAddressableAsset<T>(AsyncOperationHandle<T> handle, AssetCategory category);
void TrackAssetBundleAsset(UnityEngine.Object asset, AssetCategory category, AssetBundle bundle = null);
void TrackResourcesAsset(UnityEngine.Object asset, AssetCategory category);

// Manual control
void UntrackAsset(UnityEngine.Object asset);
Task ForceCleanupAsync(long targetMemoryMB = 0);

// Statistics
AssetMemoryStats GetMemoryStats();
```

### **Enums**
```csharp
public enum AssetCategory
{
    Scene,      // Level geometry and navigation
    UI,         // User interface elements
    Character,  // Player and NPC models
    Audio,      // Sound effects and music
    Effect,     // Particles and visual effects
    General     // Miscellaneous assets
}

public enum AssetType
{
    Addressables,   // Unity Addressables system
    AssetBundle,    // Traditional asset bundles
    Resources       // Unity Resources folder
}
```

---

## 📈 **Version History**

### **Current Version**
- Intelligent priority-based asset unloading
- Real-time memory monitoring and statistics
- Category-based asset organization
- Aggressive mode for critical memory situations
- Unity Editor integration with visual dashboard
- Comprehensive documentation and best practices

### **Performance Characteristics**
- **Memory Overhead**: ~2-5MB for tracking structures
- **CPU Overhead**: Minimal (background monitoring)
- **Unload Performance**: Batched operations prevent frame drops
- **Compatibility**: Works with all Unity asset loading systems

---

*This documentation covers the complete SmartAssetManager system for intelligent memory management in NightBlade. For the latest updates and additional features, check the official repository.*