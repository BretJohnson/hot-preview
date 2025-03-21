using System.Windows.Input;
using Microsoft.Maui.Controls;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui.ViewModels
{
    public class PreviewViewModel : PreviewsItemViewModel
    {
        public PreviewReflection Preview { get; }
        public ICommand TapCommand { get; }

        public PreviewViewModel(PreviewReflection preview)
        {
            Preview = preview;

            TapCommand = new Command(
                execute: () =>
                {
                    PreviewsViewModel.Instance.NavigateToPreview(Preview);
                }
            );
        }

        public string DisplayName => Preview.DisplayName;
    }
}
