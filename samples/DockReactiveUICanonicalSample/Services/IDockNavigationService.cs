using DockReactiveUICanonicalSample.Models;
using ReactiveUI.Reactive;

namespace DockReactiveUICanonicalSample.Services;

public interface IDockNavigationService : Dock.Model.ReactiveUI.Navigation.Services.IDockNavigationService
{
    void OpenProjectFiles(IScreen hostScreen, Project project, bool floatWindow);

    void OpenProjectFile(IScreen hostScreen, Project project, ProjectFile file, bool floatWindow);
}
