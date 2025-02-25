using System.Windows.Input;
using Microsoft.PreviewFramework.App;

namespace Microsoft.PreviewFramework.Maui.ViewModels
{
    public class PreviewViewModel
    {
        public string Title => this.Preview.DisplayName;

        public UIPreviewReflection Preview { get; }

        public ICommand TapCommand { get; }

        public PreviewViewModel(UIPreviewReflection preview)
        {
            this.Preview = preview;

            this.TapCommand = new Command(
                execute: () =>
                {
                    _ = PreviewsViewModel.Instance.PreviewNavigatorService.NavigateToPreviewAsync(this.Preview).ConfigureAwait(false);
                }
            );
        }
    }
}
