namespace HotPreview.Tooling;

public static class ImageSnapshotFormatExtensions
{
    public static string GetFileExtension(this ImageSnapshotFormat format) =>
        format switch
        {
            ImageSnapshotFormat.PNG => ".png",
            _ => throw new InvalidOperationException($"Invalid ImageFormat value: {format}"),
        };

    public static ImageSnapshotFormat GetImageFormat(string filePath)
    {
        string fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

        if (fileExtension == ".png")
        {
            return ImageSnapshotFormat.PNG;
        }
        else
        {
            throw new InvalidOperationException($"Unsupported file type: {filePath}");
        }
    }
}
