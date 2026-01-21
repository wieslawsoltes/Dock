using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUICanonicalSample.Models;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class FileActionsToolViewModel : RoutableToolBase
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;

    public FileActionsToolViewModel(
        IScreen hostScreen,
        Project project,
        ProjectFile file,
        IHostOverlayServicesProvider overlayServicesProvider)
        : base(hostScreen, overlayServicesProvider, "file-actions")
    {
        _overlayServicesProvider = overlayServicesProvider;

        Router.Navigate.Execute(new FileActionsPageViewModel(
                this,
                project,
                file,
                _overlayServicesProvider))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
