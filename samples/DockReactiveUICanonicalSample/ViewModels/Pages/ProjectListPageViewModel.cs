using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Threading;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectListPageViewModel : ReactiveObject, IRoutableViewModel, IReloadable
{
    private readonly IProjectRepository _repository;
    private readonly IDockNavigationService _dockNavigation;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IBusyServiceProvider _busyServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;

    public ProjectListPageViewModel(
        IScreen hostScreen,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
    {
        HostScreen = hostScreen;
        _repository = repository;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;
        _busyServiceProvider = busyServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;

        Projects = new ObservableCollection<ProjectListItemViewModel>();

        Dispatcher.UIThread.Post(() => _ = LoadProjectsAsync());
    }

    public string UrlPathSegment { get; } = "project-list";

    public IScreen HostScreen { get; }

    public ObservableCollection<ProjectListItemViewModel> Projects { get; }

    public Task ReloadAsync() => LoadProjectsAsync();

    private async Task LoadProjectsAsync()
    {
        var busyService = _busyServiceProvider.GetBusyService(HostScreen);

        await busyService.RunAsync("Loading projects...", async () =>
        {
            var projects = await _repository.GetProjectsAsync().ConfigureAwait(false);
            var total = projects.Count;
            var index = 0;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Projects.Clear();
            });

            foreach (var project in projects)
            {
                index++;
                busyService.UpdateMessage($"Loading projects... {index}/{total}");

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Projects.Add(new ProjectListItemViewModel(
                        project,
                        OpenProjectFiles,
                        OpenProjectFilesTab,
                        OpenProjectFilesFloating));
                });

                await Task.Delay(120).ConfigureAwait(false);
            }
        }).ConfigureAwait(false);
    }

    private void OpenProjectFiles(Project project)
    {
        HostScreen.Router.Navigate.Execute(new ProjectFilesPageViewModel(
                HostScreen,
                project,
                _repository,
                _dockNavigation,
                _workspaceFactory,
                _busyServiceProvider,
                _confirmationServiceProvider))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    private void OpenProjectFilesFloating(Project project)
    {
        _dockNavigation.OpenProjectFiles(HostScreen, project, true);
    }

    private void OpenProjectFilesTab(Project project)
    {
        _dockNavigation.OpenProjectFiles(HostScreen, project, false);
    }
}
