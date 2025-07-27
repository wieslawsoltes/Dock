using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockItemsSourceXamlTests
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
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }

    [AvaloniaFact]
    public void ItemsSource_WithXamlLikeDocumentTemplate_WorksCorrectly()
    {
        // Arrange
        var dock = new DocumentDock();
        
        // Simulate a DocumentTemplate as it would be created from XAML
        // This mimics what happens when you define a DocumentTemplate in XAML
        var template = new DocumentTemplate();
        // In XAML, the Content property gets set to a function automatically
        // For this test, we'll use null which simulates an empty template
        dock.DocumentTemplate = template;

        var items = new ObservableCollection<TestDocumentModel>
        {
            new() { Title = "Test Document", Content = "Test Content", CanClose = false }
        };

        // Act
        dock.ItemsSource = items;

        // Assert
        Assert.NotNull(dock.VisibleDockables);
        Assert.Single(dock.VisibleDockables);
        
        var document = dock.VisibleDockables[0] as Document;
        Assert.NotNull(document);
        Assert.Equal("Test Document", document.Title);
        Assert.False(document.CanClose); // Should be mapped from the model
        Assert.Equal(items[0], document.Context);
        
        // The content should be a function (even if the template is empty)
        Assert.NotNull(document.Content);
    }

    [AvaloniaFact]
    public void ItemsSource_DocumentsHaveCorrectDataContext()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate();
        dock.DocumentTemplate = template;

        var item1 = new TestDocumentModel { Title = "Doc1", Content = "Content1" };
        var item2 = new TestDocumentModel { Title = "Doc2", Content = "Content2" };
        var items = new ObservableCollection<TestDocumentModel> { item1, item2 };

        // Act
        dock.ItemsSource = items;

        // Assert
        Assert.Equal(2, dock.VisibleDockables!.Count);
        
        var doc1 = dock.VisibleDockables[0] as Document;
        var doc2 = dock.VisibleDockables[1] as Document;
        
        Assert.NotNull(doc1);
        Assert.NotNull(doc2);
        
        // Each document should have the correct item as its Context
        Assert.Equal(item1, doc1.Context);
        Assert.Equal(item2, doc2.Context);
        
        // The Content function should work without throwing
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
} 