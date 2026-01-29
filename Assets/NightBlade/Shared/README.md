# Shared Resources

This directory contains assets, scripts, and configuration that are shared across multiple parts of the project. These resources provide common functionality and avoid code duplication.

## 📁 Directory Structure

### Assets/
Shared Unity assets used across demos and systems:
- **Animations/** - Common animation clips and controllers
- **Materials/** - Shared materials and shaders
- **Models/** - Common 3D models and prefabs
- **Textures/** - Shared textures and sprites
- **Audio/** - Common sound effects and music

### Scripts/
Cross-platform scripts and utilities:
- **UI/** - Common UI components and helpers
- **Utils/** - General-purpose utility functions
- **Extensions/** - C# extension methods
- **DataStructures/** - Common data structures

### Config/
Shared configuration files:
- **NetworkSettings/** - Common network configurations
- **GameConstants/** - Global game constants
- **BuildSettings/** - Build and deployment configs

## 🔄 Usage Guidelines

### When to Use Shared Resources
- Assets used by multiple demos or systems
- Utilities needed across different modules
- Configuration that affects multiple components
- Common UI elements or behaviors

### When NOT to Use Shared Resources
- Demo-specific assets or scripts
- System-specific implementations
- Temporary or experimental code
- Platform-specific functionality

## 📦 Organization Principles

### Asset Naming
- Use clear, descriptive names
- Include category prefixes (e.g., `UI_Button_`, `FX_Explosion_`)
- Version assets when necessary

### Script Structure
- Use namespaces that reflect functionality
- Keep classes focused on single responsibilities
- Document public APIs thoroughly

### Configuration Management
- Use ScriptableObjects for Unity-serializable configs
- Implement validation for config values
- Support runtime config reloading

## 🔗 Dependencies

Shared resources should have minimal dependencies:
- Avoid depending on specific game systems
- Use interfaces for extensibility
- Support optional features gracefully

## 📝 Maintenance Guidelines

### Adding New Shared Resources
1. **Evaluate Need**: Confirm it's truly shared across multiple systems
2. **Documentation**: Add clear usage documentation
3. **Testing**: Test across all consuming systems
4. **Versioning**: Consider backward compatibility

### Updating Shared Resources
1. **Impact Assessment**: Identify all affected systems
2. **Migration Plan**: Provide upgrade path for breaking changes
3. **Communication**: Notify team of changes
4. **Testing**: Validate across all use cases

## 🎯 Best Practices

- **Modularity**: Keep shared components loosely coupled
- **Performance**: Optimize for frequent use
- **Extensibility**: Design for future expansion
- **Documentation**: Maintain comprehensive docs
- **Testing**: Include unit tests for utilities