using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public record AppInfo(
    [property: JsonPropertyName("components")] UIComponentInfo[] Components,
    [property: JsonPropertyName("commands")] CommandInfo[] Commands
);