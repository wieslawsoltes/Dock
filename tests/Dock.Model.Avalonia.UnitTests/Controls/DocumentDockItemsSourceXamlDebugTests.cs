using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Xunit;
using Xunit.Abstractions;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockItemsSourceXamlDebugTests
{
    private readonly ITestOutputHelper _output;

    public DocumentDockItemsSourceXamlDebugTests(ITestOutputHelper output)
    {
        _output = output;
    }

    public class MyDocumentModel : INotifyPropertyChanged
    {
        private string _title = "";
        private string _content = "";
        private string _editableContent = "";
        private string _status = "New";
        private bool _canClose = true;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string EditableContent
        {
            get => _editableContent;
            set => SetProperty(ref _editableContent, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool CanClose
        {
            get => _canClose;
            set => SetProperty(ref _canClose, value);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    [AvaloniaFact]
    public void ItemsSource_ExactXamlScenario_StepByStepDebug()
    {
        _output.WriteLine("=== Testing Exact XAML Scenario ===");

        // Step 1: Create the exact structure from XAML
        _output.WriteLine("Step 1: Creating DockControl");
        var dockControl = new DockControl();
        dockControl.InitializeLayout = true;
        dockControl.InitializeFactory = true;
        
        _output.WriteLine("Step 2: Creating Factory");
        var factory = new Factory();
        factory.HideToolsOnClose = true;
        factory.HideDocumentsOnClose = true;
        dockControl.Factory = factory;

        _output.WriteLine("Step 3: Creating RootDock");
        var rootDock = new RootDock();
        rootDock.Id = "Root";
        rootDock.IsCollapsable = false;

        _output.WriteLine("Step 4: Creating ProportionalDock");
        var proportionalDock = new ProportionalDock();
        proportionalDock.Id = "MainLayout";
        proportionalDock.Orientation = Dock.Model.Core.Orientation.Horizontal;

        _output.WriteLine("Step 5: Creating DocumentDock");
        var documentDock = new DocumentDock();
        documentDock.Id = "DocumentsPane";
        documentDock.CanCreateDocument = true;

        _output.WriteLine("Step 6: Creating DocumentTemplate");
        var documentTemplate = new DocumentTemplate();
        documentDock.DocumentTemplate = documentTemplate;

        _output.WriteLine("Step 7: Setting up dock hierarchy");
        proportionalDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<Dock.Model.Core.IDockable> { documentDock };
        rootDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<Dock.Model.Core.IDockable> { proportionalDock };
        rootDock.DefaultDockable = proportionalDock;
        
        // Set the layout on DockControl (this might be important!)
        _output.WriteLine("Step 8: Setting layout on DockControl");
        dockControl.Layout = rootDock;

        _output.WriteLine("Step 9: Creating Documents collection");
        var documents = new ObservableCollection<MyDocumentModel>();
        documents.Add(new MyDocumentModel
        {
            Title = "Welcome",
            Content = "Welcome content",
            EditableContent = "Editable content",
            Status = "Active",
            CanClose = true
        });
        documents.Add(new MyDocumentModel
        {
            Title = "Documentation",
            Content = "Doc content",
            EditableContent = "Doc editable",
            Status = "Active",
            CanClose = true
        });

        _output.WriteLine($"Documents collection count: {documents.Count}");

        _output.WriteLine("Step 10: Setting ItemsSource");
        documentDock.ItemsSource = documents;

        _output.WriteLine($"DocumentDock.VisibleDockables count: {documentDock.VisibleDockables?.Count ?? 0}");

        // Debug the structure
        if (documentDock.VisibleDockables != null)
        {
            for (int i = 0; i < documentDock.VisibleDockables.Count; i++)
            {
                var doc = documentDock.VisibleDockables[i] as Document;
                _output.WriteLine($"Document {i}: Title='{doc?.Title}', Id='{doc?.Id}', Context={doc?.Context?.GetType().Name}");
            }
        }

        // Test DockControl initialization
        _output.WriteLine("Step 11: Testing DockControl state");
        _output.WriteLine($"DockControl.Layout: {dockControl.Layout?.GetType().Name}");
        _output.WriteLine($"DockControl.Factory: {dockControl.Factory?.GetType().Name}");
        _output.WriteLine($"DockControl.InitializeLayout: {dockControl.InitializeLayout}");
        _output.WriteLine($"DockControl.InitializeFactory: {dockControl.InitializeFactory}");

        // Assert that the setup worked
        Assert.NotNull(documentDock.VisibleDockables);
        Assert.Equal(2, documentDock.VisibleDockables.Count);
        
        var doc1 = documentDock.VisibleDockables[0] as Document;
        var doc2 = documentDock.VisibleDockables[1] as Document;
        
        Assert.NotNull(doc1);
        Assert.NotNull(doc2);
        Assert.Equal("Welcome", doc1.Title);
        Assert.Equal("Documentation", doc2.Title);
    }

    [AvaloniaFact] 
    public void ItemsSource_WithoutDockControl_StillWorks()
    {
        _output.WriteLine("=== Testing DocumentDock Standalone ===");

        // Test without DockControl wrapper to isolate the issue
        var documentDock = new DocumentDock();
        documentDock.Id = "DocumentsPane";
        documentDock.CanCreateDocument = true;

        var documentTemplate = new DocumentTemplate();
        documentDock.DocumentTemplate = documentTemplate;

        var documents = new ObservableCollection<MyDocumentModel>();
        documents.Add(new MyDocumentModel { Title = "Test Doc", Content = "Test Content" });

        _output.WriteLine("Setting ItemsSource on standalone DocumentDock");
        documentDock.ItemsSource = documents;

        _output.WriteLine($"VisibleDockables count: {documentDock.VisibleDockables?.Count ?? 0}");

        Assert.NotNull(documentDock.VisibleDockables);
        Assert.Single(documentDock.VisibleDockables);
        
        var doc = documentDock.VisibleDockables[0] as Document;
        Assert.NotNull(doc);
        Assert.Equal("Test Doc", doc.Title);
    }
} 