using System.Windows.Input;
using ExampleFramework.App;
using Microsoft.Maui.Controls;

namespace ExampleFramework.App.Maui.ViewModels
{
    public class ExampleViewModel : ExamplesItemViewModel
    {
        public UIComponentReflection UIComponent { get; }
        public ExampleReflection Example { get; }
        public ICommand TapCommand { get; }

        public ExampleViewModel(UIComponentReflection uiComponent, ExampleReflection example)
        {
            UIComponent = uiComponent;
            Example = example;

            TapCommand = new Command(
                execute: () =>
                {
                    ExamplesViewModel.Instance.NavigateToExample(UIComponent, Example);
                }
            );
        }

        public string DisplayName => Example.DisplayName;
    }
}
