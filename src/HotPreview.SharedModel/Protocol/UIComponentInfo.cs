using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public class UIComponentInfo(string name, string uiComponentKind, string? displayName, PreviewInfo[] previews)
{
    [JsonPropertyName("name")] public string Name { get; } = name;
    [JsonPropertyName("uiComponentKind")] public string UIComponentKind { get; } = uiComponentKind;
    [JsonPropertyName("displayName")] public string? DisplayName { get; } = displayName;
    [JsonPropertyName("previews")] public PreviewInfo[] Previews { get; } = previews;
}
