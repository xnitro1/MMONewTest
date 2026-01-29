# Combat System

This directory contains the core combat mechanics, including skills, damage calculation, buffs, and battle systems.

## 📁 Directory Structure

### Core Combat Classes
- `BaseSkill.cs` - Abstract base class for all skills
- `Skill.cs` - Main skill implementation
- `SkillLevel.cs` - Skill progression and leveling
- `SkillRequirement.cs` - Prerequisites for skill usage

### Skill Types
- `PlayerSkill.cs` - Skills usable by players
- `MonsterSkill.cs` - Skills for monsters/NPCs
- `SimpleAreaAttackSkill.cs` - Area-of-effect attack skills
- `SimpleAreaBuffSkill.cs` - Area buff/debuff skills
- `SimpleResurrectionSkill.cs` - Revival skills
- `SimpleWarpToTargetSkill.cs` - Teleportation skills

### Combat Mechanics
- `Damage/` - Damage calculation and application
- `Buff/` - Status effects and temporary modifications
- `Enums/` - Combat-related enumerations

## 🎯 Combat Features

### Skill System
- Cast time and cooldown mechanics
- Resource consumption (HP, MP, Stamina)
- Movement restrictions during skill usage
- Skill leveling and progression

### Damage System
- Physical and magical damage types
- Critical hits and damage modifiers
- Armor and resistance calculations
- Area-of-effect damage

### Status Effects
- Buffs (positive effects) and debuffs (negative effects)
- Duration-based effects
- Stackable modifications
- Effect removal conditions

## 🔄 Dependencies

- **Characters**: For skill users and targets
- **World**: For combat positioning and range calculations
- **Networking**: For synchronizing combat state across clients
- **UI**: For combat feedback and skill displays

## 📊 Combat Flow

1. **Skill Activation**: Player initiates skill with input
2. **Validation**: Check cooldowns, resources, and requirements
3. **Casting**: Execute cast time and animations
4. **Execution**: Apply skill effects (damage, buffs, etc.)
5. **Resolution**: Update character states and trigger reactions

## 📝 Development Notes

When implementing combat features:
1. Use `BaseSkill` as the foundation for new skill types
2. Consider network synchronization for multiplayer combat
3. Balance skill costs with their effects
4. Test combat interactions thoroughly
5. Profile performance during large-scale battles
