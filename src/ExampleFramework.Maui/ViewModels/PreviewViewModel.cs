using System.Windows.Input;
using Microsoft.Maui.Controls;
using ExampleFramework.App;

namespace ExampleFramework.Maui.ViewModels
{
    public class PreviewViewModel : PreviewsItemViewModel
    {
        public UIComponentReflection UIComponent { get; }
        public ExampleReflection Example { get; }
        public ICommand TapCommand { get; }

        public PreviewViewModel(UIComponentReflection uiComponent, ExampleReflection preview)
        {
            UIComponent = uiComponent;
            Example = preview;

            TapCommand = new Command(
                execute: () =>
                {
                    PreviewsViewModel.Instance.NavigateToPreview(UIComponent, Example);
                }
            );
        }

        public string DisplayName => Example.DisplayName;
    }
}
