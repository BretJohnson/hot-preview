using System;
using System.Reflection;

namespace HotPreview.SharedModel;

public interface IUIComponentExclusionFilter
{
    public bool ExcludeAssembly(Assembly assembly);

    public bool ExcludeType(Type type);
}
