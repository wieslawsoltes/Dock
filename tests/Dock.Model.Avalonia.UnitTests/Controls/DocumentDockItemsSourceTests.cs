using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockItemsSourceTests
{
    public class TestDocumentModel : INotifyPropertyChanged
    {
        private string _title = "";
        private string _content = "";
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
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    private static IList<IDockable> RequireVisibleDockables(DocumentDock dock) =>
        dock.VisibleDockables ?? throw new InvalidOperationException("VisibleDockables should not be null.");

    [AvaloniaFact]
    public void ItemsSource_WhenSet_CreatesDocumentsFromItems()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var items = new List<TestDocumentModel>
        {
            new() { Title = "Doc1", Content = "Content1" },
            new() { Title = "Doc2", Content = "Content2" }
        };

        // Act
        dock.ItemsSource = items;

        // Assert
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);
        
        var doc1 = visibleDockables[0] as Document;
        var doc2 = visibleDockables[1] as Document;
        
        Assert.NotNull(doc1);
        Assert.NotNull(doc2);
        Assert.Equal("Doc1", doc1.Title);
        Assert.Equal("Doc2", doc2.Title);
        Assert.Equal(items[0], doc1.Context);
        Assert.Equal(items[1], doc2.Context);
    }

    [AvaloniaFact]
    public void ItemsSource_WhenObservableCollectionItemAdded_CreatesNewDocument()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var items = new ObservableCollection<TestDocumentModel>
        {
            new() { Title = "Doc1", Content = "Content1" }
        };

        dock.ItemsSource = items;
        Assert.Single(RequireVisibleDockables(dock));

        // Act
        items.Add(new TestDocumentModel { Title = "Doc2", Content = "Content2" });

        // Assert
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);
        var newDoc = visibleDockables[1] as Document;
        Assert.NotNull(newDoc);
        Assert.Equal("Doc2", newDoc.Title);
    }

    [AvaloniaFact]
    public void ItemsSource_WhenObservableCollectionItemRemoved_RemovesDocument()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var item1 = new TestDocumentModel { Title = "Doc1", Content = "Content1" };
        var item2 = new TestDocumentModel { Title = "Doc2", Content = "Content2" };
        var items = new ObservableCollection<TestDocumentModel> { item1, item2 };

        dock.ItemsSource = items;
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);

        // Act
        items.Remove(item1);

        // Assert
        visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var remainingDoc = visibleDockables[0] as Document;
        Assert.NotNull(remainingDoc);
        Assert.Equal("Doc2", remainingDoc.Title);
        Assert.Equal(item2, remainingDoc.Context);
    }

    [AvaloniaFact]
    public void ItemsSource_WhenObservableCollectionCleared_RemovesAllDocuments()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var items = new ObservableCollection<TestDocumentModel>
        {
            new() { Title = "Doc1", Content = "Content1" },
            new() { Title = "Doc2", Content = "Content2" }
        };

        dock.ItemsSource = items;
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);

        // Act
        items.Clear();

        // Assert
        Assert.Empty(RequireVisibleDockables(dock));
    }

    [AvaloniaFact]
    public void ItemsSource_WhenSetToNull_ClearsAllDocuments()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var items = new List<TestDocumentModel>
        {
            new() { Title = "Doc1", Content = "Content1" },
            new() { Title = "Doc2", Content = "Content2" }
        };

        dock.ItemsSource = items;
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Equal(2, visibleDockables.Count);

        // Act
        dock.ItemsSource = null;

        // Assert
        Assert.Empty(RequireVisibleDockables(dock));
    }

    [AvaloniaFact]
    public void ItemsSource_WithoutDocumentTemplate_DoesNotCreateDocuments()
    {
        // Arrange
        var dock = new DocumentDock();
        // No DocumentTemplate set

        var items = new List<TestDocumentModel>
        {
            new() { Title = "Doc1", Content = "Content1" }
        };

        // Act
        dock.ItemsSource = items;

        // Assert
        // Should not create documents without template
        Assert.True(dock.VisibleDockables == null || dock.VisibleDockables.Count == 0);
    }

    [AvaloniaFact]
    public void ItemsSource_MapsPropertiesCorrectly()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var items = new List<TestDocumentModel>
        {
            new() { Title = "Test Document", Content = "Test Content", CanClose = false }
        };

        // Act
        dock.ItemsSource = items;

        // Assert
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var doc = visibleDockables[0] as Document;
        Assert.NotNull(doc);
        Assert.Equal("Test Document", doc.Title);
        Assert.False(doc.CanClose);
        Assert.Equal(items[0], doc.Context);
    }

    [AvaloniaFact]
    public void ItemsSource_WithNameProperty_UseNameAsTitle()
    {
        // Test class with Name property instead of Title
        var itemWithName = new { Name = "Document Name", Content = "Content" };
        
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        // Act
        dock.ItemsSource = new[] { itemWithName };

        // Assert
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var doc = visibleDockables[0] as Document;
        Assert.NotNull(doc);
        Assert.Equal("Document Name", doc.Title);
    }

    [AvaloniaFact]
    public void ItemsSource_WithDisplayNameProperty_UseDisplayNameAsTitle()
    {
        // Test class with DisplayName property
        var itemWithDisplayName = new { DisplayName = "Display Name", Content = "Content" };
        
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        // Act
        dock.ItemsSource = new[] { itemWithDisplayName };

        // Assert
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var doc = visibleDockables[0] as Document;
        Assert.NotNull(doc);
        Assert.Equal("Display Name", doc.Title);
    }

    [AvaloniaFact]
    public void ItemsSource_WithoutTitleProperty_UsesToStringAsTitle()
    {
        // Test class without Title/Name/DisplayName
        var itemWithoutTitle = new { SomeProperty = "Value" };
        
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        // Act
        dock.ItemsSource = new[] { itemWithoutTitle };

        // Assert
        var visibleDockables = RequireVisibleDockables(dock);
        Assert.Single(visibleDockables);
        var doc = visibleDockables[0] as Document;
        Assert.NotNull(doc);
        // Should use ToString() as fallback
        Assert.NotNull(doc.Title);
        Assert.NotEmpty(doc.Title);
    }

    [AvaloniaFact]
    public void ItemsSource_FirstDocumentBecomesActive()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var items = new List<TestDocumentModel>
        {
            new() { Title = "Doc1", Content = "Content1" },
            new() { Title = "Doc2", Content = "Content2" }
        };

        // Act
        dock.ItemsSource = items;

        // Assert
        Assert.NotNull(dock.ActiveDockable);
        var activeDoc = dock.ActiveDockable as Document;
        Assert.NotNull(activeDoc);
        Assert.Equal("Doc1", activeDoc.Title);
    }
} 
