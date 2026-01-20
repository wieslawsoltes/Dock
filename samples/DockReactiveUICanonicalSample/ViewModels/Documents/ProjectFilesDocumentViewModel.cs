using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using Dock.Model.ReactiveUI.Services;
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
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider,
        IDockDispatcher dispatcher)
        : base(host, $"projects/{project.Id}/files")
    {
        Project = project;
        CanClose = true;

        Router.Navigate.Execute(new ProjectFilesPageViewModel(
                this,
                project,
                repository,
                dockNavigation,
                workspaceFactory,
                busyServiceProvider,
                confirmationServiceProvider,
                dispatcher))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public Project Project { get; }
}
