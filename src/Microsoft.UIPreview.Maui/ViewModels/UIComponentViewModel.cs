using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui.ViewModels;

public class UIComponentViewModel : PreviewsItemViewModel
{
    public UIComponentReflection UIComponent { get; }

    public ICommand TapCommand { get; }

    public UIComponentViewModel(UIComponentReflection uiComponent)
    {
        UIComponent = uiComponent;

        TapCommand = new Command(
            execute: () =>
            {
                if (UIComponent.HasSinglePreview)
                {
                    PreviewReflection preview = UIComponent.Previews[0];
                    PreviewsViewModel.Instance.NavigateToPreview(preview);
                }
            }
        );
    }

    public string DisplayName => UIComponent.DisplayName;
}
