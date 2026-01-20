using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dock.Model.Core;
using DockNavigationHelpers = Dock.Model.ReactiveUI.Navigation.Services.DockNavigationHelpers;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using Dock.Model.ReactiveUI.Services;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;
using SampleDockNavigationService = DockReactiveUICanonicalSample.Services.IDockNavigationService;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFilesPageViewModel : ReactiveObject, IRoutableViewModel, IReloadable, IActivatableViewModel
{
    private readonly IProjectRepository _repository;
    private readonly SampleDockNavigationService _dockNavigation;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;
    private readonly IDockDispatcher _dispatcher;
    private ObservableAsPropertyHelper<bool>? _canGoBack;
    private bool _hasLoaded;
    private bool _isLoading;

    public ProjectFilesPageViewModel(
        IScreen hostScreen,
        Project project,
        IProjectRepository repository,
        SampleDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        IDockDispatcher dispatcher)
    {
        HostScreen = hostScreen;
        Project = project;
        _repository = repository;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;
        _overlayServicesProvider = overlayServicesProvider;
        _dispatcher = dispatcher;

        Files = new ObservableCollection<ProjectFileItemViewModel>();

        var canGoBack = this.WhenAnyValue(x => x.CanGoBack);
        GoBack = ReactiveCommand.CreateFromTask(async () =>
        {
            var busyService = _overlayServicesProvider.GetServices(HostScreen).Busy;

            await busyService.RunAsync("Returning to projects...", async () =>
            {
                await Task.Delay(150).ConfigureAwait(false);
                await _dispatcher.InvokeAsync(() =>
                {
                    DockNavigationHelpers.TryNavigateBack(HostScreen);
                });
            }).ConfigureAwait(false);
        }, canGoBack);

        CloseFiles = ReactiveCommand.CreateFromTask(CloseFilesAsync);

        this.WhenActivated(disposables =>
        {
            _canGoBack = this.WhenAnyValue(x => x.HostScreen.Router.NavigationStack.Count)
                .Select(count => count > 1)
                .ToProperty(this, x => x.CanGoBack);
            disposables.Add(_canGoBack);

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
            _ = LoadFilesAsync(cts.Token);
        });
    }

    public string UrlPathSegment { get; } = "project-files";

    public ViewModelActivator Activator { get; } = new();

    public IScreen HostScreen { get; }

    public Project Project { get; }

    public ObservableCollection<ProjectFileItemViewModel> Files { get; }

    public bool CanGoBack => _canGoBack?.Value ?? false;

    public Task ReloadAsync()
    {
        _hasLoaded = false;
        return LoadFilesAsync(CancellationToken.None);
    }

    private void OpenFile(ProjectFile file)
    {
        HostScreen.Router.Navigate.Execute(new ProjectFilePageViewModel(
                HostScreen,
                Project,
                file,
                _workspaceFactory,
                _overlayServicesProvider,
                _dispatcher))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    private void OpenFileFloating(ProjectFile file)
    {
        _dockNavigation.OpenProjectFile(HostScreen, Project, file, true);
    }

    private void OpenFileTab(ProjectFile file)
    {
        _dockNavigation.OpenProjectFile(HostScreen, Project, file, false);
    }

    public ReactiveCommand<Unit, Unit> GoBack { get; }

    public ReactiveCommand<Unit, Unit> CloseFiles { get; }

    private async Task CloseFilesAsync()
    {
        var confirmation = _overlayServicesProvider.GetServices(HostScreen).Confirmations;
        var approved = await confirmation.ConfirmAsync(
            "Close Files",
            $"Close {Project.Name} files?",
            confirmText: "Close",
            cancelText: "Keep");

        if (!approved)
        {
            return;
        }

        if (HostScreen.Router.NavigationStack.Count > 1)
        {
            DockNavigationHelpers.TryNavigateBack(HostScreen);
            return;
        }

        if (HostScreen is IDockable dockable)
        {
            DockNavigationHelpers.TryCloseDockable(dockable);
        }
    }

    private async Task LoadFilesAsync(CancellationToken cancellationToken)
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        try
        {
            var busyService = _overlayServicesProvider.GetServices(HostScreen).Busy;

            await busyService.RunAsync("Loading project files...", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var files = await _repository.GetProjectFilesAsync(Project.Id).ConfigureAwait(false);
                var total = files.Count;
                var index = 0;

                await _dispatcher.InvokeAsync(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Files.Clear();
                    }
                });

                foreach (var file in files)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    index++;
                    busyService.UpdateMessage($"Loading project files... {index}/{total}");

                    await _dispatcher.InvokeAsync(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Files.Add(new ProjectFileItemViewModel(file, OpenFile, OpenFileTab, OpenFileFloating));
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
}
