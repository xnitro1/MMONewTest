# Map Server System

This directory contains the map server architecture for MMO functionality, handling world simulation, player interactions, and local game logic.

## 📁 Directory Structure

### Core Components
- `MapNetworkManager.cs` - Main map server networking logic
- `MapSpawnNetworkManager.cs` - Server spawning and management

### World Management
- `Messages/` - Map-specific network messages
  - Spawn/despawn operations
  - Entity synchronization
  - World state updates

### Server Management
- `Messages/Spawn/` - Server spawning coordination
  - `RequestSpawnMapMessage.cs` - Map spawn requests
  - `ResponseSpawnMapMessage.cs` - Spawn confirmations

## 🏗️ Architecture

### Server Roles
- **Map Server**: World instance servers
  - Local world simulation and physics
  - Player interactions within map boundaries
  - NPC AI and behavior
  - Combat resolution and effects
  - Resource management and gathering

### Communication Patterns
- **Players ↔ Map Server**: Local gameplay interactions
- **Map Server ↔ Central Server**: Player transfers, global coordination
- **Map Servers ↔ Map Servers**: Cross-map coordination (future feature)

## 🔄 Key Responsibilities

### World Simulation
- Physics and collision detection
- Entity positioning and movement
- Environmental interactions
- Time-based events and cycles

### Player Management
- Local player state synchronization
- Character actions and abilities
- Inventory and equipment management
- Social interactions (local chat, grouping)

### Combat System
- Real-time combat resolution
- Damage calculation and application
- Status effect management
- Combat logging and statistics

### NPC Management
- AI behavior and decision making
- Spawn/despawn logic
- Interaction responses
- Quest-related behaviors

## 🔗 Dependencies

- **CentralServer**: For global coordination and data
- **Database**: For world state persistence
- **Core**: Game logic and mechanics
- **Networking**: Base networking infrastructure

## 📊 Scalability Considerations

### Performance Optimization
- Spatial partitioning for entity management
- Interest management for network updates
- Object pooling for frequent spawns/despawns
- Load balancing across map instances

### Instance Management
- Dynamic server allocation based on player count
- Seamless player transfers between maps
- World state synchronization
- Memory management for large worlds

## 🎮 Gameplay Features

### Local Interactions
- Player-to-player combat and cooperation
- NPC conversations and quest interactions
- Resource gathering and crafting
- Environmental puzzles and challenges

### World Events
- Scheduled events and activities
- Dynamic content spawning
- Weather and time-of-day effects
- Population-based scaling

## 📝 Development Notes

When working with map servers:
1. Optimize for real-time performance (60+ FPS simulation)
2. Implement efficient spatial queries
3. Consider network bandwidth limitations
4. Test with maximum expected player counts
5. Monitor server resource usage
6. Implement proper error recovery

### Synchronization Challenges
- Client-side prediction for responsive feel
- Server authoritative state management
- Lag compensation techniques
- Fairness in competitive interactions

### Content Management
- Modular world design for easy expansion
- Dynamic content loading/unloading
- Asset streaming and optimization
- World versioning and updates
