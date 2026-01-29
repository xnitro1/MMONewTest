# 🔍 Safe Naming Convention Auditor

## ⚠️  SAFETY FIRST
This tool **ONLY AUDITS** your code. It **NEVER** modifies or changes any files automatically. You are completely safe to run this tool!

## What it does:
- Scans C# files in your NightBlade project
- Identifies naming convention violations
- Shows suggestions for improvements
- Exports detailed reports
- Helps you maintain consistent code style

## What it does NOT do:
- ❌ Modify your code
- ❌ Rename variables
- ❌ Change method names
- ❌ Auto-fix anything

## How to use:

### Option 1: Unity Editor Window
1. Go to `NightBlade/Tools/Safe Naming Convention Audit`
2. Configure scan settings (path, subfolders, etc.)
3. Click "🔍 Scan for Violations"
4. Review results and use "📍 Go To" to jump to violations
5. Export reports if needed

### Option 2: Console Audit
1. Go to `NightBlade/Tools/Run Safe Naming Audit (Console)`
2. Check Unity console for detailed results
3. Use "📍 Go To" buttons in the editor window for violations

## Safety Features:
- 🔒 **Scope Limited**: Only scans `Assets/NightBlade` folder
- 🚫 **Third-Party Safe**: Never touches third-party libraries
- 🛡️ **Self-Protected**: Excludes editor tools (including itself)
- 👁️ **Read-Only**: Audit-only, no file modifications
- ⚡ **Fast & Safe**: Quick scanning with no risk

## Naming Conventions Checked:
- **Classes/Structs/Interfaces**: PascalCase
- **Methods/Properties**: PascalCase
- **Fields**: camelCase (private with underscore prefix)
- **Constants**: SCREAMING_SNAKE_CASE
- **File Names**: PascalCase

## Export Reports:
Use the "📋 Export Report" button to save detailed violation reports to text files for review or sharing with your team.

---
**Remember: This tool is your coding assistant, not your code editor!** 🤝