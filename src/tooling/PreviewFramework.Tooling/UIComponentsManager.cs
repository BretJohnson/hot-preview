using Microsoft.CodeAnalysis;
using PreviewFramework.Model;

namespace PreviewFramework.Tooling;

public class UIComponentsManager : UIComponentsManagerBase<UIComponent, Preview>
{
    public UIComponentBaseTypes PageUIComponentBaseTypes => _pageUIComponentBaseTypes;
    public UIComponentBaseTypes ControlUIComponentBaseTypes => _controlUIComponentBaseTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="UIComponentsManager"/> class, processing metadata from the
    /// provided compilation and its references to gather UI component information. If/when the compilation
    /// changes, create a new instance of this class to read the new compilation.
    /// </summary>
    /// <param name="compilation">Roslyn compilation</param>
    /// <param name="includeApparentUIComponentsWithNoPreviews">Determines whether to include types that COULD be UIComponents,
    /// because they derive from a UI component class, but don't actually define any previews nor can a preview be constructed
    /// automatically. Can be set by tooling that flags these for the user, to direct them to add a preview.</param>
    public UIComponentsManager(Compilation compilation, bool includeApparentUIComponentsWithNoPreviews = false)
    {
        GetUIComponentsFromRoslyn.GetUIComponentsFromCompilation(compilation, includeApparentUIComponentsWithNoPreviews, this);
    }

    public UIComponent GetOrAddComponent(string name)
    {
        UIComponent? component = GetUIComponent(name);
        if (component is null)
        {
            component = new UIComponent(UIComponentKind.Page, name);
            AddUIComponent(component);
        }

        return component;
    }

    public void AddPreview(string uiComponentName, Preview preview)
    {
        UIComponent component = GetOrAddComponent(uiComponentName);
        component.AddPreview(preview);
    }
}
