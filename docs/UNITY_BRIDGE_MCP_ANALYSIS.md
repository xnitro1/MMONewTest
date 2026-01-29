# Unity Bridge → MCP Server: Analysis & Implementation Plan

## Executive Summary

Unity Bridge is essentially a **custom MCP-like protocol**. Converting it to an actual **MCP server** would solve all messaging issues and provide native Cline integration!

**Current Problem:** File-based polling is unreliable and requires manual notification  
**MCP Solution:** Direct tool calls with instant responses  

---

## Current Architecture vs MCP

### Current Unity Bridge (File-Based)

```
Cline/Claude
    ↓ (writes file)
unity_bridge_commands.json
    ↓ (Unity polls every 0.5s)
UnityBridge.cs (reads & executes)
    ↓ (writes result)
unity_bridge_results.json
    ↓ (Cline reads when told)
Cline/Claude
```

**Problems:**
- ❌ No real-time communication
- ❌ Polling overhead
- ❌ File I/O bottleneck
- ❌ Manual notification required
- ❌ Race conditions possible

### Proposed MCP Architecture

```
Cline (MCP Client)
    ↓ (direct tool call via stdio)
Unity MCP Server Process
    ↓ (IPC/socket to Unity)
Unity Editor Plugin
    ↓ (executes in Unity)
Return result directly
    ↓ (back through chain)
Cline (instant response!)
```

**Benefits:**
- ✅ Real-time, instant responses
- ✅ No file watching needed
- ✅ Native Cline integration
- ✅ Bi-directional communication
- ✅ Standard protocol
- ✅ No polling overhead

---

## MCP Server Design

### Tools to Expose

Map current Unity Bridge commands to MCP tools:

| Current Command | MCP Tool Name | Description |
|----------------|---------------|-------------|
| Ping | `unity_ping` | Test Unity connection |
| GetSceneInfo | `unity_get_scene_info` | Get active scene information |
| FindGameObject | `unity_find_gameobject` | Find GameObject by name |
| GetComponent | `unity_get_component` | Get component data |
| GetComponentValue | `unity_get_component_value` | Read specific field |
| SetComponentValue | `unity_set_component_value` | Modify component field |
| GetHierarchy | `unity_get_hierarchy` | Get object tree |
| GetChildren | `unity_get_children` | Get direct children |
| Log | `unity_log` | Log message to console |
| GenerateImage | `unity_generate_image` | Create 2D asset |
| GenerateShader | `unity_generate_shader` | Generate shader |
| GenerateScript | `unity_generate_script` | Generate C# script |

### Resources to Expose

Optional - MCP resources for static data:

| Resource URI | Description |
|-------------|-------------|
| `unity://scene/active` | Current scene info |
| `unity://project/stats` | Project statistics |
| `unity://bridge/status` | Bridge health status |

---

## Implementation Strategy

### Option 1: Node.js MCP Server → Unity (Recommended)

**Architecture:**
```
[Cline] ←stdio→ [Node.js MCP Server] ←HTTP/Socket→ [Unity HTTP Server]
```

**Pros:**
- ✅ Standard MCP implementation
- ✅ Works with existing Unity Bridge code
- ✅ Easy to develop and test
- ✅ Can run while Unity is closed (queues commands)

**Cons:**
- ⚠️ Requires Unity to run HTTP server
- ⚠️ Two processes (Node + Unity)

**Implementation:**
1. Create Node.js MCP server in `C:\Users\Fallen\OneDrive\Documents\Cline\MCP\unity-bridge`
2. Add HTTP server to Unity (simple REST endpoint)
3. MCP server forwards tool calls to Unity HTTP API
4. Unity responds with JSON
5. MCP server returns result to Cline

### Option 2: C# MCP Server in Unity

**Architecture:**
```
[Cline] ←stdio→ [Unity Standalone MCP Process] ←IPC→ [Unity Editor]
```

**Pros:**
- ✅ Pure C# implementation
- ✅ Native Unity integration
- ✅ No Node.js dependency

**Cons:**
- ⚠️ MCP SDK is TypeScript/Python only
- ⚠️ Would need to implement MCP protocol manually
- ⚠️ More complex IPC setup

### Option 3: Python MCP Server → Unity

**Architecture:**
```
[Cline] ←stdio→ [Python MCP Server] ←Socket→ [Unity Socket Server]
```

**Pros:**
- ✅ Python MCP SDK available
- ✅ Good for AI/ML integration
- ✅ Easy scripting

**Cons:**
- ⚠️ Requires Python runtime
- ⚠️ Similar complexity to Node.js option

---

## Recommended Approach: Node.js MCP Server

### Phase 1: Add HTTP Server to Unity

Create `UnityBridgeHTTPServer.cs`:
```csharp
// Simple HTTP server in Unity Editor
// Listens on localhost:8765
// Receives POST requests with JSON commands
// Returns JSON results
```

### Phase 2: Create Node.js MCP Server

```typescript
// unity-bridge/src/index.ts
import { Server } from '@modelcontextprotocol/sdk/server/index.js';
import axios from 'axios';

const UNITY_API = 'http://localhost:8765';

class UnityBridgeMCPServer {
  private server: Server;
  
  setupToolHandlers() {
    this.server.setRequestHandler(ListToolsRequestSchema, async () => ({
      tools: [
        {
          name: 'unity_get_scene_info',
          description: 'Get information about the active Unity scene',
          inputSchema: { type: 'object', properties: {} }
        },
        {
          name: 'unity_find_gameobject',
          description: 'Find a GameObject by name',
          inputSchema: {
            type: 'object',
            properties: {
              name: { type: 'string', description: 'GameObject name' }
            },
            required: ['name']
          }
        },
        // ... more tools
      ]
    }));
    
    this.server.setRequestHandler(CallToolRequestSchema, async (request) => {
      // Forward to Unity HTTP API
      const response = await axios.post(UNITY_API + '/execute', {
        command: request.params.name.replace('unity_', ''),
        parameters: request.params.arguments
      });
      
      return {
        content: [{
          type: 'text',
          text: JSON.stringify(response.data, null, 2)
        }]
      };
    });
  }
}
```

### Phase 3: Configure MCP

Add to `cline_mcp_settings.json`:
```json
{
  "mcpServers": {
    "unity-bridge": {
      "command": "node",
      "args": ["C:/Users/Fallen/OneDrive/Documents/Cline/MCP/unity-bridge/build/index.js"],
      "disabled": false,
      "autoApprove": []
    }
  }
}
```

---

## Benefits of MCP Conversion

### For Users
- ✅ **Instant responses** - No more manual pinging
- ✅ **Native chat** - Cline can control Unity in conversation
- ✅ **Seamless UX** - Just ask, Cline does it
- ✅ **No file juggling** - MCP handles communication

### For Developers
- ✅ **Standard protocol** - Well-documented MCP spec
- ✅ **Better debugging** - MCP has built-in error handling
- ✅ **Extensible** - Easy to add new tools
- ✅ **Maintainable** - Clean separation of concerns

### For AI Partners
- ✅ **Direct access** - No intermediary files
- ✅ **Type safety** - JSON Schema validation
- ✅ **Discoverability** - Tools self-describe
- ✅ **Reliability** - Guaranteed delivery

---

## Implementation Checklist

### Prerequisites
- [ ] Node.js installed
- [ ] Unity project with current Unity Bridge
- [ ] Understanding of HTTP/REST basics

### Phase 1: Unity HTTP Server (2-3 hours)
- [ ] Create `UnityBridgeHTTPServer.cs` in Unity
- [ ] Implement simple HTTP listener (HttpListener)
- [ ] Add POST endpoint `/execute` for commands
- [ ] Add GET endpoint `/status` for health check
- [ ] Test with Postman/curl
- [ ] Handle Unity main thread execution
- [ ] Add error handling

### Phase 2: Node.js MCP Server (2-3 hours)
- [ ] Bootstrap project: `npx @modelcontextprotocol/create-server unity-bridge`
- [ ] Install dependencies: `axios`
- [ ] Implement `ListToolsRequestSchema` handler
- [ ] Implement `CallToolRequestSchema` handler
- [ ] Map all Unity Bridge commands to MCP tools
- [ ] Add proper error handling
- [ ] Build and test locally

### Phase 3: Integration (1 hour)
- [ ] Add MCP server to `cline_mcp_settings.json`
- [ ] Restart Cline to load new server
- [ ] Test each tool
- [ ] Verify end-to-end communication
- [ ] Update documentation

### Phase 4: Advanced Features (Optional)
- [ ] Add MCP resources for scene data
- [ ] Implement resource templates
- [ ] Add subscriptions for Unity events
- [ ] Create prompts for common workflows
- [ ] Add sampling for AI-driven UI design

---

## Technical Challenges

### Challenge 1: Unity Main Thread
**Problem:** HTTP server runs on background thread, Unity API requires main thread  
**Solution:** Queue commands, execute on Unity Update() loop  

### Challenge 2: Unity Editor Restart
**Problem:** MCP server stays running, Unity restarts  
**Solution:** Heartbeat endpoint, reconnection logic  

### Challenge 3: Security
**Problem:** HTTP server exposes Unity to network  
**Solution:** Bind to localhost only, add authentication token  

### Challenge 4: Cross-Platform
**Problem:** Different paths on Mac/Windows/Linux  
**Solution:** Environment variables, config file  

---

## Comparison: Current vs MCP

| Feature | File-Based (Current) | MCP Server (Proposed) |
|---------|---------------------|---------------------|
| **Setup Time** | 30 seconds | 5 minutes (one-time) |
| **Response Time** | Manual (~10 sec) | Instant (<1 sec) |
| **Reliability** | Medium (file issues) | High (direct protocol) |
| **User Experience** | Manual ping needed | Seamless |
| **Maintenance** | Simple files | Standard protocol |
| **Discoverability** | Documented | Self-describing |
| **Type Safety** | Runtime errors | JSON Schema validation |
| **Error Handling** | Basic | Robust MCP errors |
| **Extensibility** | Add commands | Add tools |
| **Overhead** | File I/O + polling | TCP connection |

---

## Migration Path

### Phase 1: Keep Both (Recommended Start)
- File-based Unity Bridge still works
- Add MCP server alongside
- Users can choose preferred method
- Gradual transition

### Phase 2: MCP Primary
- MCP becomes default
- File-based for fallback
- Deprecation notice

### Phase 3: MCP Only
- Remove file-based code
- Pure MCP implementation
- Cleaner codebase

---

## Code Structure

### Unity Side

```
Assets/NightBlade/Core/Utils/
├── UnityBridge.cs              ← Keep for compatibility
├── UnityBridgeHTTPServer.cs    ← NEW! HTTP endpoint
├── UnityBridgeMCPAdapter.cs    ← NEW! Bridges HTTP to Unity API
└── Editor/
    ├── AIAssistantWindow.cs    ← Keep, now uses MCP internally
    └── UnityEditorBridge.cs    ← Keep for Edit mode
```

### MCP Server Side

```
C:/Users/Fallen/OneDrive/Documents/Cline/MCP/unity-bridge/
├── package.json
├── tsconfig.json
├── src/
│   └── index.ts                ← Main MCP server
└── build/
    └── index.js                ← Compiled executable
```

---

## Next Steps

### To Proceed with MCP Implementation:

1. **Decide:** Do you want to implement MCP conversion?
2. **Timeline:** This is a 6-10 hour project
3. **Benefits:** Instant AI-Unity communication, no manual pinging
4. **Compatibility:** Can keep file-based as fallback

### Quick Win Alternative:

If full MCP is too much right now, we could:
- Add a simpler **"Auto-check chat"** button that polls every 2 seconds
- Not perfect, but better than manual checking
- Takes 10 minutes to implement

### Questions to Consider:

1. **Priority:** Is instant messaging critical, or is current workflow acceptable?
2. **Complexity:** Comfortable with HTTP server in Unity?
3. **Maintenance:** Want to maintain both systems or just MCP?
4. **Timeline:** Need this working today, or can invest time in proper MCP?

---

## My Recommendation

**Short Term (Today):**
- Keep improved file-based system with copy/paste workflow
- It works, users are excited, documentation is complete

**Medium Term (Next Week):**
- Implement MCP server for instant communication
- Keep file-based as fallback
- Best of both worlds

**Long Term (Future):**
- Pure MCP implementation
- Remove file-based code
- Cleaner, more maintainable

---

## Conclusion

Yes, Unity Bridge is conceptually identical to MCP! Converting to actual MCP would:
- ✅ Solve all messaging issues
- ✅ Provide instant responses  
- ✅ Enable native Cline integration
- ✅ Follow industry standard protocol
- ✅ Make Unity a first-class Cline tool

**Ready to build it?** We can start with the Node.js MCP server right now! 🚀
