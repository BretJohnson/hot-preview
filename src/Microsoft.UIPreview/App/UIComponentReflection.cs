using System;

namespace Microsoft.UIPreview.App;

public class UIComponentReflection : UIComponentBase<PreviewReflection>
{
    private readonly Type type;

    internal UIComponentReflection(Type type, string? displayName) : base(displayName)
    {
        this.type = type;
    }

    public override string Name => type.FullName;

    public Type Type => type;
}
