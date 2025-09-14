using System.Text.Json;

namespace DStream.Providers.ConsoleOutput;

/// <summary>
/// Console Output Provider for DStream
/// Receives data from input providers and outputs to console with formatting
/// </summary>
public class Program
{
    public static async Task Main(string[] args)
    {
        var provider = new ConsoleOutputProvider();
        
        // Check for handshake mode (required by hashicorp/go-plugin)
        if (args.Length > 0 && args[0] == "handshake")
        {
            await provider.HandleHandshakeAsync();
            return;
        }
        
        // Start the provider service
        await provider.StartAsync();
    }
}

/// <summary>
/// Console output provider that receives streaming data and outputs to console
/// Implements the DStream output provider interface
/// </summary>
public class ConsoleOutputProvider
{
    private readonly JsonSerializerOptions _jsonOptions;

    public ConsoleOutputProvider()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    /// <summary>
    /// Handle the initial handshake with the Go CLI
    /// </summary>
    public async Task HandleHandshakeAsync()
    {
        var handshake = new
        {
            protocol_version = "1",
            magic_cookie_key = "DSTREAM_PLUGIN",
            magic_cookie_value = "dstream-provider-plugin"
        };

        var json = JsonSerializer.Serialize(handshake, _jsonOptions);
        await Console.Out.WriteLineAsync(json);
        await Console.Out.FlushAsync();
    }

    /// <summary>
    /// Start the provider service and begin processing input data
    /// </summary>
    public async Task StartAsync()
    {
        await Console.Error.WriteLineAsync("[Console Output Provider] Starting service...");
        
        try
        {
            // Read configuration from stdin (sent by Go CLI)
            var configJson = await Console.In.ReadLineAsync();
            if (string.IsNullOrEmpty(configJson))
            {
                await Console.Error.WriteLineAsync("[Console Output Provider] No configuration received");
                return;
            }

            await Console.Error.WriteLineAsync($"[Console Output Provider] Received config: {configJson}");
            
            // Parse configuration
            var config = JsonSerializer.Deserialize<ProviderConfig>(configJson, _jsonOptions);
            if (config == null)
            {
                await Console.Error.WriteLineAsync("[Console Output Provider] Failed to parse configuration");
                return;
            }

            await ProcessDataStreamAsync(config);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"[Console Output Provider] Error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Process incoming data stream and output to console
    /// </summary>
    private async Task ProcessDataStreamAsync(ProviderConfig config)
    {
        await Console.Error.WriteLineAsync("[Console Output Provider] Starting data processing...");
        
        var messageCount = 0;
        string? line;

        // Read data from stdin line by line
        while ((line = await Console.In.ReadLineAsync()) != null)
        {
            try
            {
                messageCount++;
                
                // Parse the incoming data envelope
                var envelope = JsonSerializer.Deserialize<DataEnvelope>(line, _jsonOptions);
                if (envelope != null)
                {
                    await OutputFormattedDataAsync(envelope, messageCount, config);
                }
                else
                {
                    await Console.Out.WriteLineAsync($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}] [Raw] {line}");
                }
            }
            catch (JsonException)
            {
                // If not valid JSON, output as raw text
                await Console.Out.WriteLineAsync($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss UTC}] [Raw] {line}");
            }
            catch (Exception ex)
            {
                await Console.Error.WriteLineAsync($"[Console Output Provider] Error processing message: {ex.Message}");
            }
        }

        await Console.Error.WriteLineAsync($"[Console Output Provider] Processed {messageCount} messages. Stream ended.");
    }

    /// <summary>
    /// Output formatted data to console based on configuration
    /// </summary>
    private async Task OutputFormattedDataAsync(DataEnvelope envelope, int messageCount, ProviderConfig config)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
        
        // Extract output format from configuration
        var outputFormat = config.OutputFormat ?? "structured";
        
        switch (outputFormat.ToLower())
        {
            case "json":
                // Output as formatted JSON
                var json = JsonSerializer.Serialize(envelope, _jsonOptions);
                await Console.Out.WriteLineAsync(json);
                break;
                
            case "compact":
                // Output in compact single-line format
                await Console.Out.WriteLineAsync($"[{timestamp}] [{envelope.Source}] {envelope.Data}");
                break;
                
            case "structured":
            default:
                // Output in structured multi-line format
                await Console.Out.WriteLineAsync($"╭─── Message #{messageCount} ───");
                await Console.Out.WriteLineAsync($"│ Timestamp: {timestamp}");
                await Console.Out.WriteLineAsync($"│ Source:    {envelope.Source}");
                await Console.Out.WriteLineAsync($"│ Type:      {envelope.Type}");
                await Console.Out.WriteLineAsync($"│ Data:      {envelope.Data}");
                
                if (envelope.Metadata != null && envelope.Metadata.Count > 0)
                {
                    await Console.Out.WriteLineAsync($"│ Metadata:");
                    foreach (var kvp in envelope.Metadata)
                    {
                        await Console.Out.WriteLineAsync($"│   {kvp.Key}: {kvp.Value}");
                    }
                }
                
                await Console.Out.WriteLineAsync($"╰─────────────────────────────");
                break;
        }
        
        await Console.Out.FlushAsync();
    }
}

/// <summary>
/// Configuration for the console output provider
/// </summary>
public class ProviderConfig
{
    public string? OutputFormat { get; set; } = "structured";
    public Dictionary<string, object>? Settings { get; set; }
}

/// <summary>
/// Data envelope structure matching the DStream data model
/// </summary>
public class DataEnvelope
{
    public string Source { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public object? Data { get; set; }
    public Dictionary<string, object>? Metadata { get; set; }
}