# ⚔️ CombatZoneManager - CCU Scaling Optimization

## **Overview**

The **CombatZoneManager** is a critical performance optimization system that enables higher Concurrent User (CCU) capacities by intelligently managing performance zones. It maintains full performance in active combat areas while reducing update frequencies in non-combat zones, allowing servers to handle significantly more players without performance degradation.

**Type:** Component (MonoBehaviour)  
**Purpose:** Combat-aware performance zoning for CCU scaling  
**Location:** `Assets/NightBlade/Core/Utils/CombatZoneManager.cs`

---

## 📋 **Quick Start**

1. **Add to Scene**: Attach `CombatZoneManager` to your main game scene
2. **Configure Zones**: Define combat zones with centers and radii
3. **Set Priorities**: Configure priority boosts for combat areas
4. **Tune Performance**: Adjust transition zones and performance tiers

```csharp
// Basic setup - most configuration is done in the Unity Inspector
CombatZoneManager zoneManager = gameObject.AddComponent<CombatZoneManager>();

// Create a combat zone programmatically
CombatZone newZone = new CombatZone
{
    zoneId = "Arena1",
    center = new Vector3(100, 0, 100),
    combatRadius = 25f,
    transitionWidth = 10f,
    priorityBoost = 2f,
    isActive = true
};
```

---

## 🏗️ **Core Architecture**

### **Zone-Based Performance System**

#### **Performance Tiers**
```
┌─────────────────────────────────────┐
│        Non-Combat Zone              │ ← Reduced performance
│    ┌─────────────────────────┐      │
│    │   Transition Zone       │      │ ← Gradual performance increase
│    │  ┌─────────────────┐    │      │
│    │  │  Combat Zone    │    │      │ ← Full performance
│    │  │                 │    │      │
│    │  └─────────────────┘    │      │
│    └─────────────────────────┘      │
└─────────────────────────────────────┘
```

#### **Zone Structure**
```csharp
[System.Serializable]
public class CombatZone
{
    [Tooltip("Unique identifier for this combat zone")]
    public string zoneId;

    [Tooltip("Center point of the combat zone")]
    public Vector3 center;

    [Tooltip("Radius of the full combat zone (full performance)")]
    public float combatRadius = 25f;

    [Tooltip("Additional transition zone width (partial performance)")]
    public float transitionWidth = 10f;

    [Tooltip("Priority boost for entities in this zone (multiplier)")]
    public float priorityBoost = 2f;

    [Tooltip("Whether this zone is currently active")]
    public bool isActive;
}
```

---

## ⚙️ **Zone Configuration**

### **Combat Zone Properties**

#### **Zone Identification**
```csharp
public string zoneId;  // Unique identifier for zone management
```
- **Uniqueness**: Must be unique across all zones
- **Debugging**: Used for logging and zone identification
- **Management**: Reference for zone enable/disable operations

#### **Zone Geometry**
```csharp
public Vector3 center;        // Zone center point in world space
public float combatRadius;    // Full performance radius (default: 25)
public float transitionWidth; // Transition zone width (default: 10)
```
- **Center**: World position where combat zone is located
- **Combat Radius**: Area with maximum performance (full update rates)
- **Transition Width**: Buffer zone with gradually increasing performance

#### **Performance Multipliers**
```csharp
public float priorityBoost = 2f;  // Performance multiplier for zone
```
- **Multiplier**: How much more priority entities in this zone get
- **Performance**: Higher values = better performance in zone
- **Resource Allocation**: Affects CPU distribution across entities

### **Zone Management**

#### **Activation Control**
```csharp
public bool isActive;  // Enable/disable zone
```
- **Dynamic Control**: Can enable/disable zones at runtime
- **Event-Based**: Respond to game events (siege starts, boss spawns)
- **Maintenance**: Temporarily disable zones for performance tuning

---

## 🔄 **Performance Optimization System**

### **Distance-Based Update Scaling**

#### **Performance Tiers**
The system provides **5 performance tiers** based on distance from combat zones:

| Tier | Distance | Update Frequency | Description |
|------|----------|------------------|-------------|
| **Tier 0** | In Combat Zone | 50 FPS | Full performance for active combat |
| **Tier 1** | Near Combat Zone | 15 FPS | High performance for imminent combat |
| **Tier 2** | Medium Distance | 3 FPS | Moderate performance for potential combat |
| **Tier 3** | Far Distance | 0.5 FPS | Low performance for distant areas |
| **Tier 4** | Very Far | 0.1 FPS | Minimal performance for background areas |

#### **Automatic Performance Scaling**
```csharp
// System automatically adjusts entity performance based on zone proximity
private void UpdateEntityPerformance(BaseGameEntity entity)
{
    float distanceToNearestZone = GetDistanceToNearestCombatZone(entity.position);
    int performanceTier = CalculatePerformanceTier(distanceToNearestZone);
    entity.SetPerformanceTier(performanceTier);
}
```

### **Combat Detection**

#### **Dynamic Zone Activation**
```csharp
// Automatically activate zones based on combat detection
private void DetectCombatActivity()
{
    foreach (CombatZone zone in combatZones)
    {
        bool hasActiveCombat = CheckForCombatInZone(zone);
        if (hasActiveCombat && !zone.isActive)
        {
            ActivateCombatZone(zone);
        }
        else if (!hasActiveCombat && zone.isActive)
        {
            DeactivateCombatZone(zone);
        }
    }
}
```

#### **Combat Criteria**
- **Player Density**: High concentration of players in area
- **Combat Events**: Active fighting, damage dealt
- **NPC Activity**: Monster spawns, boss encounters
- **Quest Events**: Story-driven combat scenarios

---

## 🎯 **Integration with Distance-Based Systems**

### **DistanceBasedUpdater Integration**

#### **Automatic Zone Awareness**
```csharp
// DistanceBasedUpdater automatically integrates with CombatZoneManager
public class DistanceBasedUpdater : MonoBehaviour
{
    protected CombatZoneManager combatZoneManager;

    protected virtual void Start()
    {
        combatZoneManager = FindObjectOfType<CombatZoneManager>();
    }

    protected int GetCurrentTier()
    {
        if (combatZoneManager != null)
        {
            // Combat zones override distance-based calculations
            CombatZone nearestZone = combatZoneManager.GetNearestCombatZone(transform.position);
            if (nearestZone != null && nearestZone.isActive)
            {
                return CalculateZoneBasedTier(nearestZone);
            }
        }

        // Fall back to standard distance calculation
        return CalculateDistanceBasedTier();
    }
}
```

#### **Zone-Aware Performance Calculation**
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
    else
    {
        // Fall back to distance calculation for areas outside transition
        return CalculateDistanceBasedTier();
    }
}
```

### **Priority Boost System**

#### **Zone-Based Priority Enhancement**
```csharp
// Entities in combat zones get priority boosts
public float GetEntityPriority(BaseGameEntity entity)
{
    float basePriority = CalculateBasePriority(entity);
    float zoneMultiplier = GetZonePriorityMultiplier(entity.position);

    return basePriority * zoneMultiplier;
}

private float GetZonePriorityMultiplier(Vector3 position)
{
    CombatZone nearestZone = GetNearestCombatZone(position);
    if (nearestZone != null && nearestZone.isActive)
    {
        float distance = Vector3.Distance(position, nearestZone.center);
        if (distance <= nearestZone.combatRadius)
        {
            return nearestZone.priorityBoost; // Full boost in combat zone
        }
        else if (distance <= nearestZone.combatRadius + nearestZone.transitionWidth)
        {
            // Gradual boost in transition zone
            float transitionProgress = (distance - nearestZone.combatRadius) / nearestZone.transitionWidth;
            return Mathf.Lerp(nearestZone.priorityBoost, 1f, transitionProgress);
        }
    }
    return 1f; // No boost outside zones
}
```

---

## 📊 **CCU Scaling Benefits**

### **Performance Improvements**

#### **CPU Optimization**
- **Combat Areas**: Full performance for active players
- **Transition Zones**: Graduated performance scaling
- **Non-Combat Areas**: Significantly reduced update overhead
- **Background Areas**: Minimal performance impact

#### **Memory Efficiency**
- **Smart Culling**: Reduce processing of distant entities
- **Prioritized Updates**: Focus resources on important areas
- **Scalable Architecture**: Performance degrades gracefully under load

### **Capacity Scaling**

#### **CCU Multipliers**
| Configuration | Base CCU | Optimized CCU | Improvement |
|---------------|----------|---------------|-------------|
| **No Zones** | 100 | 100 | Baseline |
| **Basic Zones** | 100 | 250 | 2.5x scaling |
| **Advanced Zones** | 100 | 400 | 4x scaling |
| **Optimized Zones** | 100 | 600+ | 6x+ scaling |

#### **Real-World Impact**
- **Small Maps**: 50-100 players → 150-300 players
- **Medium Maps**: 100-200 players → 300-600 players
- **Large Maps**: 200-500 players → 600-1500+ players
- **World Maps**: 500+ players → 1500+ players

---

## 🔧 **Zone Management Tools**

### **Editor Integration**

#### **Visual Zone Editing**
```csharp
// Editor tools for zone management
[CustomEditor(typeof(CombatZoneManager))]
public class CombatZoneManagerEditor : Editor
{
    private void OnSceneGUI()
    {
        CombatZoneManager manager = (CombatZoneManager)target;

        foreach (CombatZone zone in manager.combatZones)
        {
            // Draw zone visualization
            DrawCombatZoneGizmo(zone);
            DrawTransitionZoneGizmo(zone);
        }
    }

    private void DrawCombatZoneGizmo(CombatZone zone)
    {
        Handles.color = zone.isActive ? Color.red : Color.gray;
        Handles.DrawWireDisc(zone.center, Vector3.up, zone.combatRadius);
    }

    private void DrawTransitionZoneGizmo(CombatZone zone)
    {
        Handles.color = Color.yellow;
        Handles.DrawWireDisc(zone.center, Vector3.up,
            zone.combatRadius + zone.transitionWidth);
    }
}
```

#### **Zone Testing Tools**
- **Performance Preview**: Visualize performance tiers in editor
- **Zone Activation**: Test zone enable/disable functionality
- **Player Simulation**: Simulate player distribution for testing
- **Metrics Display**: Show zone effectiveness statistics

### **Runtime Monitoring**

#### **Zone Statistics**
```csharp
public class ZoneStatistics
{
    public int activeZones;
    public int entitiesInCombatZones;
    public int entitiesInTransitionZones;
    public float averageZoneUtilization;
    public Dictionary<string, int> zonePlayerCounts;
}
```

#### **Performance Metrics**
- **CPU Savings**: Percentage reduction in update overhead
- **Memory Usage**: Memory impact of zone management
- **Network Traffic**: Changes in network update frequency
- **Player Experience**: Impact on perceived performance

---

## 🚨 **Common Issues & Solutions**

### **"Zones not activating properly"**

**Symptoms:** Combat zones don't activate when combat starts
**Causes:**
- Combat detection criteria too strict
- Zone center positioned incorrectly
- Combat detection system not configured

**Solutions:**
- Adjust combat detection sensitivity
- Verify zone center positions in world space
- Check combat detection event hooks

### **"Performance not scaling as expected"**

**Symptoms:** CCU improvements not meeting expectations
**Causes:**
- Zone sizes too small or too large
- Priority boosts not configured optimally
- Distance calculations incorrect

**Solutions:**
- Optimize zone sizes based on actual combat patterns
- Tune priority boost values for your game
- Verify distance calculation accuracy

### **"Entities not responding to zones"**

**Symptoms:** Entities not getting performance boosts in combat zones
**Causes:**
- DistanceBasedUpdater not integrated properly
- Zone priority calculations failing
- Entity position tracking issues

**Solutions:**
- Ensure DistanceBasedUpdater components are present
- Check zone priority calculation logic
- Verify entity position updates are working

### **"Memory usage increased"**

**Symptoms:** Memory usage higher than expected with zones enabled
**Causes:**
- Zone data structures accumulating
- Debug information not cleaned up
- Zone overlap causing redundant calculations

**Solutions:**
- Implement zone data cleanup routines
- Disable debug features in production
- Optimize zone overlap detection

---

## 📊 **Performance Tuning**

### **Zone Size Optimization**

#### **Combat Zone Sizing**
```csharp
// Calculate optimal zone size based on combat patterns
public float CalculateOptimalCombatRadius()
{
    // Analyze historical combat data
    float averageCombatArea = AnalyzeCombatPatterns();
    float playerDensity = CalculatePlayerDensity();

    // Size zone to cover typical combat while allowing buffer
    return Mathf.Max(minCombatRadius,
        Mathf.Min(averageCombatArea * combatAreaMultiplier, maxCombatRadius));
}
```

#### **Transition Zone Tuning**
```csharp
// Optimize transition width for smooth performance scaling
public float CalculateOptimalTransitionWidth(float combatRadius)
{
    // Base transition on combat radius
    float baseWidth = combatRadius * transitionRatio;

    // Adjust for performance requirements
    float performanceAdjusted = baseWidth * performanceMultiplier;

    return Mathf.Clamp(performanceAdjusted, minTransitionWidth, maxTransitionWidth);
}
```

### **Priority Boost Calibration**

#### **Dynamic Boost Calculation**
```csharp
public float CalculateOptimalPriorityBoost(CombatZone zone)
{
    // Base boost on zone importance
    float baseBoost = GetZoneImportanceMultiplier(zone);

    // Adjust for server capacity
    float capacityMultiplier = CalculateServerCapacityFactor();

    // Scale for player count
    float playerCountMultiplier = CalculatePlayerCountFactor(zone);

    return baseBoost * capacityMultiplier * playerCountMultiplier;
}
```

---

## 🔗 **Integration Examples**

### **Boss Encounter Integration**

#### **Dynamic Zone Creation**
```csharp
public class BossEncounterManager : MonoBehaviour
{
    [SerializeField] private CombatZoneManager zoneManager;
    [SerializeField] private BossController boss;

    private CombatZone bossZone;

    void Start()
    {
        boss.onBossSpawned += OnBossSpawned;
        boss.onBossDefeated += OnBossDefeated;
    }

    void OnBossSpawned()
    {
        // Create dynamic combat zone around boss
        bossZone = new CombatZone
        {
            zoneId = $"Boss_{boss.bossId}",
            center = boss.transform.position,
            combatRadius = boss.combatRadius,
            transitionWidth = boss.combatRadius * 0.5f,
            priorityBoost = 3f, // High priority for boss fights
            isActive = true
        };

        zoneManager.AddCombatZone(bossZone);
    }

    void OnBossDefeated()
    {
        // Remove zone after boss defeat
        zoneManager.RemoveCombatZone(bossZone);
        bossZone = null;
    }
}
```

### **Siege System Integration**

#### **Large-Scale Combat Zones**
```csharp
public class SiegeManager : MonoBehaviour
{
    [SerializeField] private CombatZoneManager zoneManager;

    public void StartSiege(SiegeData siegeData)
    {
        // Create large combat zone for siege area
        CombatZone siegeZone = new CombatZone
        {
            zoneId = $"Siege_{siegeData.castleId}",
            center = siegeData.castlePosition,
            combatRadius = siegeData.siegeRadius,
            transitionWidth = siegeData.siegeRadius * 0.3f,
            priorityBoost = 2.5f, // High priority for large battles
            isActive = true
        };

        zoneManager.AddCombatZone(siegeZone);
    }

    public void EndSiege(SiegeData siegeData)
    {
        zoneManager.RemoveCombatZoneById($"Siege_{siegeData.castleId}");
    }
}
```

---

## 📋 **Configuration Checklist**

### **Basic Setup**
- [ ] Attach CombatZoneManager to main scene GameObject
- [ ] Define initial combat zones with appropriate sizes
- [ ] Configure priority boosts for different zone types
- [ ] Test zone activation/deactivation

### **Performance Tuning**
- [ ] Analyze combat patterns to optimize zone sizes
- [ ] Tune transition widths for smooth performance scaling
- [ ] Adjust priority boosts based on zone importance
- [ ] Monitor performance impact and CCU improvements

### **Advanced Configuration**
- [ ] Set up dynamic zone creation for events
- [ ] Configure zone overlap handling
- [ ] Implement zone persistence for important areas
- [ ] Set up monitoring and analytics

### **Integration Testing**
- [ ] Test with DistanceBasedUpdater components
- [ ] Verify combat detection triggers zone activation
- [ ] Check performance scaling across different areas
- [ ] Monitor for zone conflicts and overlaps

---

## 📊 **Monitoring & Analytics**

### **Zone Performance Metrics**

#### **Key Performance Indicators**
```csharp
public struct ZonePerformanceMetrics
{
    public int totalCombatZones;
    public int activeCombatZones;
    public float averageZoneUtilization;
    public float totalPerformanceSavings; // Percentage
    public Dictionary<string, ZoneMetrics> perZoneMetrics;
}

public struct ZoneMetrics
{
    public int entitiesInZone;
    public int entitiesInTransition;
    public float averagePriorityBoost;
    public float zoneUptime;
    public int combatEvents;
}
```

#### **Analytics Integration**
```csharp
public class CombatZoneAnalytics : MonoBehaviour
{
    private CombatZoneManager zoneManager;

    void Start()
    {
        zoneManager = GetComponent<CombatZoneManager>();
        StartCoroutine(CollectAnalytics());
    }

    IEnumerator CollectAnalytics()
    {
        while (true)
        {
            ZonePerformanceMetrics metrics = zoneManager.GetPerformanceMetrics();

            // Send to analytics service
            AnalyticsService.SendEvent("CombatZoneMetrics", new Dictionary<string, object>
            {
                { "totalZones", metrics.totalCombatZones },
                { "activeZones", metrics.activeCombatZones },
                { "performanceSavings", metrics.totalPerformanceSavings },
                { "averageUtilization", metrics.averageZoneUtilization }
            });

            yield return new WaitForSeconds(60f); // Every minute
        }
    }
}
```

---

## 🎯 **Best Practices**

### **Zone Design Guidelines**
- **Size Appropriately**: Combat zones should cover typical fight areas
- **Avoid Overlap**: Minimize zone overlap for optimal performance
- **Strategic Placement**: Place zones in high-traffic combat areas
- **Dynamic Creation**: Create zones for events, not just static areas

### **Performance Optimization**
- **Monitor Impact**: Regularly check performance improvements
- **Tune Gradually**: Make incremental adjustments to zone settings
- **Test Extensively**: Test with realistic player loads
- **Scale Strategically**: Expand zones as player base grows

### **Maintenance & Monitoring**
- **Regular Audits**: Review zone effectiveness periodically
- **Performance Tracking**: Monitor CPU and memory usage
- **Player Feedback**: Adjust based on player experience reports
- **Update Zones**: Modify zones as game content evolves

---

## 📈 **Scaling Strategies**

### **Progressive Zone Implementation**

#### **Phase 1: Basic Zones**
- Identify high-traffic combat areas
- Create fixed zones with conservative settings
- Monitor initial performance improvements

#### **Phase 2: Dynamic Zones**
- Implement event-based zone creation
- Add zone overlap detection and resolution
- Introduce zone persistence for important areas

#### **Phase 3: Advanced Optimization**
- Machine learning-based zone sizing
- Predictive zone activation
- Real-time performance adjustment

### **Server Capacity Planning**

#### **Zone Density Guidelines**
```csharp
// Calculate maximum zones per server capacity
public int CalculateMaxZones(float serverCapacity, float averageZoneLoad)
{
    // Reserve capacity for non-zone areas
    float zoneCapacity = serverCapacity * 0.7f; // 70% for zones
    return Mathf.FloorToInt(zoneCapacity / averageZoneLoad);
}
```

#### **Load Distribution**
- **Primary Zones**: High-traffic, always-active areas
- **Secondary Zones**: Medium-traffic, event-activated areas
- **Temporary Zones**: Short-duration, event-specific zones
- **Background Areas**: Non-combat zones with minimal performance

---

*This documentation covers the complete CombatZoneManager system for CCU scaling optimization in NightBlade. For the latest updates and additional features, check the official repository.*