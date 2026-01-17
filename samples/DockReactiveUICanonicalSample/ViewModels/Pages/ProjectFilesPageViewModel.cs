using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFilesPageViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IDockNavigationService _dockNavigation;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;

    public ProjectFilesPageViewModel(
        IScreen hostScreen,
        Project project,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory)
    {
        HostScreen = hostScreen;
        Project = project;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;

        var files = project.Files
            .Select(file => new ProjectFileItemViewModel(file, OpenFile, OpenFileTab, OpenFileFloating));
        Files = new ObservableCollection<ProjectFileItemViewModel>(files);

        GoBack = ReactiveCommand.Create(() =>
        {
            HostScreen.Router.NavigateBack.Execute()
                .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
        });
    }

    public string UrlPathSegment { get; } = "project-files";

    public IScreen HostScreen { get; }

    public Project Project { get; }

    public ObservableCollection<ProjectFileItemViewModel> Files { get; }

    private void OpenFile(ProjectFile file)
    {
        HostScreen.Router.Navigate.Execute(new ProjectFilePageViewModel(HostScreen, Project, file, _workspaceFactory))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    private void OpenFileFloating(ProjectFile file)
    {
        _dockNavigation.OpenProjectFile(Project, file, true);
    }

    private void OpenFileTab(ProjectFile file)
    {
        _dockNavigation.OpenProjectFile(Project, file, false);
    }

    public ReactiveCommand<Unit, Unit> GoBack { get; }
}
