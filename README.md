# DStream Console Output Provider

A .NET console output provider for the DStream data streaming platform. This provider receives streaming data from input providers and outputs formatted data to the console.

## Overview

The Console Output Provider is designed to:
- Receive streaming data envelopes from DStream input providers
- Format and display data in multiple configurable output formats  
- Integrate seamlessly with the DStream Go CLI via HashiCorp's go-plugin protocol
- Support cross-platform deployment as a self-contained binary

## Features

### Output Formats

The provider supports three output formats:

1. **Structured** (default): Multi-line format with clear visual separation
   ```
   ╭─── Message #1 ───
   │ Timestamp: 2024-01-15 10:30:45 UTC
   │ Source:    counter-input
   │ Type:      counter
   │ Data:      {"value": 42}
   ╰─────────────────────────────
   ```

2. **Compact**: Single-line format for dense output
   ```
   [2024-01-15 10:30:45 UTC] [counter-input] {"value": 42}
   ```

3. **JSON**: Raw JSON envelope output
   ```json
   {
     "source": "counter-input",
     "type": "counter", 
     "data": {"value": 42},
     "metadata": {}
   }
   ```

### Configuration

The provider accepts configuration via JSON:

```json
{
  "outputFormat": "structured",
  "settings": {
    "enableTimestamp": true,
    "showMetadata": true
  }
}
```

## Building

### Prerequisites
- .NET SDK (latest version recommended)
- Cross-platform build support (for macOS ARM64 targeting x64)

### Build Commands

```bash
# Restore dependencies
dotnet restore

# Build debug version
dotnet build

# Build release version  
dotnet build -c Release

# Publish self-contained binary (macOS x64)
dotnet publish -c Release -r osx-x64 --self-contained

# Publish for other platforms
dotnet publish -c Release -r linux-x64 --self-contained
dotnet publish -c Release -r win-x64 --self-contained
```

### macOS ARM64 Compatibility

On Apple Silicon Macs, the provider targets `osx-x64` to ensure compatibility with gRPC native libraries and runs under Rosetta 2 translation.

## Usage

### Standalone Testing

```bash
# Test handshake
echo '{}' | ./bin/Release/*/publish/console-output-provider handshake

# Test data processing (structured format)
echo '{"outputFormat":"structured"}' | ./bin/Release/*/publish/console-output-provider

# Test with sample data
(echo '{"outputFormat":"compact"}'; echo '{"source":"test","type":"sample","data":"Hello World"}') | \
  ./bin/Release/*/publish/console-output-provider
```

### DStream Integration

The provider is designed to work with the DStream Go CLI:

```bash
# Via DStream CLI (future integration)
dstream run --input counter --output console --output-config '{"outputFormat":"json"}'
```

## Architecture

### Data Flow

1. **Handshake**: Provider responds to Go plugin handshake protocol
2. **Configuration**: Receives JSON configuration via stdin
3. **Data Processing**: Continuously reads data envelopes from stdin
4. **Output**: Formats and writes data to stdout based on configuration

### Protocol Compatibility

- **HashiCorp go-plugin**: Compatible with go-plugin gRPC transport
- **DStream Envelope Format**: Processes standardized data envelopes
- **Streaming Interface**: Implements streaming I/O patterns (`IAsyncEnumerable<T>` equivalent)

## Development

### Project Structure

```
dstream-console-output-provider/
├── Program.cs                          # Main provider implementation
├── console-output-provider.csproj      # Project configuration
├── dstream-console-output-provider.sln # Solution file
├── README.md                           # Documentation
└── .gitignore                          # Git ignore rules
```

### Key Classes

- **`ConsoleOutputProvider`**: Main provider service handling data processing
- **`ProviderConfig`**: Configuration model for output formatting options
- **`DataEnvelope`**: Data model matching DStream envelope structure

## Deployment

The provider builds as a self-contained binary for easy deployment:

- **Single File**: All dependencies bundled in one executable
- **No Runtime Dependencies**: Includes .NET runtime
- **Cross-Platform**: Supports Windows, macOS, and Linux

## Integration Points

### DStream Ecosystem

- **Go CLI Orchestration**: Launched and managed by DStream Go CLI
- **Input Provider Compatibility**: Receives data from any DStream input provider
- **Provider Chaining**: Can be combined with transformation providers
- **Container Deployment**: Ready for OCI container packaging

### Future Enhancements

- Color-coded output based on data types
- Filtering and search capabilities  
- Export to file formats (CSV, JSON Lines)
- Real-time statistics and monitoring
- Custom formatting templates

## Contributing

This provider follows the DStream architecture principles:

1. **Streaming-First**: Designed for continuous data processing
2. **Plugin Architecture**: HashiCorp go-plugin compatibility
3. **Independent Deployment**: Self-contained binary distribution
4. **Configuration-Driven**: JSON-based configuration system