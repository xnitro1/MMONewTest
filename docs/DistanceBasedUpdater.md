# 📏 DistanceBasedUpdater - Performance Scaling System

## **Overview**

The **DistanceBasedUpdater** is a fundamental performance optimization system that automatically adjusts entity update frequencies based on distance from the player. This enables significantly higher Concurrent User (CCU) capacities by intelligently reducing computational overhead for distant entities while maintaining full performance for nearby objects.

**Type:** Abstract Base Class (MonoBehaviour)  
**Purpose:** Distance-based performance scaling for CCU optimization  
**Location:** `Assets/NightBlade/Core/Utils/DistanceBasedUpdater.cs`

---

## 📋 **Quick Start**

1. **Inherit from Base Class**: Create a class that inherits from `DistanceBasedUpdater`
2. **Implement Update Logic**: Override `PerformUpdate()` with your update code
3. **Configure Tiers**: Set distance thresholds and update frequencies
4. **Attach to Entities**: Add your custom updater to game entities

```csharp
// Example implementation
public class EnemyAIUpdater : DistanceBasedUpdater
{
    private EnemyAI aiComponent;

    protected override void Start()
    {
        base.Start();
        aiComponent = GetComponent<EnemyAI>();
    }

    protected override void PerformUpdate()
    {
        // Your AI update logic here
        aiComponent.UpdateAI();
        aiComponent.CheckPlayerProximity();
        aiComponent.UpdateNavigation();
    }
}
```

---

## 🏗️ **Core Architecture**

### **Performance Tier System**

#### **5-Tier Distance Scaling**
```
┌─────────────────────────────────────────────────────────────┐
│                        Very Far (Tier 4)                   │ ← 0.1 FPS
├─────────────────────────────────────────────────────────────┤
│                         Far (Tier 3)                       │ ← 0.5 FPS
├─────────────────────────────────────────────────────────────┤
│                      Medium (Tier 2)                       │ ← 3 FPS
├─────────────────────────────────────────────────────────────┤
│                       Near (Tier 1)                        │ ← 15 FPS
├─────────────────────────────────────────────────────────────┤
│                      Very Near (Tier 0)                    │ ← 50 FPS (Full)
└─────────────────────────────────────────────────────────────┘
```

#### **Tier Configuration**
```csharp
[Header("Distance Configuration")]
[SerializeField] protected float[] distanceTiers = { 15f, 35f, 75f, 150f };
[SerializeField] protected float[] updateFrequencies = { 50f, 15f, 3f, 0.5f, 0.1f };
```

**Distance Tiers (meters):**
- **Tier 0**: 0-15m (Full performance)
- **Tier 1**: 15-35m (High performance)
- **Tier 2**: 35-75m (Medium performance)
- **Tier 3**: 75-150m (Low performance)
- **Tier 4**: 150m+ (Minimal performance)

### **Update Frequency Management**

#### **Intelligent Scheduling**
```csharp
protected float currentUpdateInterval;
protected float nextUpdateTime;

private void Update()
{
    if (Time.time >= nextUpdateTime)
    {
        PerformUpdate();
        currentUpdateInterval = CalculateUpdateInterval();
        nextUpdateTime = Time.time + currentUpdateInterval;
    }
}
```

#### **Interval Calculation**
```csharp
protected float CalculateUpdateInterval()
{
    int currentTier = GetCurrentTier();
    float targetFrequency = updateFrequencies[currentTier];

    // Convert frequency to interval (e.g., 30 FPS = 0.033 seconds)
    return 1f / targetFrequency;
}
```

---

## ⚙️ **Configuration Options**

### **Distance Tier Customization**

#### **Adaptive Distance Tiers**
```csharp
// Customize for different entity types
float[] npcDistanceTiers = { 20f, 50f, 100f, 200f };      // NPCs - longer view distance
float[] itemDistanceTiers = { 10f, 25f, 50f, 100f };     // Items - shorter view distance
float[] effectDistanceTiers = { 5f, 15f, 30f, 60f };     // Effects - very short range
```

#### **Performance Frequency Tuning**
```csharp
// Different performance requirements
float[] highPerformanceNPC = { 60f, 30f, 10f, 2f, 0.5f };   // Fast-paced combat NPCs
float[] lowPerformanceProps = { 10f, 5f, 1f, 0.2f, 0.1f }; // Background props
float[] mediumPerformanceMobs = { 30f, 15f, 5f, 1f, 0.2f }; // Standard mobs
```

### **Runtime Monitoring**

#### **Debug Visualization**
```csharp
[Header("Runtime Monitoring")]
[SerializeField] protected bool showDebugInfo = false;

private void OnGUI()
{
    if (showDebugInfo && showDebugInfo)
    {
        GUI.Label(new Rect(10, 10, 300, 100),
            $"Distance: {currentDistanceToPlayer:F1}m\n" +
            $"Tier: {GetCurrentTier()}\n" +
            $"Update Freq: {1f/currentUpdateInterval:F1} FPS\n" +
            $"Next Update: {nextUpdateTime - Time.time:F2}s");
    }
}
```

---

## 🔄 **Integration with Combat Zones**

### **CombatZoneManager Integration**

#### **Zone-Aware Performance**
```csharp
protected CombatZoneManager combatZoneManager;

protected override void Start()
{
    base.Start();
    combatZoneManager = FindObjectOfType<CombatZoneManager>();
}

protected int GetCurrentTier()
{
    if (combatZoneManager != null)
    {
        // Check if in combat zone first
        CombatZone nearestZone = combatZoneManager.GetNearestCombatZone(transform.position);
        if (nearestZone != null && nearestZone.isActive)
        {
            return CalculateZoneBasedTier(nearestZone);
        }
    }

    // Fall back to distance-based calculation
    return CalculateDistanceBasedTier();
}
```

#### **Combat Zone Override**
```csharp
private int CalculateZoneBasedTier(CombatZone zone)
{
    float distanceToZone = Vector3.Distance(transform.position, zone.center);

    if (distanceToZone <= zone.combatRadius)
    {
        return 0; // Full performance in combat zone
    }
    else if (distanceToZone <= zone.combatRadius + zone.transitionWidth)
    {
        return 1; // High performance in transition zone
    }

    // Outside transition zone, use distance calculation
    return CalculateDistanceBasedTier();
}
```

### **Priority Boost System**

#### **Zone-Based Priority Enhancement**
```csharp
protected float GetPriorityMultiplier()
{
    if (combatZoneManager != null)
    {
        CombatZone nearestZone = combatZoneManager.GetNearestCombatZone(transform.position);
        if (nearestZone != null && nearestZone.isActive)
        {
            float distance = Vector3.Distance(transform.position, nearestZone.center);
            if (distance <= nearestZone.combatRadius)
            {
                return nearestZone.priorityBoost;
            }
        }
    }
    return 1f;
}
```

---

## 📊 **CCU Scaling Impact**

### **Performance Multipliers**

#### **Distance-Based Scaling Benefits**
| Distance | Update Frequency | CPU Savings | CCU Impact |
|----------|------------------|-------------|------------|
| **0-15m** | 50 FPS | 0% (baseline) | Full performance |
| **15-35m** | 15 FPS | 70% | 3.3x scaling |
| **35-75m** | 3 FPS | 94% | 16.7x scaling |
| **75-150m** | 0.5 FPS | 99% | 100x scaling |
| **150m+** | 0.1 FPS | 99.8% | 500x scaling |

#### **Real-World CCU Improvements**
```csharp
// Example scaling calculations
int baseCCU = 100; // Players at full performance

// With distance scaling:
int tier1Players = 50;   // 15 FPS performance
int tier2Players = 200;  // 3 FPS performance
int tier3Players = 500;  // 0.5 FPS performance
int tier4Players = 1000; // 0.1 FPS performance

int totalScaledCCU = baseCCU + tier1Players + tier2Players + tier3Players + tier4Players;
// Result: 1850 total CCU with minimal performance impact
```

### **Memory and Network Benefits**

#### **Memory Optimization**
- **Reduced Object Updates**: Less frequent transform updates
- **Cache Efficiency**: Better CPU cache utilization
- **GC Pressure Reduction**: Fewer allocations from reduced updates

#### **Network Optimization**
- **Reduced Sync Traffic**: Less frequent position updates for distant entities
- **Bandwidth Savings**: Lower network overhead for background entities
- **Server Load Reduction**: Decreased processing for non-critical updates

---

## 🔧 **Implementation Examples**

### **NPC AI Updater**

#### **Combat-Aware AI Updates**
```csharp
public class NPCAIUpdater : DistanceBasedUpdater
{
    private NPCController npc;
    private float lastFullUpdateTime;

    protected override void Start()
    {
        base.Start();
        npc = GetComponent<NPCController>();
        lastFullUpdateTime = Time.time;
    }

    protected override void PerformUpdate()
    {
        int currentTier = GetCurrentTier();

        switch (currentTier)
        {
            case 0: // Full performance
                PerformFullAIUpdate();
                lastFullUpdateTime = Time.time;
                break;

            case 1: // High performance
                PerformCombatAIUpdate();
                break;

            case 2: // Medium performance
                PerformNavigationOnlyUpdate();
                break;

            default: // Low/Minimal performance
                PerformMinimalUpdate();
                break;
        }
    }

    private void PerformFullAIUpdate()
    {
        npc.UpdateCombatLogic();
        npc.UpdateNavigation();
        npc.UpdateAnimations();
        npc.CheckPlayerInteraction();
    }

    private void PerformCombatAIUpdate()
    {
        npc.UpdateCombatLogic();
        npc.UpdateNavigation();
    }

    private void PerformNavigationOnlyUpdate()
    {
        npc.UpdateNavigation();
    }

    private void PerformMinimalUpdate()
    {
        // Only critical updates
        npc.CheckForDespawn();
    }
}
```

### **Particle Effect Updater**

#### **Visual Effect Optimization**
```csharp
public class ParticleEffectUpdater : DistanceBasedUpdater
{
    private ParticleSystem particles;
    private float originalEmissionRate;

    protected override void Start()
    {
        base.Start();
        particles = GetComponent<ParticleSystem>();
        var emission = particles.emission;
        originalEmissionRate = emission.rateOverTime.constant;
    }

    protected override void PerformUpdate()
    {
        int currentTier = GetCurrentTier();
        var emission = particles.emission;

        // Scale particle emission based on distance
        float emissionMultiplier = GetEmissionMultiplier(currentTier);
        emission.rateOverTime = originalEmissionRate * emissionMultiplier;

        // Reduce particle count for distant effects
        var main = particles.main;
        main.maxParticles = Mathf.RoundToInt(originalMaxParticles * emissionMultiplier);
    }

    private float GetEmissionMultiplier(int tier)
    {
        switch (tier)
        {
            case 0: return 1.0f;   // Full emission
            case 1: return 0.8f;   // High emission
            case 2: return 0.4f;   // Medium emission
            case 3: return 0.2f;   // Low emission
            case 4: return 0.1f;   // Minimal emission
            default: return 0.1f;
        }
    }
}
```

### **Audio Source Updater**

#### **3D Audio Optimization**
```csharp
public class AudioSourceUpdater : DistanceBasedUpdater
{
    private AudioSource audioSource;
    private float originalVolume;
    private float originalMaxDistance;

    protected override void Start()
    {
        base.Start();
        audioSource = GetComponent<AudioSource>();
        originalVolume = audioSource.volume;
        originalMaxDistance = audioSource.maxDistance;
    }

    protected override void PerformUpdate()
    {
        int currentTier = GetCurrentTier();

        // Adjust audio properties based on distance
        float volumeMultiplier = GetVolumeMultiplier(currentTier);
        audioSource.volume = originalVolume * volumeMultiplier;

        // Reduce audio range for distant sources
        float rangeMultiplier = GetRangeMultiplier(currentTier);
        audioSource.maxDistance = originalMaxDistance * rangeMultiplier;

        // Disable spatial audio for very distant sources
        audioSource.spatialBlend = (currentTier >= 3) ? 0f : 1f;
    }

    private float GetVolumeMultiplier(int tier)
    {
        switch (tier)
        {
            case 0: return 1.0f;   // Full volume
            case 1: return 0.9f;   // High volume
            case 2: return 0.6f;   // Medium volume
            case 3: return 0.3f;   // Low volume
            case 4: return 0.1f;   // Minimal volume
            default: return 0.1f;
        }
    }

    private float GetRangeMultiplier(int tier)
    {
        switch (tier)
        {
            case 0: return 1.0f;   // Full range
            case 1: return 0.8f;   // Reduced range
            case 2: return 0.5f;   // Medium range
            case 3: return 0.3f;   // Low range
            case 4: return 0.1f;   // Minimal range
            default: return 0.1f;
        }
    }
}
```

---

## 🚨 **Common Issues & Solutions**

### **"Entities not updating at expected frequency"**

**Symptoms:** Entities update too frequently or infrequently
**Causes:**
- Incorrect tier calculation
- Distance measurement errors
- Update interval miscalculation

**Solutions:**
- Verify distance calculations are accurate
- Check tier thresholds match expectations
- Enable debug visualization to monitor tier assignment

### **"Performance not scaling as expected"**

**Symptoms:** CPU usage not reducing with distance
**Causes:**
- Too many entities in high-performance tiers
- Tier thresholds too large
- Update logic not respecting tier limits

**Solutions:**
- Analyze entity distribution across tiers
- Adjust tier distance thresholds
- Optimize update logic for each tier

### **"Player transform not found"**

**Symptoms:** System falls back to default tier 0
**Causes:**
- Player object not tagged correctly
- Player transform null or inactive
- Multiple objects with "Player" tag

**Solutions:**
- Ensure player object has "Player" tag
- Verify player object is active in hierarchy
- Check for duplicate "Player" tags

### **"Combat zones not overriding distance"**

**Symptoms:** Entities in combat zones not getting priority boost
**Causes:**
- CombatZoneManager not found in scene
- Zone not active or configured correctly
- Zone priority calculation errors

**Solutions:**
- Ensure CombatZoneManager exists in scene
- Verify combat zones are active and configured
- Check zone priority boost calculations

---

## 📊 **Performance Tuning**

### **Tier Threshold Optimization**

#### **Dynamic Tier Adjustment**
```csharp
// Adjust tiers based on server load
public void OptimizeTiersForLoad(float serverLoadFactor)
{
    if (serverLoadFactor > 0.8f) // High load
    {
        // Reduce tier distances to prioritize closer entities
        distanceTiers = new float[] { 10f, 25f, 50f, 100f };
    }
    else if (serverLoadFactor < 0.3f) // Low load
    {
        // Increase tier distances for better quality
        distanceTiers = new float[] { 25f, 60f, 120f, 250f };
    }
}
```

#### **Entity-Specific Tuning**
```csharp
// Different tuning for different entity types
public void ConfigureForEntityType(string entityType)
{
    switch (entityType)
    {
        case "CombatNPC":
            distanceTiers = new float[] { 20f, 40f, 80f, 160f };
            updateFrequencies = new float[] { 60f, 30f, 10f, 2f, 0.5f };
            break;

        case "AmbientCreature":
            distanceTiers = new float[] { 15f, 35f, 75f, 150f };
            updateFrequencies = new float[] { 20f, 8f, 2f, 0.5f, 0.1f };
            break;

        case "BackgroundProp":
            distanceTiers = new float[] { 10f, 25f, 50f, 100f };
            updateFrequencies = new float[] { 5f, 2f, 0.5f, 0.2f, 0.1f };
            break;
    }
}
```

### **Quality vs Performance Balance**

#### **Adaptive Quality Scaling**
```csharp
public void AdjustQualityBasedOnPerformance(float targetFPS)
{
    float currentFPS = 1f / Time.deltaTime;

    if (currentFPS < targetFPS * 0.8f) // Too slow
    {
        // Reduce quality by increasing tier distances
        for (int i = 0; i < distanceTiers.Length; i++)
        {
            distanceTiers[i] *= 0.9f; // Reduce by 10%
        }
    }
    else if (currentFPS > targetFPS * 1.2f) // Too fast
    {
        // Increase quality by reducing tier distances
        for (int i = 0; i < distanceTiers.Length; i++)
        {
            distanceTiers[i] *= 1.1f; // Increase by 10%
        }
    }
}
```

---

## 🔍 **Debugging & Monitoring**

### **Runtime Visualization**

#### **Tier Visualization Gizmos**
```csharp
private void OnDrawGizmos()
{
    if (!showDebugInfo) return;

    int currentTier = GetCurrentTier();
    Color tierColor = GetTierColor(currentTier);

    Gizmos.color = tierColor;
    Gizmos.DrawWireSphere(transform.position, GetTierVisualizationRadius(currentTier));
}

private Color GetTierColor(int tier)
{
    switch (tier)
    {
        case 0: return Color.red;      // Full performance
        case 1: return Color.orange;   // High performance
        case 2: return Color.yellow;   // Medium performance
        case 3: return Color.green;    // Low performance
        case 4: return Color.blue;     // Minimal performance
        default: return Color.gray;
    }
}

private float GetTierVisualizationRadius(int tier)
{
    // Show tier as sphere size
    return (5 - tier) * 2f; // Tier 0 = large sphere, Tier 4 = small sphere
}
```

### **Performance Metrics**

#### **Tier Distribution Monitoring**
```csharp
public static class DistanceBasedUpdaterMetrics
{
    public static Dictionary<int, int> entityCountByTier = new Dictionary<int, int>();
    public static float totalUpdateOverheadReduction;

    public static void UpdateMetrics()
    {
        entityCountByTier.Clear();
        totalUpdateOverheadReduction = 0f;

        DistanceBasedUpdater[] updaters = FindObjectsOfType<DistanceBasedUpdater>();
        foreach (var updater in updaters)
        {
            int tier = updater.GetCurrentTier();
            if (!entityCountByTier.ContainsKey(tier))
                entityCountByTier[tier] = 0;
            entityCountByTier[tier]++;

            // Calculate overhead reduction
            float fullUpdateFrequency = updater.updateFrequencies[0];
            float currentFrequency = updater.updateFrequencies[tier];
            float reduction = 1f - (currentFrequency / fullUpdateFrequency);
            totalUpdateOverheadReduction += reduction;
        }

        totalUpdateOverheadReduction /= updaters.Length;
    }
}
```

---

## 📋 **Configuration Checklist**

### **Basic Setup**
- [ ] Create class inheriting from DistanceBasedUpdater
- [ ] Implement PerformUpdate() with your update logic
- [ ] Configure appropriate distance tiers for your use case
- [ ] Set update frequencies matching performance requirements
- [ ] Test tier assignment with debug visualization

### **Performance Tuning**
- [ ] Analyze entity update patterns and CPU usage
- [ ] Adjust tier distances based on gameplay requirements
- [ ] Tune update frequencies for optimal performance balance
- [ ] Test with realistic entity counts and distributions
- [ ] Monitor frame rate impact and adjust accordingly

### **Integration Testing**
- [ ] Verify CombatZoneManager integration if used
- [ ] Test distance calculations and tier assignments
- [ ] Check performance scaling across different areas
- [ ] Monitor for edge cases and tier transition issues
- [ ] Validate update frequency changes are working correctly

### **Production Optimization**
- [ ] Disable debug visualizations in production builds
- [ ] Optimize tier calculations for performance
- [ ] Set appropriate defaults for different entity types
- [ ] Implement performance monitoring and alerts
- [ ] Document optimal configurations for your game

---

## 🔗 **Integration Examples**

### **Batch Entity Management**

#### **Entity Manager with Distance-Based Updates**
```csharp
public class EntityManager : MonoBehaviour
{
    [SerializeField] private GameObject[] entityPrefabs;
    [SerializeField] private int maxEntities = 1000;
    [SerializeField] private float spawnRadius = 200f;

    private List<DistanceBasedUpdater> activeEntities = new List<DistanceBasedUpdater>();

    void Start()
    {
        // Spawn entities with distance-based updaters
        for (int i = 0; i < maxEntities; i++)
        {
            Vector3 spawnPos = Random.insideUnitSphere * spawnRadius;
            GameObject entity = Instantiate(entityPrefabs[Random.Range(0, entityPrefabs.Length)], spawnPos, Quaternion.identity);

            // Ensure it has a distance-based updater
            DistanceBasedUpdater updater = entity.GetComponent<DistanceBasedUpdater>();
            if (updater == null)
            {
                updater = entity.AddComponent<DistanceBasedUpdater>();
            }

            activeEntities.Add(updater);
        }
    }

    void Update()
    {
        // Monitor performance and adjust entity counts
        float currentFPS = 1f / Time.deltaTime;
        if (currentFPS < 30f && activeEntities.Count > maxEntities * 0.5f)
        {
            // Too many entities, reduce count
            int removeCount = Mathf.FloorToInt(activeEntities.Count * 0.1f);
            for (int i = 0; i < removeCount; i++)
            {
                int randomIndex = Random.Range(0, activeEntities.Count);
                Destroy(activeEntities[randomIndex].gameObject);
                activeEntities.RemoveAt(randomIndex);
            }
        }
    }
}
```

### **LOD System Integration**

#### **Multi-Level Detail with Distance**
```csharp
public class LODDistanceUpdater : DistanceBasedUpdater
{
    [SerializeField] private GameObject[] lodModels; // LOD0, LOD1, LOD2, etc.
    [SerializeField] private float[] lodDistances = { 20f, 50f, 100f };

    private int currentLOD = 0;

    protected override void PerformUpdate()
    {
        int newLOD = CalculateLODLevel();
        if (newLOD != currentLOD)
        {
            SwitchLODLevel(newLOD);
            currentLOD = newLOD;
        }
    }

    private int CalculateLODLevel()
    {
        int tier = GetCurrentTier();

        // Map tiers to LOD levels
        if (tier <= 1) return 0; // High detail
        if (tier <= 2) return 1; // Medium detail
        if (tier <= 3) return 2; // Low detail
        return 3; // Very low detail or culled
    }

    private void SwitchLODLevel(int lodLevel)
    {
        for (int i = 0; i < lodModels.Length; i++)
        {
            if (lodModels[i] != null)
            {
                lodModels[i].SetActive(i == lodLevel);
            }
        }
    }
}
```

---

## 🎯 **Best Practices**

### **Configuration Guidelines**
- **Start Conservative**: Use smaller tier distances initially
- **Test Extensively**: Verify performance scaling works as expected
- **Monitor Performance**: Regularly check CPU usage and frame rates
- **Balance Quality**: Adjust tiers based on visual and gameplay impact
- **Document Settings**: Keep records of optimal configurations

### **Implementation Best Practices**
- **Inherit Properly**: Always call base.Start() and implement PerformUpdate()
- **Handle Edge Cases**: Account for entities without player references
- **Optimize Calculations**: Cache distance calculations when possible
- **Use Appropriate Tiers**: Different entities need different scaling strategies
- **Test Integration**: Ensure CombatZoneManager integration works correctly

### **Performance Best Practices**
- **Profile Regularly**: Use Unity Profiler to verify performance improvements
- **Monitor Distribution**: Check entity distribution across tiers
- **Adjust Dynamically**: Consider runtime tier adjustments based on load
- **Balance Quality**: Don't sacrifice too much visual quality for performance
- **Scale Gradually**: Make incremental adjustments and test impact

---

## 📈 **Scaling Strategies**

### **Progressive Implementation**

#### **Phase 1: Basic Distance Scaling**
- Implement basic 5-tier distance system
- Apply to high-frequency update entities (AI, animations)
- Monitor initial performance improvements

#### **Phase 2: Advanced Optimization**
- Add CombatZoneManager integration
- Implement entity-specific tier configurations
- Add runtime performance monitoring

#### **Phase 3: Dynamic Scaling**
- Implement adaptive tier adjustments
- Add machine learning-based optimization
- Create entity-specific performance profiles

### **Entity Classification**

#### **Update Frequency Categories**
```csharp
public enum EntityUpdateCategory
{
    Critical,           // Always full performance (player, active combat)
    High,              // Combat-aware scaling (NPCs, enemies)
    Medium,            // Distance-based scaling (creatures, interactive objects)
    Low,               // Minimal updates (props, decorations)
    Static             // No updates (purely visual objects)
}
```

#### **Category-Based Configuration**
```csharp
public void ConfigureForCategory(EntityUpdateCategory category)
{
    switch (category)
    {
        case EntityUpdateCategory.Critical:
            distanceTiers = new float[] { float.MaxValue }; // Always full performance
            updateFrequencies = new float[] { 60f };
            break;

        case EntityUpdateCategory.High:
            distanceTiers = new float[] { 25f, 50f, 100f, 200f };
            updateFrequencies = new float[] { 60f, 30f, 15f, 5f, 1f };
            break;

        case EntityUpdateCategory.Medium:
            distanceTiers = new float[] { 15f, 35f, 75f, 150f };
            updateFrequencies = new float[] { 30f, 15f, 5f, 1f, 0.2f };
            break;

        case EntityUpdateCategory.Low:
            distanceTiers = new float[] { 10f, 25f, 50f, 100f };
            updateFrequencies = new float[] { 10f, 5f, 2f, 0.5f, 0.1f };
            break;

        case EntityUpdateCategory.Static:
            enabled = false; // Completely disable updates
            break;
    }
}
```

---

*This documentation covers the complete DistanceBasedUpdater system for performance scaling and CCU optimization in NightBlade. For the latest updates and additional features, check the official repository.*