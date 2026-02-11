# Dockable Property Settings

Dockable items such as documents, tools and docks implement the `IDockable` interface. This interface exposes a number of properties that control how each item behaves at runtime. Most of these flags can be set in XAML or on your view models.

## Available properties

| Property | Description |
| --- | --- |
| `Id` | Unique identifier used by the serializer and factory helpers. |
| `Title` | Text shown on tabs and windows. |
| `Context` | Optional data context associated with the dockable. |
| `Owner` | The dock or window currently hosting the item. |
| `OriginalOwner` | Where the dockable was first created. Used when restoring pinned tools. |
| `Factory` | Factory instance used to create and manage the layout. |
| `IsEmpty` | Indicates a placeholder dockable with no content. |
| `IsCollapsable` | When `false`, the dock will remain even if it contains no children. |
| `Proportion` | Size ratio used by `ProportionalDock`. |
| `Dock` | The default docking mode for this dockable (`DockMode`). |
| `Column` | Grid column index when hosted in a grid dock. |
| `Row` | Grid row index when hosted in a grid dock. |
| `ColumnSpan` | Grid column span when hosted in a grid dock. |
| `RowSpan` | Grid row span when hosted in a grid dock. |
| `IsSharedSizeScope` | Participates in shared size scope when hosted in a grid dock. |
| `CollapsedProportion` | Last known proportion before a dock is collapsed. |
| `MinWidth` | Optional minimum width. Overrides the current proportion if larger. |
| `MaxWidth` | Optional maximum width. Overrides the proportion if smaller. |
| `MinHeight` | Optional minimum height. Overrides the current proportion if larger. |
| `MaxHeight` | Optional maximum height. Overrides the proportion if smaller. |
| `DockingState` | Logical docking state (`DockingWindowState` flags) such as `Docked`, `Pinned`, `Document`, optionally combined with `Floating` and `Hidden`. When a parent dock is hidden, descendants also include `Hidden`. |
| `CanClose` | Whether the user can close the dockable via UI commands. |
| `CanPin` | Allows pinning and unpinning of tools. |
| `KeepPinnedDockableVisible` | Keeps pinned previews visible instead of auto-hiding. |
| `PinnedDockDisplayModeOverride` | Optional override for how pinned previews are displayed (`Overlay` or `Inline`). When `null`, the root dock setting is used. |
| `PinnedBounds` | Optional stored bounds for pinned previews. Used to persist inline/overlay preview sizes. |
| `CanFloat` | Controls if the item may be detached into a floating window. |
| `CanDrag` | Enables dragging the dockable to another position. |
| `CanDrop` | Determines if other dockables can be dropped onto this one. |
| `CanDockAsDocument` | Controls whether the dockable can be docked as a tabbed document. |
| `DockCapabilityOverrides` | Optional per-dockable capability overrides (`bool?` values). Highest-precedence layer in capability resolution. |
| `IsModified` | Marks a dockable as having unsaved changes. |
| `DockGroup` | Group identifier that restricts which dockables can dock together. See [Docking Groups](dock-docking-groups.md). |
| `AllowedDockOperations` | Allowed docking operations when the dockable is dragged (`DockOperationMask`). |
| `AllowedDropOperations` | Allowed docking operations when the dockable is the drop target (`DockOperationMask`). |

## Sample usage

The properties can be configured directly on your view models when creating the layout:

```csharp
var errorsTool = new ToolViewModel
{
    Id = "Errors",
    Title = "Errors",
    CanDrag = false,
    CanFloat = false,
    CanClose = false
};
```

In XAML you set them as attributes:

```xaml
<Tool x:Name="SolutionExplorer"
      Id="SolutionExplorer"
      Title="Solution Explorer"
      CanPin="False"
      CanFloat="True" />
```

`DockingState` is maintained by factory operations (`PinDockable`, `UnpinDockable`, `FloatDockable`, `HideDockable`, `RestoreDockable`) and can be used by UI bindings for diagnostics or state-aware visuals.

Capability values can also be governed by policy layers at root and dock level. See [Capability policies and overrides](dock-capability-policies.md).

## Window State Mixin

`Document` and `Tool` models implement `IDockingWindowState`, which adds bindable runtime properties:

- `IsOpen`: `true` when the item is currently present in a visible, pinned, or previewable layout area.
- `IsSelected`: `true` when the item is the `ActiveDockable` of its owner dock.
- `IsActive`: `true` when the item is focused in the active root.
- `DockingState`: the logical location flags (`Docked`, `Pinned`, `Document`, `Floating`, `Hidden`).

Changes are synchronized both ways:

- Layout -> VM: factory operations update these properties.
- VM -> layout: setting `IsOpen`, `IsSelected`, `IsActive`, or `DockingState` requests the corresponding layout operation.

`IsOpen`, `IsSelected`, and `IsActive` are runtime state and are not serialized. `DockingState` remains part of persisted layout state.

Global drag and drop behavior can be toggled using the attached properties from [`Dock.Settings`](dock-settings.md):

```xaml
<Window xmlns:dockSettings="clr-namespace:Dock.Settings;assembly=Dock.Settings"
        dockSettings:DockProperties.IsDragEnabled="False"
        dockSettings:DockProperties.IsDropEnabled="False">
    <DockControl />
</Window>
```

For reference, the FAQ shows how these properties interact with the default templates.
