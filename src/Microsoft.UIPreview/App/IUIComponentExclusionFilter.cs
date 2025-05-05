using System;
using System.Reflection;

namespace Microsoft.UIPreview.App;

public interface IUIComponentExclusionFilter
{
    public bool ExcludeAssembly(Assembly assembly);

    public bool ExcludeType(Type type);
}
