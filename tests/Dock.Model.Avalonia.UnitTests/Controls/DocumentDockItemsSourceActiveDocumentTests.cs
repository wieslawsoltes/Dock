using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;
using System.Collections.Generic;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockItemsSourceActiveDocumentTests
{
    private class TestDocumentModel : INotifyPropertyChanged
    {
        private string _title = "";
        private string _content = "";

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

        public bool CanClose { get; set; } = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }
    }

    private class TestFactory : Factory
    {
        private IDockable? _lastActiveDockable;
        private IDockable? _lastFocusedDockable;
        private IDock? _lastFocusedDock;

        public IDockable? LastActiveDockable => _lastActiveDockable;
        public IDockable? LastFocusedDockable => _lastFocusedDockable;
        public IDock? LastFocusedDock => _lastFocusedDock;

        public override void SetActiveDockable(IDockable? dockable)
        {
            _lastActiveDockable = dockable;
            base.SetActiveDockable(dockable);
        }

        public override void SetFocusedDockable(IDock? dock, IDockable? dockable)
        {
            _lastFocusedDock = dock;
            _lastFocusedDockable = dockable;
            base.SetFocusedDockable(dock, dockable);
        }
    }

    [AvaloniaFact]
    public void AddingDocumentViaItemsSource_SetsActiveAndFocused()
    {
        // Arrange
        var factory = new TestFactory();
        var documentDock = new DocumentDock
        {
            Factory = factory,
            Id = "TestDock",
            DocumentTemplate = new DocumentTemplate() // Add DocumentTemplate
        };

        var documents = new ObservableCollection<TestDocumentModel>();
        documentDock.ItemsSource = documents;

        // Act - Add a new document via ItemsSource
        var newDocument = new TestDocumentModel
        {
            Title = "Test Document",
            Content = "Test Content"
        };
        documents.Add(newDocument);

        // Assert - Check that the document was properly set as active and focused
        Assert.NotNull(factory.LastActiveDockable);
        Assert.NotNull(factory.LastFocusedDockable);
        Assert.Equal(documentDock, factory.LastFocusedDock);
        
        // The active and focused dockable should be the Document wrapper (not the raw model)
        Assert.IsType<Document>(factory.LastActiveDockable);
        Assert.IsType<Document>(factory.LastFocusedDockable);
        Assert.Equal(factory.LastActiveDockable, factory.LastFocusedDockable);
        
        // The Document's Context should be our test model
        var activeDocument = (Document)factory.LastActiveDockable!;
        Assert.Equal(newDocument, activeDocument.Context);
        Assert.Equal("Test Document", activeDocument.Title);
    }

    [AvaloniaFact]
    public void AddingSecondDocumentViaItemsSource_ReplacesActiveAndFocused()
    {
        // Arrange
        var factory = new TestFactory();
        var documentDock = new DocumentDock
        {
            Factory = factory,
            Id = "TestDock",
            DocumentTemplate = new DocumentTemplate() // Add DocumentTemplate
        };

        var documents = new ObservableCollection<TestDocumentModel>();
        documentDock.ItemsSource = documents;

        // Add first document
        var firstDocument = new TestDocumentModel { Title = "First", Content = "Content 1" };
        documents.Add(firstDocument);
        var firstActiveDocument = factory.LastActiveDockable;

        // Act - Add second document
        var secondDocument = new TestDocumentModel { Title = "Second", Content = "Content 2" };
        documents.Add(secondDocument);

        // Assert - Second document should now be active and focused
        Assert.NotNull(factory.LastActiveDockable);
        Assert.NotEqual(firstActiveDocument, factory.LastActiveDockable);
        
        var activeDocument = (Document)factory.LastActiveDockable!;
        Assert.Equal(secondDocument, activeDocument.Context);
        Assert.Equal("Second", activeDocument.Title);
        
        // Both documents should be in VisibleDockables
        Assert.Equal(2, documentDock.VisibleDockables?.Count);
    }

    [AvaloniaFact]
    public void AddingMultipleDocumentsViaItemsSource_LastDocumentIsActiveAndFocused()
    {
        // Arrange
        var factory = new TestFactory();
        var documentDock = new DocumentDock
        {
            Factory = factory,
            Id = "TestDock",
            DocumentTemplate = new DocumentTemplate() // Add DocumentTemplate
        };

        var documents = new ObservableCollection<TestDocumentModel>();
        documentDock.ItemsSource = documents;

        // Act - Add multiple documents
        documents.Add(new TestDocumentModel { Title = "Doc 1", Content = "Content 1" });
        documents.Add(new TestDocumentModel { Title = "Doc 2", Content = "Content 2" });
        documents.Add(new TestDocumentModel { Title = "Doc 3", Content = "Content 3" });

        // Assert - Last document should be active and focused
        Assert.NotNull(factory.LastActiveDockable);
        var activeDocument = (Document)factory.LastActiveDockable!;
        Assert.Equal("Doc 3", activeDocument.Title);
        
        // All documents should be in VisibleDockables
        Assert.Equal(3, documentDock.VisibleDockables?.Count);
    }
} 