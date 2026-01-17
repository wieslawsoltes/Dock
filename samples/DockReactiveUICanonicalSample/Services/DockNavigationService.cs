using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.ViewModels;
using DockReactiveUICanonicalSample.ViewModels.Documents;
using DockReactiveUICanonicalSample.ViewModels.Workspace;
using ReactiveUI;
using Splat;

namespace DockReactiveUICanonicalSample.Services;

public sealed class DockNavigationService : IDockNavigationService
{
    private DockFactory? _factory;
    private IScreen? _host;

    public void AttachFactory(DockFactory factory, IScreen host)
    {
        _factory = factory;
        _host = host;
    }

    public void OpenProjectFiles(Project project, bool floatWindow)
    {
        if (_factory is null || _host is null)
        {
            return;
        }

        var workspaceFactory = Locator.Current.GetService<ProjectFileWorkspaceFactory>();
        if (workspaceFactory is null)
        {
            return;
        }

        var document = new ProjectFilesDocumentViewModel(_host, project, this, workspaceFactory)
        {
            Id = $"ProjectFiles-{project.Id}",
            Title = $"{project.Name} Files"
        };

        _factory.OpenDocument(document, floatWindow);
    }

    public void OpenProjectFile(Project project, ProjectFile file, bool floatWindow)
    {
        if (_factory is null || _host is null)
        {
            return;
        }

        var workspaceFactory = Locator.Current.GetService<ProjectFileWorkspaceFactory>();
        if (workspaceFactory is null)
        {
            return;
        }

        var document = new ProjectFileDocumentViewModel(_host, project, file, workspaceFactory)
        {
            Id = $"ProjectFile-{project.Id}-{file.Id}",
            Title = file.Name
        };

        _factory.OpenDocument(document, floatWindow);
    }
}
