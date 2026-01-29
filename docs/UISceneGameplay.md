# 🎮 UISceneGameplay - Main Gameplay UI Manager

## **Overview**

The **UISceneGameplay** component is the central hub for all gameplay-related user interface elements in NightBlade. It manages UI components for entity targeting, interactions, crafting, trading, storage, building management, and social features, providing a unified interface for player-game world interactions.

**Type:** Component (inherits from BaseUISceneGameplay)  
**Purpose:** Comprehensive gameplay UI orchestration and management  
**Location:** `Assets/NightBlade/Core/UI/Scenes/UISceneGameplay.cs`

---

## 📋 **Quick Start**

1. **Add to Scene**: Attach `UISceneGameplay` to your main gameplay scene's UI canvas
2. **Configure Target UIs**: Assign UI components for different entity types (characters, NPCs, items)
3. **Set Up Interactions**: Configure dialog, crafting, and trading interfaces
4. **Auto-Assign Components**: Use the editor's auto-assign feature for child UI components
5. **Validate Configuration**: Run validation to ensure all critical UIs are assigned

```csharp
// Basic setup - most configuration is done in the Unity Inspector
GameObject uiCanvas = GameObject.Find("UICanvas");
UISceneGameplay uiManager = uiCanvas.AddComponent<UISceneGameplay>();

// Access singleton at runtime
BaseUISceneGameplay uiInstance = BaseUISceneGameplay.Singleton;
```

---

## 🎯 **Core Architecture**

### **UI Organization Structure**

UISceneGameplay organizes UI components into logical categories:

#### **1. Target UIs** - Entity Information Display
- **Character UIs**: Display information about selected characters
- **NPC UIs**: Show NPC interaction options and information
- **Item UIs**: Display item details and pickup options
- **Building UIs**: Show building information and management options

#### **2. Interaction UIs** - Player-NPC Communication
- **Dialog Systems**: NPC conversation interfaces
- **Quest Interfaces**: Quest acceptance and reward selection
- **Tutorial Systems**: Guided player experiences

#### **3. Crafting UIs** - Item Creation & Modification
- **Refining Systems**: Item upgrade interfaces
- **Dismantling Tools**: Item breakdown and recycling
- **Enhancement Interfaces**: Socket and stat improvements

#### **4. Trading UIs** - Commerce Systems
- **Player Trading**: P2P trade interfaces
- **Vendor Systems**: NPC merchant interactions
- **Vending Interfaces**: Player shop management

#### **5. Storage UIs** - Inventory Management
- **Personal Storage**: Player inventory systems
- **Guild Storage**: Shared guild inventories
- **Building Storage**: Container and warehouse management

---

## 🎯 **Target UIs Configuration**

### **Entity Target Assignment**

| UI Component | Purpose | Critical Level |
|--------------|---------|----------------|
| **uiTargetCharacter** | Character entity information | High |
| **uiTargetNpc** | NPC interaction display | High |
| **uiTargetItemDrop** | Ground item details | Medium |
| **uiTargetItemsContainer** | Container contents | Medium |
| **uiTargetBuilding** | Building information | High |
| **uiTargetHarvestable** | Resource node details | Medium |
| **uiTargetVehicle** | Vehicle interaction | Low |

**Assignment Guidelines:**
- **High Priority**: Essential for core gameplay functionality
- **Medium Priority**: Enhances gameplay but not critical
- **Low Priority**: Nice-to-have features

---

## 💬 **Interaction UIs Setup**

### **NPC Communication Systems**

#### **Dialog Management**
```csharp
// NPC Dialog UI Configuration
uiNpcDialog = GetComponentInChildren<UINpcDialog>();
uiQuestRewardItemSelection = GetComponentInChildren<UIQuestRewardItemSelection>();
```

#### **Dialog Flow**
1. **Player Approaches NPC** → Target UI activates
2. **Interaction Triggered** → Dialog UI opens
3. **Quest Offered** → Reward selection UI appears
4. **Conversation Complete** → UIs close gracefully

### **Quest Integration**
- **Quest Acceptance**: Handled through NPC dialogs
- **Reward Selection**: Specialized UI for choosing quest rewards
- **Progress Tracking**: Integrated with character UI systems

---

## ⚒️ **Crafting UIs Configuration**

### **Item Modification Systems**

| Crafting Type | UI Component | Function |
|---------------|--------------|----------|
| **Refining** | `uiRefineItem` | Item quality upgrades |
| **Dismantling** | `uiDismantleItem` | Single item breakdown |
| **Bulk Dismantling** | `uiBulkDismantleItems` | Multiple item processing |
| **Repair** | `uiRepairItem` | Durability restoration |
| **Socket Enhancement** | `uiEnhanceSocketItem` | Gem and rune systems |

### **Crafting Workflow**

#### **Building-Based Crafting**
```csharp
// Workbench crafting setup
uiBuildingCraftItems = GetComponentInChildren<UIBuildingCraftItems>();
uiCraftingQueueItems = GetComponentInChildren<UICraftingQueueItems>();
```

#### **Crafting Process**
1. **Player Approaches Workbench** → Building target UI
2. **Opens Crafting Interface** → Recipe selection
3. **Adds to Queue** → Queue management UI
4. **Monitors Progress** → Real-time status updates

---

## 💰 **Trading UIs Setup**

### **Commerce Systems**

#### **Player-to-Player Trading**
```csharp
// Dealing system configuration
uiDealingRequest = GetComponentInChildren<UIDealingRequest>();
uiDealing = GetComponentInChildren<UIDealing>();
```

#### **Vendor Interactions**
```csharp
// Vending system setup
uiStartVending = GetComponentInChildren<UIStartVending>();
uiVending = GetComponentInChildren<UIVending>();
showVendingUiOnActivate = true; // Auto-show on activation
```

### **Trading Flow**

#### **P2P Trading**
```
Player A Requests Trade → Dealing Request UI
Player B Accepts → Dealing UI Opens
Items Exchanged → Trade Completion
```

#### **Vendor Trading**
```
Player Activates Vendor → Vending UI Auto-shows
Select Items → Purchase/Sell Interface
Transaction Complete → UI Closes
```

---

## 📦 **Storage UIs Management**

### **Multi-Level Storage Systems**

#### **Storage Hierarchy**
- **Personal**: Player's individual inventory
- **Guild**: Shared guild resources
- **Building**: Structure-based storage (chests, warehouses)
- **Special**: Campfires, crafting stations

#### **Storage Configuration**
```csharp
// Storage UI assignments
uiPlayerStorageItems = GetComponentInChildren<UIStorageItems>();
uiGuildStorageItems = uiPlayerStorageItems; // Reuse if not specified
uiBuildingStorageItems = GetComponentInChildren<UIBuildingStorageItems>();
uiBuildingCampfireItems = GetComponentInChildren<UICampfireItems>();
```

### **Storage Access Patterns**

#### **Personal Storage**
- **Always Available**: Player inventory access
- **Persistent**: Saves across sessions
- **Organized**: Category-based item management

#### **Shared Storage**
- **Permission-Based**: Guild rank requirements
- **Synchronized**: Real-time updates across players
- **Audited**: Transaction logging for security

---

## 🏗️ **Building UIs Configuration**

### **Construction & Management**

#### **Building Lifecycle**
1. **Construction Phase** → `uiConstructBuilding`
2. **Management Phase** → `uiCurrentBuilding` and variants
3. **Specialized Functions** → Door, storage, workbench UIs

#### **Building UI Assignment**
```csharp
// Building system setup
uiConstructBuilding = GetComponentInChildren<UIConstructBuilding>();
uiCurrentBuilding = GetComponentInChildren<UICurrentBuilding>();

// Specialized building types (optional)
uiCurrentDoor = uiCurrentBuilding; // Reuse if not specified
uiCurrentStorage = GetComponentInChildren<UICurrentStorage>();
uiCurrentWorkbench = GetComponentInChildren<UICurrentWorkbench>();
```

### **Building Interaction Flow**

#### **Construction Process**
```
Player Selects Build → Construction UI Opens
Choose Building Type → Placement Interface
Confirm Location → Building Created
```

#### **Building Management**
```
Player Interacts → Appropriate Management UI
Door: Lock/Unlock Controls
Storage: Inventory Access
Workbench: Crafting Interface
```

---

## 👥 **Social UIs Setup**

### **Community Features**

#### **Party System**
```csharp
// Party management
uiPartyInvitation = GetComponentInChildren<UIPartyInvitation>();
```

#### **Guild System**
```csharp
// Guild management
uiGuildInvitation = GetComponentInChildren<UIGuildInvitation>();
```

#### **Competitive Features**
```csharp
// Dueling system
uiDuelingRequest = GetComponentInChildren<UIDuelingRequest>();
uiDueling = GetComponentInChildren<UIDueling>();
```

### **Social Interaction Flow**

#### **Party Formation**
```
Player Sends Invite → Party Invitation UI
Recipient Accepts → Party Formed
Group Activities → Coordinated Actions
```

#### **Guild Management**
```
Guild Invite Sent → Guild Invitation UI
Member Accepts → Guild Joined
Guild Activities → Shared Objectives
```

---

## ⚔️ **Combat UIs Configuration**

### **Player Activation System**

#### **Activation Menu**
```csharp
// Combat interaction setup
uiPlayerActivateMenu = GetComponentInChildren<UIPlayerActivateMenu>();
uiIsWarping = GetComponentInChildren<UIBase>(); // Warping status
```

#### **Activation Triggers**
- **Combat Engagement**: Attack, defend, special abilities
- **Interactive Objects**: Doors, levers, switches
- **NPC Interactions**: Combat, trading, quest-related

### **Combat Flow**
```
Player Targets Enemy → Activation Menu Appears
Select Action → Combat Engaged
Monitor Status → UI Updates
Combat Complete → Menu Closes
```

---

## 🔄 **Toggle UIs System**

### **Input-Based UI Toggling**

#### **Toggle UI Structure**
```csharp
[System.Serializable]
public struct UIToggleUI
{
    public UIBase ui;           // UI component to toggle
    public KeyCode keyCode;     // Keyboard activation
    public string buttonName;   // Input system button
}
```

#### **Toggle Configuration**
```csharp
// Toggle UI examples
public List<UIToggleUI> toggleUis = new List<UIToggleUI>
{
    new UIToggleUI
    {
        ui = inventoryUI,
        keyCode = KeyCode.I,
        buttonName = "Inventory"
    },
    new UIToggleUI
    {
        ui = mapUI,
        keyCode = KeyCode.M,
        buttonName = "Map"
    }
};
```

### **Toggle Behavior**

#### **Activation Methods**
- **Keyboard Input**: Direct key press (e.g., 'I' for inventory)
- **Button Input**: Input system button press
- **Priority**: First input method that triggers

#### **State Management**
- **Toggle Logic**: Press to show, press again to hide
- **Exclusive Mode**: Can close other UIs when opening
- **Persistence**: Remembers state across sessions

---

## ⚙️ **Settings & Behavior**

### **Input Handling Configuration**

#### **Pointer Detection**
```csharp
// UI interaction settings
public List<UnityTag> ignorePointerOverUITags = new List<UnityTag>();
public List<GameObject> ignorePointerOverUIObjects = new List<GameObject>();
```

#### **Controller Blocking**
```csharp
// Input blocking during UI interaction
public List<UIBase> blockControllerUis = new List<UIBase>();
```

### **Automatic UI Management**

#### **Component Auto-Assignment**
```csharp
protected override void Awake()
{
    base.Awake();
    SetUIComponentsFromChildrenIfEmpty();
    // Automatically finds and assigns UI components from children
}
```

#### **Controller Blocking Setup**
```csharp
// Automatic UIBlockController attachment
foreach (UIBase ui in blockControllerUis)
{
    if (!ui.gameObject.GetComponent<UIBlockController>())
    {
        ui.gameObject.AddComponent<UIBlockController>();
    }
}
```

---

## 🔍 **Validation & Diagnostics**

### **Configuration Validation**

#### **Critical UI Checks**
- **Target UIs**: Character, NPC, building targeting
- **Core Interactions**: Dialog, storage, activation systems
- **System Integration**: Trading, crafting, social features

#### **Validation Results**
- **✅ Valid**: All critical UIs configured
- **⚠️ Warning**: Missing non-critical components
- **❌ Error**: Missing essential UI components

### **Performance Monitoring**

#### **UI Performance Metrics**
- **Component Count**: Total assigned UI components
- **Toggle Conflicts**: Duplicate key/button assignments
- **Controller Blocking**: UIs that block character input

#### **Optimization Recommendations**
- **UI Pooling**: Reduce instantiation overhead
- **Lazy Loading**: Load UIs on demand
- **Memory Management**: Clean up unused UI components

---

## 🎨 **Editor Integration**

### **Custom Editor Features**

#### **Organized Sections**
- **Target UIs**: Entity selection interfaces
- **Interaction UIs**: NPC and quest dialogs
- **Crafting UIs**: Item creation and modification
- **Trading UIs**: Commerce and vending systems
- **Storage UIs**: Inventory management
- **Building UIs**: Construction and management
- **Social UIs**: Party and guild features
- **Combat UIs**: Player activation menus
- **Toggle UIs**: Input-based toggling
- **Settings**: Input handling and behavior

#### **Smart Tools**
- **Auto-Assignment**: Automatically find UI components in children
- **Validation**: Check configuration completeness
- **Reports**: Generate detailed UI configuration reports
- **Conflict Detection**: Identify duplicate key/button assignments

### **Visual Feedback**

#### **Progress Indicators**
- **Assignment Progress**: Visual progress bar showing configured UIs
- **Status Colors**: Green (good), Yellow (warning), Red (error)
- **Validation Results**: Detailed issue reporting

#### **Quick Actions**
- **Auto-Assign**: One-click UI component discovery
- **Validate**: Comprehensive configuration checking
- **Generate Report**: Detailed configuration logging

---

## 🚨 **Common Issues & Solutions**

### **"UI components not showing"**

**Symptoms:** UIs don't appear when expected
**Causes:**
- UI components not assigned in inspector
- Components disabled in hierarchy
- Auto-assignment not run after adding UIs

**Solutions:**
- Run "Auto-Assign" in the custom editor
- Check component references in inspector
- Verify UI GameObjects are active

### **"Toggle UIs not responding"**

**Symptoms:** Keyboard/button toggles don't work
**Causes:**
- Incorrect KeyCode or button name
- Key/button conflicts with other UIs
- Input system not configured

**Solutions:**
- Check for duplicate key/button assignments
- Verify input system button names
- Test in isolation (disable other toggles)

### **"Controller input blocked incorrectly"**

**Symptoms:** Character can't move when UI is open
**Causes:**
- UI not in blockControllerUis list
- UIBlockController component missing
- Incorrect UI references

**Solutions:**
- Add UI to blockControllerUis list
- Run "Find UIBlockController" in editor
- Verify UIBlockController components exist

### **"Performance issues with many UIs"**

**Symptoms:** UI system causing performance problems
**Causes:**
- Too many active UI components
- No UI pooling implemented
- Excessive UI updates

**Solutions:**
- Implement UI object pooling
- Use lazy loading for complex UIs
- Optimize UI update frequencies

### **"Auto-assignment not working"**

**Symptoms:** Auto-assign finds no components
**Causes:**
- UI components not in child hierarchy
- Wrong component types
- Components not deriving from UIBase

**Solutions:**
- Ensure UIs are children of UISceneGameplay
- Verify components inherit from UIBase
- Check component names match expectations

---

## 📊 **Performance Optimization**

### **UI Pooling Strategy**

#### **Object Pooling Implementation**
```csharp
// Implement UI object pooling for frequently used UIs
public class UIManager : MonoBehaviour
{
    private Dictionary<string, Queue<UIBase>> uiPools = new Dictionary<string, Queue<UIBase>>();

    public T GetUI<T>(string uiType) where T : UIBase
    {
        if (uiPools.TryGetValue(uiType, out Queue<UIBase> pool) && pool.Count > 0)
        {
            return pool.Dequeue() as T;
        }

        // Create new instance if pool empty
        return CreateNewUI<T>(uiType);
    }

    public void ReturnUI(UIBase ui, string uiType)
    {
        if (!uiPools.ContainsKey(uiType))
            uiPools[uiType] = new Queue<UIBase>();

        ui.gameObject.SetActive(false);
        uiPools[uiType].Enqueue(ui);
    }
}
```

#### **Lazy Loading**
```csharp
// Load complex UIs on demand
private Dictionary<string, UIBase> lazyLoadedUIs = new Dictionary<string, UIBase>();

public UIBase GetLazyUI(string uiName)
{
    if (!lazyLoadedUIs.TryGetValue(uiName, out UIBase ui))
    {
        // Load UI prefab or create from resources
        ui = LoadUIFromResources(uiName);
        lazyLoadedUIs[uiName] = ui;
    }

    ui.gameObject.SetActive(true);
    return ui;
}
```

### **Memory Management**

#### **UI Cleanup**
```csharp
// Automatic cleanup of unused UIs
private void CleanupUnusedUIs()
{
    foreach (var ui in GetComponentsInChildren<UIBase>(true))
    {
        if (!ui.gameObject.activeInHierarchy && TimeSinceLastUsed(ui) > cleanupThreshold)
        {
            Destroy(ui.gameObject);
        }
    }
}
```

#### **Texture Optimization**
- Use **texture atlasing** for UI sprites
- Implement **mipmap streaming** for large textures
- Enable **texture compression** for mobile platforms

---

## 🔗 **Integration Points**

### **GameInstance Integration**

#### **Automatic Setup**
```csharp
// GameInstance automatically manages UISceneGameplay
void OnSceneLoaded(Scene scene, LoadSceneMode mode)
{
    var uiManager = FindObjectOfType<UISceneGameplay>();
    if (uiManager != null)
    {
        ConfigureUIForCurrentScene(uiManager);
    }
}
```

### **Input System Integration**

#### **Custom Input Handling**
```csharp
// Integration with Unity's Input System
public class UIInputHandler : MonoBehaviour
{
    private InputAction toggleInventoryAction;

    void Awake()
    {
        toggleInventoryAction = new InputAction("ToggleInventory", InputActionType.Button);
        toggleInventoryAction.AddBinding("<Keyboard>/i");
        toggleInventoryAction.performed += ctx => ToggleInventory();
    }

    private void ToggleInventory()
    {
        var uiManager = BaseUISceneGameplay.Singleton;
        // Toggle inventory UI
    }
}
```

### **Event System Integration**

#### **Unity Events**
```csharp
// UI event handling
public UnityEvent onPlayerDeath = new UnityEvent();
public UnityEvent onPlayerRespawn = new UnityEvent();

// Custom event delegates
public System.Action<BuildingEntity> onShowConstructBuildingDialog;
public System.Action onHideConstructBuildingDialog;
```

---

## 📋 **Configuration Checklist**

### **Initial Setup**
- [ ] Attach UISceneGameplay to main UI canvas
- [ ] Configure all critical Target UIs
- [ ] Set up Interaction UIs (NPC dialogs, quests)
- [ ] Assign Storage UIs (player, guild, building)
- [ ] Configure Building UIs (construction, management)

### **Advanced Configuration**
- [ ] Set up Crafting UIs (refining, dismantling)
- [ ] Configure Trading UIs (dealing, vending)
- [ ] Add Social UIs (party, guild, dueling)
- [ ] Configure Toggle UIs with appropriate keys/buttons
- [ ] Set up Input handling (blocking, ignoring)

### **Testing & Validation**
- [ ] Run auto-assignment for missing components
- [ ] Execute validation checks
- [ ] Test all UI interactions in play mode
- [ ] Verify performance with UI stress testing
- [ ] Check for input conflicts and blocking issues

### **Production Deployment**
- [ ] Disable debug UIs and overlays
- [ ] Optimize UI pooling and lazy loading
- [ ] Verify memory management
- [ ] Test on target platforms
- [ ] Monitor performance metrics

---

## 📞 **API Reference**

### **Core Properties**

```csharp
// Singleton access
public static BaseUISceneGameplay Singleton { get; }

// Target UI components
public UICharacterEntity uiTargetCharacter;
public UIBaseGameEntity uiTargetNpc;
public UIBaseGameEntity uiTargetItemDrop;
// ... (all target UI fields)

// Interaction UIs
public UINpcDialog uiNpcDialog;
public UIQuestRewardItemSelection uiQuestRewardItemSelection;

// Crafting UIs
public UIRefineItem uiRefineItem;
public UIDismantleItem uiDismantleItem;
// ... (all crafting UI fields)

// Settings
public bool showVendingUiOnActivate;
public List<UIToggleUI> toggleUis;
public List<UnityTag> ignorePointerOverUITags;
public List<GameObject> ignorePointerOverUIObjects;
public List<UIBase> blockControllerUis;
```

### **Key Methods**

```csharp
// Initialization
protected override void Awake()
protected override void OnDestroy()

// UI Management
public void SetUIComponentsFromChildrenIfEmpty()

// Event Handling
public UnityEvent onCharacterDead;
public UnityEvent onCharacterRespawn;
```

### **Toggle UI Structure**

```csharp
[System.Serializable]
public struct UIToggleUI
{
    public UIBase ui;
    public KeyCode keyCode;
    public string buttonName;
}
```

---

## 🎯 **Best Practices**

### **1. UI Organization**
- **Logical Grouping**: Keep related UIs together in hierarchy
- **Naming Conventions**: Use consistent naming (e.g., "UI_Inventory", "UI_CharacterSheet")
- **Component Structure**: Use prefab variants for similar UIs
- **Layer Management**: Organize UI elements in appropriate canvas layers

### **2. Performance Optimization**
- **Object Pooling**: Pool frequently used UI components
- **Lazy Loading**: Load complex UIs only when needed
- **Texture Atlasing**: Combine UI textures to reduce draw calls
- **Update Optimization**: Minimize UI updates per frame

### **3. User Experience**
- **Consistent Interactions**: Standardize UI interaction patterns
- **Accessibility**: Support keyboard navigation and screen readers
- **Responsive Design**: Adapt to different screen sizes
- **Feedback Systems**: Provide clear visual and audio feedback

### **4. Development Workflow**
- **Modular Design**: Create reusable UI components
- **Version Control**: Track UI prefab changes carefully
- **Testing**: Test UI flows thoroughly before release
- **Documentation**: Document complex UI interactions

### **5. Maintenance**
- **Regular Audits**: Review UI performance periodically
- **User Feedback**: Monitor player feedback on UI usability
- **Analytics**: Track UI usage patterns
- **Updates**: Plan UI updates around major content releases

---

## 📈 **Scaling Considerations**

### **Small Projects (1-5 UIs)**
```
Simple Setup: Basic target and interaction UIs
Toggle System: 2-3 keyboard shortcuts
Storage: Single unified inventory system
Performance: Minimal pooling needed
```

### **Medium Projects (5-15 UIs)**
```
Organized Structure: Grouped UI categories
Toggle System: 5-10 keyboard/input shortcuts
Storage: Multiple specialized inventories
Performance: Basic pooling implementation
```

### **Large Projects (15+ UIs)**
```
Complex Architecture: Multi-canvas, layered systems
Toggle System: Comprehensive input mapping
Storage: Distributed inventory management
Performance: Advanced pooling and lazy loading
```

### **MMO-Scale Projects (50+ UIs)**
```
Modular System: Dynamic UI loading and unloading
Toggle System: Context-sensitive shortcuts
Storage: Cloud-synced, multi-character inventories
Performance: Enterprise-level optimization
```

---

## 🔄 **Version History**

### **Current Features**
- **Comprehensive UI Management**: 25+ UI component categories
- **Target System**: Entity selection and information display
- **Interaction Framework**: NPC dialogs and quest systems
- **Crafting Integration**: Item creation and modification
- **Commerce Systems**: Trading and vending interfaces
- **Storage Management**: Multi-level inventory systems
- **Building Systems**: Construction and management UIs
- **Social Features**: Party, guild, and competitive UIs
- **Input Integration**: Toggle systems and controller blocking
- **Editor Tools**: Auto-assignment, validation, and diagnostics

### **Key Capabilities**
- **Unity Integration**: Full Unity UI system compatibility
- **Performance Optimized**: Pooling, lazy loading, and memory management
- **Extensible Architecture**: Easy addition of new UI types
- **Developer Friendly**: Comprehensive editor tools and validation
- **Production Ready**: Error handling, logging, and monitoring

---

*This documentation covers the complete UISceneGameplay system for comprehensive gameplay UI management in NightBlade. For the latest updates and additional UI components, check the official repository.*