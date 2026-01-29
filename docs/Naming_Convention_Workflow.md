# Naming Convention Fixing Workflow

## Overview

This document provides a **safe workflow** for fixing naming convention violations identified by the `SafeNamingConventionAuditor` tool.

**⚠️ IMPORTANT: Never use regex-based auto-fix tools on C# code.** They cannot understand syntax context and will corrupt your codebase.

---

## Recommended Workflow

### Step 1: Run the Audit

1. In Unity Editor: `NightBlade > Tools > Safe Naming Convention Audit`
2. Review the violations in the audit window and exported report

### Step 2: Use IDE Rename Refactoring

**For Visual Studio / Rider:**
1. Open the file with a violation
2. Place cursor on the identifier that needs renaming
3. Press `Ctrl + R, Ctrl + R` (or `F2` in Rider)
4. Type the new name following conventions
5. Press Enter - the IDE will rename ALL references safely

**For VS Code with C# extension:**
1. Right-click the identifier
2. Select "Rename Symbol"
3. Type the new name
4. Press Enter

### Step 3: Verify Changes

After renaming:
1. Build the project to ensure no errors
2. Run unit tests if available
3. Commit changes with descriptive message

---

## Why IDE Refactoring is Safe

Your IDE uses a **semantic understanding** of the code:
- ✅ Knows the scope of each identifier
- ✅ Finds all references across files
- ✅ Understands syntax context (type vs field vs parameter)
- ✅ Preserves code structure
- ✅ Can handle partial classes, inheritance, interfaces

Regex-based tools:
- ❌ Just match text patterns
- ❌ Don't understand scope
- ❌ Replace in wrong contexts (types, parameters, comments)
- ❌ Can break syntax

---

## Prioritizing Violations

Focus on high-impact violations first:

### 1. **Public API** (Highest Priority)
- Public classes, methods, properties
- These affect other developers and external code

### 2. **Constants**
- Usually quick to fix
- High visibility violations

### 3. **Properties**
- Often public-facing
- Medium effort

### 4. **Methods**
- Can be time-consuming if many references
- High impact on readability

### 5. **Fields** (Lowest Priority)
- Usually private
- Low external impact
- Can be done incrementally

---

## Batch Fixing Strategy

### Option A: Fix by File
1. Run audit to identify worst files
2. Open one file at a time
3. Fix all violations in that file
4. Test and commit

### Option B: Fix by Type
1. Fix all ClassName violations first
2. Then all MethodName violations
3. Then PropertyName violations
4. Finally FieldName violations

### Option C: Fix by Severity
1. Sort violations by severity (High → Low)
2. Fix critical violations first
3. Work down the list

---

## Git Best Practices

Always commit naming fixes separately from functional changes:

```bash
# Good commit message examples
git commit -m "refactor: Rename PlayerController methods to PascalCase"
git commit -m "style: Fix field naming in DatabaseManager to _camelCase"
git commit -m "refactor: Rename constants to SCREAMING_SNAKE_CASE in GameConfig"
```

---

## Automation with Roslyn (Advanced)

If you need true auto-fix capabilities, consider:

### Roslyn-Based Analyzers
1. Create a custom Roslyn analyzer
2. Use `Microsoft.CodeAnalysis` APIs
3. Implement proper symbol analysis
4. Use built-in rename refactoring APIs

**Estimated effort:** 10-20 hours of development
**Benefit:** Safe, automated fixes across entire codebase

### Third-Party Tools
- **Resharper**: Has powerful rename refactoring
- **Roslynator**: Open-source analyzers and refactorings
- **SonarLint**: Code quality and naming checks

---

## Monitoring Progress

Track your progress:

1. Run audit weekly
2. Record violation counts in a spreadsheet
3. Set goals (e.g., "reduce violations by 500/week")
4. Celebrate milestones!

### Sample Progress Tracker

| Date       | Total Violations | Classes | Methods | Properties | Fields | Constants |
|------------|------------------|---------|---------|------------|--------|-----------|
| 2026-01-23 | 21,516           | 36      | 2,377   | 427        | 7,413  | 360       |
| 2026-01-30 | (target 20,000)  | ...     | ...     | ...        | ...    | ...       |

---

## Getting Help

If you encounter issues:

1. Check if the violation is a false positive
2. Review the naming conventions document: `docs/NAMING_CONVENTIONS.md`
3. Ask the team lead if unsure about a specific case
4. Document exceptions in the conventions doc

---

## Summary

**Safe Path:**
1. ✅ Use `SafeNamingConventionAuditor` to find violations
2. ✅ Use IDE rename refactoring to fix them
3. ✅ Test and commit incrementally
4. ✅ Track progress over time

**Dangerous Path:**
1. ❌ Regex-based auto-fix tools
2. ❌ Manual find-and-replace
3. ❌ Mass changes without testing

**The safe path takes longer, but it's the only way to avoid corrupting your codebase.**

Good luck! 🚀
