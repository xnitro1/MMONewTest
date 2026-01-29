# ⚔️ DefaultGameplayRule - Core Game Mechanics

## **Overview**

The **DefaultGameplayRule** ScriptableObject defines all core gameplay mechanics and balancing rules for NightBlade. This comprehensive configuration controls character progression, combat mechanics, survival systems, and economic balance.

**Type:** ScriptableObject (Create via Assets → Create → NightBlade → Gameplay Rule)  
**Purpose:** Unified gameplay balancing and rule definition  
**Impact:** Affects all player characters and game systems  

---

## 📋 **Quick Start**

1. **Create Asset**: `Assets → Create → NightBlade → Gameplay Rule`
2. **Assign to GameInstance**: Set as the `gameplayRule` in your GameInstance component
3. **Balance Core Systems**: Configure leveling, combat, and survival mechanics
4. **Test Iteratively**: Adjust values and test gameplay balance

```csharp
// Create and configure gameplay rules
DefaultGameplayRule rules = CreateInstance<DefaultGameplayRule>();
rules.increaseStatPointEachLevel = 5;
rules.expLostPercentageWhenDeath = 2f;

// Assign to GameInstance
gameInstance.gameplayRule = rules;
```

---

## 📊 **Balance Calculator**

The editor provides real-time balance metrics:

| Metric | Current Value | Recommended Range | Impact |
|--------|----------------|-------------------|---------|
| **Stat Points/Level** | `increaseStatPointEachLevel` | 3-8 | Character power progression |
| **Death Penalty** | `expLostPercentageWhenDeath`% | 1-5% | Risk/reward balance |
| **Sprint Speed** | `moveSpeedRateWhileSprinting`x | 1.2-2.0x | Combat mobility |

---

## 📈 Character Progression

### **Levelling & Stats**
Controls how characters grow through experience and levels.

| Setting | Range | Description | Balance Impact |
|---------|-------|-------------|----------------|
| **Stat Points/Level** | 1-10 | Points awarded each level | Power curve steepness |
| **Skill Points/Level** | 0-5 | Skill points per level | Specialization speed |
| **Stat Cap Level** | 0-∞ | Level where stat gains stop (0 = always) | Late-game balance |
| **Skill Cap Level** | 0-∞ | Level where skill gains stop | Learning curve |
| **Death XP Loss** | 0-100% | Experience lost on death | Risk management |

**Best Practice:** Start with 5 stat points/level for balanced progression.

---

## 🏃 Stamina & Sprint System

### **Stamina Management**
Endurance system affecting player mobility and combat.

| Setting | Range | Description | Gameplay Effect |
|---------|-------|-------------|----------------|
| **Stamina Recovery** | 1-20/sec | Rate of stamina regeneration | Combat endurance |
| **Stamina Drain** | 1-20/sec | Rate of sprint consumption | Sprint duration |
| **Overweight Penalty** | 0-1.0 | Speed multiplier when encumbered | Inventory management |

### **Sprint Mechanics**
Movement speed modifiers during different activities.

| Activity | Speed Multiplier | Increment | Use Case |
|----------|------------------|-----------|----------|
| **Sprinting** | `moveSpeedRateWhileSprinting` | `moveSpeedIncrementWhileSprinting` | Combat chasing/evasion |
| **Walking** | `moveSpeedRateWhileWalking` | `moveSpeedIncrementWhileWalking` | Stealth/sneaking |
| **Crouching** | `moveSpeedRateWhileCrouching` | `moveSpeedIncrementWhileCrouching` | Tactical movement |
| **Crawling** | `moveSpeedRateWhileCrawling` | `moveSpeedIncrementWhileCrawling` | Extreme stealth |
| **Swimming** | `moveSpeedRateWhileSwimming` | `moveSpeedIncrementWhileSwimming` | Water traversal |

**Balance Tip:** Sprint speed should be 1.3-1.8x normal speed for tactical gameplay.

---

## 🍖 Survival Systems

### **Hunger & Thirst**
Resource management adding survival depth.

| Setting | Range | Description | Player Impact |
|---------|-------|-------------|---------------|
| **Hungry Threshold** | 0-100 | Food level triggering debuffs | Resource management |
| **Thirsty Threshold** | 0-100 | Water level triggering debuffs | Survival difficulty |
| **Food Drain Rate** | 0.1-10/sec | How fast food depletes | Food scarcity |
| **Water Drain Rate** | 0.1-10/sec | How fast water depletes | Water scarcity |

### **Recovery Rates**
Health and mana regeneration mechanics.

| Recovery Type | Rate Range | Affected By | Strategic Use |
|---------------|------------|-------------|---------------|
| **HP Recovery** | 0-1.0/sec | Hunger/thirst | Safe resting |
| **MP Recovery** | 0-1.0/sec | Hunger/thirst | Spell caster sustainability |
| **Starvation HP Drain** | 0-1.0/sec | Food < threshold | Emergency management |
| **Dehydration HP Drain** | 0-1.0/sec | Water < threshold | Desert survival |

**Design Consideration:** Recovery rates should balance with combat damage output.

---

## 🔧 Equipment Durability

### **Wear & Tear System**
Equipment degradation requiring maintenance and resource management.

| Damage Type | Weapon | Shield | Armor | Gameplay Purpose |
|-------------|--------|--------|-------|------------------|
| **Normal Hits** | `normalDecreaseWeaponDurability` | `normalDecreaseShieldDurability` | `normalDecreaseArmorDurability` | Regular maintenance |
| **Blocked Attacks** | `blockedDecreaseWeaponDurability` | `blockedDecreaseShieldDurability` | `blockedDecreaseArmorDurability` | Defensive playstyle |
| **Critical Hits** | `criticalDecreaseWeaponDurability` | `criticalDecreaseShieldDurability` | `criticalDecreaseArmorDurability` | High-risk/reward |
| **Missed Attacks** | `missDecreaseWeaponDurability` | `missDecreaseShieldDurability` | `missDecreaseArmorDurability` | Accuracy incentive |

**Economic Impact:** Durability rates affect repair costs and resource sink.

---

## ⚔️ Combat Mechanics

### **Critical Hit Rules**
Special combat calculations and guarantees.

| Setting | Value | Description | Balance Effect |
|---------|-------|-------------|----------------|
| **Crit Always Hits** | `true/false` | Critical hits bypass evasion | Critical hit power |

### **Fall Damage**
Environmental damage from falling.

| Setting | Range | Description | Risk Management |
|---------|-------|-------------|----------------|
| **Min Fall Distance** | 1-20 | Distance for 1% max HP damage | Safe fall height |
| **Max Fall Distance** | 5-50 | Distance for 100% max HP damage | Lethal fall height |

**Physics Integration:** Works with Unity's character controller fall detection.

---

## ⭐ Level Up & Progression

### **Level Up Rewards**
Automatic recovery when leveling up.

| Recovery Type | Setting | Strategic Value |
|---------------|---------|-----------------|
| **HP Recovery** | `recoverHpWhenLevelUp` | Fresh start bonus |
| **MP Recovery** | `recoverMpWhenLevelUp` | Spell availability |
| **Food Recovery** | `recoverFoodWhenLevelUp` | Survival reset |
| **Water Recovery** | `recoverWaterWhenLevelUp` | Survival reset |
| **Stamina Recovery** | `recoverStaminaWhenLevelUp` | Mobility reset |

### **Battle Points System**
Competitive scoring for different character stats.

#### **Health & Mana**
| Stat | Points | Competitive Weight |
|------|--------|-------------------|
| **HP** | `hpBattlePointScore` | Tank viability |
| **HP Recovery** | `hpRecoveryBattlePointScore` | Sustained combat |
| **HP Leech** | `hpLeechRateBattlePointScore` | Aggressive playstyle |
| **MP** | `mpBattlePointScore` | Caster power |
| **MP Recovery** | `mpRecoveryBattlePointScore` | Spell spam potential |
| **MP Leech** | `mpLeechRateBattlePointScore` | Mana denial |

#### **Combat Stats**
| Stat | Points | Tactical Impact |
|------|--------|----------------|
| **Accuracy** | `accuracyBattlePointScore` | Hit consistency |
| **Evasion** | `evasionBattlePointScore` | Dodge potential |
| **Crit Rate** | `criRateBattlePointScore` | Burst damage |
| **Crit Damage** | `criDmgRateBattlePointScore` | Damage spikes |

#### **Defense & Mobility**
| Stat | Points | Strategic Role |
|------|--------|----------------|
| **Block Rate** | `blockRateBattlePointScore` | Mitigation consistency |
| **Block Damage** | `blockDmgRateBattlePointScore` | Shield tanking |
| **Move Speed** | `moveSpeedBattlePointScore` | Positioning/kiting |
| **Attack Speed** | `atkSpeedBattlePointScore` | DPS potential |

#### **Resources**
| Stat | Points | Survival Impact |
|------|--------|----------------|
| **Stamina** | `staminaBattlePointScore` | Mobility endurance |
| **Stamina Recovery** | `staminaRecoveryBattlePointScore` | Sprint sustainability |
| **Stamina Leech** | `staminaLeechRateBattlePointScore` | Fatigue denial |
| **Food** | `foodBattlePointScore` | Survival capacity |
| **Water** | `waterBattlePointScore` | Survival capacity |

**Tournament Use:** Battle points enable competitive character evaluation and balancing.

---

## 💀 Death & Consequences

### **Item Loss Mechanics**
Penalty system for player death.

| Setting | Range | Description | Risk Management |
|---------|-------|-------------|----------------|
| **Min Items Lost** | 0-10 | Minimum items dropped on death | Minimum penalty |
| **Max Items Lost** | 0-20 | Maximum items dropped on death | Maximum penalty |

**Loot Mechanics:** Dropped items become temporary loot for other players.

---

## 👹 Monster Display

### **Visual Threat Indicators**
Color-coded monster names based on level difference.

| Setting | Value | Description | Player Guidance |
|---------|-------|-------------|----------------|
| **Color Change Level** | 1-20 | Level difference triggering colors | Threat assessment |
| **High Level Color** | Color | Above player level | Danger warning |
| **Low Level Color** | Color | Below player level | Easy target |

**UI Integration:** Colors appear in monster nameplates and targeting system.

---

## 💰 Economy & Trading

### **Trading Fees**
Taxation system for player-to-player commerce.

| Setting | Range | Description | Economic Impact |
|---------|-------|-------------|----------------|
| **Tax Rate** | 0.01-0.5 | Percentage of item value as tax | Transaction cost |
| **Stack Calculation** | `true/false` | Tax based on stack size | Bulk trading |

**Market Dynamics:** Taxes fund game services and balance player economies.

---

## ⚔️ Player Killing (PK) System

### **PK Activation**
Requirements for player-versus-player combat.

| Setting | Range | Description | Community Management |
|---------|-------|-------------|---------------------|
| **Min Level** | 1-50 | Level required to enable PK | Newbie protection |
| **PK Points/Kill** | 1-100 | Points gained per kill | Escalation rate |
| **Cooldown Hours** | 1-168 | Hours before PK status expires | Aggression window |

### **PK Levels & Penalties**
Structured consequence system based on PK points.

#### **PK Level Structure**
```csharp
public struct PkData
{
    public int minPkPoint;           // Points required for this level
    public Color nameColor;          // Visual indicator color

    // Death penalties at this PK level
    public float expDecreasePercentMin;    // Min XP loss %
    public float expDecreasePercentMax;    // Max XP loss %
    public int goldDecreaseMin;           // Min gold loss
    public int goldDecreaseMax;           // Max gold loss
    public int itemDecreaseMin;           // Min items lost
    public int itemDecreaseMax;           // Max items lost

    // Ongoing penalties
    public float goldReductionRate;       // Gold earning penalty
    public float expReductionRate;        // XP earning penalty
}
```

**Escalation System:** Higher PK levels result in harsher penalties and visual indicators.

---

## 🔧 Advanced Configuration

### **Creating Custom Rules**

```csharp
[CreateAssetMenu(fileName = "MyGameplayRule", menuName = "NightBlade/Custom Gameplay Rule")]
public class MyGameplayRule : DefaultGameplayRule
{
    // Override specific mechanics
    public override float CalculateExpLoss(Character character)
    {
        // Custom death penalty logic
        return base.CalculateExpLoss(character) * customMultiplier;
    }
}
```

### **Runtime Rule Modification**

```csharp
// Temporarily modify rules for events
DefaultGameplayRule originalRules = gameInstance.gameplayRule;
DefaultGameplayRule eventRules = Instantiate(originalRules);

// Modify for special event
eventRules.expLostPercentageWhenDeath = 0f; // No death penalty during event
eventRules.moveSpeedRateWhileSprinting = 2.0f; // Double speed

gameInstance.gameplayRule = eventRules;

// Restore after event
gameInstance.gameplayRule = originalRules;
```

---

## 📊 Balance Validation

### **Built-in Validation Tools**

The editor provides validation for common balancing issues:

- **Death penalties** within reasonable ranges (0-100%)
- **Movement modifiers** provide meaningful bonuses
- **Recovery rates** prevent immortal characters
- **Durability values** encourage equipment maintenance

### **Balance Metrics**

```csharp
// Calculate balance ratios
float combatTime = maxHp / damagePerSecond;
float recoveryTime = maxHp / hpRecoveryPerSecond;
float riskRewardRatio = expLostPercentageWhenDeath / levelingSpeed;

// Ideal ranges:
// combatTime: 10-60 seconds
// recoveryTime: 30-300 seconds
// riskRewardRatio: 0.1-1.0
```

---

## 🎯 Best Practices

### **1. Iterative Balancing**
- **Start Conservative**: Begin with proven values
- **Test Small Changes**: Adjust one mechanic at a time
- **Monitor Metrics**: Track player behavior and retention
- **A/B Testing**: Compare different rule sets

### **2. Player Psychology**
- **Risk/Reward Balance**: Death penalties should encourage caution without frustration
- **Progression Feel**: Leveling should feel rewarding but not trivial
- **Fair Competition**: PK system should deter griefing while allowing PvP

### **3. Performance Impact**
- **Calculation Frequency**: Rules evaluated frequently - keep computations light
- **Memory Usage**: Complex rule sets may impact save/load times
- **Network Sync**: Rule changes may require client updates

### **4. Community Management**
- **PK Balance**: Strong enough to deter abuse, weak enough for legitimate PvP
- **Economic Stability**: Trading fees should not cripple player economies
- **Content Pacing**: Survival mechanics should complement, not conflict with, content

---

## 🚨 Common Issues & Solutions

### **"Rules not taking effect"**
**Cause:** GameInstance not updated or rules not assigned
**Solution:** Ensure GameInstance references the correct GameplayRule asset

### **"Overpowered characters"**
**Cause:** Stat point allocation too generous
**Solution:** Reduce `increaseStatPointEachLevel` or increase skill requirements

### **"Frustrating death penalties"**
**Cause:** XP loss too high for target audience
**Solution:** Lower `expLostPercentageWhenDeath` or add insurance systems

### **"Unbalanced PvP"**
**Cause:** PK penalties not scaling properly
**Solution:** Adjust PK point requirements and penalties for better progression

### **"Economic inflation"**
**Cause:** Trading taxes too low or rewards too high
**Solution:** Increase `taxByItemPriceRate` or adjust loot tables

---

## 📊 Performance Considerations

### **Calculation Overhead**
- **Per-frame evaluations**: Movement and stamina calculations
- **Combat frequency**: Damage and durability calculations
- **Save/load impact**: Complex rule serialization

### **Memory Usage**
- **Rule asset size**: Large PK data arrays
- **Runtime copies**: Instantiation for events
- **Caching strategy**: Frequently accessed values

### **Network Synchronization**
- **Rule consistency**: All clients must use same rules
- **Version compatibility**: Rule changes may require updates
- **Dynamic updates**: Limited ability to change rules at runtime

---

## 🔗 Related Systems

| System | Integration | Configuration |
|--------|-------------|---------------|
| **GameInstance** | Rule assignment | `gameInstance.gameplayRule = rules` |
| **CharacterSystem** | Stat calculations | Uses rule values for progression |
| **CombatSystem** | Damage calculations | References rule combat mechanics |
| **InventorySystem** | Durability updates | Equipment wear calculations |
| **EconomicSystem** | Trading fees | Tax rate applications |

---

## 📋 API Reference

### **Core Properties**

```csharp
public float increaseStatPointEachLevel = 5;
public float expLostPercentageWhenDeath = 2f;
public float moveSpeedRateWhileSprinting = 1.5f;
public PkData[] pkDatas = new PkData[0];
```

### **Key Methods**

```csharp
// Rule evaluation methods (inherited from BaseGameplayRule)
public virtual float GetExpLostPercentageWhenDeath() => expLostPercentageWhenDeath;
public virtual float GetMoveSpeedRateWhileSprinting() => moveSpeedRateWhileSprinting;
public virtual bool IsAlwaysHitWhenCriticalOccurs() => alwaysHitWhenCriticalOccurs;

// PK system methods
public virtual int GetMinLevelToTurnPkOn() => minLevelToTurnPkOn;
public virtual PkData GetPkData(int pkPoints) => pkDatas.FirstOrDefault(data => pkPoints >= data.minPkPoint);
```

---

## 📈 Version History

### **Revision 4 Alpha**
- Battle points scoring system for competitive balance
- Enhanced PK level progression with visual indicators
- Improved survival mechanics with hunger/thirst thresholds
- Comprehensive equipment durability system

### **Revision 3**
- Performance optimizations for rule calculations
- Enhanced security validation for rule integrity
- Improved memory management for large rule sets
- Advanced networking support for rule synchronization

### **Core Implementation**
- Complete gameplay rule framework
- ScriptableObject-based configuration
- Unity Editor integration with custom inspectors
- Comprehensive documentation and validation tools

---

## 🎮 Testing Checklist

### **Core Mechanics**
- [ ] Character leveling awards correct stat/skill points
- [ ] Death penalties apply appropriate XP/item loss
- [ ] Movement speeds modify correctly for all modes
- [ ] Stamina recovery/drain balances properly

### **Combat Systems**
- [ ] Critical hits function with guaranteed hit rule
- [ ] Fall damage scales correctly with distance
- [ ] Equipment durability decreases appropriately
- [ ] Battle points calculate accurately

### **Survival Features**
- [ ] Hunger/thirst thresholds trigger debuffs
- [ ] Resource consumption rates feel balanced
- [ ] Recovery rates provide meaningful healing
- [ ] Starvation/dehydration penalties are not too harsh

### **Social Systems**
- [ ] PK activation requires appropriate level
- [ ] PK point accumulation scales with kills
- [ ] PK penalties increase appropriately with level
- [ ] Monster colors indicate appropriate threat levels

### **Economic Balance**
- [ ] Trading taxes don't cripple commerce
- [ ] Item loss on death encourages preparation
- [ ] Recovery mechanics don't trivialize death
- [ ] Level up rewards provide meaningful bonuses

---

*This documentation covers the complete DefaultGameplayRule configuration system for NightBlade v1.95r3 + Revision 4 Alpha. For the latest updates and additional features, check the official repository.*