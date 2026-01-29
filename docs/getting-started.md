# Getting Started with NightBlade

This guide will help you get NightBlade up and running quickly. NightBlade features enhanced security, performance optimizations, and a modern architectural approach.

## 🎯 Quick Start (5 minutes)

### Prerequisites
- Unity 6.3 LTS or later
- .NET Framework 4.8 or later
- Git installed on your system
- At least 8GB RAM (16GB recommended for MMO development)

### Step 1: Clone the Repository

```bash
# Clone the NightBlade repository
git clone https://github.com/<your-username>/nightblade-mmo.git
cd nightblade-mmo
```

### Step 2: Open in Unity Hub

1. Open Unity Hub
2. Click "Add" → "Add project from disk"
3. Navigate to the cloned `nightblade-mmo` folder
4. Select the folder and click "Add Project"
5. Unity will automatically detect and open the project

### Step 3: First Time Setup

NightBlade will automatically configure on first open:
- Import all necessary assets
- Configure project settings
- Set up scripting define symbols
- Install required dependencies

### Step 4: Run Your First Demo

1. In the Project window, navigate to `Assets/NightBlade_1.95+/Demos/CoreDemo/Demo/Scenes/`
2. Double-click `00Init` to open it
3. Click the Play button in Unity

Congratulations! You're now running NightBlade.

### Accept Import Prompts

NightBlade may show several import dialogs during first setup:
- **Project Settings**: Click "Import" to apply optimized settings
- **Package Dependencies**: Click "Install/Upgrade" for required packages
- **Security Setup**: Accept the security validation setup

## 🏗️ Understanding the Architecture

Before diving deeper, let's understand NightBlade's structure:

```
Assets/NightBlade_1.95+/
├── Core/              # Foundation systems (always needed)
├── MMO/               # Server/multiplayer features (optional)
├── Demos/             # Example implementations
├── Shared/            # Common assets and configs
├── ThirdParty/        # External integrations
└── Tools/             # Development utilities
```

### Core vs MMO
- **Core**: Essential systems for any ARPG (single-player or local multiplayer)
- **MMO**: Additional systems for internet multiplayer with dedicated servers

## 🎮 Testing Multiplayer (LAN)

### Host a Game

1. With the demo scene open, click Play
2. In the main menu, select "Multiplayer"
3. Click "Host" to start a LAN game
4. Create your character
5. Click "Start Game"

### Join a Game

1. Open a second Unity instance or build
2. Select "Multiplayer" → "Find Games"
3. Select the hosted game from the list
4. Click "Join"
5. Create your character
6. Click "Start Game"

## 🛡️ Security Setup

NightBlade includes enterprise-grade security features. After import:

1. Go to `NightBlade → Setup → Security Validation`
2. This enables:
   - Input sanitization
   - Data validation
   - Network message validation
   - Anti-exploit protections

## ⚡ Performance Optimization

NightBlade comes pre-optimized with advanced performance systems:

1. **Change Detection Physics Sync (v1.95r3)**:
   - 60-80% reduction in `Physics.SyncTransforms()` calls using smart dirty flag system
   - Only syncs when transforms actually change during movement
   - Maintains 100% accuracy for attack detection and lag compensation
   - Integrated with all entity movement systems (players, monsters, vehicles)

2. **Auto-Save Settings**:
   - Default: 30-second intervals (vs 2 seconds in standard MMO frameworks)
   - Only saves when data actually changes

3. **Physics Settings**:
   - Ground alignment: 10 updates/second (configurable)
   - Reduces physics calculations by 83%

4. **Attack System Integration**:
   - Immediate physics sync for combat operations
   - Zero accuracy loss for critical gameplay systems

## 🧪 Testing Your Setup

### Basic Functionality Test
- [ ] Character creation works
- [ ] Movement and camera controls
- [ ] Combat mechanics (attack, take damage)
- [ ] Item pickup and inventory
- [ ] UI dialogs open/close properly

### Security Test
- [ ] Try entering invalid data (negative numbers, SQL injection attempts)
- [ ] Test with corrupted save files
- [ ] Verify network messages are validated

### Performance Test
- [ ] Enable Performance Monitor in GameInstance for real-time tracking
- [ ] Monitor frame rate with multiple characters (F11 for benchmark)
- [ ] Check UI Objects Pooled count (should be 20+ after TMP import)
- [ ] Verify Distance Entities count (3+ per player character)
- [ ] Check auto-save frequency in console logs
- [ ] Verify smooth ground alignment movement
- [ ] Use Performance Monitor GUI shortcuts (F12 toggle, F11 benchmark)

## 🔄 Git Workflow (For Contributors)

If you cloned NightBlade via Git and want to contribute back:

### Development Workflow

```bash
# Create a feature branch
git checkout -b feature/your-feature-name

# Make your changes
# ... edit files ...

# Stage your changes
git add .

# Commit with descriptive message
git commit -m "Add: Brief description of your changes"

# Push your branch
git push origin feature/your-feature-name
```

### Pull Request Guidelines

1. **Branch Naming**: Use `feature/`, `bugfix/`, or `docs/` prefixes
2. **Commit Messages**: Start with action verb (Add:, Fix:, Update:, etc.)
3. **Testing**: Ensure all demos still work before submitting
4. **Documentation**: Update docs for any architectural changes
5. **Security**: Add validation for any new data types

### Staying Updated

```bash
# Fetch latest changes
git fetch origin

# Merge main branch updates
git merge origin/main

# Or rebase for cleaner history
git rebase origin/main
```

## 🚀 Next Steps

### Choose Your Path

**For Single-Player Games:**
- Focus on `Core/` systems
- Use `Demos/CoreDemo/` as starting point
- Customize characters, combat, and economy

**For Multiplayer Games:**
- Include both `Core/` and `MMO/` systems
- Study `Demos/SurvivalDemo/`
- Learn server architecture and database setup

**For MMO Games:**
- Full `Core/` + `MMO/` integration
- Study server deployment and scaling
- Implement authentication and monetization

### Essential Reading
1. [Architecture Overview](architecture.md) - Understand the big picture
2. [Core Systems](core-systems.md) - Learn the foundation systems
3. [Security & Validation](security-validation.md) - Master the security features
4. [Demo Scenes](demos.md) - See working examples

### Development Tips
- **Start Small**: Begin with one demo scene and modify it
- **Use Validation**: Always add validation for new data types
- **Monitor Performance**: Use built-in profiling tools
- **Read the Code**: NightBlade is designed to be readable and self-documenting

## 🔧 Build Configuration

### For Development Builds
1. Go to `File → Build Settings`
2. Add these scenes to build:
   - `00Init` (first scene)
   - `01Home`
   - `Map01`
   - `Map02`
3. Go to `NightBlade → Setup → Development Build`
4. Build normally

### For Production Builds
1. Go to `NightBlade → Setup → Production Build`
2. This enables:
   - Optimized validation (runtime checks disabled)
   - Performance optimizations
   - Security hardening
3. Build for your target platform

## 🐛 Common Issues

### Import Errors
**Problem**: Package import fails
**Solution**: Ensure Unity version compatibility, restart Unity, try importing again

### Missing Scripts
**Problem**: Components show "Missing Script"
**Solution**: Go to `NightBlade → Setup → Reassign Scripts`

### Performance Issues
**Problem**: Game runs slowly
**Solution**: Check `NightBlade → Performance → Diagnostics` for bottlenecks

### Security Warnings
**Problem**: Validation errors in console
**Solution**: This is normal during development. Errors indicate potential exploits being blocked.

## 📞 Getting Help

- **Documentation**: Check all `.md` files in `Assets/NightBlade_1.95+/`
- **Demos**: Each demo includes working examples
- **Code Comments**: NightBlade code is extensively documented
- **Issue Tracking**: Report bugs with detailed reproduction steps

## 🎯 What's Different from Conventional MMO Frameworks

NightBlade includes several enhancements over conventional MMO frameworks:

### Architecture Modernization (v1.95r3)
- **3D-Only Framework**: Complete conversion from 2D/3D hybrid to pure 3D system
- **Eliminated Complexity**: Removed 100+ conditional checks per frame
- **Unified Physics**: Single Physics system (no more Physics2D overhead)
- **Streamlined Codebase**: 80+ files refactored for consistency

### Security Enhancements
- **Data Validation**: Comprehensive input sanitization and integrity checks
- **Network Security**: All messages validated before processing
- **Anti-Exploit**: Protection against common multiplayer exploits

### Performance Improvements
- **Dual Physics Removal**: ~30% CPU reduction from eliminating Physics2D system
- **Smart Auto-Save**: 90% reduction in save operations (30s intervals + change detection)
- **Physics Optimization**: 83% fewer raycast operations + 60-80% fewer sync calls
- **Change Detection Sync**: Intelligent physics synchronization system
- **Fast Validation**: 10x faster string processing

### Developer Experience
- **AI-Assisted Development**: Human-AI collaborative refactoring completed
- **Comprehensive Documentation**: Detailed guides and API references
- **Built-in Tools**: Performance monitoring and debugging utilities
- **Multiple Demos**: Reference implementations for different 3D game types
- **Future-Ready**: Architectural foundation for addon manager system

## 📦 Alternative: Package Import

If you have a pre-built NightBlade package (.unitypackage) instead of using Git:

1. Create a new 3D Unity project
2. Go to `Window → Package Manager`
3. Click `+` → `Add package from disk...`
4. Select your NightBlade package file
5. Follow Steps 3-4 above for setup and demo running

**Note**: Git clone is recommended for development and staying updated with the latest changes.

Now that you're set up, dive into the [Core Systems](core-systems.md) documentation to start building your game!

