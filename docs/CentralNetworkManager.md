# 🌐 CentralNetworkManager - Server Orchestration Hub

## **Overview**

The **CentralNetworkManager** is NightBlade's central server orchestration component that manages the entire multiplayer infrastructure through a sophisticated **Channel + Instance architecture**. It coordinates cluster communication, handles user authentication, manages server channels with dynamic instance scaling, and provides the backbone for massively scalable MMO operations.

**Type:** Component (inherits from LiteNetLibManager)  
**Purpose:** Central server coordination and Channel + Instance infrastructure management  
**Integration:** Core component for all NightBlade server operations  

---

## 📋 **Quick Start**

1. **Add to Central Server Scene**: Attach `CentralNetworkManager` to your central server GameObject
2. **Configure Cluster Settings**: Set cluster server port and communication parameters
3. **Set Up Channels**: Configure channels for different game modes or regions
4. **Configure User Validation**: Set username/password requirements and character naming rules
5. **Start Server**: The central server will coordinate all map servers and user connections

```csharp
// Basic central server setup
GameObject centralServer = new GameObject("CentralServer");
CentralNetworkManager centralManager = centralServer.AddComponent<CentralNetworkManager>();

// Configure for production
centralManager.clusterServerPort = 6010;
centralManager.defaultChannelMaxConnections = 1000;
centralManager.updateUserCountInterval = 5f;
```

---

## 🏗️ **Architecture Overview**

### **Central Server Responsibilities**

The CentralNetworkManager serves as the "brain" of your NightBlade MMO infrastructure:

- **🎯 User Authentication**: Login, registration, and account management
- **🏗️ Cluster Coordination**: Manages communication between all game servers
- **🗺️ Map Server Orchestration**: Spawns and monitors map server instances
- **📡 Channel Management**: Organizes players by game modes, regions, or server groups
- **📊 Statistics & Monitoring**: Tracks server health and player distribution
- **🔄 Load Balancing**: Coordinates player distribution across server instances

### **Server Hierarchy**

```
CentralNetworkManager (This Component)
├── User Authentication Server
├── Cluster Communication Hub
├── Map Server Coordinator
├── Channel Manager
└── Statistics Aggregator
```

---

## ⚙️ **Configuration Sections**

### **Cluster Configuration**

#### **Server Communication**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Cluster Server Port** | 1024-65535 | 6010 | Port for server-to-server communication |

**Port Selection Guidelines:**
- **Development**: Use 6010 (NightBlade default)
- **Production**: Use registered ports (1024-49151)
- **Avoid**: Well-known ports (1-1023) and dynamic ports (49152-65535)

---

### **Map Spawn Configuration**

#### **Server Instance Management**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Map Spawn Timeout** | 0-300000 ms | 0 | Timeout for map server spawning (0 = no timeout) |

**Timeout Considerations:**
- **0 (No Timeout)**: Wait indefinitely - good for development
- **5000-30000ms**: Reasonable for production environments
- **>120000ms**: May cause delays in error recovery

**Use Cases:**
- **Development**: 0ms (no timeout) for debugging
- **Production**: 30000ms (30 seconds) for stability
- **High Availability**: 60000ms (1 minute) for complex deployments

---

### **Channel + Instance Management**

#### **Layered Architecture**

The CentralNetworkManager implements a sophisticated **two-layer scaling architecture**:

**🌐 Channel Layer (Top Level):**
Channels organize players into separate worlds/realms based on:
- **Game Modes**: PvP, PvE, Events, Tournaments
- **Regions**: NA-East, EU-West, Asia-Pacific
- **Server Types**: Normal, VIP, Beta, Development
- **Content**: Different game versions or expansions

**⚖️ Instance Layer (Dynamic Scaling):**
- Multiple copies of maps within each channel
- Automatic instance creation/destruction based on demand
- Load balancing across instances within channels
- Cross-instance messaging within channels

**Architecture Benefits:**
- **Horizontal Scaling**: Unlimited channels for different purposes
- **Vertical Scaling**: Unlimited instances within each channel
- **Perfect Isolation**: Complete separation between channels
- **Zero Waste**: Instances only exist when needed

#### **Default Channel**
| Setting | Range | Default | Description |
|---------|-------|---------|-------------|
| **Max Connections** | 1-10000 | 500 | Maximum players per default channel |

#### **Custom Channels**
Each channel supports:
- **Unique ID**: String identifier for the channel
- **Display Title**: User-friendly name
- **Connection Limit**: Maximum players for this channel
- **Custom Settings**: Channel-specific configurations

```csharp
// Example channel configurations
channels = new List<ChannelData>
{
    new ChannelData { id = "pvp", title = "PvP Arena", maxConnections = 200 },
    new ChannelData { id = "pve", title = "PvE World", maxConnections = 1000 },
    new ChannelData { id = "event", title = "Special Events", maxConnections = 500 }
};
```

**Channel Scaling Strategy:**
```
Total Server Capacity = Σ(Channel Max Connections)
Load Distribution = Player Count / Channel Count
Peak Handling = Max Channel Capacity * Safety Factor (1.2-1.5)
```

---

### **User Account Configuration**

#### **Authentication Settings**
| Setting | Type | Default | Description |
|---------|------|---------|-------------|
| **Disable Default Login** | Bool | false | Use custom authentication system |
| **Require Email** | Bool | false | Email address required for registration |
| **Require Email Verification** | Bool | false | Email verification before account activation |

#### **Username Requirements**
| Setting | Range | Default | Validation |
|---------|-------|---------|------------|
| **Min Username Length** | 1-32 | 2 | Minimum characters required |
| **Max Username Length** | 1-64 | 24 | Maximum characters allowed |

#### **Password Requirements**
| Setting | Range | Default | Security |
|---------|-------|---------|----------|
| **Min Password Length** | 1-128 | 2 | Minimum characters required |

#### **Character Name Requirements**
| Setting | Range | Default | Validation |
|---------|-------|---------|------------|
| **Min Character Name Length** | 1-32 | 2 | Minimum characters required |
| **Max Character Name Length** | 1-64 | 16 | Maximum characters allowed |

**Security Considerations:**
- **Production**: Min password length ≥ 8, email verification enabled
- **Development**: Relaxed requirements for testing
- **Global**: Consider cultural naming conventions for character limits

---

### **Statistics & Monitoring**

#### **Performance Tracking**
| Setting | Range | Default | Purpose |
|---------|-------|---------|---------|
| **Update Interval** | 0.1-300 sec | 5 sec | How often to refresh statistics |

**Monitoring Data:**
- **Connected Users**: Total active players across all channels
- **Channel Distribution**: Player count per channel
- **Server Health**: Response times and error rates
- **Load Balancing**: Server utilization metrics

---

## 🎮 **Runtime Operations**

### **Server Startup Sequence**

1. **Initialization**: CentralNetworkManager starts and binds to cluster port
2. **Channel Setup**: Configured channels are initialized and made available
3. **Authentication Ready**: User login/registration services become active
4. **Map Coordination**: Ready to spawn and manage map server instances
5. **Statistics Tracking**: Begins monitoring server performance

### **User Connection Flow**

```
Client Request → Central Server → Authentication → Channel Assignment → Map Server Redirect
```

### **Load Balancing**

The CentralNetworkManager coordinates load balancing decisions:
- **Channel Selection**: Assigns players to appropriate channels
- **Map Server Selection**: Chooses optimal map server instances
- **Capacity Management**: Monitors and reports server utilization
- **Failover Coordination**: Handles server failures and redistribution

---

## 🔧 **Advanced Configuration**

### **Custom Authentication**

```csharp
public class CustomAuthManager : MonoBehaviour
{
    void Start()
    {
        var centralManager = GetComponent<CentralNetworkManager>();
        centralManager.disableDefaultLogin = true;

        // Implement custom authentication
        centralManager.onClientConnected += HandleCustomAuth;
    }

    private void HandleCustomAuth()
    {
        // Custom login logic here
    }
}
```

### **Dynamic Channel Management**

```csharp
public class ChannelManager : MonoBehaviour
{
    private CentralNetworkManager centralManager;

    void Start()
    {
        centralManager = GetComponent<CentralNetworkManager>();
    }

    public void AddEventChannel(string eventId, int maxPlayers)
    {
        var newChannel = new ChannelData
        {
            id = $"event_{eventId}",
            title = $"Event: {eventId}",
            maxConnections = maxPlayers
        };

        centralManager.channels.Add(newChannel);
        // Trigger channel refresh
    }

    public void RemoveEventChannel(string eventId)
    {
        centralManager.channels.RemoveAll(c => c.id == $"event_{eventId}");
        // Cleanup disconnected players
    }
}
```

### **Server Health Monitoring**

```csharp
public class ServerMonitor : MonoBehaviour
{
    private CentralNetworkManager centralManager;

    void Update()
    {
        if (centralManager.ClusterServer != null)
        {
            // Monitor cluster health
            int totalUsers = GetTotalConnectedUsers();
            float averageResponseTime = GetAverageResponseTime();

            if (totalUsers > GetMaxCapacity() * 0.9f)
            {
                Debug.LogWarning("Server approaching capacity limits");
            }

            if (averageResponseTime > 100f) // ms
            {
                Debug.LogWarning("Server response times degrading");
            }
        }
    }
}
```

---

## 📊 **Performance Optimization**

### **Connection Limits**

| Environment | Recommended Max Connections | Rationale |
|-------------|----------------------------|-----------|
| **Development** | 50-100 | Testing and debugging |
| **Staging** | 200-500 | Load testing |
| **Production Small** | 500-1000 | Small MMO or game mode |
| **Production Large** | 1000-5000 | Major MMO deployment |
| **Enterprise** | 5000+ | Massive scale operations |

### **Channel Distribution Strategy**

```
Total Capacity = Σ(All Channel Max Connections)
Optimal Channels = √(Total Capacity / 100)  // ~10 channels for 10k capacity
Channel Balance = Max Deviation < 20% from average
```

### **Monitoring Frequency**

| Update Interval | Use Case | Performance Impact |
|----------------|----------|-------------------|
| **1-5 seconds** | High-frequency monitoring | Moderate CPU usage |
| **5-30 seconds** | Standard operations | Low CPU usage |
| **30-300 seconds** | Background monitoring | Minimal CPU usage |

---

## 🚨 **Common Issues & Solutions**

### **"Cannot connect to cluster server"**

**Cause:** Port binding failure or firewall issues
**Solution:**
- Verify clusterServerPort is not in use by other applications
- Check firewall settings allow the port
- Ensure the port is within valid range (1024-65535)

### **"Channel connection limit exceeded"**

**Cause:** Too many players trying to join a channel
**Solution:**
- Increase channel maxConnections values
- Add more channels for load distribution
- Implement queueing system for peak times

### **"Map server spawn timeout"**

**Cause:** Map servers taking too long to start
**Solution:**
- Increase mapSpawnMillisecondsTimeout
- Check map server startup logs for issues
- Verify network connectivity between servers

### **"User authentication failures"**

**Cause:** Account validation configuration issues
**Solution:**
- Review username/password length requirements
- Check email verification settings
- Verify custom authentication implementation

### **"Statistics not updating"**

**Cause:** Update interval too high or monitoring disabled
**Solution:**
- Decrease updateUserCountInterval (minimum 0.1 seconds)
- Ensure statistics collection is enabled
- Check for threading or performance issues

---

## 🔗 **Integration Points**

### **Database Integration**

```csharp
public class DatabaseConnector : MonoBehaviour
{
    void Start()
    {
        var centralManager = GetComponent<CentralNetworkManager>();
        centralManager.DatabaseClient = new MyDatabaseClient();
        centralManager.DataManager = new MyDataManager();
    }
}
```

### **Map Server Coordination**

```csharp
public class MapCoordinator : MonoBehaviour
{
    private CentralNetworkManager centralManager;

    public async UniTask<MapServerInstance> SpawnMapServer(string mapId, int maxPlayers)
    {
        if (centralManager.ClusterServer != null)
        {
            // Request map server spawn through cluster
            var spawnRequest = new RequestSpawnMapMessage
            {
                channelId = CentralNetworkManager.DEFAULT_CHANNEL_ID,
                mapName = mapId,
                instanceId = $"{mapId}_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                networkAddress = GetAvailableServerAddress(),
                networkPort = GetAvailableServerPort(),
                currentMapName = mapId
            };

            // Wait for spawn confirmation
            var response = await centralManager.ClusterServer.RequestSpawnMap(
                connectionId, spawnRequest, timeoutMs: 30000);

            return new MapServerInstance(response);
        }
        return null;
    }
}
```

### **Load Balancer Integration**

```csharp
public class LoadBalancer : MonoBehaviour
{
    private CentralNetworkManager centralManager;
    private InstanceLoadBalancer balancer;

    void Start()
    {
        centralManager = GetComponent<CentralNetworkManager>();
        balancer = GetComponent<InstanceLoadBalancer>();
    }

    public string GetOptimalChannelForPlayer(PlayerConnectionInfo player)
    {
        // Get available channels from central manager
        var channels = centralManager.Channels;

        // Use balancer to select best channel
        return balancer.SelectOptimalChannel(channels, player);
    }
}
```

---

## 📊 **Monitoring & Analytics**

### **Key Metrics to Track**

#### **Connection Metrics**
- **Total Connected Users**: Real-time player count
- **Channel Distribution**: Players per channel
- **Connection Rate**: New connections per minute
- **Disconnection Rate**: Drop-off statistics

#### **Server Performance**
- **Response Times**: Authentication and channel assignment latency
- **Spawn Times**: Map server startup duration
- **Error Rates**: Failed operations percentage
- **Resource Usage**: CPU, memory, and network utilization

#### **User Behavior**
- **Session Length**: Average time spent connected
- **Channel Switching**: How often players move between channels
- **Authentication Success**: Login success rate
- **Registration Rate**: New account creation frequency

### **Dashboard Integration**

```csharp
public class ServerDashboard : MonoBehaviour
{
    private CentralNetworkManager centralManager;

    void OnGUI()
    {
        if (centralManager != null && centralManager.Channels != null)
        {
            foreach (var channel in centralManager.Channels)
            {
                int playerCount = channel.Value.connections?.Count ?? 0;
                int maxPlayers = channel.Value.maxConnections;

                float utilization = (float)playerCount / maxPlayers;
                Color barColor = utilization > 0.8f ? Color.red :
                                utilization > 0.6f ? Color.yellow : Color.green;

                // Draw utilization bar
                DrawUtilizationBar(channel.Key, utilization, barColor);
            }
        }
    }
}
```

---

## 🎯 **Best Practices**

### **1. Capacity Planning**
- **Monitor Peak Usage**: Track maximum concurrent users
- **Plan for Growth**: Design channels with 20-50% headroom
- **Load Testing**: Simulate peak loads before launch
- **Auto-scaling**: Implement dynamic channel creation for events

### **2. Security Configuration**
- **Strong Passwords**: Enforce minimum complexity requirements
- **Email Verification**: Enable for production environments
- **Rate Limiting**: Implement login attempt limits
- **Audit Logging**: Track authentication events

### **3. Performance Optimization**
- **Channel Optimization**: Balance player distribution
- **Monitoring Frequency**: Adjust based on scale (smaller = more frequent)
- **Timeout Tuning**: Set appropriate timeouts for your network conditions
- **Resource Monitoring**: Track server health metrics

### **4. Operational Management**
- **Regular Backups**: Backup user account data frequently
- **Log Rotation**: Manage log file sizes for long-term operation
- **Update Planning**: Schedule maintenance windows
- **Disaster Recovery**: Plan for server failure scenarios

---

## 📋 **Configuration Checklist**

### **Pre-Launch Setup**
- [ ] Cluster server port configured and available
- [ ] Channel limits set based on expected player counts
- [ ] User validation rules appropriate for target audience
- [ ] Authentication settings configured for security level
- [ ] Monitoring intervals set for operational visibility

### **Production Validation**
- [ ] Port accessibility confirmed through firewalls
- [ ] Channel distribution tested under load
- [ ] Authentication flow verified end-to-end
- [ ] Monitoring dashboards operational
- [ ] Backup and recovery procedures tested

### **Scaling Preparation**
- [ ] Channel templates prepared for expansion
- [ ] Load balancer integration configured
- [ ] Performance monitoring thresholds set
- [ ] Auto-scaling triggers configured
- [ ] Geographic distribution planned

---

## 📞 **API Reference**

### **Core Properties**

```csharp
public int clusterServerPort = 6010;
public int mapSpawnMillisecondsTimeout = 0;
public int defaultChannelMaxConnections = 500;
public List<ChannelData> channels = new List<ChannelData>();
public bool disableDefaultLogin = false;
public int minUsernameLength = 2;
public int maxUsernameLength = 24;
public int minPasswordLength = 2;
public int minCharacterNameLength = 2;
public int maxCharacterNameLength = 16;
public bool requireEmail = false;
public bool requireEmailVerification = false;
public float updateUserCountInterval = 5f;
```

### **Key Methods**

```csharp
// Cluster management
public ClusterServer ClusterServer { get; private set; }
public Dictionary<string, ChannelData> Channels { get; }

// Authentication events
public System.Action onClientConnected;
public System.Action<DisconnectReason, SocketError, UITextKeys> onClientDisconnected;
public System.Action onClientStopped;

// Utility methods
public void ValidateConfiguration();
public void GenerateConfigurationReport();
```

### **ChannelData Structure**

```csharp
[Serializable]
public class ChannelData
{
    public string id;              // Unique channel identifier
    public string title;           // Display name
    public int maxConnections;     // Maximum players
    public bool isActive = true;   // Channel availability
    public string region;          // Geographic region (optional)
    public string gameMode;        // Game mode type (optional)
}
```

---

## 📈 **Scaling Guidelines**

### **Small MMO (100-1000 players)**
```
Cluster Port: 6010
Default Channel: 500 max connections
Custom Channels: 2-3 (PvP, PvE, Events)
Update Interval: 5 seconds
Timeout: 30000ms
```

### **Medium MMO (1000-10000 players)**
```
Cluster Port: 6010
Default Channel: 1000 max connections
Custom Channels: 5-8 (Regional + Game Modes)
Update Interval: 3 seconds
Timeout: 45000ms
```

### **Large MMO (10000+ players)**
```
Cluster Port: 6010
Default Channel: 2000 max connections
Custom Channels: 10-15 (Multi-regional)
Update Interval: 1 second
Timeout: 60000ms
Load Balancer: Required
```

---

## 🔄 **Version History**

### **Current Version**
- Complete channel management system
- Advanced user authentication configuration
- Real-time server monitoring and statistics
- Unity Editor integration with validation tools
- Comprehensive documentation and best practices

### **Key Features**
- **Multi-channel Support**: Organize players by region, game mode, or server type
- **Flexible Authentication**: Configurable username/password requirements
- **Cluster Coordination**: Server-to-server communication infrastructure
- **Load Balancing Ready**: Integration points for player distribution
- **Monitoring Suite**: Real-time statistics and health checking

---

*This documentation covers the complete CentralNetworkManager system for NightBlade MMO server orchestration. For the latest updates and additional features, check the official repository.*