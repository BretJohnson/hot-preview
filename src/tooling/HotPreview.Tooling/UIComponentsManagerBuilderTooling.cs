using HotPreview.SharedModel;

namespace HotPreview.Tooling;

/// <summary>
/// A builder class for constructing UIComponentsManagerTooling instances.
/// This class provides mutable operations to build up the state before creating an immutable manager.
/// Derives from UIComponentsManagerBuilderBase with the appropriate tooling types.
/// </summary>
public class UIComponentsManagerBuilderTooling : UIComponentsManagerBuilderBase<UIComponentTooling, PreviewTooling>
{
    /// <summary>
    /// Creates an immutable UIComponentsManagerTooling from the builder's current state.
    /// </summary>
    /// <returns>An immutable UIComponentsManagerTooling containing all the builder's data</returns>
    public virtual UIComponentsManagerTooling ToImmutable()
    {
        Validate();
        return new UIComponentsManagerTooling(UIComponentsByName, Categories);
    }
}
