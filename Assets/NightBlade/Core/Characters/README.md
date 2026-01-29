# Characters System

This directory contains the core character management system, including character data structures, progression mechanics, and character-related functionality.

## 📁 Directory Structure

### Character/
Core character classes and base implementations:
- `BaseCharacter.cs` - Abstract base class for all characters
- `PlayerCharacter.cs` - Player character implementation
- `MonsterCharacter.cs` - Monster/NPC character implementation

### CharacterData/
Character data management and persistence:
- `CharacterData.cs` - Main character data container
- `ICharacterData.cs` - Character data interface
- `CharacterInventoryExtensions.cs` - Inventory management for characters
- `CharacterRelatesData/` - Character relationships and stats
- `SerializationSurrogates/` - Data serialization helpers

### Jobs/
Character job/class system:
- `CharacterJob.cs` - Job definitions and mechanics

## 🔄 Key Systems

### Character Progression
- Leveling and experience systems
- Attribute and stat management
- Job/class specialization

### Character Data Management
- Persistent character state
- Inventory and equipment
- Quest progress tracking
- Social relationships

### Character Types
- Player characters with full progression
- Monster characters for combat
- NPC characters for interaction

## 🔗 Dependencies

- **World**: For character positioning and environment interaction
- **Economy**: For inventory and currency management
- **Gameplay**: For quest and progression systems
- **Combat**: For character combat mechanics

## 📝 Development Notes

When working with the character system:
1. Use `ICharacterData` for data access patterns
2. Extend `BaseCharacter` for new character types
3. Character data should be serializable for persistence
4. Consider performance impact of large character data structures
