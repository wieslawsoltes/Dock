using System;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUIRoutingSample.ViewModels.Inner;
using ReactiveUI;
using System.Reactive;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolViewModel : RoutableTool
{
    public ReactiveCommand<Unit, Unit>? GoDocument { get; private set; }
    public ReactiveCommand<Unit, Unit>? GoTool { get; private set; }

    public ToolViewModel(IScreen host) : base(host)
    {
        Router.Navigate.Execute(new InnerViewModel(this, "Tool Home"));
    }

    public void InitNavigation(IRoutableViewModel document, IRoutableViewModel tool)
    {
        GoDocument = ReactiveCommand.Create(() => { HostScreen.Router.Navigate.Execute(document).Subscribe(_ => { }); });
        GoTool = ReactiveCommand.Create(() => { HostScreen.Router.Navigate.Execute(tool).Subscribe(_ => { }); });
    }
}
