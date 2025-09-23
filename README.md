# DStream Console Output Provider

A DStream output provider that displays streaming data with customizable formatting options.

[![NuGet](https://img.shields.io/nuget/v/Katasec.DStream.SDK.Core.svg)](https://www.nuget.org/packages/Katasec.DStream.SDK.Core/)
[![OCI](https://img.shields.io/badge/OCI-ghcr.io-blue)](https://github.com/writeameer/dstream-console-output-provider/pkgs/container/dstream-console-output-provider)

## 🚀 Features

- 📺 **Multiple Output Formats**: Simple, structured, and JSON formatting
- 🎨 **Colored Output**: Syntax highlighting and visual formatting
- 📊 **Message Statistics**: Real-time processing counters and summaries
- 📦 **Cross-Platform**: Linux, macOS, Windows (x64/ARM64)
- 🐳 **OCI Distribution**: Available as semantic versioned OCI artifacts
- 🛠️ **DStream Native**: Built with DStream .NET SDK for optimal compatibility

## 📦 Quick Start

### Using OCI Artifacts (Production)

```hcl
task "console-demo" {
  type = "providers"
  
  input {
    provider_ref = "ghcr.io/writeameer/dstream-counter-input-provider:v1.0.0"
    config {
      interval = 1000
      max_count = 5
    }
  }
  
  output {
    provider_ref = "ghcr.io/writeameer/dstream-console-output-provider:v1.0.0"
    config {
      outputFormat = "structured"  # simple, structured, or json
    }
  }
}
```

### Using Local Binaries (Development)

```bash
# Build the provider
dotnet publish -c Release -r osx-arm64 --self-contained

# Test directly
echo '{"outputFormat": "simple"}' | ./bin/Release/net9.0/osx-arm64/publish/console-output-provider
```

## ⚙️ Configuration

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `outputFormat` | string | "simple" | Output format: `simple`, `structured`, or `json` |

### Output Format Examples

**Simple Format** (`"outputFormat": "simple"`):
```
Message #1: {"value":1,"timestamp":"2025-09-23T10:30:45.123Z"}
Message #2: {"value":2,"timestamp":"2025-09-23T10:30:46.124Z"}
```

**Structured Format** (`"outputFormat": "structured"`):
```
┌─── Message #1 ───────────────────────────────────┐
│ Data: {"value":1,"timestamp":"..."}              │
│ Meta: {"seq":1,"provider":"counter-input"}       │
└─────────────────────────────────────────────────┘
```

**JSON Format** (`"outputFormat": "json"`):
```json
{"messageId":1,"data":{"value":1},"metadata":{"seq":1}}
{"messageId":2,"data":{"value":2},"metadata":{"seq":2}}
```

## 📊 Data Processing

The provider accepts DStream standard envelope format:

**Input Envelope**:
```json
{
  "data": {
    "value": 42,
    "timestamp": "2025-09-23T10:30:45.123Z"
  },
  "metadata": {
    "seq": 42,
    "provider": "counter-input-provider"
  }
}
```

**Processing Features**:
- ✅ Batch processing with configurable sizes
- ✅ Real-time message counting and statistics
- ✅ Error handling and malformed message logging
- ✅ Graceful stream termination detection

## 🏗️ Development

### Prerequisites
- .NET 9.0 SDK
- PowerShell (for build scripts)
- ORAS (for OCI publishing)

### Build and Test
```bash
# Build for current platform
dotnet build

# Build for specific platform
dotnet publish -c Release -r linux-x64 --self-contained

# Test locally with sample data
echo '{"outputFormat": "structured"}' | dotnet run
```

### Testing with Pipelines
```bash
# Test complete pipeline
echo '{"interval": 1000, "max_count": 3}' | \
  ../dstream-counter-input-provider/bin/Release/net9.0/publish/counter-input-provider 2>/dev/null | \
  echo '{"outputFormat": "simple"}' | \
  dotnet run
```

### Publishing OCI Artifacts
```bash
# Build and push with semantic version
pwsh ./push.ps1 -Tag "v1.0.0"

# Auto-increment patch version
pwsh ../version.ps1 patch && pwsh ./push.ps1
```

## 🐳 Available Platforms

| Platform | Runtime ID | Binary Name |
|----------|------------|-------------|
| Linux x64 | linux-x64 | plugin.linux_amd64 |
| Linux ARM64 | linux-arm64 | plugin.linux_arm64 |
| macOS x64 | osx-x64 | plugin.darwin_amd64 |
| macOS ARM64 | osx-arm64 | plugin.darwin_arm64 |
| Windows x64 | win-x64 | plugin.windows_amd64.exe |

## 📚 Usage Examples

### Development & Debugging
```hcl
task "debug-pipeline" {
  input {
    provider_path = "./my-custom-input-provider"
    config {
      # Input config
    }
  }
  
  output {
    provider_ref = "ghcr.io/writeameer/dstream-console-output-provider:latest"
    config {
      outputFormat = "structured"  # Best for debugging
    }
  }
}
```

### Performance Monitoring
```hcl
task "perf-monitor" {
  input {
    provider_ref = "ghcr.io/writeameer/dstream-kafka-input-provider:latest"
    config {
      topic = "high-volume-topic"
    }
  }
  
  output {
    provider_ref = "ghcr.io/writeameer/dstream-console-output-provider:latest"
    config {
      outputFormat = "simple"  # Minimal overhead
    }
  }
}
```

### JSON Processing
```hcl
task "json-processor" {
  input {
    provider_ref = "ghcr.io/writeameer/dstream-api-input-provider:latest"
    config {
      endpoint = "https://api.example.com/events"
    }
  }
  
  output {
    provider_ref = "ghcr.io/writeameer/dstream-console-output-provider:latest"
    config {
      outputFormat = "json"  # Machine-readable output
    }
  }
}
```

## 🔧 Technical Details

- **Language**: C# / .NET 9.0
- **SDK**: [DStream .NET SDK](https://github.com/katasec/dstream-dotnet-sdk)
- **Communication**: JSON over stdin/stdout
- **Architecture**: Single-file executable, self-contained deployment
- **Logging**: Structured logging to stderr (DStream compatible)
- **Performance**: Optimized for high-throughput streaming scenarios

## 📄 License

MIT License - see [LICENSE](LICENSE) file for details.

---

**Part of the DStream ecosystem** 🌊
- **DStream CLI**: [katasec/dstream](https://github.com/katasec/dstream)
- **DStream .NET SDK**: [katasec/dstream-dotnet-sdk](https://github.com/katasec/dstream-dotnet-sdk)
- **Counter Input Provider**: [writeameer/dstream-counter-input-provider](https://github.com/writeameer/dstream-counter-input-provider)