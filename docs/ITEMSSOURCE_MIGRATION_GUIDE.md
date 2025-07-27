# ItemsSource Migration Guide

This guide helps you migrate to the new `DocumentDock.ItemsSource` functionality introduced in Dock 11.2+.

## What's New

`DocumentDock` now supports an `ItemsSource` property that automatically creates and manages documents from a collection, similar to how `ListBox.ItemsSource` works in Avalonia.

## Benefits of ItemsSource

- **Automatic Document Management**: Documents are automatically created/removed when you add/remove items from your collection
- **MVVM Friendly**: Natural binding to `ObservableCollection<T>` in your ViewModels
- **Less Boilerplate**: No need to manually manage document creation/removal
- **Clean Separation**: Keep your business models separate from UI infrastructure
- **Production Ready**: Fully implemented and tested functionality

## Quick Start

### 1. Define Your Document Model

```csharp
public class FileDocument : INotifyPropertyChanged
{
    private string _title = "";
    private string _content = "";
    
    public string Title  // Used for tab title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    
    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
    
    public bool CanClose { get; set; } = true;  // Controls if tab can be closed
    
    // INotifyPropertyChanged implementation...
}
```

### 2. Set Up Your ViewModel

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FileDocument> Documents { get; } = new();
    
    public ICommand AddDocumentCommand { get; }
    
    public MainViewModel()
    {
        AddDocumentCommand = new Command(() => 
        {
            Documents.Add(new FileDocument 
            { 
                Title = $"Document {Documents.Count + 1}",
                Content = "New document content"
            });
        });
    }
}
```

### 3. Bind in XAML

```xml
<DocumentDock ItemsSource="{Binding Documents}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <StackPanel Margin="10" x:DataType="Document">
        <TextBlock Text="Title:" FontWeight="Bold"/>
        <TextBox Text="{Binding Title}" Margin="0,0,0,10"/>
        <TextBlock Text="Content:" FontWeight="Bold"/>
        <TextBox Text="{Binding Context.Content}" AcceptsReturn="True" Height="200"/>
      </StackPanel>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

## Migration Scenarios

### From Manual Document Creation

**Before:**
```csharp
private void AddDocument()
{
    var document = new Document
    {
        Id = Guid.NewGuid().ToString(),
        Title = "My Document",
        Content = new Func<IServiceProvider, object>(_ => new MyView())
    };
    
    _factory?.AddDockable(DocumentsPane, document);
    _factory?.SetActiveDockable(document);
}
```

**After:**
```csharp
private void AddDocument()
{
    Documents.Add(new MyDocumentModel
    {
        Title = "My Document",
        Content = "Document content"
    });
    // Document automatically created and added!
}
```

### From ViewModel + DataTemplate Pattern

**Before:**
```csharp
// Separate ViewModel inheriting from Document
public class MyDocumentViewModel : Document
{
    public string Content { get; set; }
}

// Manual document management
var doc = new MyDocumentViewModel { Title = "Test", Content = "..." };
_factory?.AddDockable(DocumentsPane, doc);
```

**After:**
```csharp
// Simple POCO model
public class MyDocumentModel : INotifyPropertyChanged
{
    public string Title { get; set; }
    public string Content { get; set; }
}

// Collection-based management
Documents.Add(new MyDocumentModel { Title = "Test", Content = "..." });
```

### From Wrapper Pattern

**Before:**
```csharp
// Complex wrapper setup
var dockDoc = new Document
{
    Title = domainModel.Name,
    Context = domainModel,
    Content = DocumentsPane.DocumentTemplate?.Content
};
DocumentsPane.AddDocument(dockDoc);
```

**After:**
```csharp
// Direct collection manipulation
Documents.Add(domainModel); // Automatically wrapped and added
```

## Key Differences

### Data Context in Templates

**Important**: In `DocumentTemplate`, your model is accessed via `Context`:

```xml
<!-- ❌ Wrong -->
<DocumentTemplate x:DataType="MyModel">
  <TextBox Text="{Binding Content}"/>
</DocumentTemplate>

<!-- ✅ Correct -->
<DocumentTemplate>
  <StackPanel x:DataType="Document">
    <TextBox Text="{Binding Context.Content}"/>
  </StackPanel>
</DocumentTemplate>
```

### Property Mapping

The system automatically maps these properties from your model to Document properties:

- `Title` → Document.Title (tab title)
- `Name` → Document.Title (alternative)
- `DisplayName` → Document.Title (alternative)
- `CanClose` → Document.CanClose (controls if tab can be closed)

## Common Patterns

### File Manager

```csharp
public class FileModel : INotifyPropertyChanged
{
    public string Name { get; set; }           // → Document title
    public string FilePath { get; set; }
    public string Content { get; set; }
    public bool IsReadOnly { get; set; }
    public bool CanClose => !IsReadOnly;      // → Document.CanClose
    
    public ICommand SaveCommand { get; }
    public ICommand RevertCommand { get; }
}
```

### Settings Pages

```csharp
public class SettingsPage : INotifyPropertyChanged
{
    public string Title { get; set; }         // → Document title
    public string Category { get; set; }
    public Dictionary<string, object> Settings { get; set; }
    public bool CanClose { get; set; } = true;
}
```

## Best Practices

1. **Use INotifyPropertyChanged**: Ensures UI updates when properties change
2. **Keep Models Simple**: Avoid UI-specific logic in your document models
3. **Use Commands**: Implement actions as commands for better testability
4. **Consider CanClose**: Use this property to prevent accidental closing of important documents
5. **Provide Meaningful Titles**: The `Title` property becomes the tab title

## When NOT to Use ItemsSource

ItemsSource might not be suitable for:

- **Highly Dynamic Content**: When document content types change dramatically at runtime
- **Complex Document Hierarchies**: When you need nested or grouped document structures
- **Legacy Integration**: When integrating with existing complex document management systems
- **Custom Serialization**: When you need very specific control over layout serialization

For these scenarios, consider the traditional ViewModel + DataTemplate approach or the wrapper pattern.

## See Also

- [Document and Tool Content Guide](dock-content-guide.md) - Comprehensive content setup guide
- [Dock FAQ](dock-faq.md) - Common questions and troubleshooting
- [Advanced Content Wrapper Pattern](dock-content-wrapper-pattern.md) - For complex scenarios 