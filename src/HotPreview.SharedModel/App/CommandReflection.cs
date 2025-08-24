using System;
using System.Reflection;
using HotPreview.SharedModel.Protocol;

namespace HotPreview.SharedModel.App;

/// <summary>
/// Reflection-based command implementation for static methods marked with [PreviewCommand].
/// </summary>
public class CommandReflection(PreviewCommandAttribute commandAttribute, MethodInfo methodInfo) : CommandBase(commandAttribute.DisplayName)
{
    public MethodInfo MethodInfo { get; } = methodInfo;

    /// <summary>
    /// Execute the command by invoking the static method.
    /// </summary>
    public void Execute()
    {
        if (MethodInfo.GetParameters().Length != 0)
            throw new InvalidOperationException($"Commands that take parameters aren't yet supported: {Name}");

        MethodInfo.Invoke(null, null);
    }

    /// <summary>
    /// FullName is intended to be what's used by the code to identify the command. It's the command's
    /// full qualified method name.
    /// </summary>
    public override string Name => MethodInfo.DeclaringType!.FullName + "." + MethodInfo.Name;

    /// <summary>
    /// Gets the command information including name and display name.
    /// </summary>
    /// <returns>A CommandInfo record with the command details, for use in the JSON RPC protocol</returns>
    public CommandInfo GetCommandInfo()
    {
        return new CommandInfo(Name, DisplayNameOverride);
    }
}
