using System.Text.Json;
using Katasec.DStream.Abstractions;
using Katasec.DStream.SDK.Core;

// Simple top-level program entry point - uses SDK infrastructure
await StdioProviderHost.RunOutputProviderAsync<ConsoleOutputProvider, ConsoleConfig>();

/// <summary>
/// Simple Console Output Provider Example
/// Shows the minimal code needed to create a DStream output provider
/// </summary>
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
                // Output as simple JSON using framework defaults
                var json = JsonSerializer.Serialize(new { envelope.Payload, envelope.Meta });
                await Console.Out.WriteLineAsync(json);
                break;
                
            default:
                // Simple format: just show the data
                await Console.Out.WriteLineAsync($"Message #{messageCount}: {envelope.Payload}");
                break;
        }
    }
}

/// <summary>
/// Simple configuration - just the output format
/// </summary>
public record ConsoleConfig
{
    public string OutputFormat { get; init; } = "simple";
}
