using Dock.Model.ReactiveUI.Navigation.Controls;
using Dock.Model.ReactiveUI.Services;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ToolPanelViewModel : RoutableToolBase
{
    private readonly IHostOverlayServicesProvider _overlayServicesProvider;

    public ToolPanelViewModel(
        IScreen hostScreen,
        string toolId,
        string title,
        string description,
        IHostOverlayServicesProvider overlayServicesProvider)
        : base(hostScreen, overlayServicesProvider, "tool-panel")
    {
        ToolId = toolId;
        Title = title;
        Description = description;
        _overlayServicesProvider = overlayServicesProvider;

        Router.Navigate.Execute(new ToolPanelPageViewModel(
                this,
                toolId,
                title,
                description,
                _overlayServicesProvider))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public string ToolId { get; }

    public string Description { get; }
}
