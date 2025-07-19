# Control Recycling Sample

This short guide explains the sample project under `samples/ControlRecyclingSample`.
It demonstrates how the `Dock.Controls.Recycling` library caches the visuals
associated with dockables so their state is preserved when they are closed and
reopened.

## XAML configuration

The application declares a `ControlRecycling` resource in `App.axaml` and assigns
it to `DockControl` via a style:

```xml
<Application.Resources>
  <ControlRecycling x:Key="ControlRecyclingKey" TryToUseIdAsKey="True" />
</Application.Resources>

<Application.Styles>
  <Style Selector="DockControl">
    <Setter Property="(ControlRecyclingDataTemplate.ControlRecycling)"
            Value="{StaticResource ControlRecyclingKey}" />
  </Style>
</Application.Styles>
```

`TryToUseIdAsKey="True"` lets the cache match dockables by their `Id` even if
their instances change after layout serialization.

## Running the sample

`MainWindow.axaml` contains a minimal layout with one document and one tool:

```xml
<DockControl x:Name="Dock" InitializeLayout="True" InitializeFactory="True">
  <RootDock>
    <DocumentDock>
      <Document Id="Doc1" Title="Welcome">
        <TextBox Text="Edit me" />
      </Document>
    </DocumentDock>
    <ToolDock>
      <Tool Id="Tool1" Title="Settings">
        <Slider Minimum="0" Maximum="100" Value="40" />
      </Tool>
    </ToolDock>
  </RootDock>
</DockControl>
```

Start the project with `dotnet run` and try closing and reopening the
`Document` or `Tool`. The text in the `TextBox` and value of the `Slider`
remain intact because the controls are retrieved from the recycling cache.

For a more in-depth discussion of the feature see the
[Control Recycling Guide](dock-control-recycling.md).

