using System.Collections.ObjectModel;
using DockReactiveUICanonicalSample.Models;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class FileActionsPageViewModel : ReactiveObject, IRoutableViewModel
{
    public FileActionsPageViewModel(IScreen hostScreen, Project project, ProjectFile file)
    {
        HostScreen = hostScreen;
        Project = project;
        File = file;

        Actions = new ObservableCollection<string>
        {
            "Open with Preview",
            "Rename File",
            "Duplicate",
            "Reveal in Explorer",
            "Copy Path",
            "Mark as Favorite"
        };
    }

    public string UrlPathSegment { get; } = "file-actions";

    public IScreen HostScreen { get; }

    public Project Project { get; }

    public ProjectFile File { get; }

    public ObservableCollection<string> Actions { get; }
}
