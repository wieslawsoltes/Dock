using System;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
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

        this.WhenActivated(disposables =>
        {
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

    public Task ReloadAsync() => LoadProjectsAsync(CancellationToken.None);

    private async Task LoadProjectsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Loading projects...", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var projects = await _repository.GetProjectsAsync().ConfigureAwait(false);
                var total = projects.Count;
                var index = 0;

                await Dispatcher.UIThread.InvokeAsync(() =>
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

                    await Dispatcher.UIThread.InvokeAsync(() =>
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
        }
        catch (OperationCanceledException)
        {
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
