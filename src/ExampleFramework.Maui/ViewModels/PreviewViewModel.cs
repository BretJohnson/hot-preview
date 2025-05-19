using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui.ViewModels
{
    public class PreviewViewModel : PreviewsItemViewModel
    {
        public UIComponentReflection UIComponent { get; }
        public PreviewReflection Preview { get; }
        public ICommand TapCommand { get; }

        public PreviewViewModel(UIComponentReflection uiComponent, PreviewReflection preview)
        {
            UIComponent = uiComponent;
            Preview = preview;

            TapCommand = new Command(
                execute: () =>
                {
                    PreviewsViewModel.Instance.NavigateToPreview(UIComponent, Preview);
                }
            );
        }

        public string DisplayName => Preview.DisplayName;
    }
}
