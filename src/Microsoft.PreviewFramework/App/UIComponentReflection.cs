using System;

namespace Microsoft.PreviewFramework.App;

public class UIComponentReflection : UIComponentBase<UIPreviewReflection>
{
    private readonly Type type;

    internal UIComponentReflection(Type type, string? displayName) : base(displayName)
    {
        this.type = type;
    }

    public override string Name => type.FullName;

    public Type Type => type;
}
