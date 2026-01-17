using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class FileActionsToolViewModel : RoutableTool
{
    public FileActionsToolViewModel(IScreen hostScreen, Project project, ProjectFile file)
        : base(hostScreen, "file-actions")
    {
        Router.Navigate.Execute(new FileActionsPageViewModel(this, project, file))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
