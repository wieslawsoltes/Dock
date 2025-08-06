using System;
using System.Collections.ObjectModel;
using System.Reactive;
using ReactiveUI;

namespace DockReactiveUIRoutingSample.ViewModels.Tools;

public class ToolDocumentPreviewViewModel : ReactiveObject, IRoutableViewModel
{
    public string UrlPathSegment { get; }
    public IScreen HostScreen { get; }
    public string Title { get; }
    public string DocumentContent { get; }
    public ObservableCollection<DocumentSection> DocumentSections { get; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? GoBack { get; private set; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? OpenInDocumentEditor { get; private set; }
    public ReactiveCommand<DocumentSection, Unit>? JumpToSection { get; private set; }
    public ReactiveCommand<Unit, IObservable<IRoutableViewModel>>? AnalyzeContent { get; private set; }

    private DocumentSection? _selectedSection;
    public DocumentSection? SelectedSection
    {
        get => _selectedSection;
        set => this.RaiseAndSetIfChanged(ref _selectedSection, value);
    }

    private string _analysisResult = "Click 'Analyze Content' to start analysis";
    public string AnalysisResult
    {
        get => _analysisResult;
        set => this.RaiseAndSetIfChanged(ref _analysisResult, value);
    }

    public ToolDocumentPreviewViewModel(IScreen host, string title, string content)
    {
        HostScreen = host;
        UrlPathSegment = GetType().Name;
        Title = title;
        DocumentContent = content;
        
        DocumentSections = new ObservableCollection<DocumentSection>
        {
            new("Introduction", "Overview of the project goals and objectives", 1),
            new("Architecture", "System design and component relationships", 2),
            new("Implementation", "Detailed implementation notes and code examples", 3),
            new("Testing", "Test strategies and validation procedures", 4),
            new("Deployment", "Deployment guidelines and environment setup", 5),
            new("Conclusion", "Summary and future considerations", 6)
        };
        
        GoBack = ReactiveCommand.Create(() =>
            HostScreen.Router.NavigateBack.Execute());
            
        OpenInDocumentEditor = ReactiveCommand.Create(() => HostScreen.Router.Navigate.Execute(new ToolCrossNavigationViewModel(HostScreen, "Document Editor")));
        
        JumpToSection = ReactiveCommand.Create<DocumentSection>(section =>
        {
            SelectedSection = section;
            AnalysisResult = $"Navigated to section: {section.Title}\n{section.Description}";
        });
        
        AnalyzeContent = ReactiveCommand.Create(() => HostScreen.Router.Navigate.Execute(new ToolCrossNavigationViewModel(HostScreen, "Content Analysis")));
    }
    
    private string GenerateContentAnalysis()
    {
        return @"📊 Content Analysis Results:

• Document Structure: Well-organized with 6 main sections
• Readability Score: 8.5/10
• Technical Complexity: Medium-High
• Estimated Reading Time: 15-20 minutes
• Key Topics Identified:
  - System Architecture (25%)
  - Implementation Details (30%)
  - Testing Procedures (20%)
  - Deployment Process (15%)
  - Project Overview (10%)

🔍 Cross-Reference Analysis:
• Links to 3 external tools
• References 5 code repositories
• Mentions 8 configuration files
• Contains 12 technical diagrams

💡 Recommendations:
• Consider adding more code examples
• Update deployment section with latest procedures
• Add troubleshooting section
• Include performance benchmarks";
    }
}

public class DocumentSection
{
    public string Title { get; }
    public string Description { get; }
    public int SectionNumber { get; }
    
    public DocumentSection(string title, string description, int sectionNumber)
    {
        Title = title;
        Description = description;
        SectionNumber = sectionNumber;
    }
}