using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IDockNavigationService
{
    void AttachFactory(DockFactory factory, IScreen host);

    void OpenProjectFiles(IScreen hostScreen, Project project, bool floatWindow);

    void OpenProjectFile(IScreen hostScreen, Project project, ProjectFile file, bool floatWindow);
}
