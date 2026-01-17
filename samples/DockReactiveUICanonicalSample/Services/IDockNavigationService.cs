using DockReactiveUICanonicalSample.Models;
using DockReactiveUICanonicalSample.ViewModels;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.Services;

public interface IDockNavigationService
{
    void AttachFactory(DockFactory factory, IScreen host);

    void OpenProjectFiles(Project project, bool floatWindow);

    void OpenProjectFile(Project project, ProjectFile file, bool floatWindow);
}
