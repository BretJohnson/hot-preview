using System;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.PreviewFramework.App;

public static class PreviewAppServiceCreator
{
    public static IUIPreviewAppService Create()
    {
        string previewAppServiceAttributeName = typeof(PreviewAppServiceAttribute).FullName;

        // We need to execute MetadataUpdateHandlers in a well-defined order. For v1, the strategy that is used is to topologically
        // sort assemblies so that handlers in a dependency are executed before the dependent (e.g. the reflection cache action
        // in System.Private.CoreLib is executed before System.Text.Json clears it's own cache.)
        // This would ensure that caches and updates more lower in the application stack are up to date
        // before ones higher in the stack are recomputed.
        List<Assembly> sortedAssemblies = TopologicalSort(AppDomain.CurrentDomain.GetAssemblies());
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (CustomAttributeData attr in assembly.GetCustomAttributesData())
            {
                if (attr.AttributeType.FullName != previewAppServiceAttributeName)
                {
                    continue;
                }

                IList<CustomAttributeTypedArgument> contructorArguments = attr.ConstructorArguments;
                if (contructorArguments.Count != 1 ||
                    contructorArguments[0].Value is not Type appServiceType)
                {
                    throw new InvalidOperationException($"{previewAppServiceAttributeName} assembly attribute in assembly {assembly.FullName} doesn't provide a single Type parameter");
                }

                object appService = Activator.CreateInstance(appServiceType);
                if (appService is not IUIPreviewAppService previewAppService)
                {
                    throw new InvalidOperationException($"{appServiceType.FullName} doesn't implement {typeof(IUIPreviewAppService).FullName}");
                }

                return previewAppService;
            }
        }

        throw new InvalidOperationException($"No {previewAppServiceAttributeName} assembly attribute found in loaded assemblies. Is the platform specific PreviewFramework assembly included in the app?");
    }

    internal static List<Assembly> TopologicalSort(Assembly[] assemblies)
    {
        var sortedAssemblies = new List<Assembly>(assemblies.Length);

        var visited = new HashSet<string>(StringComparer.Ordinal);

        foreach (Assembly assembly in assemblies)
        {
            Visit(assemblies, assembly, sortedAssemblies, visited);
        }

        static void Visit(Assembly[] assemblies, Assembly assembly, List<Assembly> sortedAssemblies, HashSet<string> visited)
        {
            string assemblyIdentifier = assembly.GetName().Name;
            if (!visited.Add(assemblyIdentifier))
            {
                return;
            }

            foreach (AssemblyName dependencyName in assembly.GetReferencedAssemblies())
            {
                Assembly dependency = Array.Find(assemblies, a => a.GetName().Name == dependencyName.Name);
                if (dependency is not null)
                {
                    Visit(assemblies, dependency, sortedAssemblies, visited);
                }
            }

            sortedAssemblies.Add(assembly);
        }

        return sortedAssemblies;
    }
}
