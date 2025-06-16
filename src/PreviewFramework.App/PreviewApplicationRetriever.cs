﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace PreviewFramework.App;

public static class PreviewApplicationRetriever
{
    /// <summary>
    /// Get the platform specific PreviewApplication, used to control the behavior of the preview, if it exists.
    ///
    /// Here's the algorithm:
    ///   1. If the app wants to support in-app example (e.g. the in app menu) or otherwise customize the example
    ///   experience, it must include the platform specified PreviewFramework assembly (e.g. PreviewFramework.Maui)
    ///   in addition to the PreviewFramework assembly. The platform assembly is discovered here, searching for any
    ///   assembly in the app with the [PreviewApplication] assembly attribute, used to specify the platform specific
    ///   PreviewApplication subclass. Note that 3rd party UI frameworks are supported here too - the platform assembly
    ///   need not be Microsoft authored, it just needs to have the [PreviewApplication] assembly attribute.
    ///
    ///   2. If app/library doesn't care about supporting in-app example UI or otherwise customizing the example experience,
    ///   it can just included the PreviewFramework assembly (this one) and still decorate example methods with
    ///   [Preview]. Normally libraries would do this, especially libraries that want to avoid platform dependencies. In this case,
    ///   null is returned - there's no PreviewApplication platform provider and VS can use fallback code in the tap (present for
    ///   some platforms but not all) to do the navigation.
    /// </summary>
    public static PreviewApplication? GetPreviewApplication()
    {
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (CustomAttributeData attr in assembly.GetCustomAttributesData())
            {
                if (attr.AttributeType.FullName != PreviewApplicationClassAttribute.TypeFullName)
                {
                    continue;
                }

                IList<CustomAttributeTypedArgument> constructorArguments = attr.ConstructorArguments;
                if (constructorArguments.Count != 1 ||
                    constructorArguments[0].Value is not Type exampleApplicationType)
                {
                    throw new InvalidOperationException($"{PreviewApplicationClassAttribute.TypeFullName} assembly attribute in assembly {assembly.FullName} doesn't provide a single Type parameter");
                }

                PropertyInfo instanceProperty = exampleApplicationType.GetProperty("Instance", BindingFlags.Static | BindingFlags.Public) ??
                    throw new InvalidOperationException($"{exampleApplicationType.FullName} doesn't have a public static property named 'Instance'");

                object exampleApplication = instanceProperty.GetValue(null) ??
                    throw new InvalidOperationException($"{exampleApplicationType.FullName}.Instance returned null");

                return (exampleApplication as PreviewApplication) ??
                    throw new InvalidOperationException($"{exampleApplicationType.FullName}.Instance isn't of type 'PreviewApplication'");
            }
        }

        return null;
    }

    public static PreviewAppService? PreviewAppService()
    {
        return GetPreviewApplication()?.GetPreviewAppService();
    }
}
