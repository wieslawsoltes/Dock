using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class ToolPanelViewModel : RoutableTool
{
    public ToolPanelViewModel(IScreen hostScreen, string toolId, string title, string description)
        : base(hostScreen, "tool-panel")
    {
        ToolId = toolId;
        Title = title;
        Description = description;

        Router.Navigate.Execute(new ToolPanelPageViewModel(this, title, description))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }

    public string ToolId { get; }

    public string Description { get; }
}
