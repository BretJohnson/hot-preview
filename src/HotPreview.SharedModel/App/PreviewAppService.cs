using System;
using System.Linq;
using System.Threading.Tasks;
using HotPreview.SharedModel.Protocol;
using StreamJsonRpc;

namespace HotPreview.SharedModel.App;

public abstract class PreviewAppService(PreviewApplication previewApplication) : IPreviewAppService
{
    public PreviewApplication PreviewApplication { get; } = previewApplication;

    protected UIComponentReflection GetUIComponent(string componentName)
    {
        return PreviewApplication.GetPreviewsManager().GetUIComponent(componentName) ??
            throw new UIComponentNotFoundException($"UIComponent {componentName} not found");
    }

    protected UIComponentReflection? GetUIComponentIfExists(string componentName)
    {
        return PreviewApplication.GetPreviewsManager().GetUIComponent(componentName);
    }

    protected CommandReflection GetCommand(string commandName)
    {
        return PreviewApplication.GetPreviewsManager().GetCommand(commandName) ??
            throw new ArgumentException($"Command {commandName} not found");
    }

    protected CommandReflection? GetCommandIfExists(string commandName)
    {
        return PreviewApplication.GetPreviewsManager().GetCommand(commandName);
    }

    [JsonRpcMethod("components/get")]
    public Task<UIComponentInfo?> GetComponentAsync(string componentName)
    {
        UIComponentReflection? component = GetUIComponentIfExists(componentName);
        return Task.FromResult(component?.GetUIComponentInfo());
    }

    [JsonRpcMethod("appinfo/get")]
    public Task<AppInfo> GetAppInfoAsync()
    {
        PreviewsManagerReflection previewsManager = PreviewApplication.GetPreviewsManager();

        UIComponentInfo[] uiComponentInfos = previewsManager.UIComponents
            .Select(component => component.GetUIComponentInfo())
            .ToArray();

        UIComponentCategoryInfo[] categoryInfos = previewsManager.Categories
            .Select(category => category.GetUIComponentCategoryInfo())
            .ToArray();

        CommandInfo[] commandInfos = previewsManager.Commands
            .Select(command => command.GetCommandInfo())
            .ToArray();

        return Task.FromResult(new AppInfo(uiComponentInfos, categoryInfos, commandInfos));
    }

    [JsonRpcMethod("previews/navigate")]
    public async Task NavigateToPreviewAsync(string componentName, string previewName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentPreviewPair(componentName, previewName);
        await PreviewApplication.GetPreviewNavigator().NavigateToPreviewAsync(uiComponentPreviewPair.UIComponent, uiComponentPreviewPair.Preview).ConfigureAwait(false);
    }

    [JsonRpcMethod("previews/snapshot")]
    public async Task<byte[]> GetPreviewSnapshotAsync(string componentName, string previewName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentPreviewPair(componentName, previewName);
        return await PreviewApplication.GetPreviewNavigator().GetPreviewSnapshotAsync(uiComponentPreviewPair.UIComponent, uiComponentPreviewPair.Preview).ConfigureAwait(false);
    }


    [JsonRpcMethod("commands/get")]
    public Task<CommandInfo?> GetCommandAsync(string commandName)
    {
        CommandReflection? command = GetCommandIfExists(commandName);
        return Task.FromResult(command?.GetCommandInfo());
    }

    [JsonRpcMethod("commands/invoke")]
    public Task InvokeCommandAsync(string commandName)
    {
        CommandReflection command = GetCommand(commandName);

        try
        {
            command.Execute();
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to execute command {commandName}: {ex.Message}", ex);
        }
    }

    protected UIComponentPreviewPairReflection GetUIComponentPreviewPair(string componentName, string previewName)
    {
        UIComponentReflection uiComponent = GetUIComponent(componentName);
        PreviewReflection preview = uiComponent.GetPreview(previewName) ?? throw new PreviewNotFoundException($"Preview {previewName} not found for UIComponent {componentName}");
        return new UIComponentPreviewPairReflection(uiComponent, preview);
    }
}
