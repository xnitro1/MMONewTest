# 🎨 NightBlade UI Modernization Plan

**Goal:** Transform to Modern MMO Style (FFXIV/Lost Ark aesthetic)

---

## 🎯 Phase 1: HUD Repositioning & Sizing

### Current State:
- HP/MP/Stamina at **(0, 585, 0)** - upper area
- Hotbar at bottom with 50x50 slots ✅ (already improved!)
- Separate labels for each resource (LabelHp, LabelMp, LabelStamina)
- Traditional "gage" bars

### Modern Target:
- **Bottom-center HUD cluster**
- HP/MP/Stamina **above** hotbar
- Larger, more prominent bars
- Remove redundant labels
- Integrate character name/level into HUD

### Changes:
1. ✅ Hotbar: 50x50 cells, 8px spacing, center-aligned
2. 🔄 Move UICharacterHpMp to bottom-center
3. 🔄 Increase bar sizes (wider, more visible)
4. 🔄 Stack vertically: Name → HP → MP → Stamina → Hotbar
5. 🔄 Remove label elements (HP/MP/Stamina text redundant)

---

## 🎯 Phase 2: Visual Modernization

### Resource Bars:
- Darker backgrounds (80-90% opacity)
- Subtle gradients on fill
- Rounded corners (4-6px)
- Drop shadows for depth
- Text overlays (centered on bars, not separate labels)

### Hotbar:
- Semi-transparent backgrounds
- Glow effects on usable skills
- Cooldown sweeps
- Keybind labels (already has TextKey)

---

## 🎯 Phase 3: Menu & Navigation

### Current: 7 text buttons (Hero, Items, Skills, Quests, Party, Guild, System)
### Modern: Icon-based menu bar

- Replace text buttons with icons
- Horizontal bar (top-right or bottom-right)
- Tooltip on hover
- Notification badges

---

## 🎯 Phase 4: Minimap

### Current: Top-left with frame
### Modern: 
- Rounded corners
- Semi-transparent
- Integrated zoom controls
- Quest markers overlay

---

## 🎯 Phase 5: Target Frame

### Current: 3 separate target systems
### Modern:
- Unified target frame (top-center)
- Boss frame (top-center, larger)
- Focus target (optional, smaller)

---

## 🎯 Phase 6: Chat

### Current: 6-tab system
### Modern:
- Collapsible/resizable
- Semi-transparent background
- Better channel indicators
- Emoji/link support

---

## 🚀 Execution Order:

1. **NOW:** HUD repositioning (move HP/MP/Stamina to bottom)
2. **NOW:** Resource bar sizing (make prominent)
3. Later: Visual polish (colors, shadows, gradients)
4. Later: Menu modernization
5. Later: Chat improvements

---

**Status:** Phase 1 in progress... 🔄
