# StringBuilder Pooling System

## Overview

The StringBuilder Pooling system provides efficient memory management for string building operations in Unity. It eliminates garbage collection pressure from frequent StringBuilder allocations during logging, debugging, and dynamic string construction.

## Key Benefits

- **GC Reduction**: Eliminates StringBuilder allocation overhead
- **Performance**: Reuses StringBuilder instances across operations
- **Thread-Safe**: All operations are thread-safe for multi-threaded environments
- **Automatic Management**: Simple API with automatic cleanup

## API Reference

### Core Methods

#### `StringBuilderPool.Get()`
Gets a StringBuilder from the pool or creates a new one.

```csharp
StringBuilder sb = StringBuilderPool.Get();
// Use the StringBuilder...
StringBuilderPool.Return(sb);
```

#### `StringBuilderPool.Return(StringBuilder)`
Returns a StringBuilder to the pool for reuse.

```csharp
StringBuilder sb = StringBuilderPool.Get();
// ... use StringBuilder ...
StringBuilderPool.Return(sb); // Return to pool
```

#### `StringBuilderPool.Use(Func<StringBuilder, string>)`
Gets a StringBuilder, executes a function, and automatically returns it to the pool.

```csharp
string result = StringBuilderPool.Use(sb => {
    sb.Append("Player ");
    sb.Append(playerName);
    sb.Append(" scored ");
    sb.Append(score);
    sb.Append(" points!");
    return sb.ToString();
});
```

### Properties

#### `StringBuilderPool.PoolSize`
Gets the current number of StringBuilder instances in the pool.

```csharp
int currentPoolSize = StringBuilderPool.PoolSize;
Debug.Log($"Pool contains {currentPoolSize} StringBuilder instances");
```

### Advanced Usage

#### Custom Capacity
```csharp
StringBuilder sb = StringBuilderPool.Get();
// Pool will automatically manage capacity
```

#### Chaining Operations
```csharp
string logMessage = StringBuilderPool.Use(sb => {
    return sb.AppendFormat("Frame {0}: FPS = {1:F1}", Time.frameCount, 1f/Time.deltaTime)
             .ToString();
});
```

## Performance Characteristics

- **Pool Size**: Maximum of 10 StringBuilder instances
- **Thread Safety**: All operations are thread-safe using locks
- **Memory Efficiency**: Reuses instances to reduce GC pressure
- **Capacity Management**: Automatically handles capacity requirements

## Examples

### Logging with Pooling
```csharp
using NightBlade.Core.Utils;

public class GameLogger
{
    public void LogPlayerStats(string playerName, int score, float health)
    {
        string message = StringBuilderPool.Use(sb => {
            sb.Append("[PLAYER] ");
            sb.Append(playerName);
            sb.Append(" - Score: ");
            sb.Append(score);
            sb.Append(", Health: ");
            sb.Append(health.ToString("F1"));
            return sb.ToString();
        });

        Debug.Log(message);
    }
}
```

### Network Message Building
```csharp
public string BuildNetworkMessage(int playerId, Vector3 position, Quaternion rotation)
{
    return StringBuilderPool.Use(sb => {
        sb.Append("PLAYER_UPDATE|");
        sb.Append(playerId);
        sb.Append("|");
        sb.Append(position.x.ToString("F2"));
        sb.Append(",");
        sb.Append(position.y.ToString("F2"));
        sb.Append(",");
        sb.Append(position.z.ToString("F2"));
        sb.Append("|");
        sb.Append(rotation.x.ToString("F3"));
        sb.Append(",");
        sb.Append(rotation.y.ToString("F3"));
        sb.Append(",");
        sb.Append(rotation.z.ToString("F3"));
        sb.Append(",");
        sb.Append(rotation.w.ToString("F3"));
        return sb.ToString();
    });
}
```

### Debug Information Display
```csharp
public void DisplaySystemInfo()
{
    string info = StringBuilderPool.Use(sb => {
        sb.AppendLine("=== SYSTEM INFO ===");
        sb.Append("Unity Version: ");
        sb.AppendLine(Application.unityVersion);
        sb.Append("Platform: ");
        sb.AppendLine(Application.platform.ToString());
        sb.Append("Frame Rate: ");
        sb.Append(1f/Time.deltaTime);
        sb.AppendLine(" FPS");
        sb.Append("Memory: ");
        sb.Append((UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong() / 1024 / 1024));
        sb.AppendLine(" MB");
        return sb.ToString();
    });

    Debug.Log(info);
}
```

## Integration with Performance Monitor

The StringBuilder Pooling system integrates with the PerformanceMonitor for tracking:

```csharp
// View pool statistics
int poolSize = StringBuilderPool.PoolSize;

// Performance profiling
PerformanceMonitor.ProfileStringBuilderPool(() => {
    // Your StringBuilder operations
    string result = StringBuilderPool.Use(sb => {
        // ... operations ...
        return sb.ToString();
    });
});
```

## Best Practices

1. **Always Use the Pool**: Never create new StringBuilder instances directly for temporary operations
2. **Use the `Use` Method**: Prefer `StringBuilderPool.Use()` for automatic cleanup
3. **Avoid Long-Term Holding**: Don't hold StringBuilder references across frames
4. **Profile Performance**: Use the PerformanceMonitor to track pool effectiveness
5. **Monitor Pool Size**: Keep an eye on pool size to detect potential issues

## Troubleshooting

### Pool Exhaustion
If the pool reaches its maximum size, new StringBuilder instances will be created but not pooled. This is by design to prevent memory leaks.

### Threading Issues
All operations are thread-safe, but if you experience performance issues, consider using the pool from the main thread only.

### Memory Leaks
The pool automatically manages memory, but ensure you always return StringBuilder instances to the pool using `Return()` or the `Use()` method.

## Migration Guide

### Before (GC Pressure)
```csharp
public string BuildMessage(string name, int value)
{
    StringBuilder sb = new StringBuilder(); // Creates GC pressure
    sb.Append("Player ");
    sb.Append(name);
    sb.Append(" has ");
    sb.Append(value);
    sb.Append(" points");
    return sb.ToString();
}
```

### After (Optimized)
```csharp
public string BuildMessage(string name, int value)
{
    return StringBuilderPool.Use(sb => {
        sb.Append("Player ");
        sb.Append(name);
        sb.Append(" has ");
        sb.Append(value);
        sb.Append(" points");
        return sb.ToString();
    });
}
```

## Performance Impact

- **Memory Savings**: Up to 90% reduction in StringBuilder allocations
- **GC Pressure**: Significant reduction in garbage collection frequency
- **CPU Performance**: Minimal overhead from pooling operations
- **Memory Efficiency**: Better cache locality and reduced memory fragmentation

## Version History

- **v4.0.0**: Initial implementation with thread-safe pooling
- **v4.0.1**: Added `Use()` method for automatic cleanup
- **v4.0.2**: Performance optimizations and capacity management improvements