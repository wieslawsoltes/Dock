using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Documents;

public class ProjectFilesDocumentViewModel : RoutableDocumentBase
{
    public ProjectFilesDocumentViewModel(
        IScreen host,
        Project project,
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        IDockDispatcher dispatcher)
        : base(host, overlayServicesProvider, $"projects/{project.Id}/files")
    {
        Project = project;
        CanClose = true;

        Router.Navigate.Execute(new ProjectFilesPageViewModel(
                this,
                project,
                repository,
                dockNavigation,
                workspaceFactory,
                overlayServicesProvider,
                dispatcher))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public Project Project { get; }
}
