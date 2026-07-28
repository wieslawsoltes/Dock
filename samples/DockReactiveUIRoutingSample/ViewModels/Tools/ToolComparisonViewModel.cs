using System;
using System.Collections.ObjectModel;
using System.Reactive;
using Unit = ReactiveUI.Primitives.RxVoid;
using ReactiveUI;
using DockReactiveUIRoutingSample.Models;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolComparisonViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public ObservableCollection<ToolMetric> ToolMetrics { get; }
    public ObservableCollection<ComparisonResult> ComparisonResults { get; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? GoBack { get; private set; }
    public ReactiveCommand<Unit, Unit>? RefreshAnalysis { get; private set; }
    public ReactiveCommand<ToolMetric, Unit>? SelectTool { get; private set; }
    public ReactiveCommand<Unit, Unit>? ExportData { get; private set; }
    public ReactiveCommand<ComparisonResult, Unit>? ViewDetails { get; private set; }

    private string _comparisonSummary;
    public string ComparisonSummary
    {
        get => _comparisonSummary;
        set => this.RaiseAndSetIfChanged(ref _comparisonSummary, value);
    }

    private ToolMetric? _selectedTool;
    public ToolMetric? SelectedTool
    {
        get => _selectedTool;
        set => this.RaiseAndSetIfChanged(ref _selectedTool, value);
    }

    private string _statusMessage;
    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ToolComparisonViewModel(IScreen host, string title)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = title;
        
        ToolMetrics = new ObservableCollection<ToolMetric>
        {
            new() { Name = "Primary Tool", Value = "95%", Unit = "efficiency", Status = "Active", NumericValue = 95.0 },
            new() { Name = "Secondary Tool", Value = "87%", Unit = "efficiency", Status = "Active", NumericValue = 87.0 },
            new() { Name = "Document Tool", Value = "92%", Unit = "efficiency", Status = "Active", NumericValue = 92.0 },
            new() { Name = "Analysis Tool", Value = "78%", Unit = "efficiency", Status = "Idle", NumericValue = 78.0 },
            new() { Name = "Utility Tool", Value = "85%", Unit = "efficiency", Status = "Background", NumericValue = 85.0 }
        };
        
        ComparisonResults = new ObservableCollection<ComparisonResult>();
        
        _comparisonSummary = "Tool comparison analysis ready. Click 'Refresh Comparison' to update data.";
        _statusMessage = "Ready for analysis";
        
        GoBack = ReactiveCommand.Create(() => HostScreen.Router.NavigateBack.Execute());
            
        RefreshAnalysis = ReactiveCommand.Create(() =>
        {
            UpdateComparisonResults();
            ComparisonSummary = GenerateComparisonSummary();
        });
        
        SelectTool = ReactiveCommand.Create<ToolMetric>(tool =>
        {
            SelectedTool = tool;
            ComparisonSummary = $"Selected: {tool.Name}\nValue: {tool.Value} {tool.Unit}\nStatus: {tool.Status}";
        });
        
        ExportData = ReactiveCommand.Create(() =>
        {
            ComparisonSummary = "Exporting comparison data to CSV format...\nExport completed successfully!";
        });
        
        ViewDetails = ReactiveCommand.Create<ComparisonResult>(result =>
        {
            StatusMessage = $"Viewing details for: {result.Category}";
        });
        
        // Initialize with default comparison
        UpdateComparisonResults();
    }
    
    private void UpdateComparisonResults()
    {
        ComparisonResults.Clear();
        
        ComparisonResults.Add(new ComparisonResult("Performance", "Primary Tool leads with 95% efficiency", "🏆"));
        ComparisonResults.Add(new ComparisonResult("Memory Usage", "Utility Tool most efficient at 32 MB", "💾"));
        ComparisonResults.Add(new ComparisonResult("Usage Time", "Primary Tool most active with 45 minutes", "⏱️"));
        ComparisonResults.Add(new ComparisonResult("Reliability", "Document Tool shows consistent performance", "🔧"));
        ComparisonResults.Add(new ComparisonResult("Resource Impact", "Analysis Tool has lowest system impact", "📊"));
        ComparisonResults.Add(new ComparisonResult("User Interaction", "Primary Tool has highest engagement", "👤"));
        ComparisonResults.Add(new ComparisonResult("Cross-Integration", "All tools show good interoperability", "🔗"));
    }
    
    private string GenerateComparisonSummary()
    {
        var totalTools = ToolMetrics.Count;
        var activeTools = 0;
        var avgPerformance = 0.0;
        
        foreach (var tool in ToolMetrics)
        {
            if (tool.Status == "Active") activeTools++;
            avgPerformance += tool.NumericValue;
        }
        
        avgPerformance /= totalTools;
        
        return $@"📈 Tool Comparison Summary (Updated: {DateTime.Now:HH:mm:ss})

🔢 Overview:
• Total Tools Analyzed: {totalTools}
• Active Tools: {activeTools}
• Average Performance: {avgPerformance:F1}%

🏅 Top Performers:
• Best Performance: Primary Tool (95%)
• Document Tool: High efficiency (92%)
• Secondary Tool: Consistent performance (87%)

💡 Insights:
• System is well-balanced across tools
• High overall performance scores
• Good distribution of workload
• Tools operating within optimal parameters

🔄 Cross-Tool Integration:
• All tools support data sharing
• Navigation between contexts is seamless
• Resource conflicts: None detected
• Synchronization status: Optimal";
    }
}



public class ComparisonResult
{
    public string Category { get; }
    public string Result { get; }
    public string Icon { get; }
    public string Score { get; }
    public string ScoreColor { get; }
    public string CategoryColor { get; }
    public string CategoryIcon { get; }
    public string Description { get; }
    
    public ComparisonResult(string category, string result, string icon, string score = "100%", string scoreColor = "#4CAF50", string categoryColor = "#2196F3", string categoryIcon = "📊", string description = "")
    {
        Category = category;
        Result = result;
        Icon = icon;
        Score = score;
        ScoreColor = scoreColor;
        CategoryColor = categoryColor;
        CategoryIcon = categoryIcon;
        Description = description;
    }
}