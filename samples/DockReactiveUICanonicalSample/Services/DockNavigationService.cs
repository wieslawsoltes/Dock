using Dock.Model.Controls;
using Dock.Model.Core;
using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public sealed class DockNavigationService : IDockNavigationService
{
    private DockFactory? _factory;
    private IScreen? _host;
    private readonly IProjectRepository _repository;
    private readonly ProjectFileWorkspaceFactory _workspaceFactory;
    private readonly IBusyServiceProvider _busyServiceProvider;
    private readonly IConfirmationServiceProvider _confirmationServiceProvider;

    public DockNavigationService(
        IProjectRepository repository,
        ProjectFileWorkspaceFactory workspaceFactory,
        IBusyServiceProvider busyServiceProvider,
        IConfirmationServiceProvider confirmationServiceProvider)
    {
        _repository = repository;
        _workspaceFactory = workspaceFactory;
        _busyServiceProvider = busyServiceProvider;
        _confirmationServiceProvider = confirmationServiceProvider;
    }

    public void AttachFactory(DockFactory factory, IScreen host)
    {
        _factory = factory;
        _host = host;
    }

    public void OpenProjectFiles(IScreen hostScreen, Project project, bool floatWindow)
    {
        if (!TryGetDocumentContext(hostScreen, out var factory, out var documentDock))
        {
            return;
        }

        var document = new ProjectFilesDocumentViewModel(
            hostScreen,
            project,
            _repository,
            this,
            _workspaceFactory,
            _busyServiceProvider,
            _confirmationServiceProvider)
        {
            Id = $"ProjectFiles-{project.Id}",
            Title = $"{project.Name} Files",
            CanClose = true
        };

        factory.OpenDocument(document, documentDock, floatWindow);
    }

    public void OpenProjectFile(IScreen hostScreen, Project project, ProjectFile file, bool floatWindow)
    {
        if (!TryGetDocumentContext(hostScreen, out var factory, out var documentDock))
        {
            return;
        }

        var document = new ProjectFileDocumentViewModel(
            hostScreen,
            project,
            file,
            _workspaceFactory,
            _busyServiceProvider,
            _confirmationServiceProvider)
        {
            Id = $"ProjectFile-{project.Id}-{file.Id}",
            Title = file.Name,
            CanClose = true
        };

        factory.OpenDocument(document, documentDock, floatWindow);
    }

    private bool TryGetDocumentContext(
        IScreen hostScreen,
        out DockFactory factory,
        out IDocumentDock documentDock)
    {
        factory = _factory!;
        documentDock = null!;

        if (TryResolveDocumentDock(hostScreen, out factory, out documentDock))
        {
            return true;
        }

        return TryResolveDocumentDock(_host, out factory, out documentDock);
    }

    private static bool TryResolveDocumentDock(
        IScreen? hostScreen,
        out DockFactory factory,
        out IDocumentDock documentDock)
    {
        factory = null!;
        documentDock = null!;

        if (hostScreen is not IDockable dockable)
        {
            return false;
        }

        var current = dockable;
        DockFactory? resolvedFactory = null;
        IDocumentDock? resolvedDock = null;

        while (current is not null)
        {
            if (resolvedFactory is null && current.Factory is DockFactory dockFactory)
            {
                resolvedFactory = dockFactory;
            }

            if (resolvedDock is null && current is IDocumentDock dock)
            {
                resolvedDock = dock;
            }

            if (resolvedFactory is not null && resolvedDock is not null)
            {
                break;
            }

            current = current.Owner;
        }

        if (resolvedDock is null && resolvedFactory is not null)
        {
            resolvedDock = resolvedFactory.FindRoot(dockable) switch
            {
                IRootDock { DefaultDockable: IDocumentDock defaultDock } => defaultDock,
                _ => null
            };
        }

        if (resolvedFactory is null || resolvedDock is null)
        {
            return false;
        }

        factory = resolvedFactory;
        documentDock = resolvedDock;
        return true;
    }

}
