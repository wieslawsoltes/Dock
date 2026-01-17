using System.Reactive;
using Dock.Model.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFilePageViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly ProjectFileWorkspace _workspace;

    public ProjectFilePageViewModel(
        IScreen hostScreen,
        Project project,
        ProjectFile file,
        ProjectFileWorkspaceFactory workspaceFactory)
    {
        HostScreen = hostScreen;
        Project = project;
        File = file;
        _workspace = workspaceFactory.CreateWorkspace(hostScreen, project, file);

        GoBack = ReactiveCommand.Create(() =>
        {
            HostScreen.Router.NavigateBack.Execute()
                .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
        });
    }

    public string UrlPathSegment { get; } = "project-file";

    public IScreen HostScreen { get; }

    public Project Project { get; }

    public ProjectFile File { get; }

    public IRootDock WorkspaceLayout => _workspace.Layout;

    public ReactiveCommand<Unit, Unit> GoBack { get; }
}
