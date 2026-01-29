# 🏭 MMOServerInstance - Server-Side MMO Orchestration

## **Overview**

The **MMOServerInstance** is the central orchestrator for server-side MMO operations in NightBlade. It manages four critical network managers (Central, MapSpawn, Map, and Database), handles complex configuration management, and provides comprehensive server infrastructure coordination for large-scale multiplayer environments.

**Type:** Singleton Component (MonoBehaviour)  
**Purpose:** Unified server-side MMO infrastructure management and orchestration  
**Location:** `Assets/NightBlade/MMO/MMOServerInstance.cs`

---

## 📋 **Quick Start**

1. **Add to Scene**: Attach `MMOServerInstance` to your server scene (requires LogGUI component)
2. **Configure Network Managers**: Assign CentralNetworkManager, MapSpawnNetworkManager, MapNetworkManager, and DatabaseNetworkManager
3. **Set Protocol**: Choose WebSocket/UDP and configure SSL certificates if using secure WebSocket
4. **Configure Database**: Set up database options and caching preferences
5. **Start Servers**: Use auto-start options or runtime controls to launch server components

```csharp
// Basic server setup
MMOServerInstance server = FindObjectOfType<MMOServerInstance>();
if (server != null)
{
    // Configure for production
    server.UseWebSocket = true;
    server.WebSocketSecure = true;
    server.WebSocketCertificateFilePath = "/path/to/cert.pfx";
    server.WebSocketCertificatePassword = "cert_password";

    // Start all servers
    server.StartCentralServer();
    server.StartMapSpawnServer();
    server.StartMapServer();
    server.StartDatabaseManagerServer();
}
```

---

## 🏗️ **Architecture Overview**

### **Four-Pillar Server Architecture**

```
┌─────────────────┐
│ MMOClientInstance │ ← Client-side orchestration
│                 │
│  CentralNetworkMgr │ ↔ Authentication & account services
│  MapNetworkManager │ ↔ World/map server networking
│                 │
│ MMOServerInstance │ ← Server-side orchestration ⭐
├─────────────────┤
│ CentralNetworkMgr │ ← Authentication & account services
│ MapSpawnNetworkMgr │ ← Dynamic map server spawning
│ MapNetworkManager │ ← World/map server networking
│ DatabaseNetworkMgr │ ← Data persistence & caching
└─────────────────┘
```

### **Server Component Responsibilities**

#### **MMOServerInstance (This Component)**
- **Infrastructure Orchestration**: Coordinate all server components
- **Configuration Management**: Handle command-line args and config files
- **Protocol Management**: WebSocket/UDP with SSL certificate support
- **Database Coordination**: Custom client or network manager options
- **Server Lifecycle**: Start/stop individual and groups of servers
- **Editor Integration**: Development tools and auto-start options

#### **CentralNetworkManager**
- **Authentication Services**: User login, registration, token validation
- **Character Management**: Character creation, selection, deletion
- **Channel Management**: Server list, channel configuration
- **Account Services**: Profile management, social features

#### **MapSpawnNetworkManager**
- **Dynamic Spawning**: Launch new map server instances on demand
- **Resource Management**: Monitor and cleanup idle map servers
- **Load Balancing**: Distribute players across map instances
- **Process Management**: Handle server executable launching

#### **MapNetworkManager**
- **World Networking**: Real-time world state synchronization
- **Player Management**: Handle player connections in world instances
- **Entity Synchronization**: Sync NPCs, items, and environmental objects
- **Gameplay Logic**: Process combat, movement, and interactions

#### **DatabaseNetworkManager**
- **Data Persistence**: Store player data, characters, items
- **Caching Layer**: Improve performance with intelligent caching
- **Connection Pooling**: Manage database connection efficiency
- **Query Optimization**: Handle complex MMO data operations

---

## ⚙️ **Core Configuration**

### **Network Manager References**

#### **Central Network Manager**
```csharp
[SerializeField]
private CentralNetworkManager centralNetworkManager = null;
```
- **Purpose**: Core authentication and account management server
- **Auto-Detection**: Finds component in hierarchy if not assigned
- **Critical**: Required for all player authentication and character operations

#### **Map Spawn Network Manager**
```csharp
[SerializeField]
private MapSpawnNetworkManager mapSpawnNetworkManager = null;
```
- **Purpose**: Dynamic map server spawning and lifecycle management
- **Auto-Detection**: Finds component in hierarchy if not assigned
- **Critical**: Required for scaling to multiple world instances

#### **Map Network Manager**
```csharp
[SerializeField]
private MapNetworkManager mapNetworkManager = null;
```
- **Purpose**: World/map server networking and gameplay
- **Auto-Detection**: Finds component in hierarchy if not assigned
- **Critical**: Required for actual world server operation

#### **Database Network Manager**
```csharp
[SerializeField]
private DatabaseNetworkManager databaseNetworkManager = null;
```
- **Purpose**: Database connectivity and data persistence
- **Optional**: Can be replaced with custom database client
- **Critical**: Required unless using custom database solution

### **Database Configuration Options**

#### **Custom Database Client**
```csharp
[SerializeField]
private bool useCustomDatabaseClient = false;
[SerializeField]
private GameObject customDatabaseClientSource = null;
```
- **Custom Client**: Use specialized database implementation
- **Source Object**: GameObject containing IDatabaseClient component
- **Fallback**: Uses DatabaseNetworkManager if custom not configured

#### **Database Options**
```csharp
public int databaseOptionIndex;
public bool disableDatabaseCaching;
```
- **Option Index**: Select database type/configuration
- **Cache Control**: Enable/disable caching for debugging

### **Protocol Configuration**

#### **WebSocket Settings**
```csharp
[SerializeField] private bool useWebSocket = false;
[SerializeField] private bool webSocketSecure = false;
[SerializeField] private string webSocketCertPath = string.Empty;
[SerializeField] private string webSocketCertPassword = string.Empty;
```
- **WebSocket**: More compatible with web/mobile clients
- **Secure WebSocket**: SSL/TLS encrypted connections
- **Certificate**: PFX/P12 certificate file path and password

**Protocol Recommendations:**
| Environment | Protocol | Security | Rationale |
|-------------|----------|----------|----------|
| **Development** | WebSocket | Disabled | Easy debugging, compatible |
| **Testing** | WebSocket | Disabled | Match development setup |
| **Production** | WebSocket | Enabled | Secure, scalable |
| **High Performance** | UDP | N/A | Lowest latency for gaming |

---

## 🎮 **Editor Development Tools**

### **Auto-Start Configuration**

#### **Server Auto-Start Options**
```csharp
public bool startCentralOnAwake;
public bool startMapSpawnOnAwake;
public bool startDatabaseOnAwake;
public bool startMapOnAwake;
public BaseMapInfo startingMap;
```
- **Central**: Auto-start authentication server on Awake
- **MapSpawn**: Auto-start dynamic spawning system
- **Database**: Auto-start database connectivity
- **Map**: Auto-start world server with specified map

#### **Development Workflow**
```csharp
// Enable auto-start for development
mmoServer.startCentralOnAwake = true;
mmoServer.startMapOnAwake = true;
mmoServer.startingMap = defaultMap;

// Enter play mode - servers start automatically
// Perfect for rapid iteration during development
```

### **Editor Integration Features**
- **Singleton Management**: Automatic DontDestroyOnLoad setup
- **Component Validation**: Runtime checks for missing dependencies
- **Configuration Reports**: Detailed setup status logging
- **Auto-Assignment**: Find and assign network managers automatically

---

## 🚀 **Server Management**

### **Individual Server Control**

#### **Starting Servers**
```csharp
// Start individual servers
public void StartCentralServer();      // Authentication & accounts
public void StartMapSpawnServer();     // Dynamic spawning system
public void StartMapServer();          // World server instance
public void StartDatabaseManagerServer(); // Database connectivity
```

#### **Server Dependencies**
```
StartCentralServer()
    ↓
StartMapSpawnServer()  ← Depends on Central for coordination
    ↓
StartMapServer()       ← Depends on Central for authentication
    ↓
StartDatabaseManagerServer() ← Required by all servers for data
```

### **Batch Server Operations**

#### **Start All Servers**
```csharp
// Runtime method for starting all configured servers
private void StartAllServers()
{
    StartCentralServer();
    StartMapSpawnServer();
    StartMapServer();
    StartDatabaseManagerServer();
}
```

#### **Server Health Monitoring**
- **Status Tracking**: Monitor which servers are running
- **Dependency Validation**: Ensure required servers are available
- **Resource Monitoring**: Track server performance and resource usage
- **Automatic Recovery**: Restart failed servers when possible

---

## ⚙️ **Configuration Management**

### **Command-Line Configuration**

#### **Server Arguments**
The MMOServerInstance supports extensive command-line configuration for production deployment:

```bash
# Protocol configuration
--use-web-socket                    # Enable WebSocket protocol
--web-socket-secure                 # Enable SSL/TLS encryption
--web-socket-cert-path /path/cert.pfx   # SSL certificate path
--web-socket-cert-password password     # Certificate password

# Network configuration
--central-address 192.168.1.100     # Central server address
--central-port 6010                 # Central server port
--cluster-port 6020                 # Cluster communication port
--public-address 203.0.113.1        # Public server address

# Database configuration
--database-option-index 0           # Database type selection
--disable-database-caching          # Disable caching for debugging

# Map configuration
--map-spawn-port 6030               # Map spawn server port
--spawn-exe-path /path/server.exe    # Map server executable path
--not-spawn-in-batch-mode           # Disable batch spawning
```

#### **Configuration File Support**
- **ServerConfig**: JSON-based configuration file
- **Environment Overrides**: Command-line arguments override config files
- **Validation**: Runtime validation of configuration values
- **Migration**: Automatic config file updates for new versions

### **Runtime Configuration**

#### **Dynamic Reconfiguration**
```csharp
// Change protocol at runtime (requires restart)
mmoServer.UseWebSocket = true;
mmoServer.WebSocketSecure = true;
// Note: Some changes require server restart
```

#### **Hot-Reload Capabilities**
- **Database Options**: Can switch database types without restart
- **Cache Settings**: Enable/disable caching dynamically
- **Network Settings**: Some network parameters can be updated
- **Map Configuration**: Change starting maps without restart

---

## 🔗 **Database Integration**

### **Database Manager Options**

#### **Network Manager Database**
```csharp
// Standard database network manager
DatabaseNetworkManager dbManager = GetComponent<DatabaseNetworkManager>();
dbManager.SetDatabaseByOptionIndex(databaseOptionIndex);
dbManager.StartServer();
```

#### **Custom Database Client**
```csharp
// Custom database implementation
useCustomDatabaseClient = true;
customDatabaseClientSource = gameObjectWithCustomDB;
IDatabaseClient customDB = customDatabaseClientSource.GetComponent<IDatabaseClient>();
```

### **Database Features**

#### **Caching System**
- **Intelligent Caching**: Cache frequently accessed data
- **Cache Invalidation**: Automatic cache cleanup on data changes
- **Performance Monitoring**: Track cache hit/miss ratios
- **Debug Controls**: Disable caching for debugging

#### **Connection Management**
- **Connection Pooling**: Reuse database connections
- **Timeout Handling**: Manage connection timeouts gracefully
- **Retry Logic**: Automatic retry on transient failures
- **Load Balancing**: Distribute queries across database servers

---

## 🔐 **Security Configuration**

### **SSL/TLS Setup**

#### **Certificate Configuration**
```csharp
// Secure WebSocket setup
UseWebSocket = true;
WebSocketSecure = true;
WebSocketCertificateFilePath = "/etc/ssl/certs/server.pfx";
WebSocketCertificatePassword = "secure_password";
```

#### **Certificate Requirements**
- **Format**: PFX or P12 certificate format
- **Contents**: Private key and certificate chain
- **Permissions**: Read access for server process
- **Validation**: Automatic certificate validation

### **Security Best Practices**

#### **Production Security**
- **Always Use SSL**: Enable WebSocketSecure in production
- **Strong Passwords**: Secure certificate passwords
- **Access Control**: Restrict certificate file permissions
- **Regular Rotation**: Update certificates regularly

#### **Development Security**
- **Self-Signed Certificates**: Acceptable for development
- **Local Testing**: Use localhost certificates
- **Debug Logging**: Enable security event logging
- **Access Monitoring**: Track authentication attempts

---

## 📊 **Runtime Monitoring**

### **Server Status Tracking**

#### **Individual Server Status**
```csharp
// Check server status at runtime
bool centralRunning = CentralNetworkManager.IsServer;
bool mapSpawnRunning = MapSpawnNetworkManager.IsServer;
bool mapRunning = MapNetworkManager.IsServer;
// Database status through IDatabaseClient interface
```

#### **Health Monitoring**
- **Connection Counts**: Track active client connections
- **Performance Metrics**: Monitor server response times
- **Resource Usage**: Track CPU, memory, and network usage
- **Error Rates**: Monitor server error conditions

### **Diagnostic Tools**

#### **Configuration Validation**
- **Dependency Checks**: Ensure all required components are present
- **Configuration Validation**: Verify network settings and certificates
- **Performance Analysis**: Identify potential bottlenecks
- **Security Auditing**: Check security configuration compliance

#### **Logging Integration**
- **LogGUI Component**: Required for server logging
- **Structured Logging**: Categorized log messages
- **Performance Logging**: Server metrics and timing data
- **Error Tracking**: Comprehensive error reporting

---

## 🚨 **Common Issues & Solutions**

### **"Network managers not found"**

**Symptoms:** Servers fail to start due to missing network managers
**Causes:**
- Network manager components not added to scene
- Incorrect component references
- Auto-assignment not run

**Solutions:**
- Run "Auto-Assign" in the custom editor
- Manually assign network managers in inspector
- Verify components exist in scene hierarchy

### **"SSL certificate errors"**

**Symptoms:** Secure WebSocket connections fail
**Causes:**
- Invalid certificate file path
- Incorrect certificate password
- Certificate format issues
- Permission problems

**Solutions:**
- Verify certificate file exists and is readable
- Check certificate password
- Ensure PFX/P12 format is used
- Validate certificate permissions

### **"Database connection failures"**

**Symptoms:** Database operations fail
**Causes:**
- Incorrect database configuration
- Network connectivity issues
- Authentication problems
- Database server unavailable

**Solutions:**
- Check database option index
- Verify database server connectivity
- Validate authentication credentials
- Check database server status

### **"Server auto-start not working"**

**Symptoms:** Servers don't start automatically in editor
**Causes:**
- Auto-start options not enabled
- Missing required components
- Configuration validation failures

**Solutions:**
- Enable appropriate auto-start options
- Ensure all required components are assigned
- Check configuration validation results
- Verify scene setup is correct

### **"Performance issues with multiple servers"**

**Symptoms:** Server performance degrades with multiple components
**Causes:**
- Resource contention between servers
- Insufficient system resources
- Configuration conflicts
- Database connection limits

**Solutions:**
- Allocate sufficient system resources
- Stagger server startup times
- Optimize database connection pooling
- Monitor resource usage per server

---

## 📊 **Performance Optimization**

### **Server Resource Management**

#### **CPU Optimization**
- **Thread Management**: Efficient thread utilization
- **Process Affinity**: Pin servers to specific CPU cores
- **Load Balancing**: Distribute workload across servers
- **Background Processing**: Offload non-critical tasks

#### **Memory Management**
- **Object Pooling**: Reuse network objects and buffers
- **Garbage Collection**: Tune GC settings for server workloads
- **Memory Limits**: Set appropriate memory limits per server
- **Leak Detection**: Monitor for memory leaks in long-running servers

### **Network Optimization**

#### **Connection Efficiency**
- **Connection Pooling**: Reuse network connections
- **Message Batching**: Combine multiple messages
- **Compression**: Enable network message compression
- **Timeout Tuning**: Optimize connection timeouts

#### **Protocol Selection**
```csharp
// Performance comparison
UDP:     Lowest latency, highest performance
WS:      Good compatibility, moderate performance
WSS:     Secure but highest overhead
```

### **Database Optimization**

#### **Query Optimization**
- **Indexing**: Ensure proper database indexes
- **Query Caching**: Cache frequently executed queries
- **Batch Operations**: Group multiple database operations
- **Connection Pooling**: Efficient database connection management

#### **Caching Strategies**
- **Multi-Level Caching**: Memory, disk, and distributed caching
- **Cache Invalidation**: Smart cache cleanup strategies
- **Cache Warming**: Pre-load frequently accessed data
- **Cache Monitoring**: Track cache hit rates and performance

---

## 🔄 **Scaling Strategies**

### **Horizontal Scaling**

#### **Server Distribution**
```
Load Balancer
    ↓
[Central Server] ← Authentication & accounts
    ↓
[Map Spawn Server] ← Instance management
    ↓
[Map Server 1] [Map Server 2] [Map Server 3] ← World instances
    ↓
[Database Cluster] ← Data persistence
```

#### **Scaling Considerations**
- **Central Server**: Usually 1-2 instances with load balancing
- **Map Spawn Server**: 1 instance per region/cluster
- **Map Servers**: Scale horizontally based on player load
- **Database**: Cluster with read/write splitting

### **Vertical Scaling**

#### **Resource Allocation**
- **CPU**: More cores for higher player concurrency
- **Memory**: More RAM for larger worlds and player counts
- **Network**: Higher bandwidth for more concurrent connections
- **Storage**: Faster I/O for database operations

#### **Configuration Tuning**
```csharp
// High-performance configuration
UseWebSocket = false;  // UDP for lowest latency
// Optimize thread pools
// Increase connection limits
// Tune database connection pools
```

---

## 📋 **Configuration Checklist**

### **Initial Setup**
- [ ] Attach MMOServerInstance to server scene (with LogGUI)
- [ ] Configure all four network managers (Central, MapSpawn, Map, Database)
- [ ] Set WebSocket protocol and SSL certificate if using secure connections
- [ ] Configure database options and caching settings
- [ ] Set up editor auto-start options for development

### **Network Configuration**
- [ ] Choose appropriate protocol (WebSocket/UDP) for target environment
- [ ] Configure SSL certificates for secure WebSocket connections
- [ ] Set up network addresses and ports for all server components
- [ ] Test network connectivity between server components
- [ ] Validate firewall and security group configurations

### **Database Setup**
- [ ] Choose database type and configure connection settings
- [ ] Set up database caching and performance options
- [ ] Configure custom database client if not using network manager
- [ ] Test database connectivity and query performance
- [ ] Set up database backup and recovery procedures

### **Development Configuration**
- [ ] Configure auto-start options for rapid development iteration
- [ ] Set up appropriate starting maps for testing
- [ ] Configure logging and diagnostic options
- [ ] Set up development certificates for SSL testing
- [ ] Test server startup and shutdown procedures

### **Production Deployment**
- [ ] Configure production SSL certificates and security settings
- [ ] Set up monitoring and alerting for server health
- [ ] Configure load balancing and failover options
- [ ] Set up automated deployment and configuration management
- [ ] Test production configuration with realistic load

### **Monitoring & Maintenance**
- [ ] Set up server performance monitoring and alerting
- [ ] Configure log aggregation and analysis
- [ ] Set up automated backup and recovery procedures
- [ ] Configure server scaling and resource management
- [ ] Set up security monitoring and intrusion detection

---

## 📞 **API Reference**

### **Core Properties**

```csharp
public static MMOServerInstance Singleton { get; }
public CentralNetworkManager CentralNetworkManager { get; }
public MapSpawnNetworkManager MapSpawnNetworkManager { get; }
public MapNetworkManager MapNetworkManager { get; }
public DatabaseNetworkManager DatabaseNetworkManager { get; }
public IDatabaseClient DatabaseClient { get; }
public IChatProfanityDetector ChatProfanityDetector { get; }
```

### **Protocol Properties**

```csharp
public bool UseWebSocket { get; set; }
public bool WebSocketSecure { get; set; }
public string WebSocketCertificateFilePath { get; set; }
public string WebSocketCertificatePassword { get; set; }
```

### **Server Control Methods**

```csharp
public void StartCentralServer();
public void StartMapSpawnServer();
public void StartMapServer();
public void StartDatabaseManagerServer();
public void StartDatabaseManagerClient();
```

### **Database Configuration**

```csharp
public bool useCustomDatabaseClient;
public GameObject customDatabaseClientSource;
public int databaseOptionIndex;
public bool disableDatabaseCaching;
```

### **Editor Development Properties**

```csharp
public bool startCentralOnAwake;
public bool startMapSpawnOnAwake;
public bool startDatabaseOnAwake;
public bool startMapOnAwake;
public BaseMapInfo startingMap;
```

---

## 🎯 **Best Practices**

### **1. Development Workflow**
- **Modular Configuration**: Use different configurations for dev/test/prod
- **Auto-Start**: Enable auto-start for rapid development iteration
- **Incremental Testing**: Test individual servers before full integration
- **Configuration Validation**: Regularly validate server configurations
- **Performance Monitoring**: Monitor server performance during development

### **2. Production Deployment**
- **Security First**: Always use SSL/TLS in production environments
- **Monitoring**: Implement comprehensive monitoring and alerting
- **Backup Strategy**: Regular backups with tested recovery procedures
- **Scaling Plan**: Design for horizontal and vertical scaling
- **Documentation**: Maintain detailed configuration documentation

### **3. Maintenance & Operations**
- **Regular Updates**: Keep server software and configurations updated
- **Performance Tuning**: Continuously monitor and optimize performance
- **Security Audits**: Regular security assessments and updates
- **Incident Response**: Prepared procedures for server incidents
- **Capacity Planning**: Monitor usage trends and plan for growth

### **4. Troubleshooting**
- **Log Analysis**: Comprehensive logging for issue diagnosis
- **Diagnostic Tools**: Built-in tools for configuration and performance analysis
- **Incremental Changes**: Make configuration changes incrementally
- **Rollback Plans**: Ability to quickly rollback configuration changes
- **Knowledge Base**: Document solutions to common issues

### **5. Security**
- **Network Security**: Secure all network communications
- **Access Control**: Implement proper authentication and authorization
- **Data Protection**: Encrypt sensitive data at rest and in transit
- **Regular Audits**: Periodic security assessments and penetration testing
- **Compliance**: Meet relevant security standards and regulations

---

## 📈 **Advanced Configuration**

### **Custom Server Orchestration**

#### **Server Startup Sequencing**
```csharp
public class CustomServerOrchestrator : MonoBehaviour
{
    private MMOServerInstance serverInstance;

    async void StartCustomServerSequence()
    {
        // Start central server first
        serverInstance.StartCentralServer();
        await WaitForServerReady(CentralNetworkManager);

        // Then start supporting services
        serverInstance.StartDatabaseManagerServer();
        serverInstance.StartMapSpawnServer();
        await WaitForServerReady(DatabaseNetworkManager);

        // Finally start world servers
        serverInstance.StartMapServer();
    }
}
```

### **Dynamic Configuration Management**

#### **Environment-Based Configuration**
```csharp
public class ServerConfigManager : MonoBehaviour
{
    public void LoadEnvironmentConfig(string environment)
    {
        var server = MMOServerInstance.Singleton;

        switch (environment)
        {
            case "development":
                ConfigureDevelopment(server);
                break;
            case "staging":
                ConfigureStaging(server);
                break;
            case "production":
                ConfigureProduction(server);
                break;
        }
    }

    private void ConfigureProduction(MMOServerInstance server)
    {
        server.UseWebSocket = true;
        server.WebSocketSecure = true;
        server.WebSocketCertificateFilePath = GetProductionCertPath();
        // Additional production settings...
    }
}
```

---

*This documentation covers the complete MMOServerInstance system for server-side MMO orchestration in NightBlade. For the latest updates and additional features, check the official repository.*