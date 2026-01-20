using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using Dock.Model.ReactiveUI.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Documents;

public class ProjectListDocumentViewModel : RoutableDocumentBase
{
    public ProjectListDocumentViewModel(
        IScreen host,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        IDockDispatcher dispatcher)
        : base(host, overlayServicesProvider, "projects")
    {
        CanClose = false;

        Router.Navigate.Execute(new ProjectListPageViewModel(
                this,
                repository,
                dockNavigation,
                workspaceFactory,
                overlayServicesProvider,
                dispatcher))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
