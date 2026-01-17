using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public RoutingState Router { get; } = new RoutingState();

    public MainWindowViewModel(
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceFactory busyServiceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider,
        IGlobalBusyService globalBusyService,
        IDialogServiceFactory dialogServiceFactory,
        IGlobalDialogService globalDialogService,
        IConfirmationServiceFactory confirmationServiceFactory,
        IGlobalConfirmationService globalConfirmationService)
    {
        var dockFactory = new DockFactory(
            this,
            repository,
            dockNavigation,
            workspaceFactory,
            busyServiceFactory,
            busyServiceProvider,
            confirmationServiceProvider,
            globalBusyService,
            dialogServiceFactory,
            globalDialogService,
            confirmationServiceFactory,
            globalConfirmationService);
        var dockViewModel = new DockViewModel(this, dockFactory);

        Router.Navigate.Execute(dockViewModel).Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
