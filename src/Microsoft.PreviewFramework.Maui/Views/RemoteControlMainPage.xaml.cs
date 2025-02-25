using Microsoft.PreviewFramework.App;

namespace Microsoft.PreviewFramework.Maui.Views;

public partial class RemoteControlMainPage : ContentPage
{
	public RemoteControlMainPage()
	{
        this.InitializeComponent();
    }

    public async Task SetPreviewAsync(UIPreviewReflection preview)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            object? previewUI = preview.Create();

            if (previewUI is ContentPage contentPage)
            {
                this.PreviewWrapper.BackgroundColor = contentPage.BackgroundColor;
                this.PreviewWrapper.Content = contentPage.Content;
                this.PreviewWrapper.BindingContext = contentPage.BindingContext;
            }
            else if (previewUI is View view)
            {
                this.PreviewWrapper.Content = view;
            }
            else
            {
                this.PreviewWrapper.Content = null;
            }
        });
    }

#if LATER
    public async Task<ImageSnapshot> GetPreviewSnapshotAsync(UIPreviewReflection preview)
    {
        return await MainThread.InvokeOnMainThreadAsync<ImageSnapshot>(async () =>
        {
            object? previewUI = preview.Create();

            if (previewUI is ContentPage contentPage)
            {
                this.PreviewWrapper.BackgroundColor = contentPage.BackgroundColor;
                this.PreviewWrapper.Content = contentPage.Content;
                this.PreviewWrapper.BindingContext = contentPage.BindingContext;

                return await this.GetPreviewWrapperSnapshotAsync();

            }
            else if (previewUI is View view)
            {
                this.PreviewWrapper.Content = view;
                return await this.GetPreviewWrapperSnapshotAsync();
            }
            else if (previewUI is null)
            {
                throw new InvalidOperationException($"Preview {preview.Name} returned null");
            }
            else
            {
                throw new InvalidOperationException($"Preview type {previewUI.GetType().FullName} returned from {preview.Name} is not supported");
            }
        });
    }

    private async Task<ImageSnapshot> GetPreviewWrapperSnapshotAsync()
    {
        byte[]? data = await VisualDiagnostics.CaptureAsPngAsync(this.PreviewWrapper);
        if (data == null)
        {
            throw new InvalidOperationException("VisualDiagnostics.CaptureAsPngAsync failed, returning null");
        }

        return new ImageSnapshot(data, ImageSnapshotFormat.PNG);
    }
#endif
}
