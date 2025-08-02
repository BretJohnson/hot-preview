using System.Windows.Input;
using HotPreview.App.Maui.Utilities;
using HotPreview.SharedModel;
using HotPreview.SharedModel.App;
using Microsoft.Maui.Controls;

namespace HotPreview.App.Maui.ViewModels;

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
