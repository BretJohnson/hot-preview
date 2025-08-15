using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public record UIComponentInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("uiComponentKind")] string UIComponentKind,
    [property: JsonPropertyName("displayName")] string? DisplayName,
    [property: JsonPropertyName("previews")] PreviewInfo[] Previews
);
