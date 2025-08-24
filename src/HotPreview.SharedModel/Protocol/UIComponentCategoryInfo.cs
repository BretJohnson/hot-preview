using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public record UIComponentCategoryInfo(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("uiComponentNames")] string[] UIComponentNames
);
