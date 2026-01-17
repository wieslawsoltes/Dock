using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Pages;

public class ProjectFilesPageViewModel : ReactiveObject, IRoutableViewModel, IReloadable, IActivatableViewModel
{
    private readonly IProjectRepository _repository;
    private readonly IDockNavigationService _dockNavigation;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IBusyServiceProvider _busyServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;
    private ObservableAsPropertyHelper<bool>? _canGoBack;

    public ProjectFilesPageViewModel(
        IScreen hostScreen,
        Project project,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
    {
        HostScreen = hostScreen;
        Project = project;
        _repository = repository;
        _dockNavigation = dockNavigation;
        _workspaceFactory = workspaceFactory;
        _busyServiceProvider = busyServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;

        Files = new ObservableCollection<ProjectFileItemViewModel>();

        var canGoBack = this.WhenAnyValue(x => x.CanGoBack);
        GoBack = ReactiveCommand.CreateFromTask(async () =>
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Returning to projects...", async () =>
            {
                await Task.Delay(150).ConfigureAwait(false);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    HostScreen.Router.NavigateBack.Execute()
                        .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
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

    public Task ReloadAsync() => LoadFilesAsync(CancellationToken.None);

    private void OpenFile(ProjectFile file)
    {
        HostScreen.Router.Navigate.Execute(new ProjectFilePageViewModel(
                HostScreen,
                Project,
                file,
                _workspaceFactory,
                _busyServiceProvider,
                _confirmationServiceProvider))
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
        var confirmation = _confirmationServiceProvider.GetConfirmationService(HostScreen);
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

    private async Task LoadFilesAsync(CancellationToken cancellationToken)
    {
        try
        {
            var busyService = _busyServiceProvider.GetBusyService(HostScreen);

            await busyService.RunAsync("Loading project files...", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var files = await _repository.GetProjectFilesAsync(Project.Id).ConfigureAwait(false);
                var total = files.Count;
                var index = 0;

                await Dispatcher.UIThread.InvokeAsync(() =>
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

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        if (!cancellationToken.IsCancellationRequested)
                        {
                            Files.Add(new ProjectFileItemViewModel(file, OpenFile, OpenFileTab, OpenFileFloating));
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
}
