using System.Reactive.Linq;
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
        ProjectFileWorkspaceFactory workspaceFactory)
    {
        var dockFactory = new DockFactory(this, repository, dockNavigation, workspaceFactory);
        var dockViewModel = new DockViewModel(this, dockFactory);

        Router.Navigate.Execute(dockViewModel).Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
