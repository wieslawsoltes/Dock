# Document and Tool Content Guide

This guide explains how to add actual content to your documents and tools in the Dock framework, addressing the most common questions about content setup.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Understanding Content in Dock](#understanding-content-in-dock)
- [Method 1: ItemsSource Collection Binding (Recommended)](#method-1-itemssource-collection-binding-recommended)
- [Method 2: ViewModel + DataTemplate Pattern](#method-2-viewmodel--datatemplate-pattern)
- [Method 3: Function-Based Content](#method-3-function-based-content)
- [Method 4: Direct XAML Content](#method-4-direct-xaml-content)
- [Working with Tools](#working-with-tools)
- [Common Issues and Troubleshooting](#common-issues-and-troubleshooting)
- [Complete Examples](#complete-examples)

## Prerequisites

Make sure you have the required NuGet packages installed:

```xml
<PackageReference Include="Dock.Avalonia" />
<PackageReference Include="Dock.Model.Avalonia" />
<PackageReference Include="Dock.Avalonia.Themes.Fluent" />
```

Examples below use `RelayCommand` from `CommunityToolkit.Mvvm.Input`. Use your own `ICommand` implementation if you prefer a different MVVM toolkit.

If you want MVVM-style base classes with `SetProperty`, install the matching model package (for example `Dock.Model.Mvvm`, `Dock.Model.ReactiveUI`, or `Dock.Model.CaliburMicro`) and use those `Document`/`Tool` base types in the examples.

For XAML usage, you need these namespace declarations:

```xaml
xmlns:dock="using:Dock.Model.Avalonia.Controls"
xmlns:dockCore="using:Dock.Model.Core"
```

## Understanding Content in Dock

The Dock framework supports four main approaches for defining content:

1. **ItemsSource Collection Binding**: Bind collections directly to `DocumentDock` and `ToolDock` for automatic dockable management
2. **ViewModel + DataTemplate**: Use view models and let Avalonia's data template system resolve views
3. **Function-Based**: Provide a function that creates the content control
4. **Direct XAML**: Define content directly in XAML (XAML-only approach)

## Method 1: ItemsSource Collection Binding (Recommended)

`DocumentDock` and `ToolDock` support automatic dockable creation from collections using the `ItemsSource` property, similar to how `ItemsControl` works in Avalonia. This is the recommended approach for most scenarios and is available in the Avalonia model (`Dock.Model.Avalonia.Controls`).

### When to Use This Approach

This is ideal when you:
- Have a collection of domain objects that should each become a document
- Have a collection of domain objects that should each become a tool pane
- Want automatic document creation/removal based on collection changes
- Prefer declarative XAML setup over manual document management
- Need to bind to existing business objects without creating wrapper ViewModels

### Step 1: Define Your Document Model

```csharp
public class MyDocumentModel : INotifyPropertyChanged
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

    // INotifyPropertyChanged implementation...
    public event PropertyChangedEventHandler? PropertyChanged;
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
```

### Step 2: Set Up Your ViewModel with a Collection

The examples below use `RelayCommand` from `CommunityToolkit.Mvvm.Input`. Replace it with any `ICommand` implementation you prefer.

```csharp
public class MainViewModel : INotifyPropertyChanged
{
    public ObservableCollection<MyDocumentModel> Documents { get; } = new();

    public ICommand AddDocumentCommand { get; }
    public ICommand RemoveDocumentCommand { get; }
    public ICommand ClearAllCommand { get; }

    public MainViewModel()
    {
        // Add some initial documents
        Documents.Add(new MyDocumentModel 
        { 
            Title = "Welcome", 
            Content = "Welcome to the ItemsSource example!" 
        });
        Documents.Add(new MyDocumentModel 
        { 
            Title = "Documentation", 
            Content = "This demonstrates automatic document creation from a collection." 
        });

        AddDocumentCommand = new RelayCommand(AddNewDocument);
        RemoveDocumentCommand = new RelayCommand(RemoveLastDocument, () => Documents.Count > 0);
        ClearAllCommand = new RelayCommand(ClearAllDocuments, () => Documents.Count > 0);
    }

    private void AddNewDocument()
    {
        Documents.Add(new MyDocumentModel
        {
            Title = $"Document {Documents.Count + 1}",
            Content = $"This is document number {Documents.Count + 1}",
            CanClose = true
        });
    }

    private void RemoveLastDocument()
    {
        if (Documents.Count > 0)
            Documents.RemoveAt(Documents.Count - 1);
    }

    private void ClearAllDocuments()
    {
        Documents.Clear();
    }
}
```

### Step 3: Bind to DocumentDock in XAML

```xaml
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:local="using:YourApp"
             x:Class="YourApp.ItemsSourceExample">

  <UserControl.DataContext>
    <local:MainViewModel />
  </UserControl.DataContext>

  <Grid RowDefinitions="Auto,*">
    <!-- Controls to add/remove items -->
    <StackPanel Grid.Row="0" Orientation="Horizontal" Margin="5">
      <Button Content="Add Document" Command="{Binding AddDocumentCommand}" Margin="0,0,5,0" />
      <Button Content="Remove Document" Command="{Binding RemoveDocumentCommand}" Margin="0,0,5,0" />
      <Button Content="Clear All" Command="{Binding ClearAllCommand}" />
    </StackPanel>

    <DockControl Grid.Row="1" InitializeLayout="True" InitializeFactory="True">
      <DockControl.Factory>
        <Factory />
      </DockControl.Factory>

      <RootDock Id="Root" IsCollapsable="False">
        <DocumentDock Id="DocumentsPane" 
                      CanCreateDocument="True"
                      ItemsSource="{Binding Documents}">
          
          <!-- Define how each document should be displayed -->
          <DocumentDock.DocumentTemplate>
            <DocumentTemplate>
              <StackPanel Margin="10" x:DataType="Document">
                <TextBlock Text="Document Title:" FontWeight="Bold"/>
                <TextBox Text="{Binding Title}" Margin="0,0,0,10"/>
                <TextBlock Text="Content:" FontWeight="Bold"/>
                <TextBox Text="{Binding Context.Content}" AcceptsReturn="True" Height="200" TextWrapping="Wrap"/>
              </StackPanel>
            </DocumentTemplate>
          </DocumentDock.DocumentTemplate>
          
        </DocumentDock>
      </RootDock>
    </DockControl>
  </Grid>
</UserControl>
```

### Step 4: Bind to ToolDock in XAML

```xaml
<ToolDock Id="ToolsPane" Alignment="Left" ItemsSource="{Binding Tools}">
  <ToolDock.ToolTemplate>
    <ToolTemplate>
      <StackPanel Margin="10" x:DataType="Tool">
        <TextBlock Text="{Binding Title}" FontWeight="Bold"/>
        <StackPanel DataContext="{Binding Context}" x:DataType="local:ToolModel">
          <TextBlock Text="{Binding Description}" TextWrapping="Wrap"/>
          <TextBlock Text="{Binding Status}" Opacity="0.75"/>
        </StackPanel>
      </StackPanel>
    </ToolTemplate>
  </ToolDock.ToolTemplate>
</ToolDock>
```

### How It Works

1. **Automatic Document Creation**: Each item in the collection becomes a `Document` instance
2. **Automatic Tool Creation**: Each item in tool collections becomes a `Tool` instance
3. **Collection Change Monitoring**: Adding/removing items automatically adds/removes generated dockables
4. **Templates**: `DocumentTemplate` and `ToolTemplate` create the visual content for generated dockables
5. **Property Mapping**: Dockable properties are automatically mapped from your model:
   - `Title` → Document title
   - `Name` → Alternative for title
   - `DisplayName` → Alternative for title  
   - `CanClose` → Whether the document can be closed
6. **Data Context**: Your model object becomes the `Context` of the created `Document`/`Tool` and is accessible via `{Binding Context.PropertyName}`

### Advanced Examples

#### Custom Title and CanClose Mapping

The system automatically looks for common property names:

```csharp
public class FileModel
{
    public string Name { get; set; } = "";        // Used for document title
    public string Path { get; set; } = "";
    public bool IsReadOnly { get; set; }
    public bool CanClose => !IsReadOnly;          // Controls if document can be closed
}
```

#### With Commands and Interactions

```xaml
<DocumentDock ItemsSource="{Binding OpenFiles}">
  <DocumentDock.DocumentTemplate>
    <DocumentTemplate>
      <Grid RowDefinitions="Auto,*,Auto" x:DataType="Document">
        <TextBlock Grid.Row="0" Text="{Binding Context.Path}" FontSize="12" Opacity="0.7"/>
        <TextBox Grid.Row="1" Text="{Binding Context.Content}" AcceptsReturn="True"/>
        <StackPanel Grid.Row="2" Orientation="Horizontal" HorizontalAlignment="Right">
          <Button Content="Save" Command="{Binding Context.SaveCommand}" Margin="5"/>
          <Button Content="Revert" Command="{Binding Context.RevertCommand}" Margin="5"/>
        </StackPanel>
      </Grid>
    </DocumentTemplate>
  </DocumentDock.DocumentTemplate>
</DocumentDock>
```

### Benefits

- **Automatic Synchronization**: Collection changes instantly reflect in the UI
- **Clean Separation**: Keep your business models separate from UI infrastructure
- **MVVM Friendly**: Natural binding to your existing ViewModels
- **Less Boilerplate**: No need to manually manage document creation/removal
- **Production Ready**: Fully implemented and tested functionality

## Method 2: ViewModel + DataTemplate Pattern

This approach follows MVVM principles and provides the best flexibility for complex scenarios.

### Step 1: Create a Document ViewModel

```csharp
using Dock.Model.Mvvm.Controls;

namespace YourApp.ViewModels.Documents;

public class TextDocumentViewModel : Document
{
    private string _text = "";
    
    public TextDocumentViewModel()
    {
        Id = Guid.NewGuid().ToString();
        Title = "New Document";
        CanClose = true;
    }

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
}
```

### Step 2: Create the View

```xaml
<!-- Views/Documents/TextDocumentView.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:YourApp.ViewModels.Documents"
             x:Class="YourApp.Views.Documents.TextDocumentView"
             x:DataType="vm:TextDocumentViewModel">

  <Grid RowDefinitions="Auto,*">
    <TextBlock Grid.Row="0" Text="{Binding Title}" FontWeight="Bold" Margin="5"/>
    <TextBox Grid.Row="1" Text="{Binding Text}" AcceptsReturn="True" Margin="5"/>
  </Grid>

</UserControl>
```

### Step 3: Register DataTemplate in App.axaml

```xaml
<Application xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:YourApp.ViewModels.Documents"
             xmlns:views="using:YourApp.Views.Documents"
             x:Class="YourApp.App">

  <Application.DataTemplates>
    <DataTemplate DataType="{x:Type vm:TextDocumentViewModel}">
      <views:TextDocumentView />
    </DataTemplate>
  </Application.DataTemplates>

</Application>
```

### Step 4: Add Documents Programmatically

```csharp
private void AddNewDocument()
{
    var existing = DocumentsPane.VisibleDockables
        ?.OfType<TextDocumentViewModel>()
        ?.FirstOrDefault(d => d.Id == "specific-id");
    
    if (existing != null)
    {
        DocumentsPane.ActiveDockable = existing;
        return;
    }

    // Create the document view model
    var document = new TextDocumentViewModel
    {
        Title = "My Document",
        Text = "Initial content"
    };

    // Add to dock
    _factory?.AddDockable(DocumentsPane, document);
    _factory?.SetActiveDockable(document);
    _factory?.SetFocusedDockable(DocumentsPane, document);
}
```

## Method 3: Function-Based Content

If you need to create views dynamically or integrate with dependency injection:

```csharp
private void AddDocumentWithFunction()
{
    var document = new Document
    {
        Id = Guid.NewGuid().ToString(),
        Title = "Function Document",
        CanClose = true,
        // Content is a function that creates the view
        Content = new Func<IServiceProvider, object>(_ => 
        {
            // You can use DI container here if needed
            var view = new TextDocumentView();
            view.DataContext = new TextDocumentViewModel { Text = "Hello World" };
            return view;
        })
    };

    _factory?.AddDockable(DocumentsPane, document);
    _factory?.SetActiveDockable(document);
}
```

## Method 4: Direct XAML Content

For simple static content, you can define it directly in XAML:

```xaml
<dock:DocumentDock x:Name="DocumentsPane" Id="DocumentsPane">
  
  <dock:Document Id="WelcomeDoc" Title="Welcome" CanClose="false">
    <StackPanel Margin="10">
      <TextBlock Text="Welcome to the Application" 
                 FontSize="18" FontWeight="Bold" Margin="0,0,0,10"/>
      <TextBlock Text="This is a welcome document with static content."
                 TextWrapping="Wrap"/>
      <Button Content="Get Started" Margin="0,10,0,0"/>
    </StackPanel>
  </dock:Document>
  
  <dock:Document Id="SettingsDoc" Title="Settings" CanClose="true">
    <ScrollViewer>
      <StackPanel Margin="10">
        <TextBlock Text="Application Settings" FontWeight="Bold" Margin="0,0,0,10"/>
        <CheckBox Content="Enable notifications" Margin="0,5"/>
        <CheckBox Content="Auto-save documents" Margin="0,5"/>
        <CheckBox Content="Dark theme" Margin="0,5"/>
      </StackPanel>
    </ScrollViewer>
  </dock:Document>
  
</dock:DocumentDock>
```

## Working with Tools

Tools work similarly to documents. Here's an example using MVVM base classes:

### Tool ViewModel

```csharp
using Dock.Model.Mvvm.Controls;

namespace YourApp.ViewModels.Tools;

public class PropertiesToolViewModel : Tool
{
    private object? _selectedObject;
    
    public PropertiesToolViewModel()
    {
        Id = "PropertiesTool";
        Title = "Properties";
        CanClose = false;
    }

    public object? SelectedObject
    {
        get => _selectedObject;
        set => SetProperty(ref _selectedObject, value);
    }
}
```

### Tool View

```xaml
<!-- Views/Tools/PropertiesToolView.axaml -->
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             xmlns:vm="using:YourApp.ViewModels.Tools"
             x:Class="YourApp.Views.Tools.PropertiesToolView"
             x:DataType="vm:PropertiesToolViewModel">

  <Grid>
    <TextBlock Text="Properties Panel" VerticalAlignment="Center" 
               HorizontalAlignment="Center" FontStyle="Italic"
               IsVisible="{Binding SelectedObject, Converter={x:Static ObjectConverters.IsNull}}"/>
    
    <ScrollViewer IsVisible="{Binding SelectedObject, Converter={x:Static ObjectConverters.IsNotNull}}">
      <StackPanel Margin="5">
        <TextBlock Text="Selected Object Properties" FontWeight="Bold" Margin="0,0,0,10"/>
        <!-- Add property editors here -->
      </StackPanel>
    </ScrollViewer>
  </Grid>

</UserControl>
```

### Register Tool DataTemplate

```xaml
<Application.DataTemplates>
  <DataTemplate DataType="{x:Type vm:PropertiesToolViewModel}">
    <views:PropertiesToolView />
  </DataTemplate>
</Application.DataTemplates>
```

## Common Issues and Troubleshooting

### Issue: "Unexpected content" Error

**Problem**: Getting `System.ArgumentException: "Unexpected content ..."` when adding documents.

**Cause**: Setting `Content` to a view model or other unsupported object without a template. `Content` must be a `Control`, a template/function (`Func<IServiceProvider, object>`), or template content produced by XAML.

**Solution**: Use one of the supported approaches above (Control/Function/DataTemplate).

```csharp
// ❌ Unsupported: view model instance without a template
var document = new Document
{
    Content = new MyViewModel()
};

// ✅ Use ViewModel + DataTemplate approach instead
var document = new MyDocumentViewModel();

// ✅ Or provide a Control or factory
var document = new Document
{
    Content = new Func<IServiceProvider, object>(_ => new MyUserControl())
};

// ✅ Direct Control assignment is valid
var document = new Document
{
    Content = new MyUserControl()
};
```

### Issue: Empty/Blank Document Tabs with ItemsSource

**Problem**: Document tabs show up but content is empty when using ItemsSource.

**Solutions**:
1. Ensure `DocumentTemplate` has proper `x:DataType="Document"` on the root element
2. Access your model properties via `{Binding Context.PropertyName}` not `{Binding PropertyName}`
3. Verify your model implements `INotifyPropertyChanged`
4. Check that your collection items have the expected property names (Title, Name, etc.)

**Example Fix**:
```xaml
<!-- ❌ Wrong DataType -->
<DocumentTemplate x:DataType="local:MyModel">
  <TextBlock Text="{Binding Title}"/>
</DocumentTemplate>

<!-- ✅ Correct DataType -->
<DocumentTemplate>
  <StackPanel x:DataType="Document">
    <TextBlock Text="{Binding Title}"/>
    <TextBlock Text="{Binding Context.Content}"/>
  </StackPanel>
</DocumentTemplate>
```

### Issue: Compiled binding errors for `Context.*`

**Problem**: Build errors like "Unable to resolve property or method ... on type 'Document'" when using `{Binding Context.SomeProperty}`.

**Cause**: `Document.Context` is typed `object?`, so compiled bindings cannot infer model properties from `Context`.

**Solution**: Rebind a subtree to the model and set `x:DataType`, or cast in the binding path (or disable compiled bindings for that subtree).

```xaml
<DocumentTemplate>
  <StackPanel x:DataType="Document">
    <StackPanel DataContext="{Binding Context}"
                x:DataType="vm:MyDocumentModel">
      <TextBox Text="{Binding Content}"/>
    </StackPanel>
  </StackPanel>
</DocumentTemplate>
```

### Issue: Empty/Blank Document Tabs with DataTemplates

**Problem**: Document tabs show up but content is empty when using ViewModel approach.

**Solutions**:
1. Check that your DataTemplate is registered in `App.axaml`
2. Verify the DataType in your DataTemplate matches your ViewModel type exactly
3. Ensure the view's `x:DataType` matches the ViewModel type
4. Check that the ViewModel namespace is correctly imported

### Issue: Missing Dock Types in XAML

**Problem**: `Unable to resolve type RootDock from namespace https://github.com/avaloniaui`

**Solution**: Add the `Dock.Model.Avalonia` package and namespace:

```xaml
xmlns:dock="using:Dock.Model.Avalonia.Controls"
```

### Issue: ItemsSource Documents Not Updating

**Problem**: Changes to your model properties don't reflect in the document content.

**Solution**: Ensure your model implements `INotifyPropertyChanged`:

```csharp
public class MyDocumentModel : INotifyPropertyChanged
{
    private string _title = "";
    
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
    
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
}
```

## Complete Examples

### File Manager using ItemsSource (Recommended)

This example shows a complete file manager implementation using the ItemsSource approach:

```csharp
// File Model
public class FileDocument : INotifyPropertyChanged
{
    private string _title = "";
    private string _content = "";
    private string _filePath = "";
    private bool _isModified;

    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string Content
    {
        get => _content;
        set 
        {
            if (SetProperty(ref _content, value))
            {
                IsModified = true;
            }
        }
    }

    public string FilePath
    {
        get => _filePath;
        set => SetProperty(ref _filePath, value);
    }

    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    public bool CanClose => !IsModified || ConfirmClose();

    public ICommand SaveCommand { get; }
    public ICommand SaveAsCommand { get; }

    public FileDocument()
    {
        SaveCommand = new RelayCommand(Save, () => IsModified);
        SaveAsCommand = new RelayCommand(SaveAs);
    }

    private void Save()
    {
        // Save to FilePath
        File.WriteAllText(FilePath, Content);
        IsModified = false;
    }

    private void SaveAs()
    {
        // Show save dialog and save
    }

    private bool ConfirmClose()
    {
        // Show "Save changes?" dialog
        return true; // or false based on user choice
    }

    // INotifyPropertyChanged implementation...
}

// Main ViewModel
public class FileManagerViewModel : INotifyPropertyChanged
{
    public ObservableCollection<FileDocument> OpenFiles { get; } = new();

    public ICommand OpenFileCommand { get; }
    public ICommand NewFileCommand { get; }

    public FileManagerViewModel()
    {
        OpenFileCommand = new RelayCommand(OpenFile);
        NewFileCommand = new RelayCommand(NewFile);
    }

    private void NewFile()
    {
        OpenFiles.Add(new FileDocument
        {
            Title = $"Untitled{OpenFiles.Count + 1}.txt",
            Content = "",
            FilePath = ""
        });
    }

    private void OpenFile()
    {
        // Show file dialog and load file
        var filePath = ShowOpenFileDialog();
        if (!string.IsNullOrEmpty(filePath))
        {
            var content = File.ReadAllText(filePath);
            OpenFiles.Add(new FileDocument
            {
                Title = Path.GetFileName(filePath),
                Content = content,
                FilePath = filePath
            });
        }
    }
}
```

**XAML:**
```xaml
<DockControl>
  <DockControl.Factory>
    <Factory />
  </DockControl.Factory>
  
  <RootDock>
    <DocumentDock ItemsSource="{Binding OpenFiles}">
      <DocumentDock.DocumentTemplate>
        <DocumentTemplate>
          <Grid RowDefinitions="Auto,*,Auto" x:DataType="Document">
            <!-- File path header -->
            <TextBlock Grid.Row="0" Text="{Binding Context.FilePath}" 
                       FontSize="10" Opacity="0.7" Margin="5"/>
            
            <!-- Main content editor -->
            <TextBox Grid.Row="1" Text="{Binding Context.Content}" 
                     AcceptsReturn="True" AcceptsTab="True"
                     FontFamily="Consolas" Margin="5"/>
            
            <!-- Action buttons -->
            <StackPanel Grid.Row="2" Orientation="Horizontal" 
                        HorizontalAlignment="Right" Margin="5">
              <Button Content="Save" Command="{Binding Context.SaveCommand}"
                      IsEnabled="{Binding Context.IsModified}"/>
              <Button Content="Save As" Command="{Binding Context.SaveAsCommand}"
                      Margin="5,0,0,0"/>
            </StackPanel>
          </Grid>
        </DocumentTemplate>
      </DocumentDock.DocumentTemplate>
    </DocumentDock>
  </RootDock>
</DockControl>
```

### Simple Text Editor Document (ViewModel Approach)

```csharp
// ViewModel
public class TextEditorViewModel : Document
{
    private string _content = "";
    private bool _isModified;

    public string Content
    {
        get => _content;
        set 
        { 
            if (SetProperty(ref _content, value))
            {
                IsModified = true;
                OnPropertyChanged(nameof(DisplayTitle));
            }
        }
    }

    public bool IsModified
    {
        get => _isModified;
        set => SetProperty(ref _isModified, value);
    }

    public string DisplayTitle => IsModified ? $"{Title}*" : Title;

    public void Save()
    {
        // Save logic here
        IsModified = false;
        OnPropertyChanged(nameof(DisplayTitle));
    }
}
```

### Properties Tool with Object Inspector

```csharp
// Tool ViewModel
public class ObjectInspectorViewModel : Tool
{
    private object? _target;
    private PropertyInfo[]? _properties;

    public object? Target
    {
        get => _target;
        set
        {
            if (SetProperty(ref _target, value))
            {
                Properties = value?.GetType().GetProperties();
            }
        }
    }

    public PropertyInfo[]? Properties
    {
        get => _properties;
        set => SetProperty(ref _properties, value);
    }
}
```

This comprehensive guide should help users understand how to properly work with document and tool content in the Dock framework, with the new ItemsSource functionality providing the most streamlined approach for most scenarios. 
