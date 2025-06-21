using PreviewFramework.Model;

namespace PreviewFramework.Tooling;

public class UIComponent : UIComponentBase<Preview>
{
    private readonly string _typeName;

    internal UIComponent(UIComponentKind kind, string typeName, string? displayNameOverride = null) : base(kind, displayNameOverride)
    {
        _typeName = typeName;
    }

    public override string Name => _typeName;
}
