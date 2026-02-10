# DocumentDock and ToolDock ItemsSource

The `DocumentDock.ItemsSource` and `ToolDock.ItemsSource` properties enable automatic dockable management by binding collections of data objects. This provides a clean, MVVM-friendly approach for both document tabs and tool panes.

## Overview

`DocumentDock.ItemsSource` and `ToolDock.ItemsSource` work similarly to `ListBox.ItemsSource` in Avalonia. When you add or remove items from an `INotifyCollectionChanged` collection (such as `ObservableCollection<T>`), corresponding dockables are created or removed automatically.

`ItemsSource` is implemented by `Dock.Model.Avalonia.Controls.DocumentDock` and `Dock.Model.Avalonia.Controls.ToolDock` in the Avalonia model layer:

- By default, `DocumentDock.ItemsSource` uses `DocumentTemplate` for generated content.
- By default, `ToolDock.ItemsSource` uses `ToolTemplate` for generated content.
- `DocumentDock.ItemContainerGenerator` and `ToolDock.ItemContainerGenerator` let you override container creation and preparation.
- `DocumentDock.DocumentItemContainerTheme` and `ToolDock.ToolItemContainerTheme` let you apply per-dock container theme metadata for generated dockables.
- `DocumentDock.DocumentItemTemplateSelector` and `ToolDock.ToolItemTemplateSelector` let you choose per-item template content while preserving template fallback behavior.

With the default generator, generated dockables are created when either:

- the dock template is set (`DocumentTemplate` / `ToolTemplate`), or
- a template selector is set and returns content for the item.

A custom `ItemContainerGenerator` can fully override this behavior.

## Behavior details

- By default, `DocumentDock` creates a `Document` for each item and stores the item in `Document.Context`.
- By default, `ToolDock` creates a `Tool` for each item and stores the item in `Tool.Context`.
- The tab title is derived from `Title`, `Name`, or `DisplayName` properties on the item (in that order), falling back to `ToString()`.
- `CanClose` is copied from the item if present; otherwise it defaults to `true`.
- When a generated document or tool is closed, the factory attempts to remove the source item from `ItemsSource` if it implements `IList`.
- Source-generated document/tool closes are treated as remove operations, even when `Factory.HideDocumentsOnClose` or `Factory.HideToolsOnClose` is enabled.
- You can replace the default generation pipeline with `IDockItemContainerGenerator` for custom container types, metadata mapping, and cleanup.
- Theme metadata from `DocumentItemContainerTheme` / `ToolItemContainerTheme` is copied to generated containers and applied by `DocumentContentControl` / `ToolContentControl`.
- Template selectors run before dock templates. If a selector returns `null`, Dock falls back to `DocumentTemplate` / `ToolTemplate`.
- Changing `DocumentTemplate`, `ToolTemplate`, per-dock theme metadata, or selector regenerates source-generated containers.

## Per-Dock Theme and Template Selector APIs

The default generator now supports per-dock presentation customization for generated items:

- `DocumentDock.DocumentItemContainerTheme`
- `ToolDock.ToolItemContainerTheme`
- `DocumentDock.DocumentItemTemplateSelector` (`IDocumentItemTemplateSelector`)
- `ToolDock.ToolItemTemplateSelector` (`IToolItemTemplateSelector`)

`DocumentItemContainerTheme` / `ToolItemContainerTheme` accept either:

- a `ControlTheme` instance, or
- a resource key that resolves to a `ControlTheme`.

Template selectors return template content for an item. They can return:

- `null` (use dock template fallback),
- a `Func<IServiceProvider, object>`,
- a `Control` instance,
- a `DocumentTemplate` / `ToolTemplate`,
- any content object supported by Dock template loading.

### Example

```xaml
<Window.Resources>
  <local:MyDocumentSelector x:Key="DocumentSelector" />
  <ControlTheme x:Key="GeneratedDocumentTheme"
                TargetType="dock:DocumentContentControl"
                BasedOn="{StaticResource {x:Type dock:DocumentContentControl}}">
    <Setter Property="Margin" Value="3" />
  </ControlTheme>
</Window.Resources>

<DocumentDock ItemsSource="{Binding Documents}"
              DocumentItemContainerTheme="GeneratedDocumentTheme"
              DocumentItemTemplateSelector="{StaticResource DocumentSelector}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <!-- fallback template for items not handled by selector -->
      <TextBlock Text="{Binding Context.Title}" />
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

### Migration Path

- Existing `ItemsSource + DocumentTemplate` / `ItemsSource + ToolTemplate` behavior is unchanged.
- Add selectors only where you need per-item template switching.
- Add per-dock container themes only where generated item hosts need distinct styling.
- Existing custom `IDockItemContainerGenerator` implementations continue to work unchanged.

## Key Benefits

- **Automatic Document Management**: Documents are created and removed automatically when the collection changes
- **MVVM Support**: Natural binding to `ObservableCollection<T>` in ViewModels
- **Clean Separation**: Business models remain separate from UI infrastructure
- **Simplified Code**: Eliminates manual document creation and management boilerplate

## Basic Usage

### 1. Document Model

Create a model class to represent your document data:

```csharp
public class FileDocument : INotifyPropertyChanged
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
    
    // INotifyPropertyChanged implementation
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
```

### 2. ViewModel Setup

Create a ViewModel with an `ObservableCollection` of your document models:

The example below uses `RelayCommand` from `CommunityToolkit.Mvvm.Input`. Replace it with any `ICommand` implementation you prefer.

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FileDocument> Documents { get; } = new();
    
    public ICommand AddDocumentCommand { get; }
    public ICommand RemoveDocumentCommand { get; }
    
    public MainViewModel()
    {
        AddDocumentCommand = new RelayCommand(AddDocument);
        RemoveDocumentCommand = new RelayCommand<FileDocument>(RemoveDocument);
        
        // Add some initial documents
        Documents.Add(new FileDocument 
        { 
            Title = "Welcome.txt",
            Content = "Welcome to the application!"
        });
    }
    
    private void AddDocument()
    {
        Documents.Add(new FileDocument 
        { 
            Title = $"Document {Documents.Count + 1}.txt",
            Content = "New document content"
        });
    }
    
    private void RemoveDocument(FileDocument document)
    {
        Documents.Remove(document);
    }
}
```

### 3. XAML Binding

Bind the collection to `DocumentDock.ItemsSource` and define a `DocumentTemplate`:

```xaml
<DocumentDock ItemsSource="{Binding Documents}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <StackPanel Margin="10" x:DataType="Document">
        <TextBlock Text="Document Properties" FontWeight="Bold" Margin="0,0,0,10"/>
        
        <TextBlock Text="Title:" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Context.Title}" Margin="0,0,0,10"/>
        
        <TextBlock Text="Content:" FontWeight="SemiBold"/>
        <TextBox Text="{Binding Context.Content}" 
                 AcceptsReturn="True" 
                 TextWrapping="Wrap"
                 Height="200"/>
      </StackPanel>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

## Property Mapping

The system automatically maps properties from your model to `Document` properties:

| Model Property | Document Property | Description |
|----------------|-------------------|-------------|
| `Title` | `Title` | Tab title display |
| `Name` | `Title` | Alternative to Title |
| `DisplayName` | `Title` | Alternative to Title |
| `CanClose` | `CanClose` | Controls if tab can be closed |

### Example with Multiple Property Options

```csharp
public class DocumentModel
{
    // Any of these will be used for the tab title (in order of preference):
    public string Title { get; set; }        // First choice
    public string Name { get; set; }         // Second choice  
    public string DisplayName { get; set; }  // Third choice
    
    // Controls whether the tab can be closed
    public bool CanClose { get; set; } = true;
}
```

## Document Template Context

`DocumentTemplate` is built with the generated `Document` as its data context, so bind to document properties directly and use `Context` to reach the source model:

```xaml
<DocumentTemplate>
  <Grid x:DataType="Document">
    <!-- Access your model properties via Context -->
    <TextBlock Text="{Binding Context.Title}"/>
    <TextBox Text="{Binding Context.Content}"/>
    
    <!-- You can also access Document properties directly -->
    <TextBlock Text="{Binding Title}"/>
  </Grid>
</DocumentTemplate>
```

> **Compiled bindings + Context**: When using compiled bindings, `Context` is `object?` on `Document`, so `{Binding Context.SomeProperty}` can fail to compile. Rebind a subtree to the model and set a concrete `x:DataType`, or cast in the binding path:
>
> ```xaml
> <DocumentTemplate>
>   <StackPanel x:DataType="Document">
>     <StackPanel DataContext="{Binding Context}"
>                 x:DataType="models:FileDocument">
>       <TextBox Text="{Binding Content}"/>
>     </StackPanel>
>   </StackPanel>
> </DocumentTemplate>
> ```

## Common Use Cases

### File Editor

```csharp
public class FileModel : INotifyPropertyChanged
{
    public string Name { get; set; }           // Becomes tab title
    public string FilePath { get; set; }
    public string Content { get; set; }
    public bool IsDirty { get; set; }
    public bool IsReadOnly { get; set; }
    
    public bool CanClose => !IsDirty;         // Prevent closing unsaved files
    
    public ICommand SaveCommand { get; }
    public ICommand RevertCommand { get; }
}
```

### Settings Panel

```csharp
public class SettingsPage : INotifyPropertyChanged
{
    public string Title { get; set; }         // Tab title
    public string Category { get; set; }
    public string Icon { get; set; }
    public Dictionary<string, object> Settings { get; set; } = new();
    
    public bool CanClose { get; set; } = true;
}
```

### Data Viewer

```csharp
public class DataView : INotifyPropertyChanged
{
    public string DisplayName { get; set; }   // Tab title
    public ObservableCollection<object> Data { get; set; } = new();
    public string Filter { get; set; } = "";
    public bool CanClose { get; set; } = true;
    
    public ICommand RefreshCommand { get; }
    public ICommand ExportCommand { get; }
}
```

## Advanced Scenarios

## ToolDock Usage

Bind a tools collection to `ToolDock.ItemsSource` and define a `ToolTemplate`:

```xaml
<ToolDock Alignment="Left" ItemsSource="{Binding Tools}">
  <ToolDock.ToolTemplate>
    <ToolTemplate>
      <StackPanel Margin="10" x:DataType="Tool">
        <TextBlock Text="{Binding Title}" FontWeight="Bold"/>
        <StackPanel DataContext="{Binding Context}" x:DataType="models:ToolItem">
          <TextBlock Text="{Binding Description}" TextWrapping="Wrap"/>
          <TextBlock Text="{Binding Status}" Opacity="0.75"/>
        </StackPanel>
      </StackPanel>
    </ToolTemplate>
  </ToolDock.ToolTemplate>
</ToolDock>
```

### Tool Property Mapping

Generated `Tool` instances use the same property mapping strategy:

| Model Property | Tool Property | Description |
|----------------|---------------|-------------|
| `Title` | `Title` | Tool tab title |
| `Name` | `Title` | Alternative title source |
| `DisplayName` | `Title` | Alternative title source |
| `CanClose` | `CanClose` | Controls whether the tool can be closed |

### Dynamic Content Types

```csharp
public class DynamicDocument : INotifyPropertyChanged
{
    public string Title { get; set; }
    public DocumentType Type { get; set; }
    public object Data { get; set; }
    
    public bool CanClose { get; set; } = true;
}

public enum DocumentType
{
    Text,
    Image,
    Chart,
    Table
}
```

```xaml
<DocumentDock ItemsSource="{Binding Documents}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <ContentControl x:DataType="Document">
        <ContentControl.Content>
          <MultiBinding Converter="{StaticResource DocumentTypeConverter}">
            <Binding Path="Context.Type"/>
            <Binding Path="Context.Data"/>
          </MultiBinding>
        </ContentControl.Content>
      </ContentControl>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

### Custom Container Generator

Use `IDockItemContainerGenerator` when you need custom container types or custom preparation/cleanup logic for source-generated dockables.

```csharp
public sealed class MyGenerator : DockItemContainerGenerator
{
    public override IDockable? CreateDocumentContainer(IItemsSourceDock dock, object item, int index)
    {
        return new MyDocumentContainer { Id = $"Doc-{index}" };
    }

    public override void PrepareDocumentContainer(IItemsSourceDock dock, IDockable container, object item, int index)
    {
        base.PrepareDocumentContainer(dock, container, item, index);
        container.Title = $"Document {container.Title}";
    }

    public override IDockable? CreateToolContainer(IToolItemsSourceDock dock, object item, int index)
    {
        return new MyToolContainer { Id = $"Tool-{index}" };
    }
}
```

Assign the generator per dock:

```xaml
<Window.Resources>
  <local:MyGenerator x:Key="MyGenerator" />
</Window.Resources>

<ToolDock ItemsSource="{Binding Tools}"
          ToolTemplate="{StaticResource ToolTemplate}"
          ItemContainerGenerator="{StaticResource MyGenerator}" />

<DocumentDock ItemsSource="{Binding Documents}"
              DocumentTemplate="{StaticResource DocumentTemplate}"
              ItemContainerGenerator="{StaticResource MyGenerator}" />
```

`Dock.Model.Avalonia.Controls.DockItemContainerGenerator` provides the default behavior and can be subclassed, or you can implement `IDockItemContainerGenerator` from scratch.

Container compatibility contract:

- `CreateDocumentContainer` should return an `IDocument` implementation (for example `Document` or a derived type).
- `CreateToolContainer` should return an `ITool` implementation (for example `Tool` or a derived type).

Incompatible container types are skipped by the pipeline and immediately cleared.

### Custom Commands Integration

```csharp
public class CommandDocument : INotifyPropertyChanged
{
    public string Title { get; set; }
    public ICommand CloseCommand { get; }
    public ICommand SaveCommand { get; }
    
    public bool CanClose { get; set; } = true;
    
    public CommandDocument()
    {
        CloseCommand = new RelayCommand(() => {
            // Custom close logic
            var parent = GetParentCollection();
            parent?.Remove(this);
        });
        
        SaveCommand = new RelayCommand(Save);
    }
    
    private void Save()
    {
        // Save logic
    }
}
```

## Best Practices

1. **Implement INotifyPropertyChanged**: Ensures UI updates when model properties change
2. **Use ObservableCollection**: Automatically notifies the UI of collection changes
3. **Keep Models Simple**: Avoid complex UI logic in document models
4. **Meaningful Titles**: Provide clear, descriptive titles for tabs
5. **Handle CanClose**: Use this property to prevent accidental data loss
6. **Consider Commands**: Implement actions as commands for better MVVM support
7. **Data Validation**: Validate model properties to ensure data integrity

## Performance Considerations

- **Large Collections**: Consider virtualization for collections with many items
- **Frequent Updates**: Batch collection changes when possible
- **Memory Management**: Ensure proper cleanup of resources in document models
- **Template Complexity**: Keep `DocumentTemplate` and `ToolTemplate` lightweight for better performance

## Troubleshooting

### Common Issues

**Documents not appearing:**
- Ensure `ItemsSource` is properly bound
- Check that the collection implements `INotifyCollectionChanged` (use `ObservableCollection`)
- Verify `DocumentTemplate` is defined

**Title not showing:**
- Check that your model has a `Title`, `Name`, or `DisplayName` property
- Ensure the property is public and has a getter

**Templates not updating:**
- Implement `INotifyPropertyChanged` in your model
- Use proper binding syntax with `Context.PropertyName`

**Tabs not closable:**
- Set `CanClose = true` in your model (default behavior)
- Check that the property is accessible

## See Also

- [Document and Tool Content Guide](dock-content-guide.md) - Comprehensive content setup
- [Dock Advanced Topics](dock-advanced.md) - Advanced docking scenarios
- [Dock FAQ](dock-faq.md) - Common questions and troubleshooting
- [`samples/DockXamlReactiveUISample`](https://github.com/wieslawsoltes/Dock/tree/master/samples/DockXamlReactiveUISample) - ReactiveUI sample using both document and tool items sources
