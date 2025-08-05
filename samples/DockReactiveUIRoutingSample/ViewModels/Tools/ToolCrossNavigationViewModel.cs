using System;
using System.Collections.ObjectModel;
using System.Reactive;
using DockReactiveUIRoutingSample.Models;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolCrossNavigationViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public ObservableCollection<CrossNavigationContent> AvailableContent { get; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? GoBack { get; private set; }
    public ReactiveCommand<CrossNavigationContent, IObservable<IRoutableViewModel>>? NavigateToContent { get; private set; }
    public ReactiveCommand<CrossNavigationContent, IObservable<IRoutableViewModel>>? ViewDetails { get; private set; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? OpenDocumentPreview { get; private set; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? ShowToolComparison { get; private set; }

    private CrossNavigationContent? _selectedContent;
    public CrossNavigationContent? SelectedContent
    {
        get => _selectedContent;
        set => this.RaiseAndSetIfChanged(ref _selectedContent, value);
    }

    public ToolCrossNavigationViewModel(IScreen host, string title)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = title;
        
        AvailableContent = new ObservableCollection<CrossNavigationContent>
        {
            new() { Id = "1", Title = "Document Analysis", ContentType = "document", Description = "Analyze document content from Document Editor", Category = "📄", Status = "Active", LastModified = DateTime.Now },
            new() { Id = "2", Title = "Tool Metrics", ContentType = "tool", Description = "View metrics from other tools in the workspace", Category = "📊", Status = "Active", LastModified = DateTime.Now },
            new() { Id = "3", Title = "Cross-Reference Data", ContentType = "cross-ref", Description = "Show relationships between documents and tools", Category = "🔗", Status = "Active", LastModified = DateTime.Now },
            new() { Id = "4", Title = "Shared Resources", ContentType = "resources", Description = "Access shared resources across all contexts", Category = "📦", Status = "Active", LastModified = DateTime.Now },
            new() { Id = "5", Title = "Navigation History", ContentType = "history", Description = "View navigation patterns across tools and documents", Category = "🕒", Status = "Active", LastModified = DateTime.Now }
        };
        
        GoBack = ReactiveCommand.Create(() =>
            HostScreen.Router.NavigateBack.Execute());
            
        NavigateToContent = ReactiveCommand.Create<CrossNavigationContent, IObservable<IRoutableViewModel>>(content =>
        {
            SelectedContent = content;
            return HostScreen.Router.Navigate.Execute(
                new ToolCrossNavigationDetailViewModel(HostScreen, content));
        });
        
        OpenDocumentPreview = ReactiveCommand.Create(() =>
        {
            // This would typically open a document in a new tab/window
            // For demo purposes, we'll navigate to a document view within this router
            return HostScreen.Router.Navigate.Execute(
                new ToolDocumentPreviewViewModel(host, "Document Preview", 
                    "This is a preview of document content accessed from the tool context."));
        });
        
        ShowToolComparison = ReactiveCommand.Create(() =>
            HostScreen.Router.Navigate.Execute(
                new ToolComparisonViewModel(host, "Tool Comparison")));
    }
}