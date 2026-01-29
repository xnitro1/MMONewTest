# 📊 PerformanceMonitor - Real-Time Performance Tracking

## **Overview**

The **PerformanceMonitor** system is a comprehensive performance tracking suite for NightBlade, completely refactored into 4 focused components for better maintainability and performance. It provides real-time monitoring, profiling markers, and diagnostic tools to help developers identify performance bottlenecks and track optimization effectiveness.

**Architecture:** Modular component system
**Components:** PerformanceMonitor (Coordinator), PerformanceStats (Data), PerformanceRenderer (GUI), PerformanceProfiler (Diagnostics)
**Purpose:** Real-time performance monitoring and diagnostics
**Integration:** Core optimization tool with Unity Profiler integration
**GUI:** Interactive overlay with real-time controls and keyboard shortcuts

## **⭐ Recent Enhancements (v4.2.0 - January 2026)**

**Major Update - Performance & Accuracy Improvements:**
1. ✅ **Real-Time FPS Tracking**: FPS now updates every frame (not once per second) for smooth, responsive feedback
2. ✅ **Optimized Memory Tracking**: Memory/GC updates every 0.5s to prevent the profiler from causing GC spikes itself (classic observer effect fix!)
3. ✅ **All 9 Pool Systems Connected**: StringBuilder, MPB, NetDataWriter, Delegates, JSON, FxCollection, Network Strings, UI Objects, Coroutines
4. ✅ **Improved Pool Metrics**: Replaced misleading "Pool Efficiency %" with clear "Pooled Objects: X (Y/9 systems)"
5. ✅ **Enhanced Detailed View**: Comprehensive 5-section breakdown (Performance, Pooling, Network, Objects, Efficiency)
6. ✅ **Top-Center GUI**: Repositioned for better visibility and less obstruction
7. ✅ **Active Coroutine Tracking**: Now tracks running animations in real-time

## **Architecture Refactor (v4.1.0)**

**Previous Major Update**: The PerformanceMonitor has been completely refactored from a single 1,411-line God Object into 4 focused components:

### **PerformanceMonitor.cs** (Coordinator)
- **Lines**: ~160 (89% reduction from original)
- **Role**: Component orchestration and public API
- **Responsibilities**: Configuration, initialization, coordination

### **PerformanceStats.cs** (Data Layer)
- **Role**: Data structures and statistical calculations
- **Features**: Real-time metrics computation, thread-safe operations
- **Integration**: Unity Profiler API integration

### **PerformanceRenderer.cs** (GUI Layer) ⭐ **ENHANCED 2026-01-24**
- **Role**: Complete GUI rendering and user interaction
- **Features**: Interactive overlay, real-time display, keyboard shortcuts
- **Performance**: Optimized rendering with smart memory tracking (prevents self-inflicted GC spikes)
- **GUI Position**: Centered at top of screen for better visibility
- **Update Strategy**: FPS every frame, Memory/GC every 0.5s (prevents profiler overhead)

### **PerformanceProfiler.cs** (Diagnostics Layer)
- **Role**: Benchmarking, testing, and system diagnostics
- **Features**: Mini-benchmarks, system tests, peak tracking, debug tools

## **Refactoring Benefits**

### **Maintainability**
- **89% code reduction** in main component
- **Single Responsibility Principle** - each component has one clear purpose
- **Modular architecture** - components can be modified independently
- **Clear separation of concerns** - data, rendering, logic properly separated

### **Performance**
- **Faster compilation** - smaller individual files
- **Better runtime performance** - optimized GUI rendering
- **Reduced memory pressure** - better garbage collection patterns
- **Improved responsiveness** - smaller OnGUI method (was 1,200+ lines)

### **Developer Experience**
- **Easier debugging** - issues isolated to specific components
- **Better testability** - individual components can be unit tested
- **Clearer code navigation** - related functionality grouped together
- **Future extensibility** - easy to add new features to specific components  

---

## 📋 **Quick Start**

1. **Add to GameInstance**: Performance monitor is automatically added via GameInstance settings
2. **Enable in Editor**: Set `Enable Performance Monitor` in GameInstance inspector
3. **Toggle GUI**: Use `Show Performance GUI` to display overlay during testing
4. **View Stats**: Monitor real-time metrics in Unity Editor or game overlay
5. **Profile**: Use profiler markers to track specific system performance

```csharp
// Enable via GameInstance (Editor only)
gameInstance.enablePerformanceMonitor = true;
gameInstance.showPerformanceGUI = true;

// Use profiler markers in code
PerformanceMonitor.ProfileNetworkUpdate(() =>
{
    // Your network operations
    SendPlayerUpdates();
    ProcessMessages();
});
```

---

## 🎨 **GUI Overlay System**

### **Visual Performance Display**

The PerformanceMonitor includes a real-time GUI overlay that displays key performance metrics with NightBlade branding:

```
        🖥️ Performance Monitor - NightBlade
               (Centered at Top)

⚡ FPS: 80.3 (12.5ms)      ← Real-time (updates every frame)
🗂️ Memory: 45.2MB          ← Updates every 0.5 seconds
♻️ GC Pressure: 0.12KB/frame  ← Averaged over 0.5s

🌐 Network Strings: 245 (active)
🎨 UI Objects Pooled: 20 objects
📏 Distance Entities: 3 active
🔄 Pooled Coroutines: 2 active
📈 Pooled Objects: 87 (7/9 systems)

[📊 Detailed] [🎯 Benchmark] [🙈 Hide] [🐛 Debug]
💡 Shortcuts: F11 Benchmark | F12 Toggle GUI
📊 Updated 0.2s ago
```

**Real-Time vs Periodic Updates:** ⭐ **OPTIMIZED 2026-01-24**
- **⚡ FPS/Frame Time**: Updates every frame for instant feedback (lightweight)
- **🗂️ Memory/GC**: Updates every 0.5 seconds (prevents profiler causing GC itself!)
- **🌐 System Stats**: Updates every 1 second with live system data
- **Anti-Observer Effect**: Memory profiling spaced out to avoid measuring its own overhead

**Keyboard Shortcuts:**
- **F11**: Run mini-benchmark (500 iterations)
- **F12**: Toggle GUI visibility
- **Input System Compatible**: Works with both legacy and new Input System
```

### **Color-Coded Indicators**

- **🟢 Green**: Performance within acceptable ranges
- **🟡 Yellow**: Performance degradation detected
- **🔴 Red**: Critical performance issues

**Indicator Thresholds:**
- **Frame Time**: < 8ms (Green), 8-16ms (Yellow), > 16ms (Red)
- **FPS**: > 60 (Green), 30-60 (Yellow), < 30 (Red)
- **GC Allocations**: < 1KB (Green), > 1KB (Yellow)
- **Memory**: < 200MB (Green), 200-500MB (Yellow), > 500MB (Red)

---

## ⚙️ **Configuration Options**

### **Monitoring Configuration**

#### **Core Settings**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Enable Detailed Logging** | Boolean | false | Log performance stats to console |
| **Stats Update Interval** | 0.1-60s | 5s | How often to refresh statistics |
| **Show GUI Stats** | Boolean | false | Display overlay in game view |

#### **GameInstance Integration**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Enable Performance Monitor** | Boolean | false | Add monitor component to GameInstance |
| **Show Performance GUI** | Boolean | false | Enable GUI overlay from GameInstance |

---

## 🔍 **Profiler Markers**

### **Built-in Markers**

The PerformanceMonitor provides Unity Profiler markers for tracking specific system performance:

#### **Network.Update**
Tracks network processing operations including:
- Player position updates
- Message sending/receiving
- Server synchronization

```csharp
PerformanceMonitor.ProfileNetworkUpdate(() =>
{
    foreach (var player in players)
    {
        SendPositionUpdate(player);
    }
});
```

#### **UI.Render**
Monitors user interface rendering performance:
- UI element updates
- Layout calculations
- Draw calls

```csharp
PerformanceMonitor.ProfileUIRender(() =>
{
    UpdateHealthBars();
    RefreshInventoryUI();
});
```

#### **Distance.Update**
Tracks distance-based entity management:
- Distance calculations
- Entity activation/deactivation
- Update prioritization

```csharp
PerformanceMonitor.ProfileDistanceUpdate(() =>
{
    UpdateVisibleEntities();
    CalculateDistances();
});
```

#### **Coroutine.Pool**
Monitors pooled coroutine operations:
- Coroutine allocation from pool
- Pool management overhead
- Async operation timing

```csharp
PerformanceMonitor.ProfileCoroutinePool(() =>
{
    StartPooledCoroutine(LoadPlayerData());
});
```

### **Custom Markers**

You can create custom profiler markers for your specific systems:

```csharp
using Unity.Profiling;

public class MySystem
{
    private static readonly ProfilerMarker MySystemMarker = new ProfilerMarker("MySystem.Operation");

    public void DoExpensiveOperation()
    {
        using (MySystemMarker.Auto())
        {
            // Your expensive operation
            ProcessData();
            UpdateEntities();
        }
    }
}
```

---

## 📊 **Performance Statistics**

### **Real-Time Metrics**

#### **Frame Performance**
- **Frame Time**: Time taken to render one frame (milliseconds)
- **FPS**: Frames per second (calculated from frame time)
- **GC Allocations**: Memory allocated by garbage collector per frame

#### **Memory Management** ⭐ **ALL 9 POOLS TRACKED (2026-01-24)**
- **Total Memory Used**: Current managed memory consumption
- **GC Pressure**: Per-frame allocation rate (averaged over 0.5s)
- **Network Strings Cached**: Number of cached network strings (String interning)
- **UI Objects Pooled**: Number of objects in UI pool (TMP damage numbers)
- **Distance-Based Entities**: Active distance-managed entities
- **Pooled Coroutines**: ACTIVE pooled coroutine operations (running animations)
- **StringBuilder Pool**: Pooled text builders ready for reuse
- **MaterialPropertyBlock Pool**: Pooled MPBs for dynamic materials
- **NetDataWriter Pool**: Pooled network message writers
- **Delegate Pool**: Pooled event handlers and callbacks
- **JSON Operation Pool**: Pooled StringBuilders for save/load
- **FxCollection Pool**: Pooled combat effect collections

### **Statistics Structure**

```csharp
public struct PerformanceStats
{
    // Real-time performance (updated every frame)
    public float FrameTime;           // Current frame time in seconds
    public float FPS;                 // Frames per second
    
    // Memory tracking (updated every 0.5s to prevent profiler overhead)
    public long GCAllocationsThisFrame; // GC allocations per frame (averaged)
    public long TotalMemoryUsed;      // Total managed memory in bytes
    
    // Pooling systems (9 total - all tracked as of 2026-01-24)
    public int NetworkStringsCached;  // Network string cache size
    public int UIPoolSize;           // UI object pool size (-1 if TMP not available)
    public int DistanceBasedEntities; // Distance-managed entities
    public int PooledCoroutinesActive; // ACTIVE pooled coroutines (not pooled waiting)
    public int StringBuilderPoolSize; // StringBuilder pool
    public int CollectionPoolSize;   // Collection pool (generic, always 0)
    public int MaterialPropertyBlockPoolSize; // MPB pool
    public int DelegatePoolSize;     // Delegate pool
    public int NetDataWriterPoolSize; // Network writer pool
    public int JsonOperationPoolSize; // JSON operation pool
    public int FxCollectionPoolSize; // FX collection pool
}
```

---

## 🔗 **System Integration**

### **Automatic Integration**

The PerformanceMonitor automatically integrates with several NightBlade systems:

#### **Distance-Based Updater**
- **Status**: ✅ Fully Integrated
- **Tracking**: Counts active distance-managed entities
- **Method**: `GetDistanceBasedEntityCount()`

#### **Network String Cache**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track cached network string count
- **Method**: `LiteNetLib.Utils.NetworkStringCache.GetCacheSize()`

#### **UI Object Pool**
- **Status**: ✅ Fully Integrated
- **Purpose**: Monitor UI object pool utilization
- **Method**: `GetUIPoolSize()`

#### **Coroutine Pool**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track ACTIVE pooled coroutines (running animations)
- **Method**: `CoroutinePool.ActiveCoroutineCount`

#### **Delegate Pool**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track pooled event handlers and callbacks
- **Method**: `DelegatePool.Advanced.PoolSizes`

#### **JSON Operation Pool**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track pooled StringBuilder for save/load operations
- **Method**: `JsonOperationPool.PoolSizes`

#### **FxCollection Pool**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track pooled combat effect collections
- **Method**: `FxCollection.GetPoolSize()`

#### **StringBuilder Pool**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track pooled text builders
- **Method**: `StringBuilderPool.PoolSize`

#### **MaterialPropertyBlock Pool**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track pooled material property blocks
- **Method**: `MaterialPropertyBlockPool.PoolSize`

#### **NetDataWriter Pool**
- **Status**: ✅ Fully Integrated (2026-01-24)
- **Purpose**: Track pooled network writers
- **Method**: `NetworkWriterPool.PoolSize`

### **Integration Points**

To integrate your systems with PerformanceMonitor:

```csharp
public class MySystemMonitor
{
    private PerformanceMonitor monitor;

    void Start()
    {
        monitor = PerformanceMonitor.Instance;
    }

    void Update()
    {
        // Report custom metrics
        ReportCustomMetrics();
    }

    private void ReportCustomMetrics()
    {
        // Access current stats
        var stats = monitor.GetStats();

        // Add custom tracking
        int mySystemEntities = CountMyEntities();
        float mySystemLoad = CalculateLoad();

        // Log or display custom metrics
        Debug.Log($"MySystem: {mySystemEntities} entities, {mySystemLoad:F2} load");
    }
}
```

---

## 🎯 **Usage Scenarios**

### **Development & Debugging**

#### **Real-Time Monitoring**
1. Enable performance monitor in GameInstance
2. Toggle GUI overlay for immediate feedback
3. Watch for performance regressions during development
4. Use profiler markers to identify bottlenecks

#### **Benchmarking**
```csharp
// Run performance benchmark
void RunBenchmark()
{
    var monitor = PerformanceMonitor.Instance;
    var startStats = monitor.GetStats();

    // Run your test scenario
    RunExpensiveOperation();

    var endStats = monitor.GetStats();

    Debug.Log($"Benchmark Results:");
    Debug.Log($"Frame Time: {startStats.FrameTime:F3} -> {endStats.FrameTime:F3}");
    Debug.Log($"Memory: {startStats.TotalMemoryUsed} -> {endStats.TotalMemoryUsed}");
}
```

### **Testing & QA**

#### **Performance Baselines**
- Set acceptable performance thresholds
- Monitor for regressions in automated tests
- Generate performance reports for QA teams

#### **Load Testing**
```csharp
public class LoadTestMonitor
{
    private PerformanceMonitor monitor;
    private List<PerformanceStats> samples = new List<PerformanceStats>();

    void StartLoadTest()
    {
        monitor = PerformanceMonitor.Instance;
        StartCoroutine(RunLoadTest());
    }

    IEnumerator RunLoadTest()
    {
        // Warm up
        yield return new WaitForSeconds(5f);

        // Sample performance during load
        for (int i = 0; i < 300; i++) // 5 minutes at 60fps
        {
            samples.Add(monitor.GetStats());
            yield return null;
        }

        // Analyze results
        AnalyzeLoadTestResults();
    }

    void AnalyzeLoadTestResults()
    {
        float avgFPS = samples.Average(s => s.FPS);
        float minFPS = samples.Min(s => s.FPS);
        long maxMemory = samples.Max(s => s.TotalMemoryUsed);

        Debug.Log($"Load Test Results:");
        Debug.Log($"Average FPS: {avgFPS:F1}");
        Debug.Log($"Minimum FPS: {minFPS:F1}");
        Debug.Log($"Peak Memory: {maxMemory / 1024 / 1024:F1}MB");
    }
}
```

### **Production Monitoring**

#### **Runtime Diagnostics**
```csharp
public class ProductionMonitor
{
    private PerformanceMonitor monitor;

    void Start()
    {
        monitor = PerformanceMonitor.Instance;
        // Disable GUI in production
        monitor.ShowGUIStats = false;
    }

    void Update()
    {
        // Check for critical performance issues
        var stats = monitor.GetStats();

        if (stats.FPS < 20f)
        {
            ReportCriticalPerformance("FPS dropped below 20");
        }

        if (stats.TotalMemoryUsed > 500 * 1024 * 1024) // 500MB
        {
            ReportCriticalPerformance("Memory usage exceeded 500MB");
        }
    }

    void ReportCriticalPerformance(string issue)
    {
        // Send to monitoring system
        Analytics.SendEvent("PerformanceIssue", new Dictionary<string, object>
        {
            { "issue", issue },
            { "timestamp", Time.time },
            { "scene", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name }
        });
    }
}
```

---

## 🔧 **Diagnostic Tools**

### **Built-in Tools**

#### **Performance Report**
Generates a comprehensive performance snapshot:
- Current frame time and FPS
- Memory usage statistics
- System integration metrics
- Timestamp and configuration info

#### **Benchmark Runner**
Runs automated performance benchmarks:
- Measures average frame time over multiple frames
- Calculates FPS variance
- Reports min/max performance bounds
- Provides timing analysis
- Stress tests CPU with trigonometric calculations

#### **Real-Time GUI Display** ⭐⭐⭐
**Fully Connected to NightBlade Systems:**
- **Network Strings**: Live count from NetworkStringCache ✅
- **UI Objects Pooled**: Real-time count from UIPoolManager ✅
- **Distance Entities**: Active DistanceBasedUpdater components ⚠️
- **Pooled Coroutines**: Available coroutines in CoroutinePool ⚠️
- **Memory Usage**: Actual managed memory consumption ✅
- **GC Monitoring**: Per-frame garbage collection tracking ✅

**UI Pooling Integration:**
- **UIDamageNumberPool**: Combat damage numbers with automatic pooling
- **UIFloatingTextPool**: Status messages (MISS, BLOCKED, IMMUNE) with pooling
- **BaseUISceneGameplay**: Automatically uses pooled system for combat text
- **Templates**: Auto-generated templates for damage numbers and floating text

#### **Interactive GUI Controls** ⭐⭐⭐
**Built-in Control Buttons:**
- **📊 Simple/Detailed**: Toggle between basic and full statistics view
- **🎯 Benchmark**: Run mini performance benchmark (500 iterations)
- **🔄 Reset**: Clear accumulated statistics and benchmark history
- **🙈 Hide**: Hide GUI overlay (shows small toggle button)
- **⚙️ Dev**: Switch to development mode (detailed, frequent updates)
- **🎮 Game**: Switch to game mode (simple, optimized updates)
- **🎨 UI Stats**: Show detailed UI pooling breakdown and efficiency

**Smart Visibility:**
- **Hidden State**: Small "Show" button appears in corner when hidden
- **Disabled State**: Small "Show Monitor" button when GUI is completely disabled
- **Auto-recovery**: Easy to re-enable if accidentally hidden

**Keyboard Shortcuts:**
- **F11**: Run mini-benchmark instantly
- **F12**: Toggle GUI visibility on/off
- **Input System Compatible**: Works with both legacy and new Input System
- **On-screen reminder**: Shortcuts displayed at bottom of GUI

**Advanced Monitoring:**
- **🧪 Test All**: Create sample components to test all optimization systems
- **🐛 Debug**: Click to log detailed system status to console
- **🎨 UI Stats**: Real-time UI pooling efficiency and peak tracking

#### **Preset Configurations**

**Development Preset:**
- Detailed logging: Enabled
- Update interval: 1 second
- GUI overlay: Visible

**Testing Preset:**
- Detailed logging: Disabled
- Update interval: 2 seconds
- GUI overlay: Visible

**Production Preset:**
- Detailed logging: Disabled
- Update interval: 5 seconds
- GUI overlay: Hidden

---

## 🔍 **Understanding Zero Stats**

Some statistics may show as zero initially. This is normal and indicates the systems haven't been used yet:

### **Expected Values in Game**
- **Network Strings (245+)**: Active string caching for bandwidth optimization
- **UI Objects Pooled (20+)**: Pre-warmed pools for combat text and effects (requires TMP resources)
- **Distance Entities (3+ per player)**: Automatic on all player NearbyEntityDetector components
- **Pooled Coroutines (varies)**: Active during UI animations (pooled damage numbers, floating text)

### **TMP Resources Required**
**UI pooling requires TextMesh Pro Essential Resources:**
1. Go to `Window > TextMesh Pro > Import TMP Essential Resources`
2. Click `Import` in the dialog
3. UI pooling will activate automatically after import
4. PerformanceMonitor will show: `🎨 UI Pooled: 20 objects`

**Without TMP resources:**
- PerformanceMonitor shows: `🎨 UI Pooled: TMP resources missing`
- No TMP importer dialog spam
- Combat text uses legacy instantiation (still works)

### **Status Indicators**
The GUI shows helpful status messages when systems are inactive:
```
🌐 Network Strings: 0 (not active)
🎨 UI Objects Pooled: 0 (no pools)
📏 Distance Entities: 0 (none found)
🔄 Pooled Coroutines: 0 (not used)
```

### **Debug System Status**
**Use the 🐛 Debug button** in the GUI to check detailed system status:
- Logs the current state of all monitored systems to the console
- Shows whether systems are initialized and active
- Helps identify why specific stats are zero

**Automatic Distance-Based Optimization:**
- Player characters automatically get `DistanceBasedNearbyEntityDetector` on all `NearbyEntityDetector` components
- Each player has 3 optimized detectors: activatable entities, item drops, and enemies
- Expect 3+ distance entities per player in active game scenes

**Example Debug Output:**
```
[PerformanceMonitor] === System Status Check ===
Network String Cache: 245 strings
UI Pool Manager: 0 pooled objects
Distance Based Updaters: 3 components found
Coroutine Pool: 12 pooled coroutines (2 pools)
[PerformanceMonitor] === End System Status Check ===
```

---

## 🚨 **Common Issues & Solutions**

### **"GUI overlay not showing"**

**Symptoms:** Performance GUI doesn't appear in game view
**Causes:**
- `ShowGUIStats` is false
- Monitor component not added to scene
- GUI rendering disabled in build

**Solutions:**
- Enable `showPerformanceGUI` in GameInstance
- Ensure PerformanceMonitor component exists
- Check that you're in play mode

### **"Keyboard shortcuts don't work"**

**Symptoms:** F11/F12 keys don't respond
**Causes:**
- Input System package configuration issues
- Conflicting input handlers

**Solutions:**
- PerformanceMonitor uses NightBlade's InputManager (compatible with both input systems)
- Check that F11/F12 keys aren't bound in Input System settings
- Ensure NightBlade's CameraAndInput package is properly imported

### **"High performance impact"** ⭐ **FIXED 2026-01-24**

**Symptoms:** Performance monitor itself causes performance issues or periodic GC spikes
**Causes:**
- Update interval too frequent
- Detailed logging enabled
- Too many profiler markers
- **FIXED**: Memory profiling every frame causing GC pressure

**Solutions:**
- ✅ **Already Optimized**: Memory/GC tracking now only updates every 0.5s (was every frame!)
- Increase `statsUpdateInterval` to 5+ seconds if still needed
- Disable detailed logging in production
- Use profiler markers sparingly

**Technical Details:**
The performance monitor was calling `System.GC.GetTotalMemory()` and `Profiler.GetMonoUsedSizeLong()` **every single frame** (60+ times per second), which caused:
- Periodic GC pressure checks
- Memory scanning overhead
- The very spikes it was trying to measure (observer effect!)

**Fix Applied:**
- FPS tracking: Every frame (lightweight, no allocations)
- Memory/GC tracking: Every 0.5 seconds (prevents profiler overhead)
- GC calculation: Averaged over time window for accurate per-frame rate

### **"TMP resources missing"**

**Symptoms:** PerformanceMonitor shows `🎨 UI Pooled: TMP resources missing`
**Causes:**
- TextMesh Pro Essential Resources not imported
- TMP package not properly set up

**Solutions:**
1. Go to `Window > TextMesh Pro > Import TMP Essential Resources`
2. Click `Import` in the dialog that appears
3. Restart Unity if necessary
4. UI pooling will activate automatically

### **"Inaccurate statistics"**

**Symptoms:** Performance stats don't match Unity Profiler
**Causes:**
- Stats update interval misconfiguration
- GC collection timing
- Platform-specific measurement differences

**Solutions:**
- Align update intervals with measurement needs
- Use Unity Profiler for precise measurements
- Account for platform differences in expectations

### **"Memory leaks in monitor"**

**Symptoms:** PerformanceMonitor consumes increasing memory
**Causes:**
- Statistics history not managed
- Profiler markers accumulating
- Singleton instance not cleaned up

**Solutions:**
- Limit statistics history size
- Clear profiler markers when not needed
- Properly destroy singleton on scene changes

---

## 📊 **Performance Baselines**

### **Target Metrics**

| Metric | Excellent | Good | Acceptable | Poor |
|--------|-----------|------|------------|------|
| **Frame Time** | < 8ms | 8-12ms | 12-16ms | > 16ms |
| **FPS** | > 120 | 60-120 | 30-60 | < 30 |
| **GC Allocation** | < 100B | 100B-1KB | 1KB-10KB | > 10KB |
| **Memory Usage** | < 100MB | 100-300MB | 300-500MB | > 500MB |

### **Platform-Specific Targets**

#### **Desktop (High-End)**
- Target FPS: 120+
- Memory Budget: < 500MB
- GC Budget: < 5KB/frame

#### **Desktop (Standard)**
- Target FPS: 60
- Memory Budget: < 300MB
- GC Budget: < 2KB/frame

#### **Mobile (High-End)**
- Target FPS: 60
- Memory Budget: < 200MB
- GC Budget: < 1KB/frame

#### **Mobile (Standard)**
- Target FPS: 30
- Memory Budget: < 150MB
- GC Budget: < 500B/frame

---

## 🔄 **Integration with GameInstance**

### **Automatic Setup**

The PerformanceMonitor integrates seamlessly with GameInstance:

```csharp
// In GameInstance inspector (Editor only)
public bool enablePerformanceMonitor = false;  // Add component
public bool showPerformanceGUI = false;       // Show overlay

// Runtime behavior
void InitializePerformanceOptimizations()
{
    // ... other initialization ...

    // Add performance monitor if enabled
    if (enablePerformanceMonitor)
    {
        AddPerformanceMonitorIfNeeded();
    }
}
```

### **Conditional Compilation**

Performance monitoring is editor-only by default:

```csharp
private void AddPerformanceMonitorIfNeeded()
{
#if UNITY_EDITOR
    if (enablePerformanceMonitor)
    {
        var monitor = gameObject.AddComponent<PerformanceMonitor>();
        monitor.ShowGUIStats = showPerformanceGUI;
    }
#endif
}
```

---

## 📋 **API Reference**

### **Core Properties**

```csharp
public static PerformanceMonitor Instance { get; }     // Singleton instance
public bool ShowGUIStats { get; set; }                // GUI overlay toggle
```

### **Statistics Access**

```csharp
public PerformanceStats GetStats()                    // Get current statistics
```

### **Profiler Methods**

```csharp
public static void ProfileNetworkUpdate(Action action)    // Profile network operations
public static void ProfileUIRender(Action action)         // Profile UI rendering
public static void ProfileDistanceUpdate(Action action)   // Profile distance updates
public static void ProfileCoroutinePool(Action action)    // Profile coroutine pooling
```

### **Statistics Structure**

```csharp
public struct PerformanceStats
{
    public float FrameTime;
    public float FPS;
    public long GCAllocationsThisFrame;
    public long TotalMemoryUsed;
    public int NetworkStringsCached;
    public int UIPoolSize;
    public int DistanceBasedEntities;
    public int PooledCoroutinesActive;
}
```

---

## 🎯 **Best Practices**

### **1. Development Workflow**
- **Enable Early**: Turn on performance monitoring during initial development
- **Set Baselines**: Establish performance targets for your project
- **Monitor Regularly**: Check performance metrics during development
- **Profile Hotspots**: Use profiler markers on performance-critical code

### **2. Testing Strategy**
- **Automated Tests**: Include performance checks in automated test suites
- **Load Testing**: Test performance under expected peak loads
- **Platform Testing**: Verify performance across target platforms
- **Regression Detection**: Monitor for performance regressions over time

### **3. Production Deployment**
- **Disable GUI**: Never show performance GUI in production builds
- **Conditional Logging**: Only enable detailed logging in development
- **Resource Budgets**: Set realistic performance budgets for your game
- **Monitoring Integration**: Send performance data to analytics systems

### **4. Optimization Workflow**
- **Identify Bottlenecks**: Use performance monitor to find slow systems
- **Measure Improvements**: Track performance changes after optimizations
- **Profile First**: Always profile before optimizing
- **Iterate**: Make incremental changes and measure impact

---

## 📈 **Advanced Usage**

### **Custom Performance Metrics**

```csharp
public class CustomPerformanceTracker : MonoBehaviour
{
    private PerformanceMonitor monitor;
    private float customMetric;

    void Start()
    {
        monitor = PerformanceMonitor.Instance;
    }

    void Update()
    {
        // Track custom performance metric
        customMetric = CalculateCustomMetric();

        // Report to analytics or monitoring system
        if (customMetric > threshold)
        {
            ReportPerformanceIssue("Custom metric exceeded threshold", customMetric);
        }
    }

    float CalculateCustomMetric()
    {
        // Your custom performance calculation
        return SomeExpensiveOperation() / Time.deltaTime;
    }
}
```

### **Automated Performance Testing**

```csharp
public class AutomatedPerformanceTest
{
    [UnityTest]
    public IEnumerator PerformanceRegressionTest()
    {
        // Setup
        var monitor = PerformanceMonitor.Instance;
        var initialStats = monitor.GetStats();

        // Run test scenario
        yield return RunTestScenario();

        // Verify performance
        var finalStats = monitor.GetStats();

        // Assert performance requirements
        Assert.Less(finalStats.FrameTime, initialStats.FrameTime * 1.5f,
            "Frame time regression detected");
        Assert.Greater(finalStats.FPS, 30f,
            "FPS dropped below minimum requirement");
    }

    IEnumerator RunTestScenario()
    {
        // Your test scenario implementation
        yield return new WaitForSeconds(5f);
    }
}
```

---

## 📞 **Troubleshooting Guide**

### **Performance Issues**

**High Frame Time:**
1. Check profiler for bottlenecks
2. Look for GC spikes in allocation metrics
3. Examine distance-based entity counts
4. Review network operation frequency

**Memory Growth:**
1. Monitor GC allocation patterns
2. Check for object pooling effectiveness
3. Look for texture/asset memory leaks
4. Review network string caching

**Inconsistent FPS:**
1. Check for frame time variance
2. Look for periodic GC collections
3. Examine coroutine scheduling
4. Review distance update frequencies

### **Integration Issues**

**Monitor Not Appearing:**
1. Verify GameInstance settings
2. Check editor-only compilation
3. Ensure component is added to scene
4. Confirm Awake/Start timing

**Inaccurate Readings:**
1. Adjust update intervals
2. Account for platform differences
3. Compare with Unity Profiler
4. Check for measurement overhead

---

*This documentation covers the complete PerformanceMonitor system for NightBlade optimization tracking. For the latest updates and additional features, check the official repository.*