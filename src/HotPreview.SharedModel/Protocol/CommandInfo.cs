using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public class CommandInfo(string name, string? displayName)
{
    [JsonPropertyName("name")] public string Name { get; } = name;
    [JsonPropertyName("displayName")] public string? DisplayName { get; } = displayName;
}
