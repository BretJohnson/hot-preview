using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public record PreviewCommandInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("displayName")] string? DisplayName
);
