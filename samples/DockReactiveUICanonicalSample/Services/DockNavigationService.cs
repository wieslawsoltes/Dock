using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class DockNavigationService : Dock.Model.ReactiveUI.Navigation.Services.DockNavigationService, IDockNavigationService
{
    private readonly IProjectRepository _repository;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;
    private readonly IDockDispatcher _dispatcher;

    public DockNavigationService(
        IProjectRepository repository,
        ProjectFileWorkspaceFactory workspaceFactory,
        IHostOverlayServicesProvider overlayServicesProvider,
        IDockDispatcher dispatcher)
    {
        _repository = repository;
        _workspaceFactory = workspaceFactory;
        _overlayServicesProvider = overlayServicesProvider;
        _dispatcher = dispatcher;
    }

    public void OpenProjectFiles(IScreen hostScreen, Project project, bool floatWindow)
    {
        var document = new ProjectFilesDocumentViewModel(
            hostScreen,
            project,
            _repository,
            this,
            _workspaceFactory,
            _overlayServicesProvider,
            _dispatcher)
        {
            Id = $"ProjectFiles-{project.Id}",
            Title = $"{project.Name} Files",
            CanClose = true
        };

        OpenDocument(hostScreen, document, floatWindow);
    }

    public void OpenProjectFile(IScreen hostScreen, Project project, ProjectFile file, bool floatWindow)
    {
        var document = new ProjectFileDocumentViewModel(
            hostScreen,
            project,
            file,
            _workspaceFactory,
            _overlayServicesProvider,
            _dispatcher)
        {
            Id = $"ProjectFile-{project.Id}-{file.Id}",
            Title = file.Name,
            CanClose = true
        };

        OpenDocument(hostScreen, document, floatWindow);
    }
}
