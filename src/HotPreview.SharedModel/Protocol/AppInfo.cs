using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public record AppInfo(
    [property: JsonPropertyName("components")] UIComponentInfo[] Components,
    [property: JsonPropertyName("categories")] UIComponentCategoryInfo[] Categories,
    [property: JsonPropertyName("commands")] CommandInfo[] Commands
);
