using System.Windows.Input;
using Microsoft.Maui.Controls;
using PreviewFramework.App.Maui.Utilities;
using PreviewFramework.SharedModel;
using PreviewFramework.SharedModel.App;

namespace PreviewFramework.App.Maui.ViewModels;

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
                    PreviewReflection preview = UIComponent.Previews[0];
                    PreviewsViewModel.Instance.NavigateToPreview(UIComponent, preview);
                }
            }
        );
    }

    public UIComponentReflection UIComponent { get; }

    public ICommand TapCommand { get; }

    public string DisplayName => UIComponent.DisplayName;

    public string PathIcon => UIComponent.PathIcon;
}
