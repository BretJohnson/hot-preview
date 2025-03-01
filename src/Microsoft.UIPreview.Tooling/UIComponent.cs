namespace Microsoft.UIPreview.Tooling;

public class UIComponent : UIComponentBase<Preview>
{
    private readonly string typeName;

    internal UIComponent(string typeName, string? displayName = null) : base(displayName)
    {
        this.typeName = typeName;
    }

    public override string Name => this.typeName;
}
