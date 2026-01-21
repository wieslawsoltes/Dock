using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectListPageViewModel : ReactiveObject, IRoutableViewModel, IReloadable, IActivatableViewModel
{
    private readonly IProjectRepository _repository;
    private readonly IDockNavigationService _dockNavigation;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;
    private readonly IDockDispatcher _dispatcher;
    private bool _hasLoaded;
    private bool _isLoading;

    public ProjectListPageViewModel(
        IScreen hostScreen,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        IDockDispatcher dispatcher)
    {
        HostScreen = hostScreen;
        _repository = repository;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;
        _overlayServicesProvider = overlayServicesProvider;
        _dispatcher = dispatcher;

        Projects = new ObservableCollection<ProjectListItemViewModel>();

        this.WhenActivated(disposables =>
        {
            if (_hasLoaded || _isLoading)
            {
                return;
            }

            var cts = new CancellationTokenSource();
            disposables.Add(Disposable.Create(() =>
            {
                cts.Cancel();
                cts.Dispose();
            }));
            _ = LoadProjectsAsync(cts.Token);
        });
    }

    public string UrlPathSegment { get; } = "project-list";

    public ViewModelActivator Activator { get; } = new();

    public IScreen HostScreen { get; }

    public ObservableCollection<ProjectListItemViewModel> Projects { get; }

    public Task ReloadAsync()
    {
        _hasLoaded = false;
        return LoadProjectsAsync(CancellationToken.None);
    }

    private async Task LoadProjectsAsync(CancellationToken cancellationToken)
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        try
        {
            var busyService = _overlayServicesProvider.GetServices(HostScreen).Busy;

            await busyService.RunAsync("Loading projects...", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var projects = await _repository.GetProjectsAsync().ConfigureAwait(false);
                var total = projects.Count;
                var index = 0;

                await _dispatcher.InvokeAsync(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Projects.Clear();
                    }
                });

                foreach (var project in projects)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    index++;
                    busyService.UpdateMessage($"Loading projects... {index}/{total}");

                    await _dispatcher.InvokeAsync(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Projects.Add(new ProjectListItemViewModel(
                                project,
                                OpenProjectFiles,
                                OpenProjectFilesTab,
                                OpenProjectFilesFloating));
                        }
                    });

                    await Task.Delay(120, cancellationToken).ConfigureAwait(false);
                }
            }).ConfigureAwait(false);

            _hasLoaded = true;
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OpenProjectFiles(Project project)
    {
        HostScreen.Router.Navigate.Execute(new ProjectFilesPageViewModel(
                HostScreen,
                project,
                _repository,
                _dockNavigation,
                _workspaceFactory,
                _overlayServicesProvider,
                _dispatcher))
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
