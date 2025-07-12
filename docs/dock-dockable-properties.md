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
| `MinWidth` | Optional minimum width. Overrides the current proportion if larger. |
| `MaxWidth` | Optional maximum width. Overrides the proportion if smaller. |
| `MinHeight` | Optional minimum height. Overrides the current proportion if larger. |
| `MaxHeight` | Optional maximum height. Overrides the proportion if smaller. |
| `CanClose` | Whether the user can close the dockable via UI commands. |
| `CanPin` | Allows pinning and unpinning of tools. |
| `CanFloat` | Controls if the item may be detached into a floating window. |
| `CanDrag` | Enables dragging the dockable to another position. |
| `CanDrop` | Determines if other dockables can be dropped onto this one. |
| `Dock` | Preferred location for the dockable when it first opens. |

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

Global drag and drop behaviour can be toggled using the attached properties from [`Dock.Settings`](dock-settings.md):

```xml
<Window xmlns:dockSettings="clr-namespace:Dock.Settings;assembly=Dock.Settings"
        dockSettings:DockProperties.IsDragEnabled="False"
        dockSettings:DockProperties.IsDropEnabled="False">
    <DockControl />
</Window>
```

For reference, the FAQ shows how these properties interact with the default templates.
