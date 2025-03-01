using System.Windows.Input;
using Microsoft.UIPreview.App;

namespace Microsoft.UIPreview.Maui.ViewModels
{
    public class PreviewViewModel
    {
        public string Title => this.Preview.DisplayName;

        public PreviewReflection Preview { get; }

        public ICommand TapCommand { get; }

        public PreviewViewModel(PreviewReflection preview)
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
