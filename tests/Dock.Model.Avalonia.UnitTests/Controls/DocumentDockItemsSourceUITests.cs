using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using Xunit.Abstractions;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockItemsSourceUITests
{
    private readonly ITestOutputHelper _output;

    public DocumentDockItemsSourceUITests(ITestOutputHelper output)
    {
        _output = output;
    }

    // Copy of the exact model from the sample
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

    private sealed class CustomGeneratedDocument : Document
    {
        public string? Marker { get; set; }
    }

    private sealed class CustomGenerator : IDockItemContainerGenerator
    {
        public IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index)
        {
            return new CustomGeneratedDocument { Id = $"Doc-{index}" };
        }

        public void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index)
        {
            container.Title = $"Generated {index}";
            container.Context = item;
            if (container is CustomGeneratedDocument doc)
            {
                doc.Marker = "prepared";
            }
        }

        public void ClearDocumentContainer(IItemsSourceDock dock, IDockable container, object? item)
        {
        }

        public IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index) => null;

        public void PrepareToolContainer(IToolItemsSourceDock dock, IDockable container, object item, int index)
        {
        }

        public void ClearToolContainer(IToolItemsSourceDock dock, IDockable container, object? item)
        {
        }
    }

    [AvaloniaFact]
    public void ItemsSource_ExactSampleScenario_DebugTest()
    {
        // Arrange - Exact simulation of the sample setup
        var dockControl = new DockControl();
        var factory = new Factory();
        dockControl.Factory = factory;

        var rootDock = new RootDock { Id = "Root", IsCollapsable = false };
        var proportionalDock = new ProportionalDock { Id = "MainLayout", Orientation = Dock.Model.Core.Orientation.Horizontal };
        var documentDock = new DocumentDock { Id = "DocumentsPane" };

        // Set up the exact same structure as in XAML
        proportionalDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<Dock.Model.Core.IDockable> { documentDock };
        rootDock.VisibleDockables = new global::Avalonia.Collections.AvaloniaList<Dock.Model.Core.IDockable> { proportionalDock };
        rootDock.DefaultDockable = proportionalDock;

        // Empty DocumentTemplate like in XAML
        var documentTemplate = new DocumentTemplate();
        documentDock.DocumentTemplate = documentTemplate;

        // Create the exact same documents as the ViewModel
        var documents = new ObservableCollection<MyDocumentModel>();
        documents.Add(new MyDocumentModel
        {
            Title = "Welcome",
            Content = "Welcome to the ItemsSource example!",
            EditableContent = "This demonstrates automatic document creation from a collection.",
            Status = $"Created at {System.DateTime.Now:HH:mm:ss}",
            CanClose = true
        });
        
        documents.Add(new MyDocumentModel
        {
            Title = "Documentation", 
            Content = "How to use ItemsSource",
            EditableContent = "Bind your collection to DocumentDock.ItemsSource and define a DocumentTemplate.",
            Status = $"Created at {System.DateTime.Now:HH:mm:ss}",
            CanClose = true
        });

        _output.WriteLine($"Initial documents count: {documents.Count}");

        // Act - Set ItemsSource like in the sample
        documentDock.ItemsSource = documents;

        // Debug output
        _output.WriteLine($"DocumentDock.VisibleDockables count: {documentDock.VisibleDockables?.Count ?? 0}");
        
        if (documentDock.VisibleDockables != null)
        {
            for (int i = 0; i < documentDock.VisibleDockables.Count; i++)
            {
                var doc = documentDock.VisibleDockables[i] as Document;
                _output.WriteLine($"Document {i}: Title='{doc?.Title}', Context={doc?.Context}, Content={doc?.Content?.GetType().Name}");
                
                // Test content creation
                if (doc?.Content is System.Func<System.IServiceProvider, object> contentFunc)
                {
                    try
                    {
                        var control = contentFunc(null!);
                        _output.WriteLine($"  Content created: {control?.GetType().Name}");
                        
                        if (control is StackPanel panel)
                        {
                            _output.WriteLine($"  StackPanel children count: {panel.Children.Count}");
                            for (int j = 0; j < panel.Children.Count; j++)
                            {
                                var child = panel.Children[j];
                                if (child is TextBlock textBlock)
                                {
                                    _output.WriteLine($"    Child {j}: TextBlock with text: '{textBlock.Text}'");
                                }
                                else
                                {
                                    _output.WriteLine($"    Child {j}: {child.GetType().Name}");
                                }
                            }
                        }
                    }
                    catch (System.Exception ex)
                    {
                        _output.WriteLine($"  Content creation failed: {ex.Message}");
                    }
                }
            }
        }

        // Assert - Verify everything is set up correctly
        Assert.NotNull(documentDock.VisibleDockables);
        Assert.Equal(2, documentDock.VisibleDockables.Count);
        
        var doc1 = documentDock.VisibleDockables[0] as Document;
        var doc2 = documentDock.VisibleDockables[1] as Document;
        
        Assert.NotNull(doc1);
        Assert.NotNull(doc2);
        Assert.Equal("Welcome", doc1.Title);
        Assert.Equal("Documentation", doc2.Title);
        
        // Verify content functions work
        Assert.NotNull(doc1.Content);
        Assert.NotNull(doc2.Content);
        
        if (doc1.Content is System.Func<System.IServiceProvider, object> contentFunc1)
        {
            var control1 = contentFunc1(null!);
            Assert.NotNull(control1);
            Assert.IsType<StackPanel>(control1);
        }
    }

    [AvaloniaFact]
    public void ItemsSource_WithCustomGenerator_UsesCustomContainer()
    {
        var documentDock = new DocumentDock
        {
            Id = "Documents",
            ItemContainerGenerator = new CustomGenerator(),
            DocumentTemplate = new DocumentTemplate()
        };

        var documents = new ObservableCollection<MyDocumentModel>
        {
            new() { Title = "One", Content = "Content" }
        };

        documentDock.ItemsSource = documents;

        Assert.NotNull(documentDock.VisibleDockables);
        Assert.Single(documentDock.VisibleDockables);
        var generated = Assert.IsType<CustomGeneratedDocument>(documentDock.VisibleDockables[0]);
        Assert.Equal("prepared", generated.Marker);
        Assert.Equal("Generated 0", generated.Title);
        Assert.Same(documents[0], generated.Context);
    }
}
