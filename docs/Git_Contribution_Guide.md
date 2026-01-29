# 🚀 Git Contribution Guide for NightBlade

**Complete Beginner's Guide to Contributing to NightBlade using Git**

Welcome to the NightBlade project! This guide will walk you through everything you need to know to contribute code, report issues, and collaborate with the development team. No prior Git experience required!

---

## 📋 Table of Contents

- [What is Git?](#-what-is-git)
- [Setting Up Git](#-setting-up-git)
- [Getting the NightBlade Code](#-getting-the-nightblade-code)
- [Understanding Branches](#-understanding-branches)
- [Making Your First Changes](#-making-your-first-changes)
- [Writing Good Commit Messages](#-writing-good-commit-messages)
- [Creating a Pull Request](#-creating-a-pull-request)
- [NightBlade Coding Standards](#-nightblade-coding-standards)
- [Getting Help](#-getting-help)

---

## 🤔 What is Git?

**Git** is like a time machine for your code! It helps you:
- **Track changes** - See what changed and when
- **Collaborate** - Work with other developers
- **Experiment safely** - Try new features without breaking things
- **Backup your work** - Never lose your code

Think of it as Google Docs for code - multiple people can work on the same project simultaneously.

---

## 🛠️ Setting Up Git

### Step 1: Download Git

**Windows:**
1. Go to [git-scm.com](https://git-scm.com/)
2. Click "Download for Windows"
3. Run the installer (accept all defaults)

**macOS:**
```bash
# Install using Homebrew (recommended)
brew install git

# Or download from: https://git-scm.com/download/mac
```

**Linux:**
```bash
# Ubuntu/Debian
sudo apt update
sudo apt install git

# CentOS/RHEL/Fedora
sudo yum install git  # or dnf install git
```

### Step 2: Configure Git

Open a terminal/command prompt and run:

```bash
# Tell Git who you are (replace with your info)
git config --global user.name "Your Full Name"
git config --global user.email "your.email@example.com"

# Set your preferred text editor (optional)
git config --global core.editor "code --wait"  # For VS Code
# Or "notepad" for Windows, "nano" for Linux, etc.
```

### Step 3: Verify Installation

```bash
git --version
# Should show: git version 2.x.x
```

---

## 📥 Getting the NightBlade Code

### Step 1: Find the Repository

The NightBlade code lives on GitHub at:
**https://github.com/your-username/nightblade-mmo**

### Step 2: Clone (Download) the Code

```bash
# Create a folder for your projects
mkdir MyProjects
cd MyProjects

# Clone the NightBlade repository
git clone https://github.com/your-username/nightblade-mmo.git
cd nightblade-mmo
```

**What just happened?**
- Downloaded all the NightBlade code to your computer
- Created a Git "repository" (repo) on your machine
- Connected your local repo to the GitHub repo

### Step 3: Open in Unity

1. Open Unity Hub
2. Click "Add" → "Add project from disk"
3. Navigate to `MyProjects/nightblade-mmo`
4. Open the project

---

## 🌿 Understanding Branches

**Branches** are like parallel universes for your code. They let you work on features without affecting the main codebase.

### Main Branches

- **`main`** - The stable, released version (don't touch!)
- **`develop`** - Latest development work

### Your Branches

When working on a feature, create your own branch:

```bash
# Create and switch to a new branch
git checkout -b feature/my-awesome-feature

# Example branch names:
git checkout -b feature/add-new-weapon
git checkout -b bugfix/fix-crash-on-startup
git checkout -b docs/update-contribution-guide
```

### Switching Between Branches

```bash
# Switch to main branch
git checkout main

# Switch to develop branch
git checkout develop

# Switch to your feature branch
git checkout feature/my-awesome-feature

# See all branches
git branch -a
```

---

## 🔧 Making Your First Changes

### Step 1: Create a Feature Branch

```bash
# Always start from develop
git checkout develop
git pull  # Get latest changes

# Create your feature branch
git checkout -b feature/my-first-contribution
```

### Step 2: Make Your Changes

1. **Open the project in Unity**
2. **Make your code changes**
3. **Test thoroughly** (we'll cover testing later)
4. **Save all files**

### Step 3: Check What Changed

```bash
# See what files you changed
git status

# See exactly what changed in each file
git diff
```

### Step 4: Stage Your Changes

```bash
# Add all changed files
git add .

# Or add specific files
git add Assets/MyScript.cs
git add Assets/MyPrefab.prefab
```

### Step 5: Commit Your Changes

```bash
# Commit with a descriptive message
git commit -m "Add new weapon system with proper validation"
```

---

## 📝 Writing Good Commit Messages

**Bad commit messages:**
```
fix bug
update code
changes
```

**Good commit messages:**
```
feat: add dual-wield combat system

- Implement weapon switching mechanics
- Add animation state handling
- Include proper validation checks

fix: resolve crash when opening inventory

- Add null check for item data
- Prevent array out of bounds error
- Add error logging for debugging

docs: update contribution guide for beginners

- Add step-by-step Git setup instructions
- Include common troubleshooting tips
- Add examples for all major workflows
```

### Commit Message Format

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat:` - New feature
- `fix:` - Bug fix
- `docs:` - Documentation
- `style:` - Code style changes
- `refactor:` - Code restructuring
- `test:` - Testing
- `chore:` - Maintenance

---

## 🔄 Creating a Pull Request

### Step 1: Push Your Branch

```bash
# Push your branch to GitHub
git push origin feature/my-awesome-feature
```

### Step 2: Create Pull Request on GitHub

1. **Go to the NightBlade repository on GitHub**
2. **Click "Pull requests" tab**
3. **Click "New pull request"**
4. **Select your branch** (compare: `feature/my-awesome-feature`)
5. **Fill out the PR template:**
   - **Title**: Clear, descriptive title
   - **Description**: What you changed and why
   - **Testing**: How you tested your changes
   - **Screenshots**: If UI changes
   - **Related Issues**: Link any related issues

### Step 3: Wait for Review

- **Automated checks** will run (compilation, tests)
- **Team members** will review your code
- **Address feedback** by making more commits to your branch
- **Get approved** and merged!

---

## 📏 NightBlade Coding Standards

### File Naming

**Scripts:**
```csharp
// ✅ GOOD
PlayerController.cs
InventoryManager.cs
WeaponSystem.cs

// ❌ BAD
playercontroller.cs
inventory_manager.cs
WeaponSystemScript.cs
```

**Prefabs & Assets:**
```csharp
// ✅ GOOD
PlayerCharacter.prefab
MainCamera.prefab
UI_InventoryPanel.prefab

// ❌ BAD
player.prefab
Camera.prefab
inventory panel.prefab
```

### Code Style

#### Classes and Methods

```csharp
// ✅ GOOD
public class PlayerController : MonoBehaviour
{
    private float moveSpeed = 5.0f;

    public void MovePlayer(Vector3 direction)
    {
        // Implementation
    }

    private void UpdateMovement()
    {
        // Implementation
    }
}

// ❌ BAD
public class playerController : MonoBehaviour
{
    private float movespeed = 5.0f;

    public void moveplayer(Vector3 direction)
    {
        // Implementation
    }

    private void updatemovement()
    {
        // Implementation
    }
}
```

#### Naming Conventions

- **Classes**: `PascalCase` (PlayerController)
- **Methods**: `PascalCase` (MovePlayer)
- **Properties**: `PascalCase` (CurrentHealth)
- **Private fields**: `_camelCase` with underscore (_moveSpeed)
- **Local variables**: `camelCase` (playerPosition)
- **Constants**: `SCREAMING_SNAKE_CASE` (MAX_HEALTH)

#### Code Structure

```csharp
// ✅ GOOD: Clear structure
public class InventorySystem : MonoBehaviour
{
    // Constants
    private const int MAX_INVENTORY_SLOTS = 50;

    // Serialized fields
    [SerializeField] private int maxWeight = 100;

    // Private fields
    private List<Item> items = new List<Item>();

    // Properties
    public int CurrentWeight => CalculateWeight();

    // Unity methods
    private void Awake()
    {
        InitializeInventory();
    }

    private void Update()
    {
        HandleInput();
    }

    // Public methods
    public bool AddItem(Item item)
    {
        if (CanAddItem(item))
        {
            items.Add(item);
            return true;
        }
        return false;
    }

    // Private methods
    private bool CanAddItem(Item item)
    {
        return CurrentWeight + item.Weight <= maxWeight;
    }

    private int CalculateWeight()
    {
        return items.Sum(item => item.Weight);
    }
}
```

#### XML Documentation

```csharp
/// <summary>
/// Manages player inventory system with weight limits.
/// </summary>
public class InventorySystem : MonoBehaviour
{
    /// <summary>
    /// Adds an item to the inventory if there's space.
    /// </summary>
    /// <param name="item">The item to add</param>
    /// <returns>True if item was added successfully</returns>
    public bool AddItem(Item item)
    {
        // Implementation
    }
}
```

### Error Handling

```csharp
// ✅ GOOD: Proper error handling
public bool LoadPlayerData(string playerId)
{
    try
    {
        var data = LoadDataFromFile(playerId);
        if (data == null)
        {
            Debug.LogError($"Failed to load data for player {playerId}");
            return false;
        }

        ApplyPlayerData(data);
        return true;
    }
    catch (System.Exception ex)
    {
        Debug.LogError($"Error loading player data: {ex.Message}");
        return false;
    }
}

// ❌ BAD: Silent failures
public void LoadPlayerData(string playerId)
{
    var data = LoadDataFromFile(playerId);
    ApplyPlayerData(data); // Might crash if data is null
}
```

---

## 🧪 Testing Your Changes

### Unity Testing

1. **Open the project in Unity**
2. **Press Play** to test in the editor
3. **Check the Console** for errors
4. **Test edge cases** (empty inventories, max values, etc.)

### Manual Testing Checklist

- [ ] **Compile successfully** - No errors in Unity
- [ ] **Basic functionality works** - Feature does what it's supposed to
- [ ] **No crashes** - Test with various inputs
- [ ] **UI looks correct** - If visual changes
- [ ] **Performance acceptable** - No major frame rate drops

### Code Review Checklist

Before submitting your PR:

- [ ] **Naming conventions** followed
- [ ] **XML documentation** added for public methods
- [ ] **No magic numbers** (use named constants)
- [ ] **Proper error handling** implemented
- [ ] **Code is readable** and well-commented
- [ ] **No unnecessary code** (debug logs, commented code)

---

## 🆘 Getting Help

### Where to Ask Questions

1. **GitHub Issues**: For bugs and feature requests
2. **GitHub Discussions**: For general questions
3. **Discord**: Real-time chat for quick questions

### Common Issues & Solutions

#### "Permission denied" when pushing
```bash
# Make sure you're pushing to your fork, not the main repo
git remote -v  # Check your remotes
git push origin feature/my-branch
```

#### Merge conflicts
```bash
# Pull latest changes first
git checkout develop
git pull origin develop

# Then merge into your branch
git checkout feature/my-branch
git merge develop

# Resolve conflicts in Unity, then:
git add .
git commit -m "Resolve merge conflicts"
```

#### Lost commits
```bash
# See recent commits
git log --oneline -10

# If you need to undo a commit
git reset --soft HEAD~1  # Keep changes
git reset --hard HEAD~1  # Discard changes
```

### Learning Resources

- **Git Documentation**: https://git-scm.com/doc
- **GitHub Guides**: https://guides.github.com/
- **Unity Learn**: https://learn.unity.com/
- **NightBlade Wiki**: [Link to project wiki]

---

## 🎯 Quick Reference

### Essential Commands

```bash
# Get started
git clone <repository-url>
git checkout -b feature/my-feature

# Daily workflow
git status              # Check current state
git add .              # Stage all changes
git commit -m "message" # Save changes
git push origin branch  # Share with team

# Stay updated
git checkout develop
git pull origin develop
git checkout my-branch
git merge develop
```

### NightBlade-Specific

- **Use PascalCase** for classes, methods, properties
- **Use _camelCase** for private fields
- **Add XML docs** for public APIs
- **Test thoroughly** before submitting PRs
- **Follow the commit message format**

---

## 🎉 Congratulations!

You've completed the NightBlade contribution guide! You're now ready to:

- ✅ **Contribute code** to the project
- ✅ **Follow proper Git workflows**
- ✅ **Write clean, maintainable code**
- ✅ **Collaborate effectively** with the team

Remember: **Every expert was once a beginner**. Your contributions, no matter how small, are valuable to the project!

**Happy coding!** 🚀💻✨

---

*This guide is maintained by the NightBlade development team. Found an error or want to improve it? Submit a pull request!* 😉