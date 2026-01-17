using System.Collections.Specialized;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFilePageViewModel : ReactiveObject, IRoutableViewModel, IReloadable
{
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IBusyServiceProvider _busyServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;
    private bool _canGoBack;
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

        ObserveNavigation();
        Dispatcher.UIThread.Post(() => _ = LoadWorkspaceAsync());

        var canGoBack = this.WhenAnyValue(x => x.CanGoBack);
        GoBack = ReactiveCommand.CreateFromTask(async () =>
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Returning to files...", async () =>
            {
                await Task.Delay(150).ConfigureAwait(false);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    HostScreen.Router.NavigateBack.Execute()
                        .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
                });
            }).ConfigureAwait(false);
        }, canGoBack);

        CloseFile = ReactiveCommand.CreateFromTask(CloseFileAsync);
    }

    public string UrlPathSegment { get; } = "project-file";

    public IScreen HostScreen { get; }

    public Project Project { get; }

    public ProjectFile File { get; }

    public bool CanGoBack
    {
        get => _canGoBack;
        private set => this.RaiseAndSetIfChanged(ref _canGoBack, value);
    }

    public IRootDock? WorkspaceLayout
    {
        get => _workspaceLayout;
        private set => this.RaiseAndSetIfChanged(ref _workspaceLayout, value);
    }

    public ReactiveCommand<Unit, Unit> GoBack { get; }

    public ReactiveCommand<Unit, Unit> CloseFile { get; }

    public async Task ReloadAsync()
    {
        await Dispatcher.UIThread.InvokeAsync(() => WorkspaceLayout = null);
        await LoadWorkspaceAsync().ConfigureAwait(false);
    }

    private void ObserveNavigation()
    {
        if (HostScreen.Router.NavigationStack is INotifyCollectionChanged notify)
        {
            notify.CollectionChanged += OnNavigationStackChanged;
        }

        UpdateCanGoBack();
    }

    private void OnNavigationStackChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        UpdateCanGoBack();
    }

    private void UpdateCanGoBack()
    {
        CanGoBack = HostScreen.Router.NavigationStack.Count > 1;
    }

    private async Task LoadWorkspaceAsync()
    {
        var busyService = _busyServiceProvider.GetBusyService(HostScreen);

        await busyService.RunAsync("Loading document view...", async () =>
        {
            var workspace = await _workspaceFactory
                .CreateWorkspaceAsync(HostScreen, Project, File)
                .ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() => WorkspaceLayout = workspace.Layout);
        }).ConfigureAwait(false);
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

        dockable.CanClose = true;
        factory.CloseDockable(dockable);

        if (dockable.Owner is IDock owner
            && owner.VisibleDockables?.Contains(dockable) == true)
        {
            factory.RemoveDockable(dockable, true);
        }
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
