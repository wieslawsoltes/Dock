using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.Services;
using DockReactiveUICanonicalSample.ViewModels.Pages;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Documents;

public class ProjectFileDocumentViewModel : RoutableDocument
{
    public ProjectFileDocumentViewModel(
        IScreen host,
        Project project,
        ProjectFile file,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
        : base(host, $"projects/{project.Id}/files/{file.Id}")
    {
        Project = project;
        File = file;

        Router.Navigate.Execute(new ProjectFilePageViewModel(
                this,
                project,
                file,
                workspaceFactory,
                busyServiceProvider,
                confirmationServiceProvider))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public Project Project { get; }

    public ProjectFile File { get; }
}
