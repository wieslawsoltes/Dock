using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Documents;

public class ProjectListDocumentViewModel : RoutableDocument
{
    public ProjectListDocumentViewModel(
        IScreen host,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory)
        : base(host, "projects")
    {
        Router.Navigate.Execute(new ProjectListPageViewModel(this, repository, dockNavigation, workspaceFactory))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
