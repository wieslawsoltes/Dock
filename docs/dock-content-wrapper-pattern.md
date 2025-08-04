# Advanced Content Wrapper Pattern

This guide covers an advanced pattern for wrapping your own domain models with Dock's document system. This approach is useful when you want to maintain separation between your application's document model and Dock's infrastructure.

> **⚠️ Important**: With the introduction of `DocumentDock.ItemsSource`, most scenarios that required this wrapper pattern can now be solved more elegantly. See [Method 1: ItemsSource Collection Binding](dock-content-guide.md#method-1-itemssource-collection-binding-recommended) in the content guide for the recommended approach.

> **Note**: This is an advanced pattern. For most use cases, the standard [Document and Tool Content Guide](dock-content-guide.md) approaches are recommended. **Consider ItemsSource first** - it provides the same domain separation with much less code.

## Modern Approach: ItemsSource (Recommended)

Before implementing the wrapper pattern below, consider using `ItemsSource` which provides automatic domain model binding:

```xml
<!-- Modern approach with ItemsSource -->
<DocumentDock ItemsSource="{Binding SceneDocuments}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <views:SceneView DataContext="{Binding Context}" x:DataType="Document"/>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

With a simple domain model:
```csharp
public class SceneDocument : INotifyPropertyChanged
{
    public string Title { get; set; } = "Untitled Scene";  // Automatically used for tab title
    public List<SceneObject> Objects { get; set; } = new();
    // ... rest of your domain model
}

// In your ViewModel
public ObservableCollection<SceneDocument> SceneDocuments { get; } = new();
```

This achieves the same domain separation goals with much less code and automatic collection synchronization.

## When You Still Need the Wrapper Pattern

The wrapper pattern below may still be useful for:

1. **Legacy Integration**: Existing complex document systems that can't be easily adapted to ItemsSource
2. **Dynamic Content Types**: When you need to dynamically switch between completely different content types within documents
3. **Advanced Serialization**: Complex layout persistence scenarios requiring custom document handling

## The Wrapper Pattern

Instead of inheriting directly from Dock's `Document` class, you can create a wrapper pattern that separates your domain model from the docking infrastructure:

### Step 1: Define Your Domain Model

```csharp
// Your application's document model
public abstract class AppDocument
{
    public string Name { get; set; } = "";
    public object? Content { get; set; }
}

public class SceneDocument : AppDocument
{
    public SceneDocument()
    {
        Name = "Untitled Scene";
    }
    
    // Scene-specific properties
    public List<SceneObject> Objects { get; set; } = new();
}
```

### Step 2: Create the View

```xml
<!-- Views/SceneView.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:models="using:YourApp.Models"
             x:Class="YourApp.Views.SceneView"
             x:DataType="models:SceneDocument">

  <Design.DataContext>
    <models:SceneDocument />
  </Design.DataContext>

  <Grid RowDefinitions="Auto,*">
    <TextBlock Grid.Row="0" Text="{Binding Name}" FontWeight="Bold" Margin="5"/>
    <Border Grid.Row="1" Background="LightGray" Margin="5">
      <TextBlock Text="Scene viewport here" 
                 HorizontalAlignment="Center" 
                 VerticalAlignment="Center"/>
    </Border>
  </Grid>

</UserControl>
```

### Step 3: Set Up Document Template

```xml
<!-- In your MainView.axaml or wherever you define the DocumentDock -->
<dock:DocumentDock x:Name="DocumentsPane" Id="DocumentsPane">
  
  <dock:DocumentDock.DocumentTemplate>
    <dock:DocumentTemplate x:DataType="dock:Document">
      <!-- Use ContentControl to display the wrapped content -->
      <ContentControl DataContext="{Binding Context}" 
                      x:DataType="models:AppDocument"
                      Content="{Binding Content}" />
    </dock:DocumentTemplate>
  </dock:DocumentDock.DocumentTemplate>

</dock:DocumentDock>
```

### Step 4: Register DataTemplate for Your Domain Model

```xml
<!-- In App.axaml -->
<Application.DataTemplates>
  <DataTemplate DataType="{x:Type models:SceneDocument}">
    <views:SceneView />
  </DataTemplate>
</Application.DataTemplates>
```

### Step 5: Create and Add Documents

```csharp
private void AddSceneDocument()
{
    // Create your domain model
    var sceneDoc = new SceneDocument
    {
        Name = "New Scene",
        Content = new SceneView() // Set the view as content
    };

    // Wrap it in a Dock Document
    var dockDoc = new Dock.Model.Avalonia.Controls.Document
    {
        Id = Guid.NewGuid().ToString(),
        Title = sceneDoc.Name,
        Context = sceneDoc, // Your domain model becomes the Context
        Content = DocumentsPane.DocumentTemplate?.Content // Use the template
    };

    DocumentsPane.AddDocument(dockDoc);
    DocumentsPane.ActiveDockable = dockDoc;
}
```

## Migration from Wrapper Pattern to ItemsSource

If you're currently using the wrapper pattern and want to migrate to the simpler ItemsSource approach:

### Before (Wrapper Pattern):
```csharp
// Complex wrapper setup
var dockDoc = new Document
{
    Title = sceneDoc.Name,
    Context = sceneDoc,
    Content = DocumentsPane.DocumentTemplate?.Content
};
DocumentsPane.AddDocument(dockDoc);
```

### After (ItemsSource):
```csharp
// Simple collection manipulation
SceneDocuments.Add(sceneDoc); // Automatically creates and adds document
```

The XAML also becomes much simpler with ItemsSource since the binding and template application is handled automatically.

## Alternative: Composition Over Inheritance

A cleaner version of this pattern uses composition:

```csharp
public class DocumentWrapper : Document
{
    private AppDocument? _appDocument;
    
    public AppDocument? AppDocument
    {
        get => _appDocument;
        set
        {
            _appDocument = value;
            if (value != null)
            {
                Title = value.Name;
                Content = value.Content;
            }
        }
    }
}
```

Then use it like:

```csharp
var wrapper = new DocumentWrapper
{
    AppDocument = new SceneDocument { Name = "My Scene" }
};

DocumentsPane.AddDocument(wrapper);
```

This approach provides the same benefits but with a cleaner API and better integration with Dock's systems.

## Summary

While the wrapper pattern provides flexibility for complex scenarios, the new `ItemsSource` functionality covers the majority of use cases with significantly less complexity. Consider using `ItemsSource` first, and only fall back to the wrapper pattern for advanced scenarios that require the additional control and flexibility it provides. 