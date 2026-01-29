# 📜 Batch Files Guide

Complete guide to using the provided batch files for server management and testing.

---

## 📂 Location

All batch file examples are located in:
```
/Batch Examples/
```

This folder contains ready-to-use batch scripts for common MMO server operations.

---

## 🚀 Available Batch Files

### 1. **START_SERVERS.bat**

**Purpose:** Launches all NightBlade MMO servers in the correct order with proper configuration.

**Location:** `/Batch Examples/START_SERVERS.bat`

**What it does:**
1. Starts Central Server + Database Server (combined window)
2. Waits 2 seconds for initialization
3. Starts MapSpawn Server (spawns map instances automatically)

**Configuration:**
```batch
# Executable path
D:\MMO\NightBlade.exe

# Command-line arguments
-startCentralServer       # Launches Central Server
-startDatabaseServer      # Launches Database Server
-startMapSpawnServer      # Launches MapSpawn Server
-spawnExePath             # Path to exe for spawning instances
-publicAddress            # Server public address (default: 127.0.0.1)
```

**How to use:**
1. Ensure your build exists at `D:\MMO\NightBlade.exe`
2. Double-click `START_SERVERS.bat`
3. Wait for all windows to open (should see 2-3+ windows)
4. Check console output for successful startup messages

**Expected output:**
```
[1/2] Starting Central + Database Server...
[2/2] Starting MapSpawn Server...

All servers started!
```

**Windows that will open:**
- **Window 1:** "NightBlade - Central+Database" - Main coordination server
- **Window 2:** "NightBlade - MapSpawn" - Instance spawning server
- **Window 3+:** Individual map instances (Map001, Map002, etc.) spawned automatically

---

### 2. **KILL_SERVERS.bat**

**Purpose:** Forcefully stops all running NightBlade server processes.

**Location:** `/Batch Examples/KILL_SERVERS.bat`

**What it does:**
- Finds all `NightBlade.exe` processes
- Terminates them immediately
- Provides feedback on success/failure

**How to use:**
1. Double-click `KILL_SERVERS.bat`
2. All server windows close instantly
3. See confirmation message

**Expected output:**
```
Stopping all NightBlade.exe processes...
SUCCESS: All servers killed!
```

**Use cases:**
- Stopping servers after testing
- Cleaning up stuck processes
- Quick restart (kill, then start again)
- Emergency shutdown

---

## 🔧 Customizing Batch Files

### Changing the Executable Path

If your build is located elsewhere, edit the batch files:

**In START_SERVERS.bat:**
```batch
# Change this line:
D:\MMO\NightBlade.exe

# To your path:
C:\Your\Path\To\NightBlade.exe
```

**Update in 2 places:**
1. Central+Database server start command
2. MapSpawn server start command

### Changing Server Ports

Add port arguments to the start commands:

```batch
NightBlade.exe -startCentralServer -networkPort 7000
NightBlade.exe -startDatabaseServer -networkPort 7001
```

### Adding More Servers

To start additional servers, add new `start` commands:

```batch
REM Start Chat Server
echo [3/3] Starting Chat Server...
start "NightBlade - Chat" cmd /k "D:\MMO\NightBlade.exe -startChatServer"
```

---

## 🧪 Testing Workflow

### Standard Testing Process:

1. **Build your project** to `D:\MMO\`
2. **Double-click** `START_SERVERS.bat`
3. **Watch console output** for errors
4. **Test your game** (connect clients, test features)
5. **When done**, double-click `KILL_SERVERS.bat`

### Quick Restart:

```batch
# Method 1: Use both batch files
1. Double-click KILL_SERVERS.bat
2. Wait 2 seconds
3. Double-click START_SERVERS.bat

# Method 2: Create a RESTART_SERVERS.bat
@echo off
call KILL_SERVERS.bat
timeout /t 2 /nobreak >nul
call START_SERVERS.bat
```

---

## 🐛 Troubleshooting

### "NightBlade.exe not found"

**Problem:** Batch file can't find the executable.

**Solution:**
1. Verify build exists at `D:\MMO\NightBlade.exe`
2. If in different location, edit batch file paths
3. Ensure path has no typos

### "Port already in use"

**Problem:** Previous server instance still running.

**Solution:**
1. Run `KILL_SERVERS.bat` first
2. Wait 2-3 seconds
3. Try `START_SERVERS.bat` again

### Servers start but crash immediately

**Problem:** Missing dependencies or configuration.

**Solutions:**
- Check `D:\MMO\NightBlade_Data\` folder exists
- Verify all DLLs are present
- Check database configuration
- Review console output for error messages

### MapSpawn not spawning instances

**Problem:** Instance spawn configuration incorrect.

**Solution:**
1. Verify `spawnExePath` points to correct exe
2. Check `MMOServerInstance.prefab` configuration
3. Ensure maps are in Build Settings
4. Review MapSpawn console for errors

---

## 📊 Server Architecture

### How the Servers Work Together:

```
START_SERVERS.bat runs:
  │
  ├─► Central Server (port 6000)
  │   └─► Manages player connections
  │   └─► Routes players to map instances
  │   └─► Handles authentication
  │
  ├─► Database Server (port 6001)
  │   └─► Stores player data
  │   └─► Manages persistent storage
  │
  └─► MapSpawn Server
      └─► Spawns Map001 instance (auto)
      └─► Spawns Map002 instance (auto)
      └─► Monitors instance health
      └─► Auto-scales based on player count
```

### Instance Lifecycle:

1. **MapSpawn reads config** → Spawns initial instances
2. **Instances register** → Central Server tracks them
3. **Players connect** → Routed to optimal instance
4. **Load threshold hit** → Auto-spawn new instances
5. **Instances empty** → Auto-cleanup after timeout

---

## 🔐 Production Considerations

### Security:

**DO NOT use these batch files in production as-is!**

Changes needed for production:
- Remove `-publicAddress "127.0.0.1"` (use actual IP)
- Add firewall rules
- Use proper SSL certificates
- Implement authentication
- Add logging and monitoring

### Deployment:

For production, use:
- Service managers (Windows Service, systemd)
- Process supervisors (PM2, Forever)
- Container orchestration (Docker, Kubernetes)
- Cloud services (AWS, Azure, GCP)

### Monitoring:

Add logging to batch files:
```batch
# Redirect output to log files
start "Central" cmd /k "NightBlade.exe -startCentralServer > logs\central.log 2>&1"
```

---

## 📚 Related Documentation

- **[Server Architecture](core-systems.md)** - How servers communicate
- **[CCU System Flow](../CCU_SYSTEM_FLOW.md)** - Player connection flow
- **[Auto-Scaling System](../AUTOSCALING_SYSTEM_FIXED.md)** - Instance management
- **[Launch Guide](../HOW_TO_LAUNCH_SERVERS_CORRECTLY.md)** - Manual launch process
- **[Troubleshooting](troubleshooting.md)** - Common issues and fixes

---

## 💡 Best Practices

### Development:
✅ Use batch files for quick testing  
✅ Keep KILL_SERVERS.bat handy  
✅ Check console output regularly  
✅ Test with fresh builds  

### Testing:
✅ Start servers in correct order  
✅ Wait for initialization messages  
✅ Monitor all console windows  
✅ Kill servers between test runs  

### Production:
❌ Don't use batch files directly  
✅ Use proper service management  
✅ Implement health checks  
✅ Add monitoring and alerting  
✅ Use load balancers  

---

## 🔄 Version History

**v1.0** (2026-01-25)
- Initial batch file creation
- START_SERVERS.bat with proper ordering
- KILL_SERVERS.bat for cleanup
- Configured for D:\MMO\ build location

---

## 🆘 Need Help?

If you encounter issues:
1. Check console output for errors
2. Review [Troubleshooting Guide](troubleshooting.md)
3. Verify build configuration
4. Check [GitHub Issues](https://github.com/Fallen-Entertainment/NightBlade_MMO/issues)

---

**Location:** `/Batch Examples/` in project root  
**Maintained by:** Fallen Entertainment  
**Last Updated:** 2026-01-25
