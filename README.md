# 🌟 NightBlade

**High-performance MMO framework** with **95%+ GC reduction** and **simple, stable architecture**.

[![Version](https://img.shields.io/badge/version-4.2.1-blue.svg?style=for-the-badge)](docs/Versioning.md)
[![Unity](https://img.shields.io/badge/Unity-6.3%2B-000000?style=for-the-badge&logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-green?style=for-the-badge)](LICENSE)
[![Discord](https://img.shields.io/badge/Discord-Community-5865F2?style=for-the-badge&logo=discord)](https://discord.gg/nightblade)

[📖 Docs](docs/) • [🚀 Quick Start](docs/getting-started.md) • [⚡ Performance](docs/performance.md) • [🎮 Demos](docs/demos.md) • [🌉 Unity Bridge](docs/UNITY_BRIDGE_GUIDE.md)

---

## ⚡ **Performance First**

**95%+ GC Reduction** • **Zero-Allocation Architecture** • **Simple Static Servers**

### 🎯 **Core Optimizations**
- **Advanced Pooling**: 9 pool types (StringBuilder, Collections, Materials, Delegates, FxCollections, Network, JSON)
- **Distance-Based Updates**: 35-55% CPU reduction through intelligent scaling
- **Smart Asset Unloading**: 20-40% memory optimization
- **Network Batching**: 10-20% bandwidth efficiency
- **Real-time Monitoring**: Built-in performance profiling and diagnostics

[📊 Performance Guide](docs/performance.md)

---

## 🆕 **Recent Updates (v4.2.1)**

### 🐛 **Critical Bug Fixes & Performance Improvements**
**Comprehensive quality & stability update:**
- **Critical Fixes**: 11 critical bugs resolved including race conditions, null reference crashes, and data corruption
- **Performance**: 5-10% frame rate improvement from caching optimizations
- **Thread Safety**: Network dictionaries now use ConcurrentDictionary to prevent crashes
- **Error Handling**: 7 async methods now properly log exceptions instead of failing silently
- **Static State Protection**: Runtime detection prevents multi-server data corruption
- **WarpPortal Fix**: Null reference bug preventing portal activation resolved

**Bug Fixes:**
- ⚡ **GetComponent Caching**: Eliminated ~180 allocations/second in Update loops
- ⚡ **Camera.main Caching**: Reduced redundant lookups in skill aiming
- 🛡️ **Combat Null Checks**: Fixed 4 crash scenarios in weapon damage calculations
- 🛡️ **Race Conditions**: Eliminated network dictionary corruption with atomic operations
- 🛡️ **WarpPortal**: Fixed null reference preventing portal interaction
- 🔧 **Production Safety**: Testing flags now default to production-safe values
- 📝 **Code Quality**: Optimized excessive null checking (13 checks → 1 in message manager)

[📋 Full Bug Report](BUG_HUNT_REPORT.md) • [✅ Fixes Summary](FIXES_COMPLETED_SUMMARY.md) • [🏗️ Architecture Guide](docs/Instance_Based_Server_Architecture.md)

---

## 🌉 **Unity Bridge (v4.2.0)**

**Revolutionary AI-assisted development system:**
- **Direct AI Integration**: External AI tools can now query and modify Unity in real-time
- **Runtime Bridge**: Test UI changes instantly during Play mode
- **Editor Bridge**: Make permanent changes to scenes and prefabs
- **Persistence System**: Save successful experiments automatically
- **9 Core Commands**: Complete API for scene inspection and manipulation
- **AI as Development Partner**: Design, debug, and refactor through conversation

**Use Cases:**
- 🎨 **Rapid UI Prototyping**: AI positions elements through iteration
- 🔍 **Live Debugging**: Inspect and fix issues without stopping Play mode
- 🚀 **Batch Operations**: Update hundreds of prefabs automatically
- 🤖 **AI-Assisted Design**: Let AI suggest and apply modern UI layouts

[📖 Unity Bridge Guide](docs/UNITY_BRIDGE_GUIDE.md)

---

## 📋 **Previous Updates (v4.1.0)**

**Thanks to [@Denarius](https://github.com/Denarius) for these important improvements:**

### 🔄 **Core Functionality Restructuring**
- **Disabled non-functional core features** moved to working addon implementations:
  - Player Icons → Available in respective addons ✅ (core files disabled)
  - Player Frames → Available in respective addons ✅ (core files disabled)
  - Player Titles → Available in respective addons ✅ (core files disabled)
  - Unlockable Content → Available in respective addons ✅ (networking disabled)

### 🛠️ **Enhanced DevExt Framework**
- `ClearData()` method added to GameInstance
- `SetClientHandlersRef()` added to BaseGameManager
- `PrepareMapHandlers()` added to MapNetworkManager
- `PrepareLanRpgHandlers()` added to LanRpgNetworkManager
- Mail system enhanced with `HaveItemsToClaim()`, `Serialize()`, and `Deserialize()` methods

### 📦 **First-Class Addon Support**
- Addons can now register client and server handlers (e.g., `IClientUserContentHandlers`)
- Streamlined handler registration without core modifications

### 🌐 **Addon Localization System**
- New `DevExt.LocaleTexts` integration with `DefaultLocale`
- `UIFormatKeysExt` and `UITextKeysExt` for addon-specific text
- Addons can define custom locale texts independently

### 🔧 **Code Quality Improvements**
- **PerformanceMonitor Refactoring**: Massive 1,411-line God Object broken down into 4 focused components
- **89% code reduction** in main monitoring class for better maintainability
- **Improved separation of concerns** between data, rendering, profiling, and coordination
- **Enhanced testability** with modular component architecture

[📋 Full Changelog](CHANGELOG.md) • [📦 Addon Marketplace](docs/addon-manager.md)

---

## 🤝 **Contributing**

We welcome contributions from developers of all skill levels! Whether you're fixing bugs, adding features, or improving documentation.

### 📚 **Getting Started**
- **[Complete Git Guide for Beginners](docs/Git_Contribution_Guide.md)** - Step-by-step instructions from zero Git knowledge
- **[Code Quality Standards](docs/Code_Quality_Issues.md)** - Our coding conventions and best practices
- **[Development Setup](docs/getting-started.md)** - Environment configuration and project structure

### 🛠️ **Development Workflow**
1. **Fork** the repository on GitHub
2. **Clone** your fork locally
3. **Create a feature branch** (`git checkout -b feature/my-feature`)
4. **Make your changes** following our [coding standards](docs/Code_Quality_Issues.md)
5. **Test thoroughly** and ensure no regressions
6. **Commit** with clear messages (`git commit -m "feat: add new combat system"`)
7. **Push** and create a **Pull Request**

### 📋 **Pull Request Guidelines**
- **Title**: Clear, descriptive summary
- **Description**: What changed and why
- **Testing**: How you validated your changes
- **Screenshots**: For UI/visual changes
- **Related Issues**: Link any relevant issues

### 💬 **Need Help?**
- **[GitHub Discussions](https://github.com/your-username/nightblade-mmo/discussions)** - General questions and help
- **[Issues](https://github.com/your-username/nightblade-mmo/issues)** - Bug reports and feature requests
- **[Discord](https://discord.gg/nightblade)** - Real-time community support

---

## 🚀 **Quick Start**

**Unity 6.3+** required. Supports Windows, macOS, Linux, mobile.

```bash
git clone https://github.com/your-username/nightblade-mmo.git
cd nightblade-mmo
# Open in Unity Hub - auto-configures all settings
# Run demo: Assets/NightBlade/Demos/CoreDemo/Scenes/Init
```

**All optimizations activate automatically** - zero configuration needed.

[📖 Setup Guide](docs/getting-started.md) • [🎮 Demo Scenes](docs/demos.md) • [🏗️ Architecture](docs/core-systems.md) • [🛡️ Security](docs/security-validation.md) • [🔧 Troubleshooting](docs/troubleshooting.md) • [🌉 Unity Bridge](docs/UNITY_BRIDGE_GUIDE.md)
