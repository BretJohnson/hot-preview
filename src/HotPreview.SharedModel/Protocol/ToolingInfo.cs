using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public record class ToolingInfo
{
    public const string CurrentProtocolVersion = "1.1";

    [JsonPropertyName("protocolVersion")] public string ProtocolVersion { get; init; } = CurrentProtocolVersion;
    [JsonPropertyName("appConnectionString")] public string? AppConnectionString { get; init; }
    [JsonPropertyName("mcpUrl")] public string? McpUrl { get; init; }
}
