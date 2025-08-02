using System;

namespace HotPreview;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class UIComponentCategoryAttribute(string name, params Type[] uiComponents) : Attribute
{
    public string Name { get; } = name;

    public Type[] UIComponentTypes { get; } = uiComponents;
}
