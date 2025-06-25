# Dock XAML Guide

This guide shows how to create Dock layouts entirely in XAML. The [DockXamlSample](../samples/DockXamlSample) demonstrates these techniques.

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
   dotnet add package Dock.Serializer
   ```

3. **Declare the layout in XAML**

   Add a `DockControl` and the initial docks in `MainWindow.axaml`.

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

4. **Save and load layouts**

   Use `DockSerializer` from code-behind to persist or restore the layout.

5. **Run the application**

   ```bash
   dotnet run
   ```

## Installing

Add the core Dock packages to your project:

```powershell
Install-Package Dock.Avalonia
Install-Package Dock.Model.Avalonia
Install-Package Dock.Serializer
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
