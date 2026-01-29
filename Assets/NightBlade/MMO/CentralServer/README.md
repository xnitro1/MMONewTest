# Central Server System

This directory contains the central server architecture for MMO functionality, handling authentication, character management, and server coordination.

## 📁 Directory Structure

### Core Components
- `CentralNetworkManager.cs` - Main central server networking logic
- `CentralUserPeerInfo.cs` - User connection management
- `CentralServerPeerInfo.cs` - Server-to-server communication

### Authentication & Login
- `CentralNetworkManager_Login.cs` - Login request handling
- `Messages/Login/` - Login-related network messages

### Character Management
- `CentralNetworkManager_Character.cs` - Character operations
- `Messages/Character/` - Character-related network messages

### Server Coordination
- `Cluster/` - Server cluster management
  - `ClusterClient.cs` - Client for cluster communication
  - `ClusterServer.cs` - Server cluster coordination
- `Messages/Cluster/` - Inter-server communication messages

### Data Management
- `DataManager/` - Central data persistence
  - `DefaultCentralServerDataManager.cs` - Default implementation

## 🏗️ Architecture

### Server Roles
- **Central Server**: Single point of coordination
  - User authentication and account management
  - Character data persistence
  - Server discovery and load balancing
  - Global game state coordination

### Communication Patterns
- **User Clients ↔ Central Server**: Authentication, character selection
- **Map Servers ↔ Central Server**: Player transfers, global events
- **Central Server ↔ Database**: Persistent data operations

## 🔄 Key Responsibilities

### Authentication Flow
1. Client connects to central server
2. User credentials validation
3. Account and character data retrieval
4. Server selection and redirection

### Character Management
- Character creation and customization
- Character progression data persistence
- Cross-server character transfers
- Character deletion and recovery

### Server Coordination
- Map server registration and health monitoring
- Player distribution across map servers
- Load balancing decisions
- Global event broadcasting

## 🔗 Dependencies

- **Database**: For persistent data storage
- **Networking**: Base networking layer
- **Config**: Server configuration management
- **Utils**: Common utilities and helpers

## 📊 Scalability Considerations

### Performance Optimization
- Connection pooling for database operations
- Caching frequently accessed data
- Asynchronous request processing
- Rate limiting for spam protection

### Fault Tolerance
- Server health monitoring
- Automatic failover procedures
- Data consistency checks
- Graceful degradation handling

## 📝 Development Notes

When working with the central server:
1. Ensure all operations are asynchronous to prevent blocking
2. Implement proper error handling and logging
3. Consider database connection pooling
4. Test with multiple concurrent users
5. Monitor memory usage and connection limits
6. Implement proper session management

### Security Considerations
- Validate all incoming requests
- Implement rate limiting
- Use secure communication channels
- Log suspicious activities
- Regular security audits
