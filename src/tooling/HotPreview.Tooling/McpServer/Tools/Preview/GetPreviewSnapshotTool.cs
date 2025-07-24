using System.ComponentModel;
using HotPreview.Tooling;
using ModelContextProtocol.Server;

namespace HotPreview.Tooling.McpServer.Tools.Preview;

[McpServerToolType]
public class GetPreviewSnapshotTool
{
    private readonly AppsManager _appsManager;

    public GetPreviewSnapshotTool(AppsManager appsManager)
    {
        _appsManager = appsManager;
    }

    /// <summary>
    /// Gets a preview snapshot by preview name and saves it to a temporary file.
    /// Searches across all connected apps and UI components to find the preview.
    /// </summary>
    /// <param name="previewName">
    /// The name of the preview to capture. The preview will be searched across all
    /// UI components in all connected apps.
    /// </param>
    /// <returns>
    /// A string containing a file URI to the saved image file, or null if the preview
    /// is not found or snapshot operation fails. The format is "Image saved to: file:///path/to/image.png".
    /// </returns>
    /// <remarks>
    /// This method searches through all connected apps and their UI components to find
    /// a preview with the specified name. If multiple previews with the same name exist
    /// across different components, the first one found will be used. The image is saved
    /// to a temporary file that can be accessed by MCP clients.
    /// </remarks>
    [McpServerTool(Name = "get_preview_snapshot")]
    [Description("Gets a preview snapshot by preview name and saves it to a temporary file, returning a file URI.")]
    public async Task<string?> GetPreviewSnapshotAsync(string previewName)
    {
        try
        {
            if (string.IsNullOrEmpty(previewName))
            {
                throw new ArgumentException("Preview name cannot be null or empty.", nameof(previewName));
            }

            // Search through all apps and their connections
            foreach (AppManager appManager in _appsManager.Apps)
            {
                if (appManager.PreviewsManager is null)
                    continue;

                // Search through all UI components in this app
                foreach (UIComponentTooling uiComponent in appManager.PreviewsManager.UIComponents)
                {
                    // Look for the preview in this component
                    PreviewTooling? preview = uiComponent.Previews.FirstOrDefault(p => p.Name == previewName);
                    if (preview is not null)
                    {
                        // Found the preview, now get the snapshot from an active connection
                        foreach (AppConnectionManager connection in appManager.AppConnections)
                        {
                            if (connection.PreviewsManager?.HasPreview(uiComponent.Name, preview.Name) ?? false)
                            {
                                var previewPair = new UIComponentPreviewPairTooling(uiComponent, preview);
                                ImageSnapshot snapshot = await connection.GetPreviewSnapshotAsync(previewPair);

                                // Create a temporary file with appropriate extension
                                string tempFilePath = Path.GetTempFileName();
                                string tempFileWithExtension = Path.ChangeExtension(tempFilePath, snapshot.Format.GetFileExtension());

                                // Delete the original temp file and use the one with correct extension
                                File.Delete(tempFilePath);

                                // Save the image to the temp file
                                File.WriteAllBytes(tempFileWithExtension, snapshot.Data);

                                // Convert to file URI format
                                string fileUri = new Uri(tempFileWithExtension).ToString();
                                return $"Image saved to: {fileUri}";
                            }
                        }
                    }
                }
            }

            // Preview not found
            return null;
        }
        catch
        {
            return null;
        }
    }
}
