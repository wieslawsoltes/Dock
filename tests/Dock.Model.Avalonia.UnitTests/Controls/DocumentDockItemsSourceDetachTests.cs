using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockItemsSourceDetachTests
{
    [AvaloniaFact]
    public void ItemsSource_DetachedDocument_DoesNotCrash_WhenSourceCollectionModified()
    {
        // Arrange
        var factory = new TestFactory();
        
        var originalDock = new DocumentDock();
        originalDock.Factory = factory;
        originalDock.DocumentTemplate = new DocumentTemplate();
        
        var targetDock = new DocumentDock();
        targetDock.Factory = factory;
        targetDock.DocumentTemplate = new DocumentTemplate();
        
        var doc1 = new TestDocumentModel { Title = "Doc1", CanClose = true };
        var doc2 = new TestDocumentModel { Title = "Doc2", CanClose = true };
        
        var documents = new ObservableCollection<TestDocumentModel> { doc1, doc2 };
        originalDock.ItemsSource = documents;
        
        // Verify initial state
        Assert.Equal(2, originalDock.VisibleDockables!.Count);
        
        // Get the first document that was generated
        var generatedDocument = originalDock.VisibleDockables[0] as Document;
        Assert.NotNull(generatedDocument);
        Assert.Equal(doc1, generatedDocument.Context);
        
        // Act - Simulate detaching the document to another dock (like dragging to a different dock)
        factory.MoveDockable(originalDock, targetDock, generatedDocument, null);
        
        // Verify the document is now in the target dock
        Assert.Single(originalDock.VisibleDockables);
        Assert.Single(targetDock.VisibleDockables);
        Assert.Equal(generatedDocument, targetDock.VisibleDockables[0]);
        Assert.Equal(targetDock, generatedDocument.Owner);
        
        // Act - Remove the source item from the collection (this used to crash)
        // This should not crash because the document is no longer tracked by the original dock
        var exception = Record.Exception(() => documents.Remove(doc1));
        
        // Assert - No exception should be thrown
        Assert.Null(exception);
        
        // The document should still exist in the target dock even though the source item was removed
        Assert.Single(targetDock.VisibleDockables);
        Assert.Equal(generatedDocument, targetDock.VisibleDockables[0]);
        
        // The original dock should still have one document (for doc2)
        Assert.Single(originalDock.VisibleDockables);
    }
    
    [AvaloniaFact]
    public void ItemsSource_ClearCollection_DoesNotCrash_WithDetachedDocuments()
    {
        // Arrange
        var factory = new TestFactory();
        
        var originalDock = new DocumentDock();
        originalDock.Factory = factory;
        originalDock.DocumentTemplate = new DocumentTemplate();
        
        var targetDock = new DocumentDock();
        targetDock.Factory = factory;
        targetDock.DocumentTemplate = new DocumentTemplate();
        
        var doc1 = new TestDocumentModel { Title = "Doc1", CanClose = true };
        var doc2 = new TestDocumentModel { Title = "Doc2", CanClose = true };
        
        var documents = new ObservableCollection<TestDocumentModel> { doc1, doc2 };
        originalDock.ItemsSource = documents;
        
        // Get both generated documents
        var generatedDoc1 = originalDock.VisibleDockables![0] as Document;
        var generatedDoc2 = originalDock.VisibleDockables[1] as Document;
        
        // Detach one document to another dock
        factory.MoveDockable(originalDock, targetDock, generatedDoc1!, null);
        
        // Act - Clear the entire collection (this used to crash)
        var exception = Record.Exception(() => documents.Clear());
        
        // Assert - No exception should be thrown
        Assert.Null(exception);
        
        // The detached document should still exist in the target dock
        Assert.Single(targetDock.VisibleDockables);
        Assert.Equal(generatedDoc1, targetDock.VisibleDockables[0]);
        
        // The original dock should be empty now
        Assert.Empty(originalDock.VisibleDockables);
    }
    
    [AvaloniaFact]
    public void ItemsSource_DisposeDock_DoesNotCrash_WithDetachedDocuments()
    {
        // Arrange
        var factory = new TestFactory();
        
        var originalDock = new DocumentDock();
        originalDock.Factory = factory;
        originalDock.DocumentTemplate = new DocumentTemplate();
        
        var targetDock = new DocumentDock();
        targetDock.Factory = factory;
        targetDock.DocumentTemplate = new DocumentTemplate();
        
        var doc1 = new TestDocumentModel { Title = "Doc1", CanClose = true };
        
        var documents = new ObservableCollection<TestDocumentModel> { doc1 };
        originalDock.ItemsSource = documents;
        
        // Get the generated document
        var generatedDoc1 = originalDock.VisibleDockables![0] as Document;
        
        // Detach the document to another dock
        factory.MoveDockable(originalDock, targetDock, generatedDoc1!, null);
        
        // Act - Dispose the original dock (this used to crash)
        var exception = Record.Exception(() => originalDock.Dispose());
        
        // Assert - No exception should be thrown
        Assert.Null(exception);
        
        // The detached document should still exist in the target dock
        Assert.Single(targetDock.VisibleDockables);
        Assert.Equal(generatedDoc1, targetDock.VisibleDockables[0]);
    }
}

// Test model class
public class TestDocumentModel : INotifyPropertyChanged
{
    private string _title = "";
    private bool _canClose = true;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
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

// Simple test factory
public class TestFactory : Factory
{
    public override IRootDock CreateLayout()
    {
        var root = new RootDock
        {
            Id = "Root"
        };
        return root;
    }
}