using System;
using System.Reactive.Linq;
using Dock.Model.ReactiveUI.Navigation.Controls;
using ReactiveUI;
using System.Reactive;

namespace DockReactiveUIRoutingSample.ViewModels;

public class DocumentViewModel : RoutableDocument
{
    public ReactiveCommand<Unit, Unit>? GoDocument { get; private set; }
    public ReactiveCommand<Unit, Unit>? GoTool { get; private set; }

    public DocumentViewModel(IScreen host) : base(host)
    {
        Router.Navigate.Execute(new InnerViewModel(this, "Home"));
    }

    public void InitNavigation(IRoutableViewModel document, IRoutableViewModel tool)
    {
        GoDocument = ReactiveCommand.Create(() => { HostScreen.Router.Navigate.Execute(document).Subscribe(_ => { }); });
        GoTool = ReactiveCommand.Create(() => { HostScreen.Router.Navigate.Execute(tool).Subscribe(_ => { }); });
    }
}
