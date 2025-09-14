# DStream Console Output Provider

A simple .NET console output provider for the DStream data streaming platform. This provider receives streaming JSON data via stdin and outputs formatted data to the console via stdout.

## Overview

The Console Output Provider is designed to:
- Receive streaming JSON envelopes via stdin from DStream orchestration
- Format and display data in configurable output formats (simple text or JSON)
- Integrate seamlessly with the DStream ecosystem using stdin/stdout communication
- Demonstrate the minimal code needed to create a DStream output provider

## Features

### Output Formats

The provider supports two simple output formats:

1. **Simple** (default): Clean message format with counter
   ```
   Message #1: {"value":1,"timestamp":"2025-09-14T17:11:21.5590040+00:00"}
   Message #2: {"value":2,"timestamp":"2025-09-14T17:11:22.9125080+00:00"}
   ```

2. **JSON**: Raw JSON envelope output
   ```json
   {"Payload":{"value":1,"timestamp":"2025-09-14T17:11:21.5590040+00:00"},"Meta":{"seq":1,"interval_ms":500,"provider":"counter-input-provider"}}
   ```

### Configuration

The provider accepts simple configuration via JSON:

```json
{
  "outputFormat": "simple"
}
```

**Available Options:**
- `outputFormat`: `"simple"` (default) or `"json"`

## Building

### Prerequisites
- .NET 9.0 SDK
- DStream .NET SDK (referenced as project dependencies)

### Build Commands

```bash
# Build debug version (PowerShell on macOS)
/usr/local/share/dotnet/dotnet build

# Build release version  
/usr/local/share/dotnet/dotnet build -c Release

# Publish self-contained binary (macOS x64)
/usr/local/share/dotnet/dotnet publish -c Release -r osx-x64 --self-contained
```

## Usage

### Standalone Testing

```bash
# Test configuration parsing and basic functionality
echo '{"outputFormat": "simple"}
{"source":"test","type":"test","data":{"value":123},"metadata":{"seq":1}}' | /usr/local/share/dotnet/dotnet run

# Test with JSON output format
echo '{"outputFormat": "json"}
{"source":"test","type":"test","data":{"value":456},"metadata":{"seq":2}}' | /usr/local/share/dotnet/dotnet run
```

### Pipeline Testing

```bash
# Test full pipeline with counter input provider
echo '{"interval": 500, "max_count": 3}' | ../dstream-counter-input-provider/bin/Debug/net9.0/osx-x64/counter-input-provider 2>/dev/null | \
echo '{"outputFormat": "simple"}' | /usr/local/share/dotnet/dotnet run
```

## Architecture

### Data Flow

1. **Configuration**: Receives JSON configuration via stdin (first line)
2. **Data Processing**: Continuously reads JSON envelopes from stdin (subsequent lines)
3. **Output**: Formats and writes data to stdout based on configuration
4. **Logging**: Writes status messages to stderr for debugging

### Communication Protocol

- **stdin/stdout**: Simple JSON-based communication
- **DStream Envelope Format**: Processes standardized data envelopes
- **SDK Integration**: Uses `StdioProviderHost` from DStream .NET SDK

## Implementation

### Code Structure (Complete Provider)

```csharp
using System.Text.Json;
using Katasec.DStream.Abstractions;
using Katasec.DStream.SDK.Core;

// Simple top-level program entry point - uses SDK infrastructure
await StdioProviderHost.RunOutputProviderAsync<ConsoleOutputProvider, ConsoleConfig>();

public class ConsoleOutputProvider : ProviderBase<ConsoleConfig>, IOutputProvider
{
    private static int _messageCount = 0;

    public async Task WriteAsync(IEnumerable<Envelope> batch, IPluginContext ctx, CancellationToken ct)
    {
        foreach (var envelope in batch)
        {
            if (ct.IsCancellationRequested) break;
            
            _messageCount++;
            await OutputFormattedEnvelopeAsync(envelope, _messageCount, Config);
        }
    }

    private async Task OutputFormattedEnvelopeAsync(Envelope envelope, int messageCount, ConsoleConfig config)
    {
        var format = config.OutputFormat?.ToLower() ?? "simple";
        
        switch (format)
        {
            case "json":
                var json = JsonSerializer.Serialize(new { envelope.Payload, envelope.Meta });
                await Console.Out.WriteLineAsync(json);
                break;
                
            default:
                await Console.Out.WriteLineAsync($"Message #{messageCount}: {envelope.Payload}");
                break;
        }
    }
}

public record ConsoleConfig
{
    public string OutputFormat { get; init; } = "simple";
}
```

### Key Components

- **`ConsoleOutputProvider`**: Main provider class inheriting from `ProviderBase<ConsoleConfig>`
- **`ConsoleConfig`**: Simple configuration record with output format option
- **`StdioProviderHost`**: SDK infrastructure handling stdin/stdout communication

## SDK Benefits

**What the SDK handles for you:**
- JSON configuration parsing and binding
- stdin/stdout communication protocol
- Process lifecycle and graceful shutdown  
- Envelope deserialization
- Error handling and logging

**What you focus on:**
- Business logic (formatting and output)
- Configuration model
- Data processing logic

This demonstrates the power of the DStream .NET SDK - a complete output provider in ~50 lines of code!
