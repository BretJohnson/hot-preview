namespace Microsoft.UIPreview.Tooling;

public abstract class Preview : PreviewBase
{
    private readonly string name;

    public Preview(string name, string? displayName) : base(displayName)
    {
        this.name = name;
    }

    public override string Name => this.name;
}
