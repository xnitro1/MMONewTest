# 🗺️ MapNetworkManager - World Server Orchestration

## **Overview**

The **MapNetworkManager** is the core component for individual map servers in NightBlade's MMO architecture. It manages world instances, player connections, and coordinates with the central cluster server to provide seamless multiplayer experiences across different game worlds.

**Type:** Component (inherits from BaseGameNetworkManager)  
**Purpose:** World server management and player coordination  
**Integration:** Core component for map server instances  

---

## 📋 **Quick Start**

1. **Add to Map Scene**: Attach `MapNetworkManager` to your world/map scene
2. **Configure Cluster Connection**: Set central server address and port
3. **Set Public Address**: Configure how clients connect to this map server
4. **Adjust Timeouts**: Configure spawn and disconnection settings
5. **Deploy**: Map server will register with cluster and accept player connections

```csharp
// Basic map server setup
GameObject mapServer = new GameObject("MapNetworkManager");
MapNetworkManager manager = mapServer.AddComponent<MapNetworkManager>();

// Configure for production
manager.clusterServerAddress = "central.yourgame.com";
manager.clusterServerPort = 6010;
manager.publicAddress = "map01.yourgame.com";
manager.playerCharacterDespawnMillisecondsDelay = 10000;
```

---

## 🏗️ **Server Types & Architecture**

### **Server Classification**

MapNetworkManager supports three server types based on configuration:

#### **1. Standard Map Server**
- **Purpose**: Persistent world content (overworld, cities, dungeons)
- **Characteristics**: Always running, handles persistent data
- **Registration**: Registers as `CentralServerPeerType.MapServer`

#### **2. Instance Map Server**
- **Purpose**: Temporary instances (events, private dungeons, battlegrounds)
- **Characteristics**: Created on-demand, auto-terminates when empty
- **Registration**: Registers as `CentralServerPeerType.InstanceMapServer`
- **Termination**: Auto-shutdown after 30 seconds of no players

#### **3. Allocate Map Server**
- **Purpose**: Dynamic load balancing and resource allocation
- **Characteristics**: Managed by cluster for optimal distribution
- **Registration**: Registers as `CentralServerPeerType.AllocateMapServer`

---

## ⚙️ **Configuration Sections**

### **Central Network Connection**

#### **Cluster Server Connection**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Cluster Address** | IP/Hostname | 127.0.0.1 | Central cluster server location |
| **Cluster Port** | 1024-65535 | 6010 | Cluster server communication port |

**Connection Requirements:**
- Must connect to the CentralNetworkManager
- Required for player routing and server coordination
- Handles cross-server communication and data synchronization

#### **Public Server Address**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Public Address** | IP/Hostname | 127.0.0.1 | Client connection endpoint |

**Client Access Configuration:**
- Address clients use to connect to this map server
- Must be accessible from client networks
- Different from cluster address for security/load balancing

### Map Information & Registration

#### **Instance-Specific Map Data**

Each MapNetworkManager instance maintains its own map information:

```csharp
public string RefId
{
    get
    {
        if (IsAllocate)
            return MapInfo?.Id ?? "Unknown";  // Instance-specific
        if (IsInstanceMap())
            return MapInstanceId;
        return MapInfo?.Id ?? "Unknown";  // Instance-specific
    }
}
```

**Key Changes (v2.x):**
- Uses `MapInfo.Id` (instance-specific) instead of `CurrentMapInfo.Id` (shared)
- Prevents cross-contamination between different map server instances
- Ensures correct registration with cluster server

---

### **Map Spawn Configuration**

#### **Spawn Coordination**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Spawn Timeout** | 0-300000 ms | 0 | Map spawning operation timeout |

**Spawn Process:**
1. **Request**: Cluster requests map server creation
2. **Allocation**: Map server initializes and registers
3. **Confirmation**: Server confirms readiness to cluster
4. **Routing**: Players directed to new server instance

**Timeout Considerations:**
- **0ms**: No timeout - wait indefinitely (development)
- **30000-60000ms**: Reasonable production timeout
- **>120000ms**: May cause player routing delays

---

### **Player Disconnection Handling**

#### **Character Persistence**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Despawn Delay** | 1000-60000 ms | 5000 | Delay before removing disconnected players |

**Disconnection Flow:**
```
Client Disconnects → Delay Timer Starts → Character Marked Inactive →
Delay Expires → Character Despawned → Data Saved to Database
```

#### **Recommended Delay Settings**

| Environment | Delay | Rationale |
|-------------|-------|-----------|
| **Development** | 1000-5000ms | Quick iteration, minimal wait |
| **Testing** | 5000-10000ms | Balance testing with temporary disconnects |
| **Production** | 10000-30000ms | Handle network issues, prevent character loss |
| **High-Latency** | 30000-60000ms | Compensate for poor connections |

---

## 🎮 **Runtime Operations**

### **Server Lifecycle**

#### **1. Initialization**
- Component attaches to map scene
- Establishes cluster server connection
- Registers with central server peer type
- Initializes data handlers and storage systems

#### **2. Player Management**
- Accepts player connections from cluster routing
- Manages character spawning and despawning
- Handles player data synchronization
- Coordinates cross-server operations

#### **3. Data Operations**
- Loads/saves character data to database
- Manages inventory and equipment changes
- Processes quest and achievement updates
- Handles guild and party operations

#### **4. Termination**
- Instance servers auto-terminate when empty
- Saves all pending data before shutdown
- Unregisters from cluster server
- Cleans up network connections

### **Instance Management**

#### **Instance Server Features**
- **Auto-Creation**: Spawned by cluster when needed
- **Warp Coordination**: Players teleported to instance entry point
- **Resource Management**: Automatic cleanup when empty
- **Data Isolation**: Instance data separate from persistent world

#### **Termination Logic**
```csharp
// Server terminates if:
NoPlayersConnected && TimeSinceLastPlayer > TERMINATE_INSTANCE_DELAY (30s)
```

---

## 🔧 **Advanced Configuration**

### **Custom Server Types**

```csharp
public class CustomMapServer : MapNetworkManager
{
    protected override void Awake()
    {
        // Custom initialization for special server types
        base.Awake();

        // Additional setup for battleground server
        ConfigureForBattleground();
    }

    private void ConfigureForBattleground()
    {
        // Special configuration for competitive gameplay
        playerCharacterDespawnMillisecondsDelay = 2000; // Quick cleanup
        // Custom battleground logic
    }
}
```

### **Dynamic Configuration**

```csharp
public class AdaptiveMapServer : MapNetworkManager
{
    void Update()
    {
        // Adaptive despawn delay based on server load
        if (GetPlayerCount() > maxPlayers * 0.8f)
        {
            // High load - quick cleanup
            playerCharacterDespawnMillisecondsDelay = 3000;
        }
        else
        {
            // Normal load - generous delay
            playerCharacterDespawnMillisecondsDelay = 15000;
        }
    }
}
```

### **Server Health Monitoring**

```csharp
public class MonitoredMapServer : MapNetworkManager
{
    private float lastHealthCheck;

    void Update()
    {
        base.Update();

        if (Time.time - lastHealthCheck > 60f) // Every minute
        {
            SendHealthReport();
            lastHealthCheck = Time.time;
        }
    }

    private void SendHealthReport()
    {
        var report = new ServerHealthReport
        {
            ServerId = RefId,
            PlayerCount = GetPlayerCount(),
            MemoryUsage = GetMemoryUsage(),
            NetworkLatency = GetAverageLatency(),
            LastHealthCheck = Time.time
        };

        // Send to cluster server for monitoring
        ClusterClient.SendHealthReport(report);
    }
}
```

---

## 📊 **Performance Optimization**

### **Memory Management**

#### **Data Loading Strategy**
- **Lazy Loading**: Character data loaded on demand
- **Batch Operations**: Multiple database operations grouped
- **Cache Management**: Frequently accessed data cached in memory
- **Cleanup Routines**: Automatic cleanup of unused data

#### **Network Optimization**
- **Message Batching**: Multiple updates combined into single packets
- **Delta Compression**: Only changed data sent over network
- **Connection Pooling**: Reuse database connections
- **Async Operations**: Non-blocking database queries

### **Scaling Considerations**

#### **Player Capacity**
| Server Type | Max Players | Hardware Requirements |
|-------------|-------------|----------------------|
| **Small Map** | 50-100 | 2-4 CPU cores, 4GB RAM |
| **Medium Map** | 100-300 | 4-8 CPU cores, 8GB RAM |
| **Large Map** | 300-1000 | 8-16 CPU cores, 16GB+ RAM |
| **Mega Map** | 1000+ | 16+ CPU cores, 32GB+ RAM |

#### **Database Load**
- **Read Operations**: Character loading, inventory queries
- **Write Operations**: Position updates, inventory changes
- **Batch Processing**: Group multiple updates per player
- **Connection Limits**: Database connection pool management

---

## 🚨 **Common Issues & Solutions**

### **"Cannot connect to cluster server"**

**Symptoms:** Server fails to start, connection errors in logs
**Causes:**
- Incorrect cluster server address/port
- Firewall blocking cluster communication
- Cluster server not running

**Solutions:**
- Verify cluster server is running and accessible
- Check network connectivity between servers
- Validate address resolution (DNS/hosts file)

### **"Map spawn timeout exceeded"**

**Symptoms:** Map instances fail to create, players cannot enter
**Causes:**
- Server startup taking too long
- Resource constraints preventing initialization
- Network issues during spawn process

**Solutions:**
- Increase `mapSpawnMillisecondsTimeout` for slow hardware
- Optimize server startup process
- Check server logs for initialization errors

### **"Players losing progress on disconnect"**

**Symptoms:** Character data not saving properly
**Causes:**
- Despawn delay too short for save operations
- Database connectivity issues
- Server crash during save process

**Solutions:**
- Increase `playerCharacterDespawnMillisecondsDelay`
- Implement emergency save on disconnect
- Add database connection monitoring

### **"High memory usage on map server"**

**Symptoms:** Server memory growing over time
**Causes:**
- Character data not being cleaned up
- Asset leaks from frequent loading/unloading
- Database connection accumulation

**Solutions:**
- Monitor character cleanup routines
- Implement asset unloading strategies
- Check database connection pooling

### **"Instance servers not terminating"**

**Symptoms:** Empty instance servers staying active
**Causes:**
- Players leaving but not properly disconnecting
- Termination logic not triggering
- Server configuration issues

**Solutions:**
- Check player disconnect handling
- Verify termination timer logic
- Monitor server logs for termination attempts

---

## 🔗 **Integration Points**

### **CentralNetworkManager Coordination**

```csharp
// Map server registration with central server
void OnServerStart()
{
    var registerMessage = new AppServerRegisterMessage
    {
        PeerType = PeerType,
        RefId = RefId,
        Address = AppAddress,
        Port = AppPort,
        CurrentMapId = CurrentMapInfo.Id
    };

    ClusterClient.SendRegisterMessage(registerMessage);
}
```

### **Database Integration**

```csharp
// Character data management
async UniTask LoadCharacterData(string characterId)
{
    if (DatabaseClient != null)
    {
        var characterData = await DatabaseClient.LoadCharacterAsync(characterId);
        // Process loaded data
    }
}

async UniTask SaveCharacterData(PlayerCharacterData data)
{
    if (DatabaseClient != null)
    {
        await DatabaseClient.SaveCharacterAsync(data);
    }
}
```

### **Cross-Server Communication**

```csharp
// Coordinate with other map servers
void SendPlayerToOtherMap(string characterId, string targetMapId, Vector3 position)
{
    var transferMessage = new PlayerTransferMessage
    {
        CharacterId = characterId,
        TargetMapId = targetMapId,
        TargetPosition = position,
        TransferToken = GenerateTransferToken()
    };

    ClusterClient.SendPlayerTransfer(transferMessage);
}
```

---

## 📊 **Monitoring & Analytics**

### **Key Metrics to Track**

#### **Server Performance**
- **Active Connections**: Current player count
- **Memory Usage**: RAM consumption over time
- **CPU Utilization**: Processing load by component
- **Network Traffic**: Bandwidth usage and packet rates

#### **Player Activity**
- **Session Length**: Average time spent on server
- **Movement Patterns**: Popular areas and routes
- **Interaction Frequency**: NPC talks, item usage, combat
- **Disconnect Patterns**: When and why players leave

#### **Database Performance**
- **Query Response Times**: Database operation latency
- **Connection Pool Usage**: Database connection utilization
- **Cache Hit Rates**: Memory cache effectiveness
- **Data Synchronization**: Cross-server data consistency

### **Health Monitoring**

```csharp
public class MapServerHealthMonitor : MonoBehaviour
{
    private MapNetworkManager mapManager;
    private float lastReportTime;

    void Start()
    {
        mapManager = GetComponent<MapNetworkManager>();
    }

    void Update()
    {
        if (Time.time - lastReportTime > 300f) // Every 5 minutes
        {
            GenerateHealthReport();
            lastReportTime = Time.time;
        }
    }

    private void GenerateHealthReport()
    {
        var report = new MapServerHealthReport
        {
            ServerId = mapManager.RefId,
            ServerType = mapManager.PeerType.ToString(),
            PlayerCount = GetPlayerCount(),
            MemoryUsage = GetMemoryUsage(),
            DatabaseLatency = GetDatabaseLatency(),
            NetworkHealth = CheckNetworkHealth(),
            Timestamp = Time.time
        };

        // Send to monitoring system
        SendHealthReport(report);
    }
}
```

---

## 🎯 **Best Practices**

### **1. Server Configuration**
- **Match Hardware**: Configure limits based on server capabilities
- **Network Planning**: Ensure proper firewall and routing setup
- **Monitoring Setup**: Implement comprehensive logging and alerting
- **Backup Planning**: Regular data backups and recovery testing

### **2. Performance Tuning**
- **Load Testing**: Test with expected peak player counts
- **Memory Monitoring**: Set appropriate limits and alerts
- **Database Optimization**: Index frequently queried data
- **Network Optimization**: Minimize latency for critical operations

### **3. Operational Management**
- **Regular Maintenance**: Schedule downtime for updates
- **Monitoring Dashboards**: Real-time server status visibility
- **Incident Response**: Prepared procedures for server issues
- **Capacity Planning**: Monitor growth and plan scaling

### **4. Security Considerations**
- **Access Control**: Proper authentication and authorization
- **Data Validation**: Validate all client input
- **Rate Limiting**: Prevent abuse and DDoS attacks
- **Audit Logging**: Track important operations and changes

---

## 📋 **Configuration Checklist**

### **Pre-Deployment Setup**
- [ ] Cluster server address and port configured
- [ ] Public address accessible from client networks
- [ ] Appropriate timeouts for server environment
- [ ] Player despawn delays balanced for game type

### **Production Validation**
- [ ] Server successfully registers with cluster
- [ ] Players can connect and move between servers
- [ ] Character data saves and loads correctly
- [ ] Performance remains stable under load

### **Monitoring Setup**
- [ ] Server health monitoring configured
- [ ] Alert thresholds set for critical metrics
- [ ] Log aggregation and analysis setup
- [ ] Backup and recovery procedures tested

---

## 📞 **API Reference**

### **Core Properties**

```csharp
public string clusterServerAddress = "127.0.0.1";
public int clusterServerPort = 6010;
public string publicAddress = "127.0.0.1";
public int mapSpawnMillisecondsTimeout = 0;
public int playerCharacterDespawnMillisecondsDelay = 5000;

// Runtime properties
public string MapInstanceId { get; set; }
public Vector3 MapInstanceWarpToPosition { get; set; }
public bool IsAllocate { get; set; }
public CentralServerPeerType PeerType { get; }
public string RefId { get; }
```

### **Key Methods**

```csharp
// Server management
protected override void Awake()
protected override void Start()
protected override void Update()

// Player management
public void HandlePlayerConnect(long connectionId, string userId)
public void HandlePlayerDisconnect(long connectionId)
public void DespawnPlayerCharacter(long connectionId)

// Cluster communication
public ClusterClient ClusterClient { get; private set; }
public void RegisterWithCluster()
public void UnregisterFromCluster()
```

### **Constants**

```csharp
public const float TERMINATE_INSTANCE_DELAY = 30f;  // Empty instance cleanup
public const float UPDATE_USER_COUNT_DELAY = 5f;   // Statistics update frequency
```

---

## 📈 **Scaling Guidelines**

### **Small Scale Deployment (1-10 servers)**
```
Cluster Configuration: Single central server
Map Servers: 2-5 world servers
Instance Servers: On-demand spawning
Load Balancing: Basic round-robin
Monitoring: Essential metrics only
```

### **Medium Scale Deployment (10-50 servers)**
```
Cluster Configuration: Redundant central servers
Map Servers: 10-30 world servers
Instance Servers: Pre-warmed pools
Load Balancing: Geographic + load-based
Monitoring: Comprehensive metrics
```

### **Large Scale Deployment (50+ servers)**
```
Cluster Configuration: Distributed cluster with failover
Map Servers: 30-100+ world servers
Instance Servers: Dynamic scaling with auto-termination
Load Balancing: Advanced AI-driven distribution
Monitoring: Real-time analytics and predictive scaling
```

---

## 🔄 **Version History**

### **Current Version**
- Complete map server orchestration system
- Instance server lifecycle management
- Cluster server integration and communication
- Player session management and data persistence
- Unity Editor integration with comprehensive configuration
- Extensive documentation and best practices

### **Key Features**
- **Multi-Server Architecture**: Support for persistent and instance-based worlds
- **Cluster Coordination**: Seamless communication with central server
- **Player Management**: Comprehensive session handling and data persistence
- **Performance Optimization**: Memory management and network optimization
- **Monitoring Integration**: Health reporting and analytics support

---

*This documentation covers the complete MapNetworkManager system for NightBlade world server management. For the latest updates and additional features, check the official repository.*