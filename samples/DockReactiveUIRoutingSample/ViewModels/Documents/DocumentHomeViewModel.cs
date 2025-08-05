using System;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Documents;

public class DocumentHomeViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public ReactiveCommand<Unit, IDisposable>? GoToDetails { get; private set; }
    public ReactiveCommand<Unit, IDisposable>? GoToEditor { get; private set; }

    public DocumentHomeViewModel(IScreen host)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = "Document Home";
        
        GoToDetails = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new DocumentDetailViewModel(host, "Document Details", "This document contains detailed information about the current project and its specifications.")).Subscribe(_ => { }));
            
        GoToEditor = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(new DocumentEditorViewModel(host, "Document Editor")).Subscribe(_ => { }));
    }
}