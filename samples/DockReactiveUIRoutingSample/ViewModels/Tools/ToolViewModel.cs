using System;
using System.Reactive;
using Dock.Model.ReactiveUI.Navigation.Controls;
using DockReactiveUIRoutingSample.ViewModels.Inner;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolViewModel : RoutableTool
{
    public ReactiveCommand<Unit, IDisposable>? GoDocument1 { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoDocument2 { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoNextTool { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoToDetails { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoToSettings { get; private set; }

    public ToolViewModel(IScreen host) : base(host)
    {
        Router.Navigate.Execute(new ToolHomeViewModel(this));
        
        GoToDetails = ReactiveCommand.Create(() =>
            Router.Navigate.Execute(new ToolDetailViewModel(this, "Tool Details", "This is a detailed view of the tool with additional information and functionality.")).Subscribe(_ => { }));
            
        GoToSettings = ReactiveCommand.Create(() =>
            Router.Navigate.Execute(new ToolSettingsViewModel(this, "Tool Settings")).Subscribe(_ => { }));
    }

    public void InitNavigation(
        IRoutableViewModel? document1,
        IRoutableViewModel? document2,
        IRoutableViewModel? nextTool)
    {
        if (document1 is not null)
        {
            GoDocument1 = ReactiveCommand.Create(() =>
                HostScreen.Router.Navigate.Execute(document1).Subscribe(_ => { }));
        }

        if (document2 is not null)
        {
            GoDocument2 = ReactiveCommand.Create(() =>
                HostScreen.Router.Navigate.Execute(document2).Subscribe(_ => { }));
        }

        if (nextTool is not null)
        {
            GoNextTool = ReactiveCommand.Create(() =>
                HostScreen.Router.Navigate.Execute(nextTool).Subscribe(_ => { }));
        }
    }
}
