using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ToolPanelPageViewModel : ReactiveObject, IRoutableViewModel
{
    public ToolPanelPageViewModel(IScreen hostScreen, string title, string description)
    {
        HostScreen = hostScreen;
        Title = title;
        Description = description;
    }

    public string UrlPathSegment { get; } = "tool-panel";

    public IScreen HostScreen { get; }

    public string Title { get; }

    public string Description { get; }
}
