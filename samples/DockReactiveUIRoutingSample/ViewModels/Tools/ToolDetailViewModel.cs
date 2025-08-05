using System;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolDetailViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public string Description { get; }
    public ReactiveCommand<Unit, IDisposable>? GoBack { get; private set; }

    public ToolDetailViewModel(IScreen host, string title, string description)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = title;
        Description = description;
        
        GoBack = ReactiveCommand.Create(() =>
            HostScreen.Router.NavigateBack.Execute().Subscribe(_ => { }));
    }
}