using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Documents;

public class ProjectFilesDocumentViewModel : RoutableDocument
{
    public ProjectFilesDocumentViewModel(
        IScreen host,
        Project project,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory)
        : base(host, $"projects/{project.Id}/files")
    {
        Project = project;

        Router.Navigate.Execute(new ProjectFilesPageViewModel(this, project, dockNavigation, workspaceFactory))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public Project Project { get; }
}
