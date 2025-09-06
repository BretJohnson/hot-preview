using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public class ToolingInfo(string protocolVersion, string? appConnectionString, string? mcpUrl)
{
    public const string CurrentProtocolVersion = "1.1";

    [JsonPropertyName("protocolVersion")] public string ProtocolVersion { get; } = protocolVersion;
    [JsonPropertyName("appConnectionString")] public string? AppConnectionString { get; } = appConnectionString;
    [JsonPropertyName("mcpUrl")] public string? McpUrl { get; } = mcpUrl;
}
