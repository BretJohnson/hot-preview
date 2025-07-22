using HotPreview.Tooling;

namespace HotPreview.DevToolsApp.ViewModels.NavTree;

public class CommandViewModel : NavTreeItemViewModel
{
    private readonly MainPageViewModel _mainPageViewModel;
    private DateTime _lastClickTime = DateTime.MinValue;
    private const int DOUBLE_CLICK_INTERVAL_MS = 500;

    public CommandViewModel(MainPageViewModel mainPageViewModel, PreviewCommandTooling command)
    {
        _mainPageViewModel = mainPageViewModel;
        Command = command;
    }

    public PreviewCommandTooling Command { get; }

    public override string DisplayName => Command.DisplayName;
    public override string PathIcon => "M8 2L16 12H12V22L4 12H8V2Z"; // Lightning bolt icon for commands

    public override void OnItemInvoked()
    {
        DateTime currentTime = DateTime.Now;

        // Check if this is a double-click (within the double-click interval)
        if (currentTime - _lastClickTime <= TimeSpan.FromMilliseconds(DOUBLE_CLICK_INTERVAL_MS))
        {
            // This is a double-click, execute the command
            _ = ExecuteCommandAsync(); // Fire and forget
        }
        else
        {
            // This is a single click, just update status bar
            _mainPageViewModel.UpdateStatusMessage("Double click to execute command");
        }

        _lastClickTime = currentTime;
    }

    public async Task ExecuteCommandAsync()
    {
        AppManager? appManager = _mainPageViewModel.CurrentApp;
        if (appManager is not null)
        {
            try
            {
                _mainPageViewModel.UpdateStatusMessage($"Executing command {Command.DisplayName}...");
                await appManager.InvokeCommandAsync(Command);
                _mainPageViewModel.UpdateStatusMessage($"Command {Command.DisplayName} executed successfully");
            }
            catch (Exception ex)
            {
                _mainPageViewModel.UpdateStatusMessage($"Command execution failed: {ex.Message}");
            }
        }
        else
        {
            _mainPageViewModel.UpdateStatusMessage("No app connected");
        }
    }
}
