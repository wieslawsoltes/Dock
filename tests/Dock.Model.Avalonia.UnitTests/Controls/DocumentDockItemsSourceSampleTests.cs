using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

/// <summary>
/// Tests that simulate the exact sample scenario to verify ItemsSource works correctly
/// </summary>
public class DocumentDockItemsSourceSampleTests
{
    // Exact copy of the model from the sample
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
    public void Sample_ScenarioTest_WorksCorrectly()
    {
        // Arrange - Simulate the sample setup
        var dock = new DocumentDock();
        
        // Empty DocumentTemplate as would be created from XAML
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        // Create the same collection as in the sample
        var documents = new ObservableCollection<MyDocumentModel>();
        
        // Add initial documents like in the sample
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

        // Act - Set ItemsSource like in the sample
        dock.ItemsSource = documents;

        // Assert - Verify documents are created correctly
        Assert.NotNull(dock.VisibleDockables);
        Assert.Equal(2, dock.VisibleDockables.Count);
        
        var doc1 = dock.VisibleDockables[0] as Document;
        var doc2 = dock.VisibleDockables[1] as Document;
        
        Assert.NotNull(doc1);
        Assert.NotNull(doc2);
        
        // Check document properties
        Assert.Equal("Welcome", doc1.Title);
        Assert.Equal("Documentation", doc2.Title);
        Assert.True(doc1.CanClose);
        Assert.True(doc2.CanClose);
        
        // Check Context is set correctly
        Assert.Equal(documents[0], doc1.Context);
        Assert.Equal(documents[1], doc2.Context);
        
        // Verify content functions work (they should create fallback content)
        if (doc1.Content is System.Func<System.IServiceProvider, object> contentFunc1)
        {
            var control1 = contentFunc1(null!);
            Assert.NotNull(control1);
        }
        
        if (doc2.Content is System.Func<System.IServiceProvider, object> contentFunc2)
        {
            var control2 = contentFunc2(null!);
            Assert.NotNull(control2);
        }
    }

    [AvaloniaFact]
    public void Sample_AddDocument_WorksLikeInSample()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var documents = new ObservableCollection<MyDocumentModel>();
        dock.ItemsSource = documents;
        
        Assert.Empty(dock.VisibleDockables!);

        // Act - Simulate adding a document like in the sample
        var newDoc = new MyDocumentModel
        {
            Title = "Document 1",
            Content = "This is automatically generated document #1",
            EditableContent = $"You can edit this content. Document created at {System.DateTime.Now:HH:mm:ss}",
            Status = $"Created at {System.DateTime.Now:HH:mm:ss}",
            CanClose = true
        };
        
        documents.Add(newDoc);

        // Assert
        Assert.Single(dock.VisibleDockables);
        
        var document = dock.VisibleDockables[0] as Document;
        Assert.NotNull(document);
        Assert.Equal("Document 1", document.Title);
        Assert.Equal(newDoc, document.Context);
        Assert.True(document.CanClose);
        
        // Should be set as active (first document becomes active)
        Assert.Equal(document, dock.ActiveDockable);
    }

    [AvaloniaFact]
    public void Sample_RemoveDocument_WorksLikeInSample()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var doc1 = new MyDocumentModel { Title = "Doc1", CanClose = true };
        var doc2 = new MyDocumentModel { Title = "Doc2", CanClose = true };
        
        var documents = new ObservableCollection<MyDocumentModel> { doc1, doc2 };
        dock.ItemsSource = documents;
        
        Assert.Equal(2, dock.VisibleDockables!.Count);

        // Act - Remove a document like in the sample
        documents.Remove(doc1);

        // Assert
        Assert.Single(dock.VisibleDockables);
        
        var remainingDocument = dock.VisibleDockables[0] as Document;
        Assert.NotNull(remainingDocument);
        Assert.Equal("Doc2", remainingDocument.Title);
        Assert.Equal(doc2, remainingDocument.Context);
    }

    [AvaloniaFact]
    public void Sample_ClearAll_WorksLikeInSample()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var documents = new ObservableCollection<MyDocumentModel>
        {
            new() { Title = "Doc1" },
            new() { Title = "Doc2" },
            new() { Title = "Doc3" }
        };
        
        dock.ItemsSource = documents;
        Assert.Equal(3, dock.VisibleDockables!.Count);

        // Act - Clear all like in the sample
        documents.Clear();

        // Assert
        Assert.Empty(dock.VisibleDockables);
    }
} 