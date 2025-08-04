# Dock XAML Guide

This guide shows how to create Dock layouts entirely in XAML.  Using XAML can be
convenient when the layout rarely changes or when you wish to edit it without
recompiling. The [DockXamlSample](../samples/DockXamlSample) demonstrates these
techniques.

## Step-by-step tutorial

These steps outline how to set up a small Dock application that defines its layout in XAML.

1. **Create a new Avalonia project**

   ```bash
   dotnet new avalonia.app -o MyDockApp
   cd MyDockApp
   ```

2. **Install the Dock packages**

   ```powershell
   dotnet add package Dock.Avalonia
   dotnet add package Dock.Model.Avalonia
   dotnet add package Dock.Avalonia.Themes.Fluent
   ```

   **Optional packages for serialization (choose one):**
   ```powershell
   dotnet add package Dock.Serializer.Newtonsoft        # JSON (Newtonsoft.Json)
   dotnet add package Dock.Serializer.SystemTextJson    # JSON (System.Text.Json)
   dotnet add package Dock.Serializer.Protobuf          # Binary
   dotnet add package Dock.Serializer.Xml               # XML
   dotnet add package Dock.Serializer.Yaml              # YAML
   ```

3. **Set up View Locator (Required)**

   Even for XAML-only layouts, you need a view locator for document and tool content. Choose one of the following approaches:

   **Option A: Static View Locator with Source Generators (Recommended)**

   Add the StaticViewLocator package:
   ```bash
   dotnet add package StaticViewLocator
   ```

   Create a `ViewLocator.cs` file:
   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;
   using StaticViewLocator;

   namespace MyDockApp;

   [StaticViewLocator]
   public partial class ViewLocator : IDataTemplate
   {
       public Control? Build(object? data)
       {
           if (data is null)
               return null;

           var type = data.GetType();
           if (s_views.TryGetValue(type, out var func))
               return func.Invoke();

           // Fallback for simple content
           if (data is string text)
               return new TextBox { Text = text, AcceptsReturn = true };

           return new TextBlock { Text = data.ToString() };
       }

       public bool Match(object? data)
       {
           return data is IDockable || data != null;
       }
   }
   ```

   **Option B: Simple View Locator for Basic Content**

   ```csharp
   using System;
   using Avalonia.Controls;
   using Avalonia.Controls.Templates;
   using Dock.Model.Core;

   namespace MyDockApp;

   public class ViewLocator : IDataTemplate
   {
       public Control? Build(object? data)
       {
           return data switch
           {
               IDockable dockable when !string.IsNullOrEmpty(dockable.Title) => 
                   new TextBox { Text = $"Content for {dockable.Title}", AcceptsReturn = true },
               string text => new TextBox { Text = text, AcceptsReturn = true },
               _ => new TextBlock { Text = data?.ToString() ?? "No Content" }
           };
       }

       public bool Match(object? data) => true;
   }
   ```

   Register the view locator in `App.axaml`:
   ```xaml
   <Application xmlns="https://github.com/avaloniaui"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                xmlns:local="using:MyDockApp"
                x:Class="MyDockApp.App">

     <Application.DataTemplates>
       <local:ViewLocator />
     </Application.DataTemplates>

     <Application.Styles>
       <FluentTheme />
       <DockFluentTheme />
     </Application.Styles>
   </Application>
   ```

4. **Declare the layout in XAML**

   **Option A: Traditional Static Layout**

   Add a `DockControl` and the initial docks in `MainWindow.axaml`:

   ```xaml
   <DockControl x:Name="Dock" InitializeLayout="True" InitializeFactory="True">
       <DockControl.Factory>
           <Factory />
       </DockControl.Factory>
       <RootDock>
           <DocumentDock>
               <Document Id="Doc1" Title="Welcome" />
           </DocumentDock>
       </RootDock>
   </DockControl>
   ```

   **Option B: ItemsSource Data Binding (Recommended)**

   For dynamic document management, use `ItemsSource` to bind to your data collections:

   ```xaml
   <DockControl InitializeLayout="True" InitializeFactory="True">
       <DockControl.Factory>
           <Factory />
       </DockControl.Factory>
       <RootDock>
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
       </RootDock>
   </DockControl>
   ```

   For this approach, you'll need a ViewModel with a document collection:

   ```csharp
   public class MainViewModel : INotifyPropertyChanged
   {
       public ObservableCollection<FileDocument> Documents { get; } = new()
       {
           new FileDocument { Title = "Document 1", Content = "Content here..." },
           new FileDocument { Title = "Document 2", Content = "More content..." }
       };
   }

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

       // INotifyPropertyChanged implementation...
   }
   ```

   > **💡 Tip**: The ItemsSource approach (Option B) provides automatic document management and is recommended for most applications. For details, see the [DocumentDock ItemsSource guide](dock-itemssource.md).

4. **Save and load layouts**

Use `DockSerializer` from code-behind to persist or restore the layout.

When declaring layouts this way you typically still provide a small
`Factory` implementation. The factory allows you to resolve your view
models and hook into runtime events while keeping the layout defined in
markup.

5. **Run the application**

   ```bash
   dotnet run
   ```

## Installing

Add the core Dock packages to your project:

```powershell
Install-Package Dock.Avalonia
Install-Package Dock.Model.Avalonia
Install-Package Dock.Serializer.Newtonsoft
Install-Package Dock.Serializer.Protobuf
Install-Package Dock.Avalonia.Themes.Fluent
```

These packages provide the `DockControl` and layout serialization helpers.

## Defining the layout

Layouts can be declared directly in your XAML files. The sample's `MainView.axaml` contains a `DockControl` that initializes the default `Factory` and loads the layout from markup:

```xaml
<DockControl x:Name="Dock" Grid.Row="1" InitializeLayout="True" InitializeFactory="True">
  <DockControl.Factory>
    <Factory />
  </DockControl.Factory>
  <RootDock x:Name="Root" Id="Root" IsCollapsable="False" DefaultDockable="{Binding #MainLayout}">
    <ProportionalDock x:Name="MainLayout" Id="MainLayout" Orientation="Horizontal">
      <ToolDock x:Name="LeftPane" Id="LeftPane" Proportion="0.25" Alignment="Left">
        <Tool x:Name="SolutionExplorer" Id="SolutionExplorer" Title="Solution Explorer" />
      </ToolDock>
      <ProportionalDockSplitter x:Name="LeftSplitter" Id="LeftSplitter" />
      <!-- Additional docks omitted -->
    </ProportionalDock>
  </RootDock>
</DockControl>
```

The hierarchy of `RootDock`, `ProportionalDock`, `ToolDock` and `DocumentDock` mirrors the structure you would build from code. Setting `InitializeLayout` and `InitializeFactory` to `True` instructs `DockControl` to create and initialize the layout automatically.

## Saving and loading layouts

`DockSerializer` can persist layouts to disk. `MainView.axaml.cs` implements simple commands for loading and saving a JSON file:

```csharp
var layout = _serializer.Load<IDock?>(stream);
if (layout is { })
{
    dock.Layout = layout;
    _dockState.Restore(layout);
}
```

Use `SaveFilePickerAsync` and `OpenFilePickerAsync` from Avalonia to choose the file location. The sample stores the current dock state so it can be restored after loading a saved layout.

## Next steps

Use the XAML sample as a template if you prefer declaring layouts in markup rather than creating them via a factory. You can combine this approach with MVVM or ReactiveUI view models for additional logic.

For an overview of all guides see the [documentation index](README.md).
