using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Navigation.Services;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using Dock.Model.ReactiveUI.Services;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFilePageViewModel : ReactiveObject, IRoutableViewModel, IReloadable, IActivatableViewModel
{
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IBusyServiceProvider _busyServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;
    private readonly IDockDispatcher _dispatcher;
    private ObservableAsPropertyHelper<bool>? _canGoBack;
    private IRootDock? _workspaceLayout;
    private bool _hasLoaded;
    private bool _isLoading;

    public ProjectFilePageViewModel(
        IScreen hostScreen,
        Project project,
        ProjectFile file,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider,
        IDockDispatcher dispatcher)
    {
        HostScreen = hostScreen;
        Project = project;
        File = file;
        _workspaceFactory = workspaceFactory;
        _busyServiceProvider = busyServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;
        _dispatcher = dispatcher;

        var canGoBack = this.WhenAnyValue(x => x.CanGoBack);
        GoBack = ReactiveCommand.CreateFromTask(async () =>
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Returning to files...", async () =>
            {
                await Task.Delay(150).ConfigureAwait(false);
                await _dispatcher.InvokeAsync(() =>
                {
                    DockNavigationHelpers.TryNavigateBack(HostScreen);
                });
            }).ConfigureAwait(false);
        }, canGoBack);

        CloseFile = ReactiveCommand.CreateFromTask(CloseFileAsync);

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
            _ = LoadWorkspaceAsync(cts.Token);
        });
    }

    public string UrlPathSegment { get; } = "project-file";

    public ViewModelActivator Activator { get; } = new();

    public IScreen HostScreen { get; }

    public Project Project { get; }

    public ProjectFile File { get; }

    public bool CanGoBack => _canGoBack?.Value ?? false;

    public IRootDock? WorkspaceLayout
    {
        get => _workspaceLayout;
        private set => this.RaiseAndSetIfChanged(ref _workspaceLayout, value);
    }

    public ReactiveCommand<Unit, Unit> GoBack { get; }

    public ReactiveCommand<Unit, Unit> CloseFile { get; }

    public async Task ReloadAsync()
    {
        _hasLoaded = false;
        await _dispatcher.InvokeAsync(() => WorkspaceLayout = null);
        await LoadWorkspaceAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private async Task LoadWorkspaceAsync(CancellationToken cancellationToken)
    {
        if (_isLoading)
        {
            return;
        }

        _isLoading = true;

        try
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Loading document view...", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var workspace = await _workspaceFactory
                    .CreateWorkspaceAsync(HostScreen, Project, File, cancellationToken)
                    .ConfigureAwait(false);

                await _dispatcher.InvokeAsync(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        WorkspaceLayout = workspace.Layout;
                    }
                });
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

    private async Task CloseFileAsync()
    {
        var confirmation = _confirmationServiceProvider.GetConfirmationService(HostScreen);
        var approved = await confirmation.ConfirmAsync(
            "Close File",
            $"Close {File.Name}?",
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
}
