using System;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using Dock.Model.ReactiveUI.Services;
using Dock.Model.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new RoutingState();

    public MainWindowViewModel(
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        Func<IHostOverlayServices> hostServicesFactory,
        IWindowLifecycleService windowLifecycleService,
        IDockDispatcher dispatcher)
    {
        var dockFactory = new DockFactory(
            this,
            repository,
            dockNavigation,
            workspaceFactory,
            overlayServicesProvider,
            hostServicesFactory,
            windowLifecycleService,
            dispatcher);
        var dockViewModel = new DockViewModel(this, dockFactory);

        Router.Navigate.Execute(dockViewModel).Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
