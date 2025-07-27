using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Model.Avalonia.UnitTests.Controls;

public class DocumentDockItemsSourceFallbackTests
{
    public class TestDocumentModel : INotifyPropertyChanged
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

        public override string ToString() => $"TestDocument: {Title}";
    }

    [AvaloniaFact]
    public void ItemsSource_WithEmptyDocumentTemplate_CreatesFallbackContent()
    {
        // Arrange
        var dock = new DocumentDock();
        
        // Create an empty DocumentTemplate (simulates XAML template with no content)
        var template = new DocumentTemplate();
        // Content is null, which will cause Build() to throw ArgumentException
        dock.DocumentTemplate = template;

        var items = new ObservableCollection<TestDocumentModel>
        {
            new() { Title = "Test Document", Content = "Test Content" }
        };

        // Act
        dock.ItemsSource = items;

        // Assert
        Assert.NotNull(dock.VisibleDockables);
        Assert.Single(dock.VisibleDockables);
        
        var document = dock.VisibleDockables[0] as Document;
        Assert.NotNull(document);
        Assert.Equal("Test Document", document.Title);
        Assert.Equal(items[0], document.Context);
        
        // The content should be a function that creates fallback content
        Assert.NotNull(document.Content);
        Assert.IsType<System.Func<System.IServiceProvider, object>>(document.Content);
        
        // Execute the content function to verify it creates fallback content
        if (document.Content is System.Func<System.IServiceProvider, object> contentFunc)
        {
            var control = contentFunc(null!);
            Assert.NotNull(control);
            
            // Should be a StackPanel with fallback content
            Assert.IsType<StackPanel>(control);
            var stackPanel = (StackPanel)control;
            
            // Should have children (title and content blocks)
            Assert.True(stackPanel.Children.Count >= 2);
            
            // First child should be the title
            var titleBlock = stackPanel.Children[0] as TextBlock;
            Assert.NotNull(titleBlock);
            Assert.Equal("Test Document", titleBlock.Text);
            
            // DataContext should be set to the item
            Assert.Equal(items[0], stackPanel.DataContext);
        }
    }

    [AvaloniaFact]
    public void ItemsSource_FallbackContent_DisplaysItemCorrectly()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate(); // Empty template
        dock.DocumentTemplate = template;

        var customItem = new TestDocumentModel 
        { 
            Title = "Custom Title", 
            Content = "Custom Content" 
        };
        
        var items = new ObservableCollection<TestDocumentModel> { customItem };

        // Act
        dock.ItemsSource = items;

        // Assert
        var document = dock.VisibleDockables![0] as Document;
        Assert.NotNull(document);
        
        // Execute the content function
        if (document.Content is System.Func<System.IServiceProvider, object> contentFunc)
        {
            var stackPanel = (StackPanel)contentFunc(null!);
            
            // Check title block
            var titleBlock = (TextBlock)stackPanel.Children[0];
            Assert.Equal("Custom Title", titleBlock.Text);
            
            // Check content block
            var contentBlock = (TextBlock)stackPanel.Children[1];
            Assert.Equal("TestDocument: Custom Title", contentBlock.Text); // Uses ToString()
        }
    }

    [AvaloniaFact]
    public void ItemsSource_MultipleItems_AllGetFallbackContent()
    {
        // Arrange
        var dock = new DocumentDock();
        var template = new DocumentTemplate(); // Empty template
        dock.DocumentTemplate = template;

        var items = new ObservableCollection<TestDocumentModel>
        {
            new() { Title = "Doc1", Content = "Content1" },
            new() { Title = "Doc2", Content = "Content2" },
            new() { Title = "Doc3", Content = "Content3" }
        };

        // Act
        dock.ItemsSource = items;

        // Assert
        Assert.Equal(3, dock.VisibleDockables!.Count);
        
        for (int i = 0; i < 3; i++)
        {
            var document = dock.VisibleDockables[i] as Document;
            Assert.NotNull(document);
            Assert.Equal($"Doc{i + 1}", document.Title);
            
            // Each document should have working fallback content
            if (document.Content is System.Func<System.IServiceProvider, object> contentFunc)
            {
                var control = contentFunc(null!);
                Assert.NotNull(control);
                Assert.IsType<StackPanel>(control);
            }
        }
    }
} 