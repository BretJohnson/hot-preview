namespace HotPreview.SharedModel.Protocol;

public record UIComponentInfo(
    string Name,
    string UIComponentKindInfo,
    string? DisplayName,
    PreviewInfo[] Previews);
