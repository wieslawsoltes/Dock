using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using Dock.Model.ReactiveUI.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Documents;

public class ProjectFileDocumentViewModel : RoutableDocumentBase
{
    public ProjectFileDocumentViewModel(
        IScreen host,
        Project project,
        ProjectFile file,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        IDockDispatcher dispatcher)
        : base(host, overlayServicesProvider, $"projects/{project.Id}/files/{file.Id}")
    {
        Project = project;
        File = file;
        CanClose = true;

        Router.Navigate.Execute(new ProjectFilePageViewModel(
                this,
                project,
                file,
                workspaceFactory,
                overlayServicesProvider,
                dispatcher))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public Project Project { get; }

    public ProjectFile File { get; }
}
