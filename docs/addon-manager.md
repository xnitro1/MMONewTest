# NightBlade Addon Manager

**Version 1.0-rc1** - Unity Editor addon marketplace for NightBlade MMO

The Addon Manager provides a clean, professional interface for discovering, installing, and managing NightBlade addons directly within the Unity Editor.

## 🎯 Overview

NightBlade's Addon Manager functions as a private, curated "addon marketplace" that allows developers to extend their games with community-created or official addons without leaving the Unity Editor.

### Key Features
- **🎨 Unity Package Manager-inspired UI** - Clean, professional interface
- **📦 Dynamic Addon Discovery** - Addons loaded from central manifest
- **🔍 Smart Filtering** - Category, publisher, status, and recency filters
- **📁 Organized Structure** - Addons saved to dedicated folders by category
- **📊 Opt-in Analytics** - Anonymous usage statistics (addon popularity only)
- **🔄 Update Management** - Easy updates for installed addons

## 🚀 Getting Started

### Accessing the Addon Manager

1. **Open Unity Editor**
2. **Navigate to**: `NightBlade → Addon Manager`
3. **Browse Available Addons**

### First Time Setup

The Addon Manager automatically:
- Downloads the latest addon manifest
- Loads available addons from the central repository
- Checks for installed addons in your project
- Displays status indicators (installed, updates available, etc.)

## 📋 Interface Guide

### Main Window Layout

```
┌─────────────────────────────────────────────────────────────┐
│ [Logo] NightBlade Addon Manager                    [×]      │
├─────────────────────────────────────────────────────────────┤
│ [Reload] [Category ▼] [Status ▼] [Updated ▼] [Core ▼]       │
├─────────────────────────────────────────────────────────────┤
│ ┌─ Addon List ──────────────────────────────────────────┐  │
│ │ □ Addon Name (v1.0.0) [Install]                       │  │
│ │   Brief description of what this addon does...        │  │
│ │   By: Author Name • Category: Combat                  │  │
│ │                                                        │  │
│ └────────────────────────────────────────────────────────┘  │
├─────────────────────────────────────────────────────────────┤
│ ┌─ Addon Details ────────────────────────────────────────┐  │
│ │ [Screenshot]                                           │  │
│ │ Full description with details about features,          │  │
│ │ requirements, and usage instructions...                │  │
│ │                                                         │  │
│ │ [Install] [Update] [Delete] [View Repository]          │  │
│ └─────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────┘
```

### Status Indicators

| Icon | Status | Description |
|------|--------|-------------|
| ✅ | Installed | Addon is installed and up-to-date |
| 🔄 | Update Available | Newer version available |
| 🆕 | New | Added within the last week |
| 🏆 | Core | Official NightBlade addon |
| 🔥 | Hot | Popular based on download stats |

## 🎮 Using Addons

### Installing Addons

1. **Browse** the addon list or use filters to find what you need
2. **Select** an addon to view details and screenshots
3. **Click "Install"** to download and import the addon
4. **Wait** for the import process to complete
5. **Use** the addon in your project!

### Updating Addons

1. **Look for** addons with the 🔄 Update Available indicator
2. **Select** the addon to see version information
3. **Click "Update"** to install the latest version
4. **Existing addon data is preserved** during updates

### Removing Addons

1. **Select** an installed addon
2. **Click "Delete"** to remove it from your project
3. **Confirm** the deletion when prompted
4. **Addon files are completely removed** from your project

## 🔍 Filtering & Discovery

### Filter Options

- **Category**: Characters, Monsters, NPCs, Combat, Economy, Items, Gameplay, UI, MMO, Tools
- **Publisher**: Core (official) or Community addons
- **Status**: All, Installed, Update Available, Not Installed
- **Recency**: Anytime, This week, This month

### Smart Discovery

The Addon Manager automatically highlights:
- **New addons** added in the last week
- **Popular addons** based on community downloads
- **Updates available** for your installed addons
- **Core addons** from the official NightBlade team

## 📁 Project Structure

Addons are installed to maintain clean project organization:

```
Assets/
├── NightBlade_addons/
│   ├── Characters/
│   │   ├── HeroPack/
│   │   │   ├── Models/
│   │   │   ├── Animations/
│   │   │   ├── Materials/
│   │   │   └── Scripts/
│   │   └── EnemyPack/
│   ├── Combat/
│   │   ├── NewWeapons/
│   │   └── SpellSystem/
│   └── UI/
│       └── CustomInterface/
└── NightBlade_1.95+/
    └── [Main Framework]
```

## 🔒 Security & Validation

### Built-in Security Features

- **Validated Sources**: All addons come from approved repositories
- **Package Integrity**: Unity packages are validated during import
- **Permission System**: Addons cannot access unauthorized Unity APIs
- **Sandbox Environment**: Addons run within Unity's safe execution context

### Data Privacy

- **Anonymous Analytics**: Optional usage statistics (addon downloads only)
- **No Personal Data**: No user information, project data, or sensitive information collected
- **Local Storage**: All addon data stored locally in your project
- **User Control**: You can disable analytics at any time

## 🛠️ For Developers

### Accessing Addon Manager APIs

```csharp
using NightBlade.AddonManager;

// Get installed addons
var installedAddons = AddonManagerWindow.GetInstalledAddons();

// Check for updates
var updatesAvailable = packages.Where(p => p.status == PackageStatus.Outdated);

// Install programmatically
AddonManagerWindow.InstallAddon(selectedPackage);
```

### Integration Points

The Addon Manager provides several integration points:
- **Custom Categories**: Extend the category system
- **Validation Hooks**: Add custom addon validation
- **Install Callbacks**: React to addon installations
- **UI Extensions**: Add custom UI elements to the manager

## 🐛 Troubleshooting

### Common Issues

**"Failed to load manifest"**
- Check your internet connection
- Verify the manifest URL is accessible
- Try the "Reload" button

**"Import failed"**
- Ensure Unity version compatibility
- Check for file permission issues
- Verify the addon package integrity

**"Addon not appearing"**
- Clear filters to show all addons
- Check if the addon is in an allowed category
- Try reloading the addon list

### Getting Help

- **Documentation**: Check this guide and related docs
- **Community**: Join the NightBlade community discussions
- **GitHub Issues**: Report bugs or request features
- **Support**: Contact the addon author directly

## 📊 Analytics & Popularity

### Opt-in Analytics

The Addon Manager collects completely anonymous usage statistics to:
- **Highlight popular addons** in the community
- **Help developers** understand user preferences
- **Improve discovery** of high-quality addons

**What we collect:**
- ✅ Addon download/install counts
- ❌ Personal information
- ❌ Project data
- ❌ Usage patterns
- ❌ System information

**What we don't collect:**
- ❌ Your name or contact information
- ❌ Project names or contents
- ❌ Unity version or system specs
- ❌ Any identifiable information

### Managing Analytics

You can disable analytics at any time:
1. Open Addon Manager settings
2. Toggle "Enable Analytics"
3. Changes take effect immediately

## 🎯 Best Practices

### For Users
- **Read Descriptions**: Understand what each addon does before installing
- **Check Compatibility**: Verify Unity version requirements
- **Backup Projects**: Consider backing up before major addon installations
- **Start Small**: Test addons in development builds first

### For Addon Developers
- **Clear Documentation**: Provide detailed descriptions and usage instructions
- **Version Carefully**: Use semantic versioning for updates
- **Test Thoroughly**: Ensure addons work across different project configurations
- **Support Users**: Provide contact information for addon support

## 📈 Future Roadmap

### Planned Features
- **Bulk Operations**: Install/update multiple addons at once
- **Dependency Management**: Automatic resolution of addon dependencies
- **Rating System**: Community ratings and reviews for addons
- **Private Addons**: Support for private, project-specific addons
- **Auto-Updates**: Optional automatic updates for stable addons

### Community Growth
- **Expanded Categories**: More specific addon categories
- **Creator Tools**: Enhanced tools for addon developers
- **Showcase Events**: Community events to highlight great addons
- **Monetization**: Future support for creator monetization

---

*The Addon Manager represents NightBlade's commitment to community-driven development and extensibility.*

