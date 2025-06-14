using System;
using System.Reflection;

namespace ExampleFramework.App;

public interface IUIComponentExclusionFilter
{
    public bool ExcludeAssembly(Assembly assembly);

    public bool ExcludeType(Type type);
}
