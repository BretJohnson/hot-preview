using System.Text.Json.Serialization;

namespace HotPreview.SharedModel.Protocol;

public class UIComponentCategoryInfo(string name, string[] uiComponentNames)
{
    [JsonPropertyName("name")] public string Name { get; } = name;
    [JsonPropertyName("uiComponentNames")] public string[] UIComponentNames { get; } = uiComponentNames;
}
