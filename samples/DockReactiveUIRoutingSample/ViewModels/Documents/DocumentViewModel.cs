using System;
using System.Reactive;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUIRoutingSample.ViewModels.Inner;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Documents;

public class DocumentViewModel : RoutableDocument
{
    public ReactiveCommand<Unit, IDisposable>? GoDocument { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoTool1 { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoTool2 { get; private set; }

    public DocumentViewModel(IScreen host) : base(host)
    {
        Router.Navigate.Execute(new InnerViewModel(this, "Home"));
    }

    public void InitNavigation(
        IRoutableViewModel? document,
        IRoutableViewModel? tool1,
        IRoutableViewModel? tool2)
    {
        if (document is not null)
        {
            GoDocument = ReactiveCommand.Create(() =>
                HostScreen.Router.Navigate.Execute(document).Subscribe(_ => { }));
        }

        if (tool1 is not null)
        {
            GoTool1 = ReactiveCommand.Create(() =>
                HostScreen.Router.Navigate.Execute(tool1).Subscribe(_ => { }));
        }

        if (tool2 is not null)
        {
            GoTool2 = ReactiveCommand.Create(() =>
                HostScreen.Router.Navigate.Execute(tool2).Subscribe(_ => { }));
        }
    }
}
