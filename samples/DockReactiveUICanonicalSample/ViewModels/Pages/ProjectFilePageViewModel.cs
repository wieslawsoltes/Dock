using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dock.Model.Controls;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFilePageViewModel : ReactiveObject, IRoutableViewModel, IReloadable, IActivatableViewModel
{
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IBusyServiceProvider _busyServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;
    private ObservableAsPropertyHelper<bool>? _canGoBack;
    private IRootDock? _workspaceLayout;

    public ProjectFilePageViewModel(
        IScreen hostScreen,
        Project project,
        ProjectFile file,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
    {
        HostScreen = hostScreen;
        Project = project;
        File = file;
        _workspaceFactory = workspaceFactory;
        _busyServiceProvider = busyServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;

        var canGoBack = this.WhenAnyValue(x => x.CanGoBack);
        GoBack = ReactiveCommand.CreateFromTask(async () =>
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Returning to files...", async () =>
            {
                await Task.Delay(150).ConfigureAwait(false);
                await MainThreadDispatcher.InvokeAsync(() =>
                {
                    HostScreen.Router.NavigateBack.Execute()
                        .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
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
        await MainThreadDispatcher.InvokeAsync(() => WorkspaceLayout = null);
        await LoadWorkspaceAsync(CancellationToken.None).ConfigureAwait(false);
    }

    private async Task LoadWorkspaceAsync(CancellationToken cancellationToken)
    {
        try
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Loading document view...", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var workspace = await _workspaceFactory
                    .CreateWorkspaceAsync(HostScreen, Project, File, cancellationToken)
                    .ConfigureAwait(false);

                await MainThreadDispatcher.InvokeAsync(() =>
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        WorkspaceLayout = workspace.Layout;
                    }
                });
            }).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
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
            HostScreen.Router.NavigateBack.Execute()
                .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
            return;
        }

        if (HostScreen is IDockable dockable)
        {
            CloseDockable(dockable);
        }
    }

    private static void CloseDockable(IDockable dockable)
    {
        var factory = FindFactory(dockable);
        if (factory is null)
        {
            return;
        }

        factory.CloseDockable(dockable);
    }

    private static IFactory? FindFactory(IDockable dockable)
    {
        IDockable? current = dockable;
        while (current is not null)
        {
            if (current is IDock dock && dock.Factory is { } factory)
            {
                return factory;
            }

            current = current.Owner;
        }

        return null;
    }
}
