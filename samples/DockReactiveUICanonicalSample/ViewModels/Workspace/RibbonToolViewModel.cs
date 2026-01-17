using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class RibbonToolViewModel : RoutableTool
{
    public RibbonToolViewModel(
        IScreen hostScreen,
        ProjectFileWorkspace workspace,
        IEnumerable<ToolPanelViewModel> tools)
        : base(hostScreen, "ribbon-tool")
    {
        Router.Navigate.Execute(new RibbonPageViewModel(this, workspace, tools))
            .Subscribe(System.Reactive.Observer.Create<IRoutableViewModel>(_ => { }));
    }
}
