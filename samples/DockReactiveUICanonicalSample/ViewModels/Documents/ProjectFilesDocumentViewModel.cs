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
        IProjectRepository repository,
        IDockNavigationService dockNavigation,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
        : base(host, $"projects/{project.Id}/files")
    {
        Project = project;

        Router.Navigate.Execute(new ProjectFilesPageViewModel(
                this,
                project,
                repository,
                dockNavigation,
                workspaceFactory,
                busyServiceProvider,
                confirmationServiceProvider))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public Project Project { get; }
}
