using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public class AppInfo(UIComponentInfo[] components, UIComponentCategoryInfo[] categories, CommandInfo[] commands)
{
    [JsonPropertyName("components")] public UIComponentInfo[] Components { get; } = components;
    [JsonPropertyName("categories")] public UIComponentCategoryInfo[] Categories { get; } = categories;
    [JsonPropertyName("commands")] public CommandInfo[] Commands { get; } = commands;
}
