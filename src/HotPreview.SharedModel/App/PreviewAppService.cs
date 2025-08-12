using System;
using System.Linq;
using System.Threading.Tasks;
using HotPreview.SharedModel.Protocol;

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

    protected PreviewCommandReflection GetCommand(string commandName)
    {
        return PreviewApplication.GetPreviewsManager().GetCommand(commandName) ??
            throw new ArgumentException($"Command {commandName} not found");
    }

    protected PreviewCommandReflection? GetCommandIfExists(string commandName)
    {
        return PreviewApplication.GetPreviewsManager().GetCommand(commandName);
    }

    public Task<UIComponentInfo?> GetComponentAsync(string componentName)
    {
        UIComponentReflection? component = GetUIComponentIfExists(componentName);
        return Task.FromResult(component?.GetUIComponentInfo());
    }

    public Task<UIComponentInfo[]> GetComponentsAsync()
    {
        PreviewsManagerReflection previewsManager = PreviewApplication.GetPreviewsManager();

        UIComponentInfo[] uiComponentInfos = previewsManager.UIComponents
            .Select(component => component.GetUIComponentInfo())
            .ToArray();

        return Task.FromResult(uiComponentInfos);
    }

    public async Task NavigateToPreviewAsync(string componentName, string previewName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentPreviewPair(componentName, previewName);
        await PreviewApplication.GetPreviewNavigator().NavigateToPreviewAsync(uiComponentPreviewPair.UIComponent, uiComponentPreviewPair.Preview).ConfigureAwait(false);
    }

    public async Task<byte[]> GetPreviewSnapshotAsync(string componentName, string previewName)
    {
        UIComponentPreviewPairReflection uiComponentPreviewPair = GetUIComponentPreviewPair(componentName, previewName);
        return await PreviewApplication.GetPreviewNavigator().GetPreviewSnapshotAsync(uiComponentPreviewPair.UIComponent, uiComponentPreviewPair.Preview).ConfigureAwait(false);
    }

    public Task<PreviewCommandInfo[]> GetCommandsAsync()
    {
        PreviewsManagerReflection previewsManager = PreviewApplication.GetPreviewsManager();

        PreviewCommandInfo[] commandInfos = previewsManager.Commands
            .Select(command => command.GetPreviewCommandInfo())
            .ToArray();

        return Task.FromResult(commandInfos);
    }

    public Task<PreviewCommandInfo?> GetCommandAsync(string commandName)
    {
        PreviewCommandReflection? command = GetCommandIfExists(commandName);
        return Task.FromResult(command?.GetPreviewCommandInfo());
    }

    public Task InvokeCommandAsync(string commandName)
    {
        PreviewCommandReflection command = GetCommand(commandName);

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
