using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

/// <summary>
/// Tests to verify correct namespace handling for the sample scenario
/// </summary>
public class DocumentDockItemsSourceNamespaceTests
{
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
    public void ItemsSource_WithCorrectNamespaces_WorksCorrectly()
    {
        // Arrange - Using the correct namespace classes like in the fixed sample
        var dockControl = new DockControl();
        var factory = new Factory(); // From Dock.Model.Avalonia namespace
        dockControl.Factory = factory;
        
        var documentDock = new DocumentDock(); // From Dock.Model.Avalonia.Controls namespace
        var documentTemplate = new DocumentTemplate(); // From Dock.Model.Avalonia.Controls namespace
        documentDock.DocumentTemplate = documentTemplate;

        var documents = new ObservableCollection<MyDocumentModel>
        {
            new() { Title = "Welcome", Content = "Welcome to the ItemsSource example!" },
            new() { Title = "Documentation", Content = "How to use ItemsSource" }
        };

        // Act - This simulates the corrected XAML setup
        documentDock.ItemsSource = documents;

        // Assert - Verify everything works with the correct namespaces
        Assert.NotNull(documentDock.VisibleDockables);
        Assert.Equal(2, documentDock.VisibleDockables.Count);
        
        var doc1 = documentDock.VisibleDockables[0] as Document;
        var doc2 = documentDock.VisibleDockables[1] as Document;
        
        Assert.NotNull(doc1);
        Assert.NotNull(doc2);
        Assert.Equal("Welcome", doc1.Title);
        Assert.Equal("Documentation", doc2.Title);
        
        // Verify Context is set correctly
        Assert.Equal(documents[0], doc1.Context);
        Assert.Equal(documents[1], doc2.Context);
        
        // Verify the content creation works (should create fallback content)
        if (doc1.Content is System.Func<System.IServiceProvider, object> contentFunc1)
        {
            var control1 = contentFunc1(null!);
            Assert.NotNull(control1);
        }
    }

    [AvaloniaFact]
    public void ItemsSource_CanInstantiateAllRequiredClasses()
    {
        // This test ensures all the classes we reference in XAML can be instantiated
        // and are from the correct namespaces
        
        // From Dock.Avalonia.Controls namespace
        var dockControl = new DockControl();
        Assert.NotNull(dockControl);
        
        // From Dock.Model.Avalonia namespace  
        var factory = new Factory();
        Assert.NotNull(factory);
        
        // From Dock.Model.Avalonia.Controls namespace
        var documentDock = new DocumentDock();
        var documentTemplate = new DocumentTemplate();
        var rootDock = new RootDock();
        var proportionalDock = new ProportionalDock();
        
        Assert.NotNull(documentDock);
        Assert.NotNull(documentTemplate);
        Assert.NotNull(rootDock);
        Assert.NotNull(proportionalDock);
        
        // Verify they can be connected properly
        dockControl.Factory = factory;
        documentDock.DocumentTemplate = documentTemplate;
        
        // This confirms the namespace structure is correct
        Assert.Equal(factory, dockControl.Factory);
        Assert.Equal(documentTemplate, documentDock.DocumentTemplate);
    }

    [AvaloniaFact]
    public void ItemsSource_FactoryProperties_WorkCorrectly()
    {
        // Test that the Factory class has the expected properties from the sample
        var factory = new Factory();
        
        // These properties should be settable as they appear in the XAML
        factory.HideToolsOnClose = true;
        factory.HideDocumentsOnClose = true;
        
        Assert.True(factory.HideToolsOnClose);
        Assert.True(factory.HideDocumentsOnClose);
    }
} 