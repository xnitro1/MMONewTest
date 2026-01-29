# 🚀 How to Launch NightBlade MMO Servers

Simple guide for the static server setup.

---

## ⚡ Quick Start

**1. Build your project:**
- Build to: `D:\MMO\`
- Result: `D:\MMO\NightBlade.exe`

**2. Start all servers:**
```
Double-click: Batch Examples/START_SERVERS.bat
```

**3. Servers will start:**
- Window 1: Central + Database Server
- Window 2: Map001 Server (port 8000) - Static map
- Window 3: Map002 Server (port 8001) - Static map
- Window 4: Map Spawn Server - Spawns dynamic instances (dungeons, raids, etc.)

**4. Connect and play:**
- Launch your game client
- Login with your account
- Select a character
- You'll automatically connect to Map001, Map002, or a dynamic dungeon instance

**5. Stop servers when done:**
```
Double-click: Batch Examples/KILL_SERVERS.bat
```

---

## 🎯 What Each Server Does

### Central + Database Server (Window 1)
**Handles:**
- Player authentication (login/register)
- Character management (create/delete/select)
- Finding which map server to connect to
- Database operations (SQLite)
- Coordinating map spawn requests

**Port:** 6010 (cluster), 7000 (player connections)

### Map001 Server (Window 2) - STATIC
**Handles:**
- Map001 gameplay
- Players in Map001
- Combat, movement, NPCs
- Always running (persistent world map)

**Port:** 8000

### Map002 Server (Window 3) - STATIC
**Handles:**
- Map002 gameplay
- Players in Map002
- Combat, movement, NPCs
- Always running (persistent world map)

**Port:** 8001

### Map Spawn Server (Window 4) - DYNAMIC
**Handles:**
- Spawning dungeon instances on-demand
- Creating raid instances when parties enter
- Launching temporary map instances (Dungeon001, Dungeon002, etc.)
- Managing dynamic instance lifecycle

**Ports:** Starts at 9000, increments for each spawned instance

---

## 🔧 Manual Launch (Advanced)

If you need to start servers manually:

**Terminal 1: Central + Database**
```bash
D:\MMO\NightBlade.exe -startCentralServer -startDatabaseServer -spawnExePath "D:\MMO\NightBlade.exe"
```

**Terminal 2: Map001 (Static)**
```bash
D:\MMO\NightBlade.exe -startMapServer -mapName Map001 -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8000
```

**Terminal 3: Map002 (Static)**
```bash
D:\MMO\NightBlade.exe -startMapServer -mapName Map002 -channelId 1 -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -mapPort 8001
```

**Terminal 4: Map Spawn Server (Dynamic)**
```bash
D:\MMO\NightBlade.exe -startMapSpawnServer -centralAddress 127.0.0.1 -centralPort 6010 -publicAddress 127.0.0.1 -spawnExePath "D:\MMO\NightBlade.exe" -spawnStartPort 9000
```

---

## 🐛 Troubleshooting

### Can't Login?
**Check:**
1. Are all 4 servers running? (4 console windows should be open)
2. Look at Window 1 (Central) for "Register Map Server" messages
3. You should see: `Register Map Server: [_Map001]`, `[_Map002]`, and `Register Map Spawn Server`

### Servers Won't Start?
**Try:**
1. Run `KILL_SERVERS.bat` first
2. Make sure `D:\MMO\NightBlade.exe` exists
3. Check logs in `D:\MMO\log\` folder

### Wrong Map Loads?
**This was a Unity cache bug. Fix:**
1. Close Unity
2. Delete `Library` folder in project
3. Reopen Unity
4. Rebuild to `D:\MMO\`

---

## 📝 Notes

### Static vs Dynamic Maps

**STATIC Maps** (Map001, Map002):
- Always running, even when empty
- Persistent world locations
- One instance shared by all players
- Use for: Towns, overworld, persistent areas

**DYNAMIC Instances** (Dungeon001, raids, etc.):
- Spawned on-demand by Map Spawn Server
- Private/party-only instances
- Destroyed when empty
- Use for: Dungeons, raids, instanced content

### Server Management
- **Manual management:** You start/stop servers manually
- **Static capacity:** Static map servers handle unlimited players
- **Dynamic scaling:** Map Spawn Server creates instances as needed
- **Simple and stable:** No complex CCU logic to debug

---

**System:** Hybrid - Static maps + Dynamic instances  
**Updated:** 2026-01-28  
**Map Spawn Server:** ADDED for dynamic dungeons
