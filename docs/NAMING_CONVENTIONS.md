# NightBlade MMO - Naming Conventions

**Version**: 1.0  
**Last Updated**: January 23, 2026  
**Status**: Enforced  

---

## Purpose

This document establishes the official naming conventions for the NightBlade MMO codebase. Following these standards ensures consistency, readability, and maintainability across the project.

## Enforcement

- **Tool**: SafeAutoFixNamingAuditor (Assets/NightBlade/Tools/Editor/)
- **Frequency**: Monthly audits
- **Target**: < 50 total violations across entire codebase

---

## Naming Rules

### 1. Classes, Structs, and Interfaces

**Convention**: `PascalCase`

**Rules**:
- Start with uppercase letter
- Each word capitalized
- No underscores or special characters

**Examples**:
```csharp
// ✅ Correct
public class PlayerCharacter { }
public struct CharacterStats { }
public interface IGameplayRule { }

// ❌ Incorrect
public class playerCharacter { }
public class player_character { }
public class PLAYERCHARACTER { }
```

**Interfaces**: Prefix with `I`
```csharp
public interface IDamageable { }
public interface IInventorySystem { }
```

---

### 2. Methods

**Convention**: `PascalCase`

**Rules**:
- Start with uppercase letter
- Use descriptive verb or verb phrase
- Each word capitalized

**Examples**:
```csharp
// ✅ Correct
public void AttackTarget() { }
public int GetPlayerHealth() { }
public bool CanCastSkill() { }

// ❌ Incorrect
public void attackTarget() { }
public void attack_target() { }
public void ATTACKTARGET() { }
```

**Special Cases**:
- Unity callbacks: Follow Unity convention (OnEnable, Start, Update, etc.)
- Overrides: Match base class naming

---

### 3. Properties

**Convention**: `PascalCase`

**Rules**:
- Start with uppercase letter
- Descriptive noun or noun phrase
- Each word capitalized

**Examples**:
```csharp
// ✅ Correct
public int MaxHealth { get; set; }
public string PlayerName { get; private set; }
public bool IsAlive => health > 0;

// ❌ Incorrect
public int maxHealth { get; set; }
public int max_health { get; set; }
public int MAXHEALTH { get; set; }
```

---

### 4. Fields (Variables)

#### **4.1 Private Fields**

**Convention**: `_camelCase` (underscore prefix + camelCase)

**Rules**:
- Start with underscore
- First word lowercase
- Subsequent words capitalized

**Examples**:
```csharp
// ✅ Correct
private int _playerHealth;
private string _characterName;
private bool _isAlive;

// ❌ Incorrect
private int playerHealth;     // Missing underscore
private int _PlayerHealth;    // Should be camelCase after underscore
private int player_health;    // Wrong style
```

#### **4.2 Public/Protected Fields**

**Convention**: `camelCase` (no underscore)

**Rules**:
- Start with lowercase letter
- Subsequent words capitalized
- Avoid public fields when possible (use properties instead)

**Examples**:
```csharp
// ✅ Correct (but prefer properties)
public int maxHealth;
protected float moveSpeed;

// ✅ Better (use properties)
public int MaxHealth { get; set; }
protected float MoveSpeed { get; set; }
```

#### **4.3 SerializedField (Unity)**

**Convention**: `_camelCase` (treated as private)

**Examples**:
```csharp
// ✅ Correct
[SerializeField] private int _maxHealth;
[SerializeField] private float _moveSpeed;

// ❌ Incorrect
[SerializeField] private int maxHealth;  // Missing underscore
```

---

### 5. Constants

**Convention**: `SCREAMING_SNAKE_CASE` or `PascalCase`

**Rules**:
- All uppercase with underscores OR
- PascalCase (team preference: use PascalCase)
- Descriptive and clear

**Examples**:
```csharp
// ✅ Correct (Preferred)
private const int MaxPlayers = 100;
private const string DefaultPlayerName = "Player";

// ✅ Acceptable (Alternative)
private const int MAX_PLAYERS = 100;
private const string DEFAULT_PLAYER_NAME = "Player";

// ❌ Incorrect
private const int maxPlayers = 100;
private const int max_players = 100;
```

**Note**: For NightBlade, prefer PascalCase for constants to match Unity's style.

---

### 6. Parameters and Local Variables

**Convention**: `camelCase`

**Rules**:
- Start with lowercase letter
- Descriptive names
- Subsequent words capitalized

**Examples**:
```csharp
// ✅ Correct
public void DealDamage(int damageAmount, Character targetCharacter)
{
    int finalDamage = CalculateDamage(damageAmount);
    float damageMultiplier = GetDamageMultiplier();
}

// ❌ Incorrect
public void DealDamage(int DamageAmount, Character TargetCharacter)  // Should be camelCase
{
    int FinalDamage = CalculateDamage(DamageAmount);  // Should be camelCase
}
```

---

### 7. File Names

**Convention**: Match the primary class name (PascalCase)

**Rules**:
- One primary class per file
- File name matches class name exactly
- `.cs` extension

**Examples**:
```
✅ Correct:
PlayerCharacter.cs      → contains class PlayerCharacter
CharacterStats.cs       → contains class/struct CharacterStats
IGameplayRule.cs        → contains interface IGameplayRule

❌ Incorrect:
playercharacter.cs      → Wrong casing
Player_Character.cs     → Underscores not allowed
character.cs            → Doesn't match class name
```

---

### 8. Namespaces

**Convention**: `PascalCase` (hierarchical)

**Rules**:
- Match folder structure
- Use meaningful hierarchy
- No underscores

**Examples**:
```csharp
// ✅ Correct
namespace NightBlade.Core.Characters
namespace NightBlade.UI.Inventory
namespace NightBlade.Networking.Packets

// ❌ Incorrect
namespace nightblade.core.characters
namespace NightBlade_Core_Characters
namespace NIGHTBLADE.CORE.CHARACTERS
```

---

## Special Cases

### Unity-Specific Naming

#### **Prefabs**:
- PascalCase
- Descriptive names
- Example: `PlayerCharacterPrefab.prefab`

#### **Scenes**:
- PascalCase
- Descriptive names
- Example: `MainMenuScene.unity`

#### **ScriptableObjects**:
- PascalCase
- Include type suffix
- Example: `WarriorCharacterData.asset`

#### **Unity Events/Callbacks**:
- Follow Unity convention
- Examples: `Awake()`, `Start()`, `Update()`, `OnEnable()`, `OnDisable()`

---

## Common Patterns

### Boolean Naming

Prefix with `Is`, `Has`, `Can`, `Should`:

```csharp
// ✅ Correct
private bool _isAlive;
private bool _hasInventory;
public bool CanMove { get; set; }
public bool ShouldRespawn { get; set; }

// ❌ Incorrect
private bool _alive;         // Not clear it's boolean
private bool _inventory;     // Ambiguous
```

### Collection Naming

Use plural forms:

```csharp
// ✅ Correct
private List<Player> _players;
private Dictionary<int, Item> _inventoryItems;
public Character[] AllCharacters { get; }

// ❌ Incorrect
private List<Player> _playerList;      // Redundant "List"
private Dictionary<int, Item> _items;  // Not specific enough
```

### Event Naming

Use past tense or noun form:

```csharp
// ✅ Correct
public event Action OnPlayerDied;
public event Action<int> OnHealthChanged;
public UnityEvent PlayerDeath;

// ❌ Incorrect
public event Action OnPlayerDie;      // Use past tense
public event Action OnPlayerDying;    // Use past tense
```

---

## Code Review Checklist

Before committing code, verify:

- [ ] All classes use PascalCase
- [ ] All methods use PascalCase
- [ ] All properties use PascalCase
- [ ] Private fields start with underscore + camelCase
- [ ] Constants use PascalCase
- [ ] Parameters and local variables use camelCase
- [ ] File names match primary class names
- [ ] No C# keywords used as identifiers
- [ ] Boolean fields have clear prefixes (Is/Has/Can/Should)
- [ ] Collections use plural names

---

## Automated Enforcement

### Tools Available:

1. **SafeNamingConventionAuditor** (Read-only)
   - Location: `Assets/NightBlade/Tools/Editor/SafeNamingConventionAuditor.cs`
   - Use for: Quick audits and reports
   - Safety: Never modifies code

2. **SafeAutoFixNamingAuditor** (Auto-fix)
   - Location: `Assets/NightBlade/Tools/Editor/SafeAutoFixNamingAuditor.cs`
   - Use for: Batch fixing violations
   - Safety: Git-based commits, preview mode, revertible

### Running an Audit:

```
Unity Menu → NightBlade → Tools → Safe Naming Convention Audit
```

### Applying Auto-Fixes:

```
Unity Menu → NightBlade → Tools → Safe Auto-Fix Naming Tool
1. Enable desired categories (Classes, Properties, Fields)
2. Click "Scan for Violations"
3. Click "Preview Changes"
4. Review carefully
5. Click "Apply Auto-Fixes"
```

---

## Migration Strategy

### For Existing Code:

1. **Phase 1**: Auto-fix Fields, Properties, Classes (LOW risk)
2. **Phase 2**: Manual review Methods (MEDIUM risk)
3. **Phase 3**: Manual review Constants (MEDIUM risk)
4. **Phase 4**: File renames as needed (LOW risk, time-consuming)

### For New Code:

- Follow conventions from day one
- Use IDE/Rider code style settings
- Review during code review process

---

## Exceptions

### When NOT to Follow These Rules:

1. **Third-party code**: Never modify (excluded from scans)
2. **Unity API overrides**: Follow Unity's naming
3. **Interop/External APIs**: Match external naming conventions
4. **Generated code**: Excluded from audits
5. **Legacy integrations**: Document exceptions clearly

---

## Team Agreement

By working on NightBlade MMO, all contributors agree to:

- Follow these naming conventions for new code
- Gradually improve legacy code when touched
- Run naming audits before major commits
- Document any necessary exceptions
- Review naming during code reviews

---

## Questions or Changes

If you have suggestions for improving these conventions:

1. Open a discussion with the team
2. Document the proposed change
3. Get team consensus
4. Update this document
5. Increment version number

---

## Version History

| Version | Date | Changes | Author |
|---------|------|---------|--------|
| 1.0 | 2026-01-23 | Initial standards document | NightBlade Team |

---

**Remember**: Consistency is more important than perfection. When in doubt, match the existing code in the same file or module.

**Goal**: Make NightBlade MMO professional, maintainable, and a joy to work on! 🚀