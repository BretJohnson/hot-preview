using System;

namespace Microsoft.UIPreview;

/// <summary>
/// An attribute that specifies this is the app service that should handle preview requests.
/// The specified type must implement IPreviewAppService and have a parameterless contructor.
/// </summary>
[AttributeUsage(AttributeTargets.Assembly)]
public sealed class PreviewAppServiceAttribute : Attribute
{
    public Type AppServiceType { get; }

    public PreviewAppServiceAttribute(Type appServiceType)
    {
        AppServiceType = appServiceType;
    }
}
