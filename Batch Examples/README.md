# 📜 Batch Examples

Ready-to-use batch files for NightBlade MMO server management.

## 📂 Files in This Folder

### 🚀 START_SERVERS.bat ⭐ **RECOMMENDED**
Launches all servers for simple static setup:
1. Central + Database Server (window 1)
2. Map001 Server on port 8000 (window 2)
3. Map002 Server on port 8001 (window 3)

**Usage:** Double-click to start servers  
**Note:** Servers stay running (no auto-quit)

---

### 🛑 KILL_SERVERS.bat
Stops all running NightBlade server processes instantly.

**Usage:** Double-click to stop all servers  
**Features:** Aggressive kill with verification

---

## ⚡ Quick Start

1. Build your project to `D:\MMO\`
2. Double-click `START_SERVERS.bat`
3. Wait for 3 console windows to open
4. Login and select character
5. When done, double-click `KILL_SERVERS.bat`

---

## 🔧 Customization

**If your build is NOT at `D:\MMO\NightBlade.exe`:**

Edit `START_SERVERS.bat` and change:
```batch
D:\MMO\NightBlade.exe
```

To your actual build path.

---

## 📚 Full Documentation

For complete guide: [`/docs/batch-files-guide.md`](../docs/batch-files-guide.md)

---

**System:** Simple static map servers (CCU/Auto-scaling removed)
