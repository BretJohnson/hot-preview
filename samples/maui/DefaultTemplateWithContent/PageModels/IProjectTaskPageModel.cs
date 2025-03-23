using CommunityToolkit.Mvvm.Input;
using DefaultTemplateWithContent.Models;

namespace DefaultTemplateWithContent.PageModels;
public interface IProjectTaskPageModel
{
    IAsyncRelayCommand<ProjectTask> NavigateToTaskCommand { get; }
    bool IsBusy { get; }
}