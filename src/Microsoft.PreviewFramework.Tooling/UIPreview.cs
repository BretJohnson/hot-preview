namespace Microsoft.PreviewFramework.Tooling;

public abstract class UIPreview : UIPreviewBase
{
    private readonly string name;

    public UIPreview(string name, string? displayName) : base(displayName)
    {
        this.name = name;
    }

    public override string Name => this.name;
}
