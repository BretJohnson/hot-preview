namespace PreviewFramework.Model.Protocol;

public record UIComponentInfo(
    string Name,
    string? DisplayName,
    PreviewInfo[] Previews);
