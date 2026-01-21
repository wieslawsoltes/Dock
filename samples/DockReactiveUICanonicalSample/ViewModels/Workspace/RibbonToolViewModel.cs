using System.Collections.Generic;
using Dock.Model.ReactiveUI.Navigation.Controls;
using ReactiveUI;

namespace DockReactiveUICanonicalSample.ViewModels.Workspace;

public class RibbonToolViewModel : RoutableToolBase
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
