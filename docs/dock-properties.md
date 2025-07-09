# Dock Properties in Control Templates

`DockProperties` is a set of attached properties provided by the `Dock.Settings` package. They expose metadata used by the drag and drop engine to detect where docking interactions can start and where dockables can be dropped. The default themes apply these properties so the layout system can find the correct visuals during a drag operation.

## Why are DockProperties required?

When a user drags a document or tool the `DockControl` walks up the visual tree to locate elements marked as drop areas or drag handles. Without these markers the engine cannot determine valid targets and dragging will not behave correctly. The built‑in styles set these properties on specific elements (for example tab headers or layout panels) but custom templates must do the same.

The available properties are:

| Property | Type | Purpose |
| -------- | ---- | ------- |
| `IsDockTarget` | `bool` | Marks a control as a surface that can show a dock adorner and accept drops. |
| `IsDragArea` | `bool` | Identifies an element that can initiate a drag when pressed. |
| `IsDropArea` | `bool` | Identifies an element that can accept dropped dockables. |
| `IsDragEnabled` | `bool` | Enables or disables dragging of dockables contained within the control. |
| `IsDropEnabled` | `bool` | Enables or disables dropping of dockables onto the control. |
| `DockAdornerHost` | `Control` | Specifies which control should display the dock adorner when this element is targeted. |

## Using the properties in control themes

Every control template that participates in docking should set the appropriate `DockProperties`. The default themes include them on tab strips, pinned panels and window chrome. When creating a custom template copy these setters so dragging continues to work:

```xaml
<Style Selector="{x:Type DocumentTabStrip}">
    <Setter Property="(DockProperties.IsDragEnabled)" Value="{Binding CanDrag}" />
    <Setter Property="(DockProperties.IsDropEnabled)" Value="{Binding CanDrop}" />
</Style>

<!-- Parts of the template that accept drops -->
<Border x:Name="PART_BorderFill"
        DockProperties.IsDropArea="True"
        DockProperties.DockAdornerHost="{Binding #PART_Border}" />

<!-- Content host where documents appear -->
<Border x:Name="PART_Border"
        DockProperties.IsDropArea="True" />

<!-- Tool dock tab strip -->
<ToolTabStrip DockProperties.IsDropArea="True"
              DockProperties.DockAdornerHost="{Binding #PART_Border}" />
```

Without the attached properties above the drag logic would not detect the border as a valid drop area and documents could not be rearranged. The content border must also be marked so docking works when the tab strip is hidden or empty.

When a drop area defines `DockAdornerHost` the dock adorner is attached to that host control and docking defaults to *Fill* when the pointer is over the drop area but outside the host. This allows empty tab strips to accept docked content just like the main content region.

## Setting DockProperties in code

The properties can be accessed through static getter and setter methods. They are normal Avalonia attached properties, so you may modify them at runtime if needed:

```csharp
// Disable dropping onto a specific control
DockProperties.SetIsDropEnabled(myControl, false);

// Query whether a control is marked as a drag handle
bool isDragArea = DockProperties.GetIsDragArea(myBorder);
```

These helpers are useful when toggling behaviour dynamically, for example to disable drag operations while a modal dialog is shown.

For global drag and drop settings see the [Dock settings](dock-settings.md) guide which demonstrates how the same properties can be attached at the window level.
