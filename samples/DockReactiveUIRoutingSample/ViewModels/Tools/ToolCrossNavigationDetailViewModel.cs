using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Unit = ReactiveUI.Primitives.RxVoid;
using ReactiveUI;
using DockReactiveUIRoutingSample.Models;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolCrossNavigationDetailViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public CrossNavigationContent ContentItem { get; }
    public ObservableCollection<string> DetailedInfo { get; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? GoBack { get; private set; }
    public ReactiveCommand<Unit, Unit>? RefreshData { get; private set; }
    public ReactiveCommand<Unit, Unit>? ExportData { get; private set; }

    private string _statusMessage = "Ready";
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ToolCrossNavigationDetailViewModel(IScreen host, CrossNavigationContent contentItem)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        ContentItem = contentItem;
        Title = $"Details: {contentItem.Title}";
        
        DetailedInfo = new ObservableCollection<string>();
        LoadDetailedInfo();
        
        GoBack = ReactiveCommand.Create(() => HostScreen.Router.NavigateBack.Execute());
            
        RefreshData = ReactiveCommand.Create(() =>
        {
            StatusMessage = "Refreshing data...";
            LoadDetailedInfo();
            StatusMessage = "Data refreshed successfully";
        });
        
        ExportData = ReactiveCommand.Create(() =>
        {
            StatusMessage = $"Exporting {contentItem.Title} data...";
            // Simulate export process
            StatusMessage = "Export completed successfully";
        });
    }
    
    private void LoadDetailedInfo()
    {
        DetailedInfo.Clear();
        
        switch (ContentItem.ContentType)
        {
            case "document":
                DetailedInfo.Add("📄 Document Analysis Results:");
                DetailedInfo.Add("• Total Documents: 3");
                DetailedInfo.Add("• Last Modified: 2 hours ago");
                DetailedInfo.Add("• Content Type: Mixed (Text, Images)");
                DetailedInfo.Add("• Word Count: 1,247 words");
                DetailedInfo.Add("• Cross-references: 5 external links");
                break;
                
            case "tool":
                DetailedInfo.Add("📊 Tool Metrics Overview:");
                DetailedInfo.Add("• Active Tools: 2");
                DetailedInfo.Add("• Total Usage Time: 45 minutes");
                DetailedInfo.Add("• Most Used Feature: Settings Configuration");
                DetailedInfo.Add("• Performance Score: 94%");
                DetailedInfo.Add("• Memory Usage: 128 MB");
                break;
                
            case "cross-ref":
                DetailedInfo.Add("🔗 Cross-Reference Analysis:");
                DetailedInfo.Add("• Document ↔ Tool Links: 8");
                DetailedInfo.Add("• Shared Data Objects: 12");
                DetailedInfo.Add("• Navigation Patterns: 3 common paths");
                DetailedInfo.Add("• Data Synchronization: Real-time");
                DetailedInfo.Add("• Dependency Graph: 15 nodes, 23 edges");
                break;
                
            case "resources":
                DetailedInfo.Add("📦 Shared Resources Status:");
                DetailedInfo.Add("• Available Resources: 24");
                DetailedInfo.Add("• Cache Hit Rate: 87%");
                DetailedInfo.Add("• Storage Used: 2.3 GB");
                DetailedInfo.Add("• Network Requests: 156 today");
                DetailedInfo.Add("• Resource Types: Images, Data, Configs");
                break;
                
            case "history":
                DetailedInfo.Add("🕒 Navigation History Analysis:");
                DetailedInfo.Add("• Total Navigation Events: 342");
                DetailedInfo.Add("• Most Visited: Document Editor (23%)");
                DetailedInfo.Add("• Average Session Time: 12 minutes");
                DetailedInfo.Add("• Back Navigation Rate: 31%");
                DetailedInfo.Add("• Cross-Context Switches: 45");
                break;
                
            default:
                DetailedInfo.Add("ℹ️ General Information:");
                DetailedInfo.Add("• No specific data available");
                DetailedInfo.Add("• Please select a valid content type");
                break;
        }
    }
}