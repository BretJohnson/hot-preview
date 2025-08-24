using HotPreview.SharedModel;

namespace HotPreview.Tooling;

/// <summary>
/// A builder class for constructing PreviewsManagerTooling instances.
/// This class provides mutable operations to build up the state before creating an immutable manager.
/// Derives from PreviewsManagerBuilderBase with the appropriate tooling types.
/// </summary>
public class PreviewsManagerBuilderTooling : PreviewsManagerBuilderBase<UIComponentTooling, PreviewTooling, CommandTooling>
{
    /// <summary>
    /// Creates an immutable PreviewsManagerTooling from the builder's current state.
    /// </summary>
    /// <returns>An immutable PreviewsManagerTooling containing all the builder's data</returns>
    public virtual PreviewsManagerTooling ToImmutable()
    {
        Validate();
        return new PreviewsManagerTooling(UIComponentsByName, Categories, CommandsByName);
    }
}
