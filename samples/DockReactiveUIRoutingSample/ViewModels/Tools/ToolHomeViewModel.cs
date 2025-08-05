using System;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolHomeViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public ReactiveCommand<Unit, IDisposable>? GoToDetails { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoToSettings { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoBack { get; private set; }

    public ToolHomeViewModel(IScreen host)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = "Tool Home";
        
        GoToDetails = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new ToolDetailViewModel(host, "Tool Details", "This is a detailed view of the tool with additional information and functionality.")).Subscribe(_ => { }));
            
        GoToSettings = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new ToolSettingsViewModel(host, "Tool Settings")).Subscribe(_ => { }));
            
        GoBack = ReactiveCommand.Create(() => HostScreen.Router.NavigateBack.Execute().Subscribe(_ => { }));
    }
}