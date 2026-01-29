# Versioning Scheme

NightBlade uses [Semantic Versioning 2.0.0](https://semver.org/) for clear, predictable version numbering.

## Format
```
MAJOR.MINOR.PATCH[-PRERELEASE][+BUILD]

Examples:
4.0.0        # Stable release
4.0.1        # Bug fix
4.1.0        # New feature (backward compatible)
5.0.0        # Breaking changes
4.0.0-alpha  # Pre-release
4.0.0-beta.1 # Beta release
```

## Release Types
- **MAJOR**: Breaking changes, API modifications
- **MINOR**: New features, enhancements (backward compatible)
- **PATCH**: Bug fixes, security updates (backward compatible)

## Current Version
**4.2.1** - Critical Bug Fixes & Performance Improvements

## Release History
- [4.2.1] - Critical bug fixes, performance improvements, static state protection, WarpPortal fix
- [4.2.0] - Unity Bridge system for AI-assisted development and direct Unity manipulation
- [4.1.0] - Complete pooling ecosystem, FxCollection pooling, Network/JSON pooling
- [4.0.1] - Advanced pooling systems, StringBuilder/Collection/Material/Delegate pooling
- [4.0.0] - Enterprise-grade MMO framework with performance optimizations

## Branching Strategy
- `main` - Stable releases
- `develop` - Development branch
- `feature/*` - Feature branches
- `hotfix/*` - Critical fixes

## Pre-release Identifiers
- `alpha` - Early testing, may have bugs
- `beta` - Feature complete, minor fixes only
- `rc` - Release candidate, final testing

## Build Metadata
Optional build information appended with `+`:
```
4.1.0+build.1.sha.abc123
```

## Compatibility
- Versions with same MAJOR version are API compatible
- MINOR and PATCH versions are backward compatible
- Pre-release versions may break compatibility

## Support Policy
- Latest MINOR.PATCH version receives active support
- Security fixes provided for last 2 MAJOR versions
- Critical bugs fixed in supported versions