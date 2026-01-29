# 🔄 UI Change Persistence System

## 🎯 Problem Solved

**Unity loses all runtime changes when you exit Play mode!**

This system uses **ScriptableObjects** to persist changes made at runtime and automatically apply them to your scene/prefabs when you exit Play mode.

---

## 🚀 Quick Start

### 1. Create the Change Log Asset

1. Right-click in Project window
2. `Create > NightBlade > UI Change Log`
3. Name it `UIChangeLog`
4. Place it in `Assets/NightBlade/Resources/` (or anywhere convenient)

### 2. Assign to UnityBridge

1. Find the `UnityBridge` GameObject in your scene
2. Drag the `UIChangeLog` asset into the `Change Log` field

### 3. Configure Settings

In the `UIChangeLog` asset Inspector:
- ✅ **Auto Apply On Exit Play Mode** - Automatically apply changes when stopping Play mode
- ✅ **Clear After Apply** - Clear the log after applying (recommended)

---

## 🎮 How It Works

### **During Play Mode:**

```json
// Move UI element (with "record": true)
{
  "type": "MoveGameObject",
  "parameters": {
    "object": "HealthBar",
    "x": 100,
    "y": 50,
    "z": 0,
    "record": true  ← This records the change!
  }
}

// Change component value
{
  "type": "SetComponentValue",
  "parameters": {
    "object": "HealthBar",
    "component": "Image",
    "field": "color",
    "value": {"r": 1, "g": 0, "b": 0, "a": 1}
  }
}

// Then record it separately
{
  "type": "RecordChange",
  "parameters": {
    "object": "HealthBar",
    "component": "Image",
    "field": "color",
    "value": {"r": 1, "g": 0, "b": 0, "a": 1}
  }
}
```

Changes are **immediately saved to the ScriptableObject** (even during Play mode!)

### **When Exiting Play Mode:**

The `UIChangeApplicator` automatically:
1. ✅ Reads all changes from `UIChangeLog`
2. ✅ Finds the objects in the scene
3. ✅ Applies the changes using `Undo` (so you can undo if needed!)
4. ✅ Marks scene as dirty
5. ✅ Clears the log (if enabled)

**Your changes are now permanent!** 🎉

---

## 📋 Supported Change Types

| Type | Description | Example |
|------|-------------|---------|
| **Position** | Move GameObject | `transform.position = (100, 50, 0)` |
| **Scale** | Resize GameObject | `transform.localScale = (1.5, 1.5, 1)` |
| **Rotation** | Rotate GameObject | `transform.rotation = Quaternion.Euler(0, 45, 0)` |
| **Float** | Numeric values | `lerpSpeed = 10.0` |
| **Bool** | True/False | `isVisible = true` |
| **String** | Text values | `text = "100 HP"` |
| **Vector3** | 3D vectors | `offset = (5, 10, 0)` |
| **Vector2** | 2D vectors | `anchoredPosition = (100, 50)` |
| **Color** | RGBA colors | `color = Color.red` |

---

## 🔧 Manual Control

If `autoApplyOnExitPlayMode` is disabled, you can manually apply changes:

**Menu:** `NightBlade > UI > Apply Recorded Changes`

This is useful if you want to:
- Review changes before applying
- Apply changes selectively
- Test without auto-apply

---

## 🎨 Workflow Example

### Typical Design Session:

1. **Enter Play Mode** ▶️
2. **Use AI Bridge** to move/tweak UI:
   ```json
   {"type": "MoveGameObject", "object": "HealthBar", "x": 100, "y": 80, "record": true}
   {"type": "SetComponentValue", "object": "HealthBar", "component": "Image", "field": "color", "value": {"r": 0, "g": 1, "b": 0, "a": 1}}
   ```
3. **Test in game** - See changes live!
4. **Exit Play Mode** ⏹️
5. **Changes are applied automatically!** ✅
6. **Save scene** (Ctrl+S)

**Done!** Your UI is updated! 🎉

---

## 🐛 Troubleshooting

### Changes Not Applied?

- ✅ Is `UIChangeLog` assigned to `UnityBridge`?
- ✅ Is `autoApplyOnExitPlayMode` enabled?
- ✅ Check Console for `[UIChangeApplicator]` messages
- ✅ Did you use `"record": true` in commands?

### Object Not Found?

- The system uses **GameObject paths** like `Canvas/HealthBar/FillImage`
- If hierarchy changes, paths might break
- Solution: Use unique names or update paths

### Changes Applied to Wrong Object?

- Multiple objects with same name? Use full hierarchy path
- Check the `UIChangeLog` asset to see recorded paths

---

## 💡 Tips

1. **Name objects uniquely** - Makes path matching reliable
2. **Test small changes** - Easier to undo if something goes wrong
3. **Review the log** - Inspect `UIChangeLog` asset to see recorded changes
4. **Use Undo** - All changes use Unity's Undo system (Ctrl+Z works!)
5. **Clear manually** - If log gets cluttered, clear it: `changeLog.Clear()`

---

## 🔥 Advanced

### Custom Change Types

Extend `UIChangeLog.UIChange` and `UIChangeApplicator.ApplyChange()` to support:
- Prefab modifications
- Material changes
- Animation states
- Custom component types

### Batch Operations

Record multiple changes, then apply all at once for complex UI redesigns!

---

**Built with ❤️ by your AI assistant for seamless runtime-to-edit mode persistence!** 🤖✨
