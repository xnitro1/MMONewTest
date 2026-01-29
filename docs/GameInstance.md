# 🎮 GameInstance - NightBlade Core Configuration

## **Overview**

The **GameInstance** component is the central hub of NightBlade's game configuration system. It manages all core gameplay systems, default data, and runtime settings for your MMO experience.

**Version:** NightBlade v1.95r3 + Revision 4 Alpha  
**Purpose:** Unified configuration and management of all game systems  
**Location:** `Assets/NightBlade/Core/Gameplay/GameInstance.cs`

---

## 📋 **Quick Start**

1. **Add to Scene**: Attach `GameInstance` component to an empty GameObject
2. **Configure Systems**: Set up your core gameplay managers in the "Gameplay Systems" section
3. **Set Defaults**: Configure character creation and game rules
4. **Enable Monitoring** (Optional): Configure performance monitoring for development
5. **Performance Optimizations**: Automatic initialization of optimization systems
6. **Test**: Use the built-in editor testing tools

```csharp
// Basic setup - most configuration is done in the Unity Inspector
GameObject gameInstanceObj = new GameObject("GameInstance");
GameInstance gameInstance = gameInstanceObj.AddComponent<GameInstance>();

// Access singleton at runtime
GameInstance.Singleton.DoSomething();
```

---

## 🎯 **Configuration Sections**

### **🎮 Gameplay Systems**

Core managers that handle fundamental game functionality. These components are required for basic gameplay.

| Property | Type | Description |
|----------|------|-------------|
| **Message Manager** | `BaseMessageManager` | Handles in-game messaging and notifications |
| **Save System** | `BaseGameSaveSystem` | Manages player progress and data persistence |
| **Gameplay Rule** | `BaseGameplayRule` | Defines core game rules and mechanics |
| **Inventory Manager** | `BaseInventoryManager` | Controls item storage and management |
| **Day/Night System** | `BaseDayNightTimeUpdater` | Manages time-of-day cycles |
| **GM Commands** | `BaseGMCommands` | Game master/administration commands |
| **Equipment Bones** | `BaseEquipmentModelBonesSetupManager` | Character equipment positioning |
| **Network Settings** | `NetworkSetting` | Multiplayer networking configuration |

**Best Practice:** Always assign these core systems for proper game functionality.

---

### **🎯 Gameplay Objects**

Prefabs and objects used throughout gameplay. Configure visual and interactive elements.

| Property | Type | Description |
|----------|------|-------------|
| **EXP Drop Item** | `BaseItem` | Visual representation for experience points |
| **Gold Drop Item** | `BaseItem` | Visual representation for gold currency |
| **Currency Items** | `CurrencyItemPair[]` | Custom currency drop representations |

---

### **✨ Gameplay Effects**

Settings for visual effects, combat feedback, and environmental interactions.

**Note:** This section contains advanced visual and gameplay effect configurations that affect how players perceive game events.

---

### **💾 Gameplay Database & Default Data**

Core game data and default values that define your game's fundamental behavior.

| Property | Type | Description |
|----------|------|-------------|
| **Default Character** | `CharacterData` | Base character template |
| **Monster Levels** | Various | Default monster stats and behaviors |
| **Item Database** | Various | Default item properties and effects |
| **Quest System** | Various | Default quest configurations |
| **Building System** | Various | Default building properties |

**Important:** These settings establish the baseline for all game content.

---

### **🏷️ Object Tags & Layers**

Unity-specific configuration for physics, rendering, and gameplay categorization.

**Purpose:** Ensures proper collision detection, rendering, and gameplay logic by configuring Unity's tagging system.

---

### **⚙️ Gameplay Configs - Generic**

Core gameplay settings that affect all players and game mechanics globally.

| Setting | Range | Description |
|---------|-------|-------------|
| **Player Movement** | Various | Movement speed, jump height, etc. |
| **Combat System** | Various | Attack ranges, damage calculations |
| **Interaction Distance** | Float | How close players need to be for interactions |
| **Loot Settings** | Various | Drop rates, loot table configurations |
| **Experience Rates** | Float | XP multipliers for leveling |
| **Cooldowns** | Various | Global ability and action cooldowns |

---

### **🎒 Items, Inventory & Storage**

Configuration for item management, inventory systems, and storage mechanics.

| Property | Type | Description |
|----------|------|-------------|
| **Inventory Size** | Int | Default inventory slots per character |
| **Stack Limits** | Int | Maximum items per stack |
| **Storage Fees** | Int | Costs for using storage systems |
| **Item Durability** | Bool | Whether items degrade over time |
| **Trading Settings** | Various | Auction house and trading configurations |

---

### **🐉 Summon Systems**

Settings for monster summoning, pet systems, and NPC companion mechanics.

| Property | Type | Description |
|----------|------|-------------|
| **Summon Limits** | Int | Maximum active summons per player |
| **Summon Duration** | Float | How long summons remain active |
| **Pet System** | Bool | Enable/disable pet companions |
| **Monster AI** | Various | Behavior patterns for summoned creatures |

---

### **👤 New Character Settings**

Configure how new characters are created and what they start with.

#### **Primary Configuration:**
- **New Character Setting**: ScriptableObject containing complete character template
- **Fallback Options**: When no template is set, use individual properties below

#### **Fallback Properties** (when NewCharacterSetting is null):
| Property | Type | Description |
|----------|------|-------------|
| **Start Gold** | Int | Initial gold amount for new characters |
| **Start Items** | `ItemAmount[]` | Items automatically added to inventory |

#### **Testing Configuration:**
- **Testing Character Setting**: Editor-only template for testing scenarios

**Best Practice:** Use `NewCharacterSetting` ScriptableObject for consistent character creation across your game.

---

### **🖥️ Server Settings**

Server-side configuration for multiplayer functionality.

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| **Update Animation At Server** | Bool | `true` | Whether server processes animation updates |

**Note:** These settings affect server performance and network synchronization.

---

### **🎮 Player Configs**

Settings that control player account and character management.

| Property | Type | Range | Description |
|----------|------|-------|-------------|
| **Min Character Name Length** | Int | 1-32 | Minimum characters in character names |
| **Max Character Name Length** | Int | 1-32 | Maximum characters in character names |
| **Max Character Saves** | Byte | 0-255 | Maximum characters per account (0 = unlimited) |

**Validation:** Names are automatically validated against these limits during character creation.

---

### **📱 Platform Configs**

Platform-specific optimizations and settings.

| Property | Type | Description |
|----------|------|-------------|
| **Server Target Frame Rate** | Int | Target FPS for server operations |

---

### **🎯 Editor Testing**

Tools for testing gameplay in the Unity Editor.

| Property | Type | Description |
|----------|------|-------------|
| **Test In Editor Mode** | Enum | Testing mode for editor play |
| **Offline Network Manager** | AssetReference | Network manager for single-player testing |
| **Enable Performance Monitor** | Boolean | Add performance monitoring component |
| **Show Performance GUI** | Boolean | Display performance overlay in game view |

**Modes:**
- **Standalone**: Standard desktop testing
- **Mobile**: Simulate mobile device constraints
- **Mobile With Key Inputs**: Mobile with keyboard input support
- **Console**: Simulate console gaming experience

**Performance Monitoring:**
- **Enable Performance Monitor**: Adds the `PerformanceMonitor` component for real-time performance tracking

### **🔧 Automatic Performance Optimizations**

GameInstance **automatically initializes** performance optimization systems:

| System | Purpose | Status |
|--------|---------|---------|
| **Network String Cache** | Bandwidth optimization for MMO communications | ✅ Auto-initialized |
| **Coroutine Pool** | GC-free UI animations and effects | ✅ Auto-initialized |
| **Smart Asset Manager** | Automatic memory management | ✅ Auto-initialized |
| **UI Object Pooling** | Combat text and UI element reuse | ✅ Auto-initialized (requires TMP) |
| **Performance Monitor** | Real-time performance tracking | ⚙️ Optional (enable in inspector) |

#### **Initialization Sequence**
1. **Deferred Start**: Optimizations initialize after scene loading (0.1s delay)
2. **TMP Check**: UI pooling waits for TextMesh Pro resources
3. **Safe Fallbacks**: Systems work even if some components fail
4. **Real-time Monitoring**: PerformanceMonitor tracks all systems

#### **TMP Resources Required**
For UI pooling to work, import TextMesh Pro Essential Resources:
```
Window > TextMesh Pro > Import TMP Essential Resources
```

Without TMP resources:
- UI pooling gracefully disables
- Combat text uses legacy instantiation
- PerformanceMonitor shows clear status
- No errors or dialog spam

#### **UI Pool Initialization (v2.x Improvements)**
Enhanced UI pool initialization with improved reliability:

- **Template Persistence**: Templates are automatically parented to prevent scene-change destruction
- **Pool Clearing**: Previous instances are properly cleaned up to prevent contamination
- **Instance Isolation**: Each server instance maintains its own pool state
- **Debug Logging**: Comprehensive logging for troubleshooting pool issues

```csharp
// Automatic initialization sequence:
1. ClearAllPools() - Clean previous state
2. CreateDamageNumberTemplate() - Build templates
3. RegisterTemplate() - Parent and register (scene-persistent)
4. PreWarmPool() - Create initial objects
5. Success: "Pre-warmed 20 damage number objects. Total pooled: 20"
```
- **Show Performance GUI**: Displays performance metrics overlay (recommended for development/testing only)

---

## 🔧 **Runtime Usage**

### **Accessing GameInstance**

```csharp
// Get singleton instance
GameInstance instance = GameInstance.Singleton;

// Check if properly initialized
if (instance != null)
{
    // Access configured systems
    var inventoryManager = instance.GetComponent<BaseInventoryManager>();
    var saveSystem = instance.GetComponent<BaseGameSaveSystem>();
}
```

### **System Integration**

```csharp
// Example: Custom system integration
public class MyCustomSystem : MonoBehaviour
{
    void Start()
    {
        if (GameInstance.Singleton != null)
        {
            // Register with GameInstance
            GameInstance.Singleton.RegisterCustomSystem(this);
        }
    }
}
```

---

## 🎮 **Testing & Validation**

### **Built-in Editor Tools**

The GameInstance inspector provides several testing and validation tools:

1. **Configuration Validation**: Checks for missing required components
2. **Report Generation**: Creates detailed configuration reports
3. **Runtime Status**: Shows active GameInstance in play mode
4. **Performance Monitoring**: Integrated performance tracking and diagnostics

### **Manual Testing Steps**

1. **Basic Setup Test**:
   - Attach GameInstance to GameObject
   - Assign core systems (MessageManager, SaveSystem, etc.)
   - Enter play mode and check console for errors

2. **Character Creation Test**:
   - Configure New Character settings
   - Test character creation flow
   - Verify starting items and stats

3. **Multiplayer Test**:
   - Configure network settings
   - Test client-server communication
   - Validate synchronization

4. **Performance Monitoring Test**:
   - Enable "Performance Monitor" in GameInstance
   - Set "Show Performance GUI" for visual feedback
   - Enter play mode and observe performance metrics
   - Use PerformanceMonitor editor tools for diagnostics

---

## 🚨 **Common Issues & Solutions**

### **"GameInstance Singleton is null"**

**Cause:** GameInstance component not present in scene or not properly initialized.

**Solution:**
1. Add GameInstance component to a GameObject
2. Ensure GameInstance has `DontDestroyOnLoad` behavior
3. Check script execution order

### **Missing Core Systems**

**Cause:** Required gameplay systems not assigned.

**Solution:**
1. Assign all core systems in "Gameplay Systems" section
2. Create system prefabs if they don't exist
3. Check for missing script references

### **Character Creation Issues**

**Cause:** NewCharacterSetting configuration problems.

**Solution:**
1. Either assign NewCharacterSetting ScriptableObject
2. Or configure startGold and startItems as fallback
3. Test character creation in editor

### **Performance Issues**

**Cause:** Suboptimal server settings or too many systems active.

**Solution:**
1. Adjust Server Target Frame Rate appropriately
2. Disable unused systems in gameplay configs
3. Optimize inventory and item management settings

---

## 📊 **Performance Considerations**

### **Server Optimization**

- **Target Frame Rate**: Balance between responsiveness and server load
- **System Activation**: Only enable systems you actually use
- **Network Settings**: Optimize for your expected player count

### **Memory Management**

- **Item Caching**: Configure inventory limits appropriately
- **Asset Loading**: Use addressable assets for better memory control
- **System Cleanup**: Regularly clean up unused game objects

### **Scalability**

- **Database Size**: Monitor growth of save data
- **Network Traffic**: Optimize message batching
- **Player Limits**: Set appropriate character/account limits

---

## 🔗 **Related Systems**

| System | Purpose | Documentation |
|--------|---------|---------------|
| **MapInstanceManager** | Multi-instance scaling | `Multiple_Map_Instances_CCU.md` |
| **InstanceLoadBalancer** | Player distribution | `Multiple_Map_Instances_CCU.md` |
| **CrossInstanceMessenger** | Inter-instance communication | `Multiple_Map_Instances_CCU.md` |
| **CentralNetworkManager** | Server coordination | Core networking docs |
| **CharacterSystem** | Player character management | Character system docs |

---

## 📚 **API Reference**

### **Core Properties**

```csharp
public static GameInstance Singleton { get; protected set; }
public static IClientCashShopHandlers ClientCashShopHandlers { get; set; }
// ... additional handler properties
```

### **Key Methods**

```csharp
// Initialization
void Awake()
void Start()
void OnDestroy()

// Runtime management
void Update()
void FixedUpdate()
void LateUpdate()

// Utility methods
void ValidateConfiguration()
void GenerateConfigReport()
```

---

## 🎯 **Best Practices**

### **1. Organization**
- Use ScriptableObjects for complex configurations
- Group related settings logically
- Document custom configurations

### **2. Testing**
- Test character creation thoroughly
- Validate in both editor and build
- Monitor performance metrics

### **3. Maintenance**
- Keep backups of working configurations
- Document configuration changes
- Test after Unity updates

### **4. Scalability**
- Plan for growth in player counts
- Optimize database queries
- Monitor server resource usage

---

## 🚀 **Advanced Configuration**

### **Custom Character Templates**

Create ScriptableObject-based character templates:

```csharp
[CreateAssetMenu(fileName = "NewCharacterTemplate", menuName = "NightBlade/New Character Setting")]
public class NewCharacterSetting : ScriptableObject
{
    public string characterName;
    public int startLevel = 1;
    public int startGold = 100;
    public ItemAmount[] startItems;
    // ... additional properties
}
```

### **System Extensions**

Extend GameInstance for custom functionality:

```csharp
public partial class GameInstance
{
    // Add custom properties and methods
    public MyCustomManager CustomManager { get; private set; }

    void InitializeCustomSystems()
    {
        CustomManager = GetComponent<MyCustomManager>();
    }
}
```

---

## 📞 **Support & Resources**

- **Documentation**: [NightBlade Docs](https://github.com/denariigames/nightblade)
- **Community**: [Discord Server](https://discord.gg/nightblade)
- **Issues**: [GitHub Issues](https://github.com/denariigames/nightblade/issues)
- **Updates**: [Releases](https://github.com/denariigames/nightblade/releases)

---

## 📋 **Changelog**

### **Revision 4 Alpha**
- Multiple Map Instances support
- Enhanced CCU scaling (2-5x capacity)
- Improved load balancing
- Cross-instance messaging

### **Revision 3**
- Performance optimizations (35-55% CPU reduction)
- Enhanced security validation
- Improved memory management
- Advanced networking features

### **v1.95r3**
- Core GameInstance system
- Comprehensive configuration framework
- Unity Editor integration
- Professional tooling

---

*This documentation is for NightBlade v1.95r3 + Revision 4 Alpha. For the latest updates, check the official repository.*