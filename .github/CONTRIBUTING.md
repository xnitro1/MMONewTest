# Contributing to NightBlade

Thank you for your interest in contributing to NightBlade! We welcome contributions from developers of all skill levels and backgrounds. This document provides guidelines and information for contributors.

> **🎓 New to Git or Contributing?** Start with our [Complete Git Contribution Guide for Beginners](docs/Git_Contribution_Guide.md) - step-by-step instructions from zero experience required!

## 🚀 Quick Start

1. **Fork** the repository on GitHub
2. **Clone** your fork locally: `git clone https://github.com/your-username/nightblade-mmo.git`
3. **Create** a feature branch: `git checkout -b feature/your-feature-name`
4. **Make** your changes following our guidelines
5. **Test** thoroughly across demo scenes
6. **Commit** with clear messages: `git commit -m "feat: add amazing feature"`
7. **Push** to your branch: `git push origin feature/your-feature-name`
8. **Open** a Pull Request on GitHub

## 📋 Contribution Guidelines

### 🏗️ Architecture Principles

NightBlade follows a **layered architecture** with clear boundaries:

```
┌─────────────────────────────────┐
│    Performance Layer (Auto)     │ ← Optimization systems
├─────────────────────────────────┤
│         Application Layer       │ ← Your game logic & demos
├─────────────────────────────────┤
│       MMO Layer (Optional)      │ ← Server, database, multiplayer
├─────────────────────────────────┤
│         Core Layer              │ ← Characters, combat, economy
├─────────────────────────────────┤
│ Infrastructure Layer (Secure)   │ ← Networking, validation, utils
└─────────────────────────────────┘
```

**Keep changes within appropriate layer boundaries** to maintain clean separation of concerns.

### 🛡️ Security Requirements

- **Add validation** for any new data types or network messages
- **Follow existing patterns** for input sanitization
- **Test exploit scenarios** for new features
- **Document security implications** in PR descriptions

### 📚 Documentation Standards

- **Update documentation** for any architectural changes
- **Add code comments** for complex logic (especially performance optimizations)
- **Update API docs** for public method changes
- **Add examples** for new features in demo scenes

### 🧪 Testing Requirements

- **Test across all demo scenes** to ensure compatibility
- **Verify performance impact** using built-in monitoring tools
- **Test multiplayer scenarios** for network-related changes
- **Validate security** with invalid inputs and edge cases

## 🎯 Development Workflow

### Branch Naming Convention

```
feat/feature-name          # New features
fix/bug-description       # Bug fixes
docs/documentation-update # Documentation updates
refactor/component-name   # Code refactoring
perf/optimization-name    # Performance improvements
security/fix-description  # Security fixes
```

### Commit Message Format

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New features
- `fix`: Bug fixes
- `docs`: Documentation
- `style`: Code style changes
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Testing
- `chore`: Maintenance

**Examples:**
```
feat(ui): add character selection screen
fix(network): resolve player sync desync issue
docs(performance): update optimization guide
perf(rendering): optimize particle system pooling
```

### Pull Request Process

1. **Create descriptive PR title** following commit conventions
2. **Provide detailed description** including:
   - What changes were made
   - Why they were necessary
   - How to test the changes
   - Any breaking changes
   - Performance impact (if applicable)

3. **Reference related issues** with `#issue-number`
4. **Request review** from maintainers
5. **Address feedback** promptly

## 🛠️ Development Environment

### Prerequisites

- **Unity 6.3 LTS** or later
- **.NET Framework 4.8** or later
- **Git** for version control
- **16GB RAM** recommended for MMO development

### Project Setup

```bash
git clone https://github.com/your-username/nightblade-mmo.git
cd nightblade-mmo
# Open in Unity Hub - auto-configuration enabled
```

### Recommended Tools

- **Visual Studio 2022** or **VS Code** with C# extensions
- **GitHub Desktop** or **GitKraken** for Git management
- **Unity Hub** for project management
- **Rider** (JetBrains) for advanced Unity development

## 🎮 Testing Your Changes

### Demo Scene Testing

NightBlade includes multiple demo scenes for testing:

1. **CoreDemo**: Basic gameplay systems
2. **MMODemo**: Multiplayer functionality
3. **SurvivalDemo**: Survival mechanics
4. **GuildWarDemo**: Guild warfare systems

**Test your changes across all relevant demo scenes** before submitting.

### Performance Validation

Use NightBlade's built-in performance monitoring:

1. Open **Performance Monitor** window
2. Enable **Real-time Tracking**
3. Monitor **CPU**, **Memory**, and **Network** usage
4. Compare before/after your changes

### Security Testing

Test for common exploits:
- **Invalid input validation**
- **Buffer overflows**
- **SQL injection** (if database code)
- **Unauthorized access**
- **Race conditions**

## 📝 Code Style Guidelines

### C# Standards

- **Use modern C# features** (async/await, pattern matching, etc.)
- **Follow Unity conventions** for component lifecycle methods
- **Use meaningful variable names** and clear method signatures
- **Add XML documentation** for public APIs

### Unity-Specific Guidelines

- **Use Addressables** for asset management
- **Implement object pooling** for frequently spawned objects
- **Use the Input System** instead of legacy input
- **Follow URP best practices** for rendering

### Performance Considerations

- **Profile before optimizing** - measure actual impact
- **Use appropriate data structures** for performance-critical code
- **Avoid allocations in Update/FixedUpdate** methods
- **Consider cache locality** for frequently accessed data

## 📞 Getting Help

### Community Support

- **GitHub Discussions**: General questions and community support
- **GitHub Issues**: Bug reports and feature requests
- **Discord**: Real-time community chat (https://discord.gg/nightblade)
- **Documentation**: Comprehensive guides in `/docs` folder

### When to Ask for Help

- **Architecture decisions** for complex changes
- **Performance optimization** strategies
- **Security implementation** guidance
- **Integration questions** for third-party systems

## 🙏 Recognition

Contributors are recognized in several ways:

- **GitHub Contributors** list on repository
- **Changelog** entries for significant contributions
- **Credits** in documentation for major features
- **Community recognition** in Discord and discussions

## 📄 License

By contributing to NightBlade, you agree that your contributions will be licensed under the same MIT License that covers the project.

---

**Thank you for contributing to NightBlade!** Your efforts help make Unity MMO development more accessible and powerful for developers worldwide. 🚀