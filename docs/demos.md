# Demo Scenes Guide

NightBlade includes multiple specialized demo scenes that showcase different ways to use the framework. Each demo is a complete, working example that demonstrates specific features and architectural patterns. Unlike traditional MMORPG Kit demos, NightBlade demos are focused on specific game types and implementation approaches.

## 🎮 Demo Overview

NightBlade provides demos for different game types and architectural approaches:

```
Demos/
├── CoreDemo/         # Basic single-player/multiplayer foundation
├── SurvivalDemo/     # Survival mechanics with crafting
├── ShooterDemo/      # Third-person shooter gameplay
├── GuildWarDemo/     # Large-scale PvP warfare
└── MMO3DDemo/        # Full 3D MMO experience (future)
```

## 🏗️ CoreDemo - Foundation Systems

The CoreDemo provides the most basic NightBlade implementation, perfect for understanding the framework fundamentals.

### What It Demonstrates
- ✅ **Core Systems**: Characters, combat, economy, UI
- ✅ **Single-Player**: Complete solo gameplay experience
- ✅ **Local Multiplayer**: LAN-based multiplayer
- ✅ **Basic Architecture**: How systems integrate together
- ✅ **Security Features**: Data validation in action
- ✅ **Performance**: Optimized auto-save and physics

### Getting Started

1. **Open the Demo**
   ```bash
   # In Unity Project window
   Navigate to: Assets/NightBlade_1.95+/Demos/CoreDemo/Demo/Scenes/
   Open: 00Init.scene
   ```

2. **Run the Demo**
   - Press Play in Unity
   - Select "Single Player" or "Multiplayer"
   - Create a character
   - Explore the game world

### Key Features Demonstrated

#### Character System
- Character creation with class selection
- Attribute progression (Strength, Dexterity, Vitality, Intelligence)
- Skill learning and leveling
- Equipment and inventory management

#### Combat System
- Real-time combat with monsters
- Damage calculation and effects
- Health/mana regeneration
- Death and respawn mechanics

#### Economy System
- Item pickup and looting
- Inventory management
- Shop system with buying/selling
- Currency (Gold) management

#### UI System
- Character stats display
- Inventory and equipment screens
- Skill trees and hotbars
- Quest log and NPC dialogs

### Controls
- **WASD**: Movement
- **Mouse**: Camera rotation and targeting
- **Left Click**: Attack/interact
- **C**: Character stats
- **I**: Inventory
- **T**: Skill tree
- **Q**: Quest log
- **P**: Party management
- **G**: Guild management
- **Tab**: Select target
- **1-0**: Use hotkey skills/items

### Architecture Lessons

The CoreDemo shows the basic NightBlade architecture:

```csharp
// Core systems are independent
CharacterSystem characterSys = new CharacterSystem();
CombatSystem combatSys = new CombatSystem();
EconomySystem economySys = new EconomySystem();

// Systems communicate through interfaces
characterSys.OnDamageTaken += combatSys.HandleDamage;
economySys.OnItemAcquired += characterSys.UpdateInventory;

// Security validation is built-in
if (!DataValidation.IsValidCharacterStats(character)) {
    Debug.LogError("Invalid character data!");
    return;
}
```

### Customization Example

To modify the CoreDemo for your game:

```csharp
// 1. Create custom character class
[CreateAssetMenu(fileName = "CustomClass", menuName = "NightBlade/Custom Class")]
public class CustomCharacterClass : CharacterClass {
    public float customStat = 100f;

    public override void Initialize(Character character) {
        base.Initialize(character);
        character.AddCustomStat("CustomStat", customStat);
    }
}

// 2. Add to CharacterClassDatabase
// Assets/NightBlade_1.95+/Demos/CoreDemo/Demo/Resources/Database/CharacterClasses.asset

// 3. Create custom items
[CreateAssetMenu(fileName = "CustomWeapon", menuName = "NightBlade/Custom Weapon")]
public class CustomWeapon : WeaponItem {
    public float customDamage = 50f;

    public override float CalculateDamage() {
        return base.CalculateDamage() + customDamage;
    }
}
```

## 🏕️ SurvivalDemo - Survival Mechanics

Focuses on survival gameplay with crafting and resource management.

### Key Systems
- **Resource Gathering**: Mining, chopping, harvesting
- **Crafting System**: Create items from gathered resources
- **Building Construction**: Build shelters and structures
- **Hunger/Thirst**: Survival mechanics
- **Day/Night Cycle**: Environmental changes

### Survival Mechanics Example

```csharp
public class SurvivalSystem : MonoBehaviour {
    [Header("Survival Stats")]
    public float maxHunger = 100f;
    public float maxThirst = 100f;
    public float hungerDecreaseRate = 1f; // per minute
    public float thirstDecreaseRate = 1.5f; // per minute

    private float currentHunger;
    private float currentThirst;

    void Start() {
        currentHunger = maxHunger;
        currentThirst = maxThirst;
        StartCoroutine(SurvivalDecay());
    }

    IEnumerator SurvivalDecay() {
        while (true) {
            yield return new WaitForSeconds(60f); // Every minute

            currentHunger = Mathf.Max(0, currentHunger - hungerDecreaseRate);
            currentThirst = Mathf.Max(0, currentThirst - thirstDecreaseRate);

            if (currentHunger <= 0 || currentThirst <= 0) {
                // Player suffers consequences
                HandleStarvation();
            }
        }
    }

    public void ConsumeFood(float nutrition) {
        currentHunger = Mathf.Min(maxHunger, currentHunger + nutrition);
    }
}
```

## 🎯 ShooterDemo - Third-Person Shooter

Demonstrates FPS/third-person shooter mechanics within the ARPG framework.

### Shooter Features
- **Precise Controls**: Mouse-look and aiming
- **Weapon System**: Guns, ammo, reloading
- **Cover System**: Tactical movement
- **Headshots**: Critical hit mechanics
- **Recoil/Knockback**: Realistic weapon feedback

### Weapon System Implementation

```csharp
public class GunWeapon : WeaponItem {
    [Header("Gun Stats")]
    public int maxAmmo = 30;
    public float fireRate = 0.1f; // seconds between shots
    public float reloadTime = 2f;
    public float damage = 25f;
    public float range = 100f;

    private int currentAmmo;
    private float lastFireTime;
    private bool isReloading;

    void Start() {
        currentAmmo = maxAmmo;
    }

    public override bool CanFire() {
        return currentAmmo > 0 && !isReloading &&
               Time.time - lastFireTime >= fireRate;
    }

    public override void Fire() {
        if (!CanFire()) return;

        // Perform raycast for hit detection
        RaycastHit hit;
        if (Physics.Raycast(firePoint.position, firePoint.forward, out hit, range)) {
            // Apply damage to target
            IDamageable target = hit.collider.GetComponent<IDamageable>();
            if (target != null) {
                target.TakeDamage(damage);
            }
        }

        currentAmmo--;
        lastFireTime = Time.time;

        // Visual effects
        PlayMuzzleFlash();
        PlaySound("gunshot");
    }

    public void Reload() {
        if (isReloading) return;
        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine() {
        isReloading = true;
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = maxAmmo;
        isReloading = false;
    }
}
```

## ⚔️ GuildWarDemo - Large-Scale PvP

Showcases massive PvP battles with guild warfare mechanics.

### Warfare Features
- **Guild System**: Create and manage guilds
- **Territory Control**: Capture and defend areas
- **Siege Weapons**: Catapults, ballistas, towers
- **Large Battles**: 50+ players per battle
- **War Declaration**: Strategic guild conflicts

### Guild War Mechanics

```csharp
public class GuildWarSystem : MonoBehaviour {
    [Header("War Settings")]
    public float warDuration = 1800f; // 30 minutes
    public int maxPlayersPerGuild = 25;
    public float territoryCaptureTime = 60f; // seconds

    private Dictionary<string, Guild> guilds = new Dictionary<string, Guild>();
    private List<WarZone> warZones = new List<WarZone>();
    private War currentWar;

    public void DeclareWar(Guild attacker, Guild defender) {
        if (!CanDeclareWar(attacker, defender)) return;

        currentWar = new War(attacker, defender, warDuration);
        StartCoroutine(WarCoroutine());

        // Notify all players
        BroadcastWarDeclaration(attacker, defender);
    }

    IEnumerator WarCoroutine() {
        // War preparation phase
        yield return new WaitForSeconds(300f); // 5 minutes

        // Active war phase
        float warEndTime = Time.time + warDuration;
        while (Time.time < warEndTime) {
            UpdateWarZones();
            CheckVictoryConditions();
            yield return null;
        }

        // War resolution
        ResolveWar();
    }

    void UpdateWarZones() {
        foreach (WarZone zone in warZones) {
            Guild controllingGuild = GetControllingGuild(zone);
            zone.UpdateControl(controllingGuild);
        }
    }

    Guild DetermineWinner() {
        int attackerScore = currentWar.attacker.GetWarScore();
        int defenderScore = currentWar.defender.GetWarScore();

        if (attackerScore > defenderScore) return currentWar.attacker;
        if (defenderScore > attackerScore) return currentWar.defender;
        return null; // Draw
    }
}
```

## 🛠️ Using Demos as Starting Points

### Choosing the Right Demo

| Game Type | Recommended Demo | Why |
|-----------|------------------|-----|
| Single-player RPG | CoreDemo | Clean foundation, easy to modify |
| Survival Game | SurvivalDemo | Resource and crafting systems |
| Shooter | ShooterDemo | Combat and weapon mechanics |
| Large-scale PvP | GuildWarDemo | Guild and warfare systems |

### Demo Modification Workflow

1. **Duplicate the Demo**
   ```bash
   # Copy the demo folder
   cp -r CoreDemo MyGameDemo
   ```

2. **Modify Core Systems**
   ```csharp
   // Create your custom systems
   public class MyCharacterSystem : CharacterSystem {
       // Add your custom logic
       public override void LevelUp(Character character) {
           base.LevelUp(character);
           // Your custom level-up effects
       }
   }
   ```

3. **Update Scene References**
   - Update prefabs to use your custom components
   - Modify UI to match your design
   - Adjust game rules and balance

4. **Add New Features**
   ```csharp
   // Add new gameplay systems
   public class MyNewFeature : MonoBehaviour {
       void Start() {
           // Integrate with existing NightBlade systems
           GameInstance.Singleton.onGameStarted += OnGameStarted;
       }

       void OnGameStarted() {
           // Initialize your feature
       }
   }
   ```

### Demo Architecture Patterns

#### Component-Based Architecture
```csharp
// Demos use composition over inheritance
public class PlayerCharacter : MonoBehaviour {
    [Header("Core Components")]
    public Character character;
    public CharacterController2D controller;
    public CharacterCombat combat;
    public CharacterInventory inventory;

    [Header("Demo-Specific")]
    public SurvivalStats survivalStats; // Only in SurvivalDemo
    public ShooterController shooterController; // Only in ShooterDemo
}
```

#### Service Locator Pattern
```csharp
// Systems register themselves for easy access
public static class GameServices {
    public static CharacterSystem Character { get; private set; }
    public static CombatSystem Combat { get; private set; }
    public static EconomySystem Economy { get; private set; }

    static GameServices() {
        Character = new CharacterSystem();
        Combat = new CombatSystem();
        Economy = new EconomySystem();
    }
}

// Usage throughout the demo
GameServices.Character.LevelUp(player);
GameServices.Combat.DealDamage(attacker, target);
```

## 🔧 Demo Configuration

### Performance Settings

Each demo includes optimized performance settings:

```csharp
// CoreDemo - Balanced for general use
public static class CoreDemoConfig {
    public const float AutoSaveInterval = 30f;
    public const float PhysicsUpdateRate = 10f;
    public const ValidationLevel SecurityLevel = ValidationLevel.Standard;
}

```

### Build Settings

Demos include pre-configured build settings:

```json
// build.json for CoreDemo
{
    "scenes": [
        "Assets/NightBlade_1.95+/Demos/CoreDemo/Demo/Scenes/00Init.unity",
        "Assets/NightBlade_1.95+/Demos/CoreDemo/Demo/Scenes/01Home.unity",
        "Assets/NightBlade_1.95+/Demos/CoreDemo/Demo/Scenes/Map01.unity"
    ],
    "scriptingDefineSymbols": [
        "NIGHTBLADE_CORE",
        "VALIDATION_ENABLED"
    ]
}
```

## 🧪 Testing Demos

### Automated Testing

NightBlade demos include automated tests:

```csharp
[TestFixture]
public class CoreDemoTests {
    [Test]
    public void Character_CanCreateAndLevelUp() {
        // Test character creation
        var character = Character.Create("TestCharacter", CharacterClass.Warrior);
        Assert.IsNotNull(character);

        // Test leveling
        character.AddExp(1000);
        Assert.AreEqual(2, character.Level);
    }

    [Test]
    public void Combat_DamageCalculationWorks() {
        var attacker = Character.Create("Attacker", CharacterClass.Warrior);
        var defender = Character.Create("Defender", CharacterClass.Mage);

        float damage = CombatSystem.CalculateDamage(attacker, defender, 50f);
        Assert.Greater(damage, 0);
    }
}
```

### Performance Testing

```csharp
[PerformanceTest]
public void Demo_PerformanceUnderLoad() {
    // Simulate 50 players
    var players = new List<Character>();
    for (int i = 0; i < 50; i++) {
        players.Add(Character.Create($"Player{i}", CharacterClass.Warrior));
    }

    // Measure performance
    var stopwatch = Stopwatch.StartNew();
    for (int i = 0; i < 1000; i++) {
        foreach (var player in players) {
            player.Update(Time.deltaTime);
        }
    }
    stopwatch.Stop();

    // Assert performance requirements
    Assert.Less(stopwatch.ElapsedMilliseconds, 500); // Should complete in < 500ms
}
```

## 📚 Demo Learning Path

1. **Start with CoreDemo**: Understand basic NightBlade architecture
2. **Try SurvivalDemo**: See resource management systems
4. **Examine ShooterDemo**: Study combat mechanics
5. **Analyze GuildWarDemo**: Understand large-scale features

Each demo builds upon the previous, teaching progressive concepts while demonstrating NightBlade's flexibility and performance optimizations.

