namespace HotPreview.SharedModel.Protocol;

internal record UIComponentInfo(
    string Name,
    string UIComponentKindInfo,
    string? DisplayName,
    PreviewInfo[] Previews);
