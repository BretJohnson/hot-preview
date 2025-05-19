namespace Microsoft.UIPreview.Tooling;

public class UIComponent : UIComponentBase<Preview>
{
    private readonly string _typeName;

    internal UIComponent(UIComponentKind kind, string typeName, string? displayName = null) : base(kind, displayName)
    {
        _typeName = typeName;
    }

    public override string Name => _typeName;
}
