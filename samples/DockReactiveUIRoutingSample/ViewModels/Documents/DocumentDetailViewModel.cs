using System;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Documents;

public class DocumentDetailViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public string Content { get; }
    public ReactiveCommand<Unit, IDisposable>? GoBack { get; private set; }

    public DocumentDetailViewModel(IScreen host, string title, string content)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = title;
        Content = content;
        
        GoBack = ReactiveCommand.Create(() =>
            HostScreen.Router.NavigateBack.Execute().Subscribe(_ => { }));
    }
}