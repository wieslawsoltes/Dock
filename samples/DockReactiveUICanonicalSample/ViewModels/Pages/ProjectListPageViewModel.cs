using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectListPageViewModel : ReactiveObject, IRoutableViewModel
{
    private readonly IDockNavigationService _dockNavigation;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;

    public ProjectListPageViewModel(
        IScreen hostScreen,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory)
    {
        HostScreen = hostScreen;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;

        var projects = repository.GetProjects()
            .Select(project => new ProjectListItemViewModel(
                project,
                OpenProjectFiles,
                OpenProjectFilesTab,
                OpenProjectFilesFloating));
        Projects = new ObservableCollection<ProjectListItemViewModel>(projects);
    }

    public string UrlPathSegment { get; } = "project-list";

    public IScreen HostScreen { get; }

    public ObservableCollection<ProjectListItemViewModel> Projects { get; }

    private void OpenProjectFiles(Project project)
    {
        HostScreen.Router.Navigate.Execute(new ProjectFilesPageViewModel(HostScreen, project, _dockNavigation, _workspaceFactory))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    private void OpenProjectFilesFloating(Project project)
    {
        _dockNavigation.OpenProjectFiles(project, true);
    }

    private void OpenProjectFilesTab(Project project)
    {
        _dockNavigation.OpenProjectFiles(project, false);
    }
}
