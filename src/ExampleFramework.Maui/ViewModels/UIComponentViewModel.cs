using System.Windows.Input;
using Microsoft.Maui.Controls;
using ExampleFramework.App;
using ExampleFramework.Maui.Utilities;

namespace ExampleFramework.Maui.ViewModels;

public class UIComponentViewModel : PreviewsItemViewModel
{
    public UIComponentViewModel(UIComponentReflection uiComponent)
    {
        UIComponent = uiComponent;

        TapCommand = new Command(
            execute: () =>
            {
                if (UIComponent.HasSinglePreview)
                {
                    ExampleReflection preview = UIComponent.Previews[0];
                    PreviewsViewModel.Instance.NavigateToPreview(UIComponent, preview);
                }
            }
        );
    }

    public UIComponentReflection UIComponent { get; }

    public ICommand TapCommand { get; }

    public string DisplayName => UIComponent.DisplayName;

    public string Icon => IconUtilities.GetIcon(UIComponent.Kind == UIComponentKind.Control ? "control" : "page");
}
