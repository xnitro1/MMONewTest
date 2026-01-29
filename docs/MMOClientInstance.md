# 🌐 MMOClientInstance - Client-Side MMO Orchestration

## **Overview**

The **MMOClientInstance** is the central orchestrator for client-side MMO operations in NightBlade. It manages the complete client lifecycle, from authentication through world connection, coordinating between the CentralNetworkManager (for account services) and MapNetworkManager (for world gameplay).

**Type:** Singleton Component (MonoBehaviour)  
**Purpose:** Unified client-side MMO management and connection orchestration  
**Location:** `Assets/NightBlade/MMO/MMOClientInstance.cs`

---

## 📋 **Quick Start**

1. **Add to Scene**: Attach `MMOClientInstance` to your main client scene
2. **Configure Managers**: Assign or auto-assign CentralNetworkManager and MapNetworkManager
3. **Set Network Protocol**: Choose WebSocket/UDP and security settings
4. **Configure Servers**: Add network settings for available server environments
5. **Connect**: Use the built-in connection methods or UI integration

```csharp
// Basic client setup
MMOClientInstance client = FindObjectOfType<MMOClientInstance>();
if (client != null)
{
    // Configure for production
    client.UseWebSocket = true;
    client.WebSocketSecure = true;

    // Connect to central server
    client.StartCentralClient("game.yourserver.com", 6010);
}
```

---

## 🏗️ **Architecture Overview**

### **Client Architecture Layers**

```
┌─────────────────┐
│ MMOClientInstance │ ← Main orchestrator
├─────────────────┤
│ CentralNetworkMgr │ ← Authentication & account services
│ MapNetworkManager │ ← World & gameplay networking
└─────────────────┘
```

### **Component Responsibilities**

#### **MMOClientInstance (This Component)**
- **Connection Management**: Start/stop central and map server connections
- **Protocol Configuration**: WebSocket vs UDP, security settings
- **Session State**: Track authentication, character selection, channel info
- **Event Coordination**: Relay connection events to UI and game systems
- **API Orchestration**: Provide unified interface for client operations

#### **CentralNetworkManager**
- **Authentication**: Login, registration, token validation
- **Character Management**: Create, delete, select characters
- **Channel Services**: Server list, channel selection
- **Account Services**: User profile, social features

#### **MapNetworkManager**
- **World Connection**: Connect to specific map/game servers
- **Gameplay Networking**: Real-time world state synchronization
- **Entity Management**: Player, NPC, item network updates

---

## ⚙️ **Core Configuration**

### **Network Manager References**

#### **Central Network Manager**
```csharp
[SerializeField]
private CentralNetworkManager centralNetworkManager = null;
```
- **Purpose**: Handles all central server communications
- **Auto-Detection**: Finds component in hierarchy if not assigned
- **Critical**: Required for authentication and character management

#### **Map Network Manager**
```csharp
[SerializeField]
private MapNetworkManager mapNetworkManager = null;
```
- **Purpose**: Manages world/map server connections
- **Auto-Detection**: Finds component in hierarchy if not assigned
- **Critical**: Required for gameplay world participation

### **Protocol Configuration**

#### **WebSocket Settings**
```csharp
[SerializeField] private bool useWebSocket = false;
[SerializeField] private bool webSocketSecure = false;
```
- **WebSocket**: More compatible with web/mobile platforms
- **Secure WebSocket**: Encrypted connections (recommended for production)
- **UDP**: Better performance for real-time gaming

**Protocol Recommendations:**
| Environment | Protocol | Security | Rationale |
|-------------|----------|----------|----------|
| **Development** | WebSocket | Disabled | Easy debugging, firewall-friendly |
| **Testing** | WebSocket | Disabled | Consistent with development |
| **Production** | WebSocket | Enabled | Secure, web-compatible |
| **High Performance** | UDP | N/A | Lowest latency, best for gaming |

### **Network Settings Array**

#### **Server Configuration**
```csharp
[SerializeField]
private MmoNetworkSetting[] networkSettings = new MmoNetworkSetting[0];
```

**MmoNetworkSetting Structure:**
```csharp
public struct MmoNetworkSetting
{
    public string title;      // Server name/description
    public string address;    // Server IP or hostname
    public int port;         // Server port
    public bool isDefault;   // Use as default connection
}
```

**Configuration Examples:**
```csharp
// Development server
new MmoNetworkSetting
{
    title = "Development Server",
    address = "127.0.0.1",
    port = 6010,
    isDefault = true
}

// Production server
new MmoNetworkSetting
{
    title = "US East - Production",
    address = "us-east.game.com",
    port = 6010,
    isDefault = false
}
```

---

## 🔗 **Connection Management**

### **Central Server Connection**

#### **Connection Methods**
```csharp
// Connect with auto-detection
public void StartCentralClient();

// Connect to specific server
public void StartCentralClient(string address, int port);

// Disconnect
public void StopCentralClient();

// Check connection status
public bool IsConnectedToCentralServer();
```

#### **Connection Flow**
```
Client Start → CentralNetworkManager.StartClient() →
Server Connection → Authentication Available →
Character Selection → Map Server Routing
```

### **Map Server Connection**

#### **World Connection Methods**
```csharp
// Connect to specific map server
public void StartMapClient(BaseMapInfo mapInfo, string address, int port);

// Disconnect from map
public void StopMapClient();
```

#### **Map Connection Process**
```
Character Selected → Map Server Assigned →
MapNetworkManager Configured → World Connection →
Gameplay Begins → Real-time Synchronization
```

### **Session State Management**

#### **Client Data Tracking**
```csharp
public static string SelectedCentralAddress { get; private set; }
public static int SelectedCentralPort { get; private set; }
public string SelectedChannelId { get; set; }

// Clear all session data
public void ClearClientData();
```

**Session Data Includes:**
- Central server connection info
- User authentication (ID, access token)
- Selected character ID
- Channel/server selection

---

## 📡 **Event System**

### **Connection Events**

#### **Central Server Events**
```csharp
public static event System.Action OnCentralClientConnectedEvent;
public static event System.Action<DisconnectReason, SocketError, UITextKeys> OnCentralClientDisconnectedEvent;
public static event System.Action OnCentralClientStoppedEvent;
```

#### **Map Server Events**
```csharp
public static event System.Action OnMapClientConnectedEvent;
public static event System.Action<DisconnectReason, SocketError, UITextKeys> OnMapClientDisconnectedEvent;
public static event System.Action OnMapClientStoppedEvent;
```

### **Event Usage Example**
```csharp
void Start()
{
    MMOClientInstance.OnCentralClientConnectedEvent += HandleCentralConnected;
    MMOClientInstance.OnCentralClientDisconnectedEvent += HandleCentralDisconnected;
    MMOClientInstance.OnMapClientConnectedEvent += HandleMapConnected;
}

void HandleCentralConnected()
{
    Debug.Log("Connected to central server - ready for authentication");
    ShowLoginUI();
}

void HandleCentralDisconnected(DisconnectReason reason, SocketError error, UITextKeys message)
{
    Debug.Log($"Disconnected from central server: {reason}");
    ShowReconnectUI();
}
```

---

## 🔐 **Authentication & Account API**

### **User Authentication**

#### **Login Process**
```csharp
public void RequestUserLogin(string username, string password,
    ResponseDelegate<ResponseUserLoginMessage> callback);
```

**Login Flow:**
```
User Input → RequestUserLogin() → Central Server Validation →
Success: Access Token Received → Character Selection Available
Failure: Error Message → Retry Login
```

#### **Registration**
```csharp
public void RequestUserRegister(string username, string password, string email,
    ResponseDelegate<ResponseUserRegisterMessage> callback);
```

#### **Token Validation**
```csharp
public void RequestValidateAccessToken(string userId, string accessToken,
    ResponseDelegate<ResponseValidateAccessTokenMessage> callback);
```

### **Character Management**

#### **Character Operations**
```csharp
// Get character list
public void RequestCharacters(ResponseDelegate<ResponseCharactersMessage> callback);

// Create new character
public void RequestCreateCharacter(PlayerCharacterData characterData,
    ResponseDelegate<ResponseCreateCharacterMessage> callback);

// Delete character
public void RequestDeleteCharacter(string characterId,
    ResponseDelegate<ResponseDeleteCharacterMessage> callback);
```

#### **Character Selection**
```csharp
// Select character (auto-detects channel)
public void RequestSelectCharacter(string characterId,
    ResponseDelegate<ResponseSelectCharacterMessage> callback);

// Select character with specific channel
public void RequestSelectCharacter(string channelId, string characterId,
    ResponseDelegate<ResponseSelectCharacterMessage> callback);
```

**Character Selection Flow:**
```
Character Selected → Channel Validation → Map Server Assignment →
Connection Details Received → Map Server Connection Initiated →
World Join Complete → Gameplay Begins
```

---

## 🌐 **Server Discovery & Channel Management**

### **Channel System**

#### **Channel Requests**
```csharp
// Get available channels/servers
public void RequestChannels(ResponseDelegate<ResponseChannelsMessage> callback);
```

**Channel Structure:**
```csharp
public struct ChannelInfo
{
    public string id;
    public string title;
    public string description;
    public int maxConnections;
    public int currentConnections;
    // ... additional metadata
}
```

### **Server Selection Strategy**

#### **Configuration-Based Selection**
- **Network Settings Array**: Pre-configured server list
- **Default Server**: Auto-selected based on configuration
- **Region-Based**: Geographic server selection
- **Load-Based**: Server with available capacity

#### **Dynamic Server Discovery**
```csharp
// Request available servers
client.RequestChannels((responseHandler, responseCode, response) =>
{
    if (responseCode == AckResponseCode.Success)
    {
        foreach (var channel in response.channels)
        {
            Debug.Log($"Server: {channel.title} ({channel.currentConnections}/{channel.maxConnections})");
        }
    }
});
```

---

## 🔄 **Runtime Operation**

### **Initialization Sequence**

#### **Awake Phase**
```csharp
void Awake()
{
    // Singleton setup
    if (Singleton != null) Destroy(gameObject);
    DontDestroyOnLoad(gameObject);
    Singleton = this;

    // SSL certificate acceptance
    ServicePointManager.ServerCertificateValidationCallback = ...;

    // Event subscriptions
    SubscribeToNetworkEvents();
}
```

#### **Connection Lifecycle**
```
Client Start → Central Server Connect → Authentication →
Character Select → Map Server Assignment → Map Server Connect →
Gameplay Session → Disconnect Handling → Cleanup
```

### **Error Handling & Recovery**

#### **Connection Recovery**
```csharp
void OnMapDisconnected(DisconnectReason reason, SocketError error, UITextKeys message)
{
    // Automatic central server reconnection after map disconnect
    if (!IsConnectedToCentralServer())
    {
        StartCentralClient();
    }
}
```

#### **Session Recovery**
- **Token Persistence**: Access tokens survive disconnects
- **Character Memory**: Last selected character remembered
- **Channel Memory**: Preferred server/channel retained

---

## 🛠️ **Integration Examples**

### **UI Integration**

#### **Login System Integration**
```csharp
public class LoginUI : MonoBehaviour
{
    [SerializeField] private InputField usernameField;
    [SerializeField] private InputField passwordField;

    public void OnLoginButtonClicked()
    {
        var client = MMOClientInstance.Singleton;
        client.RequestUserLogin(
            usernameField.text,
            passwordField.text,
            HandleLoginResponse
        );
    }

    private void HandleLoginResponse(RequestHandlerData requestHandler,
        AckResponseCode responseCode, ResponseUserLoginMessage response)
    {
        if (responseCode == AckResponseCode.Success)
        {
            // Login successful - proceed to character selection
            ShowCharacterSelection();
        }
        else
        {
            // Show error message
            ShowError(response.message);
        }
    }
}
```

#### **Character Selection Integration**
```csharp
public class CharacterSelectionUI : MonoBehaviour
{
    public void OnCharacterSelected(string characterId)
    {
        var client = MMOClientInstance.Singleton;
        client.RequestSelectCharacter(characterId, HandleCharacterSelection);
    }

    private void HandleCharacterSelection(RequestHandlerData requestHandler,
        AckResponseCode responseCode, ResponseSelectCharacterMessage response)
    {
        if (responseCode == AckResponseCode.Success)
        {
            // Character selection successful
            // Client will automatically connect to assigned map server
            Debug.Log($"Entering world: {response.mapName}");
        }
    }
}
```

### **Game Manager Integration**

#### **Connection State Management**
```csharp
public class GameManager : MonoBehaviour
{
    void Start()
    {
        // Subscribe to connection events
        MMOClientInstance.OnCentralClientConnectedEvent += OnCentralConnected;
        MMOClientInstance.OnMapClientConnectedEvent += OnMapConnected;
        MMOClientInstance.OnCentralClientDisconnectedEvent += OnDisconnected;
    }

    void OnCentralConnected()
    {
        // Show login UI
        uiManager.ShowLoginScreen();
    }

    void OnMapConnected()
    {
        // Start gameplay
        gameStateManager.StartGameplay();
    }

    void OnDisconnected(DisconnectReason reason, SocketError error, UITextKeys message)
    {
        // Handle disconnection
        uiManager.ShowDisconnectScreen(reason, message);
    }
}
```

---

## 🔧 **Configuration Management**

### **Editor Integration**

#### **Auto-Assignment Features**
- **Network Manager Detection**: Automatically finds CentralNetworkManager and MapNetworkManager
- **Component Validation**: Checks for missing required components
- **Configuration Reports**: Generates detailed setup reports

#### **Runtime Controls**
- **Connection Testing**: Test central and map server connections
- **Session Management**: Clear client data for testing
- **Status Monitoring**: Real-time connection status display

### **Validation System**

#### **Configuration Checks**
- **Network Managers**: Required component validation
- **Network Settings**: Server configuration completeness
- **Protocol Settings**: WebSocket and security validation
- **Connection Status**: Runtime connectivity verification

#### **Performance Monitoring**
- **Connection Quality**: Network latency and stability
- **Session Persistence**: Authentication token management
- **Resource Usage**: Memory and connection pool monitoring

---

## 🚨 **Common Issues & Solutions**

### **"Cannot connect to central server"**

**Symptoms:** Client fails to connect to central server
**Causes:**
- Incorrect server address/port
- Network manager not assigned
- Firewall blocking connections
- WebSocket protocol mismatch

**Solutions:**
- Verify network settings in configuration
- Use auto-assignment to find network managers
- Check firewall settings for required ports
- Ensure WebSocket settings match server configuration

### **"Authentication fails"**

**Symptoms:** Login requests return errors
**Causes:**
- Invalid credentials
- Server authentication service down
- Token validation issues
- SSL certificate problems (for secure WebSocket)

**Solutions:**
- Verify username/password
- Check server authentication service status
- Clear client data and retry login
- For WebSocket secure: verify SSL certificates

### **"Map server connection fails"**

**Symptoms:** Character selection succeeds but world connection fails
**Causes:**
- Map server not running
- Incorrect map server assignment
- Network routing issues
- Protocol mismatch between client and map server

**Solutions:**
- Verify assigned map server is running
- Check network connectivity to map server
- Ensure WebSocket settings consistent
- Review server logs for connection attempts

### **"Session data lost on disconnect"**

**Symptoms:** User logged out after temporary disconnect
**Causes:**
- Token expiration
- Server session timeout
- Client data clearing on disconnect

**Solutions:**
- Check token validity period
- Implement automatic reconnection
- Preserve session data across disconnects
- Add connection recovery logic

### **"Multiple MMOClientInstance singletons"**

**Symptoms:** Connection issues, multiple instances
**Causes:**
- Multiple MMOClientInstance components in scene
- Scene loading without proper cleanup
- DontDestroyOnLoad conflicts

**Solutions:**
- Ensure only one MMOClientInstance per scene
- Check for duplicate components in hierarchy
- Verify singleton destruction logic
- Use FindObjectOfType for safety checks

---

## 📊 **Performance Optimization**

### **Connection Pooling**

#### **Persistent Connections**
- **Central Server**: Maintain connection for session duration
- **Map Server**: Connect on demand, disconnect when leaving world
- **Connection Reuse**: Avoid frequent connect/disconnect cycles

#### **Network Efficiency**
```csharp
// Configure for optimal performance
MMOClientInstance client = MMOClientInstance.Singleton;
client.UseWebSocket = false; // Use UDP for gaming performance
// Configure connection pooling in network managers
```

### **Memory Management**

#### **Session Data Cleanup**
```csharp
// Clear unnecessary data after disconnection
void OnCentralDisconnected(DisconnectReason reason, SocketError error, UITextKeys message)
{
    // Clear sensitive data
    ClearClientData();

    // Keep essential session info for reconnection
    // (server address, user preferences, etc.)
}
```

#### **Resource Pooling**
- **Network Objects**: Pool frequently created network entities
- **UI Components**: Pool dialog and interface elements
- **Data Structures**: Reuse collections and buffers

---

## 🔐 **Security Considerations**

### **Data Protection**

#### **Token Security**
- **Access Token Storage**: Secure local storage (not plain text)
- **Token Expiration**: Regular token refresh
- **Secure Transmission**: Use WebSocket Secure in production

#### **Connection Security**
```csharp
// Production security configuration
client.UseWebSocket = true;
client.WebSocketSecure = true; // Always use WSS in production
```

### **Validation & Sanitization**

#### **Input Validation**
- **User Input**: Validate usernames, passwords, emails
- **Server Data**: Verify received data integrity
- **Connection Parameters**: Sanitize addresses and ports

#### **Error Handling**
- **Graceful Failures**: Handle network errors without crashes
- **User Feedback**: Clear error messages for users
- **Logging**: Secure logging without exposing sensitive data

---

## 📋 **Configuration Checklist**

### **Initial Setup**
- [ ] Attach MMOClientInstance to main client scene
- [ ] Configure WebSocket vs UDP protocol
- [ ] Set up network settings for target servers
- [ ] Assign or verify CentralNetworkManager reference
- [ ] Assign or verify MapNetworkManager reference

### **Authentication Configuration**
- [ ] Test user login flow
- [ ] Verify character creation/deletion
- [ ] Test character selection process
- [ ] Confirm channel/server selection

### **Network Testing**
- [ ] Test central server connections
- [ ] Verify map server routing
- [ ] Check WebSocket/UDP compatibility
- [ ] Test secure connection options

### **Integration Testing**
- [ ] Connect UI systems to client events
- [ ] Test disconnection recovery
- [ ] Verify session persistence
- [ ] Check error handling flows

### **Production Deployment**
- [ ] Enable secure WebSocket connections
- [ ] Configure production server addresses
- [ ] Set up SSL certificates
- [ ] Test load balancing scenarios
- [ ] Verify firewall configurations

---

## 📞 **API Reference**

### **Core Properties**

```csharp
public static MMOClientInstance Singleton { get; }
public CentralNetworkManager CentralNetworkManager { get; }
public MapNetworkManager MapNetworkManager { get; }
public bool UseWebSocket { get; set; }
public bool WebSocketSecure { get; set; }
public MmoNetworkSetting[] NetworkSettings { get; }
public string SelectedChannelId { get; set; }
```

### **Connection Methods**

```csharp
public void StartCentralClient();
public void StartCentralClient(string address, int port);
public void StopCentralClient();
public void StartMapClient(BaseMapInfo mapInfo, string address, int port);
public void StopMapClient();
public bool IsConnectedToCentralServer();
```

### **Authentication API**

```csharp
public void RequestUserLogin(string username, string password, ResponseDelegate<ResponseUserLoginMessage> callback);
public void RequestUserRegister(string username, string password, string email, ResponseDelegate<ResponseUserRegisterMessage> callback);
public void RequestUserLogout(ResponseDelegate<INetSerializable> callback);
public void RequestValidateAccessToken(string userId, string accessToken, ResponseDelegate<ResponseValidateAccessTokenMessage> callback);
```

### **Character Management API**

```csharp
public void RequestCharacters(ResponseDelegate<ResponseCharactersMessage> callback);
public void RequestCreateCharacter(PlayerCharacterData characterData, ResponseDelegate<ResponseCreateCharacterMessage> callback);
public void RequestDeleteCharacter(string characterId, ResponseDelegate<ResponseDeleteCharacterMessage> callback);
public void RequestSelectCharacter(string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback);
public void RequestSelectCharacter(string channelId, string characterId, ResponseDelegate<ResponseSelectCharacterMessage> callback);
```

### **Server Discovery API**

```csharp
public void RequestChannels(ResponseDelegate<ResponseChannelsMessage> callback);
```

### **Session Management**

```csharp
public void ClearClientData();
```

### **Events**

```csharp
public static event System.Action OnCentralClientConnectedEvent;
public static event System.Action<DisconnectReason, SocketError, UITextKeys> OnCentralClientDisconnectedEvent;
public static event System.Action OnCentralClientStoppedEvent;
public static event System.Action OnMapClientConnectedEvent;
public static event System.Action<DisconnectReason, SocketError, UITextKeys> OnMapClientDisconnectedEvent;
public static event System.Action OnMapClientStoppedEvent;
```

---

## 🎯 **Best Practices**

### **1. Connection Management**
- **Persistent Central Connection**: Keep central server connection for session
- **Lazy Map Connection**: Connect to map servers on demand
- **Graceful Disconnection**: Handle network interruptions smoothly
- **Automatic Recovery**: Implement reconnection logic

### **2. Session Handling**
- **Token Persistence**: Maintain authentication across disconnects
- **State Preservation**: Remember user preferences and selections
- **Clean Logout**: Properly clear sensitive data on logout
- **Error Recovery**: Handle authentication failures gracefully

### **3. Network Configuration**
- **Environment-Specific Settings**: Different configs for dev/test/prod
- **Protocol Selection**: Choose based on platform and requirements
- **Security First**: Always use secure connections in production
- **Load Balancing**: Distribute across multiple servers

### **4. UI Integration**
- **Event-Driven Updates**: React to connection state changes
- **User Feedback**: Provide clear connection status indicators
- **Error Handling**: Show meaningful error messages
- **Loading States**: Indicate connection progress

### **5. Performance & Optimization**
- **Connection Pooling**: Reuse network connections
- **Lazy Loading**: Load UI and data on demand
- **Memory Management**: Clean up unused resources
- **Network Efficiency**: Minimize unnecessary traffic

### **6. Security & Privacy**
- **Secure Transmission**: Use encrypted connections
- **Data Validation**: Verify all network data
- **Token Security**: Protect authentication tokens
- **Audit Logging**: Track important operations

---

## 📈 **Scaling Considerations**

### **Small Scale (1-10 concurrent users)**
```
Single Server: One central + one map server
Simple Config: Basic network settings
Development Focus: Easy setup and debugging
```

### **Medium Scale (10-100 concurrent users)**
```
Multi-Server: Load balanced map servers
Dynamic Routing: Channel-based server selection
Monitoring: Basic connection tracking
```

### **Large Scale (100-1000+ concurrent users)**
```
Global Distribution: Geographic server clusters
Advanced Routing: AI-driven load balancing
Monitoring: Comprehensive analytics and alerting
Security: Enterprise-grade authentication
```

---

## 🔄 **Version History**

### **Current Features**
- **Unified Client Architecture**: Single point of control for MMO operations
- **Dual Network Manager Coordination**: Central + Map server management
- **Protocol Flexibility**: WebSocket and UDP support with security options
- **Server Discovery**: Dynamic channel and server selection
- **Authentication System**: Complete user account and character management
- **Session Persistence**: Token-based authentication with recovery
- **Event-Driven Design**: Comprehensive event system for UI integration
- **Editor Integration**: Advanced configuration and validation tools

### **Key Capabilities**
- **Cross-Platform Support**: Works on desktop, mobile, and web platforms
- **Security Options**: SSL/TLS encryption for secure connections
- **Developer Tools**: Comprehensive editor with diagnostics and testing
- **Performance Optimized**: Efficient connection management and pooling
- **Production Ready**: Error handling, logging, and monitoring integration

---

*This documentation covers the complete MMOClientInstance system for client-side MMO orchestration in NightBlade. For the latest updates and additional features, check the official repository.*