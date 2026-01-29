# 🦾 Unity Bridge Extended - Quick Reference

**Version:** 1.0 | **Full Guide:** [UNITY_BRIDGE_EXTENDED_GUIDE.md](UNITY_BRIDGE_EXTENDED_GUIDE.md)

---

## ⚡ Quick Command Reference

### 🎨 GenerateImage - Create 2D Assets

```json
{
  "commands": [{
    "id": "create_sprite",
    "type": "GenerateImage",
    "parameters": {
      "prompt": "Pixel art treasure chest, 32x32, golden, transparent background",
      "model": "gpt-image-1",
      "size": "1024x1024",
      "count": 1
    }
  }]
}
```

**Models:** `gpt-image-1`, `dall-e-3`, `dall-e-2`  
**Sizes:** `256x256`, `512x512`, `1024x1024`, `1536x1024`, `1024x1536`, `1792x1024`, `1024x1792`

---

### 🌈 GenerateShader - Create Custom Shaders

```json
{
  "commands": [{
    "id": "glow_shader",
    "type": "GenerateShader",
    "parameters": {
      "prompt": "Hologram shader with scan lines and edge glow"
    }
  }]
}
```

---

### 💻 GenerateScript - Create C# Scripts

```json
{
  "commands": [{
    "id": "player_move",
    "type": "GenerateScript",
    "parameters": {
      "prompt": "2D player controller with WASD movement and Space to jump"
    }
  }]
}
```

---

### 🎵 GenerateSound - Create Audio (Coming Soon!)

```json
{
  "commands": [{
    "id": "footstep_sfx",
    "type": "GenerateSound",
    "parameters": {
      "prompt": "Footstep on stone floor",
      "duration": 0.5
    }
  }]
}
```

**Status:** 🚧 Planned

---

### 🗿 Generate3DModel - Create 3D Assets (Coming Soon!)

```json
{
  "commands": [{
    "id": "sword_3d",
    "type": "Generate3DModel",
    "parameters": {
      "imagePath": "Assets/AI_Generated/sword_concept.png"
    }
  }]
}
```

**Status:** 🚧 Planned

---

### ℹ️ GetCapabilities - Check Available Features

```json
{
  "commands": [{
    "id": "check_powers",
    "type": "GetCapabilities"
  }]
}
```

---

## 📊 Response Format

```json
{
  "commandId": "your_id",
  "success": true,
  "message": "Status message",
  "timestamp": "2026-01-24 23:35:00",
  "data": {
    "status": "complete",
    "paths": ["Assets/AI_Generated/..."]
  }
}
```

**Status Values:**
- `generating` - In progress
- `complete` - Done!
- `not_implemented` - Coming soon

---

## 🎯 Common Workflows

### Create UI Button Asset

```json
{
  "commands": [{
    "id": "ui_btn",
    "type": "GenerateImage",
    "parameters": {
      "prompt": "Fantasy RPG button, glowing purple border, 512x128, transparent",
      "size": "1024x1024"
    }
  }]
}
```

### Generate Complete Feature

```json
{
  "commands": [
    {
      "id": "step1_shader",
      "type": "GenerateShader",
      "parameters": {
        "prompt": "Water shader with animated waves"
      }
    },
    {
      "id": "step2_script",
      "type": "GenerateScript",
      "parameters": {
        "prompt": "Script that applies the shader and animates water movement"
      }
    }
  ]
}
```

---

## ⚙️ Configuration

**Assets saved to:** `Assets/AI_Generated/`  
**Requires:** OpenAI API key configured in uAI Settings  
**Component:** Add `UnityBridgeExtended` to scene

---

## 💡 Pro Tips

✅ **Be Specific:** "Pixel art 32x32 health potion, red liquid, glass bottle, transparent"  
✅ **Right Size:** Icons 256x256, Textures 1024x1024, Concept Art 1536x1024  
✅ **Iterate:** Refine prompts if first result isn't perfect  
✅ **Combine:** Use with base bridge to position/apply generated assets

---

## 🐛 Quick Troubleshooting

**"Extended commands are disabled"**  
→ Enable in UnityBridgeExtended component

**"Image generation failed"**  
→ Check OpenAI API key in uAI Settings

**"Unknown extended command"**  
→ Check spelling (case-sensitive!)

---

## 📚 See Also

- **Full Guide:** [UNITY_BRIDGE_EXTENDED_GUIDE.md](UNITY_BRIDGE_EXTENDED_GUIDE.md)
- **Base Bridge:** [UNITY_BRIDGE_QUICK_REFERENCE.md](UNITY_BRIDGE_QUICK_REFERENCE.md)
- **Index:** [UNITY_BRIDGE_INDEX.md](UNITY_BRIDGE_INDEX.md)

---

🦾 **AI Superpowers Activated!** | v1.0 | 2026-01-24
