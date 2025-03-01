using System;

namespace Microsoft.UIPreview.App;

public abstract class PreviewReflection : PreviewBase
{
    private readonly Type? uiComponentType;
    //private Dictionary<string, ImageSnapshot?>? _snapshotsByEnvironment;

    public PreviewReflection(PreviewAttribute previewAttribute) : base(previewAttribute.DisplayName)
    {
        uiComponentType = previewAttribute.UIComponentType;
    }

    public PreviewReflection(Type uiComponentType) : base(null)
    {
        this.uiComponentType = uiComponentType;
    }

    /// <summary>
    /// Create an instance of the preview. Normally this returns an instance of a UI framework control/page, suitable
    /// for display.
    /// </summary>
    /// <returns>instantiated preview</returns>
    public abstract object Create();

    public Type UIComponentType
    {
        get
        {
            if (uiComponentType != null)
            {
                return uiComponentType;
            }

            Type? defaultUIComponentType = DefaultUIComponentType;
            if (defaultUIComponentType == null)
                throw new InvalidOperationException($"No DefaultUIComponentType specified for example: {Name}");
            else return defaultUIComponentType;
        }
    }

    /// <summary>
    /// Default component type (when there is one), e.g. based on the method return type. If there's no default
    /// type, this will be null.
    /// </summary>
    public abstract Type? DefaultUIComponentType { get; }
}
