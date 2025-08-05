using System;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Documents;

public class DocumentEditorViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public string DocumentText { get; set; }
    public ReactiveCommand<Unit, IDisposable>? GoBack { get; private set; }
    public ReactiveCommand<Unit, Unit>? SaveDocument { get; private set; }

    public DocumentEditorViewModel(IScreen host, string title)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = title;
        DocumentText = "Enter your document content here...";
        
        GoBack = ReactiveCommand.Create(() =>
            HostScreen.Router.NavigateBack.Execute().Subscribe(_ => { }));
            
        SaveDocument = ReactiveCommand.Create(() =>
        {
            // Simulate saving the document
        });
    }
}