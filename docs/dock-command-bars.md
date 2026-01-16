# Command Bars and Merging

Dock can merge command bars from the active dockable into a host control at the top of `DockControl`. This is implemented by `DockCommandBarHost` and `DockCommandBarManager` and is controlled by `DockSettings.CommandBarMergingEnabled`.

## How it works

- `DockControl` templates include a `DockCommandBarHost` named `PART_CommandBarHost`.
- `DockCommandBarManager` listens for active dockable changes and queries the active dockable (or its `Context`) for command bar definitions.
- Definitions are merged with any base bars and rendered using the default adapter into menu bars, tool bars, and ribbon bars.

## Enable command bar merging

Enable merging globally and choose which dockable contributes bars:

```csharp
DockSettings.CommandBarMergingEnabled = true;
DockSettings.CommandBarMergingScope = DockCommandBarMergingScope.ActiveDocument;
```

`DockCommandBarMergingScope.ActiveDocument` uses the active document only, while `ActiveDockable` includes tools.

## Provide command bars from a dockable

Implement `IDockCommandBarProvider` on a document/tool (or on its `Context`) and return `DockCommandBarDefinition` instances. The example below uses `RelayCommand` from `CommunityToolkit.Mvvm.Input`.

```csharp
using System;
using System.Collections.Generic;
using System.Windows.Input;
using Dock.Model.CommandBars;
using Dock.Model.Mvvm.Controls;

public class EditorDocument : Document, IDockCommandBarProvider
{
    public event EventHandler? CommandBarsChanged;

    public ICommand SaveCommand { get; } = new RelayCommand(() => { /* save */ });

    public IReadOnlyList<DockCommandBarDefinition> GetCommandBars()
    {
        return new[]
        {
            new DockCommandBarDefinition("EditorMenu", DockCommandBarKind.Menu)
            {
                MergeMode = DockCommandBarMergeMode.Append,
                Items = new[]
                {
                    new DockCommandBarItem("_File")
                    {
                        Items = new[]
                        {
                            new DockCommandBarItem("_Save") { Command = SaveCommand },
                            new DockCommandBarItem(null) { IsSeparator = true },
                            new DockCommandBarItem("E_xit")
                        }
                    }
                }
            }
        };
    }

    private void RaiseBarsChanged() => CommandBarsChanged?.Invoke(this, EventArgs.Empty);
}
```

## Merge modes and ordering

`DockCommandBarDefinition.MergeMode` controls how bars are combined:

- `Replace`: Replace base bars of the same kind.
- `Append`: Append the bar to the merged list.
- `MergeByGroup`: Merge items by `GroupId` and `Order`.

Each bar and item has `Order` and optional `GroupId` to control placement.

## Base command bars

`DockCommandBarHost.BaseCommandBars` can supply app-level bars that always appear. This is typically configured in a custom `DockControl` template or a theme. Bars from active dockables are merged with these base definitions.

## Definitions and items

`DockCommandBarDefinition` describes a bar and `DockCommandBarItem` describes items within that bar:

### DockCommandBarDefinition

| Property | Type | Description |
| --- | --- | --- |
| `Id` | `string` | Identifier used for merges. |
| `Kind` | `DockCommandBarKind` | Menu, ToolBar, or Ribbon. |
| `MergeMode` | `DockCommandBarMergeMode` | Replace, Append, or MergeByGroup. |
| `Order` | `int` | Sort order among bars. |
| `GroupId` | `string?` | Group identifier used for `MergeByGroup`. |
| `Items` | `IReadOnlyList<DockCommandBarItem>?` | Bar items. |
| `Content` | `object?` | Custom content rendered by the adapter. |

When `Content` is set to a `Control`, the default adapter uses it directly. If `Content` is set and `Items` is empty, the adapter wraps it in a `ContentControl`.

### DockCommandBarItem

| Property | Type | Description |
| --- | --- | --- |
| `Header` | `object?` | Menu header or button text. |
| `Icon` | `object?` | Icon content for menus/toolbars. |
| `Command` | `ICommand?` | Command to execute. |
| `CommandParameter` | `object?` | Command parameter passed to `Command`. |
| `Items` | `IReadOnlyList<DockCommandBarItem>?` | Child items for submenus. |
| `GroupId` | `string?` | Group identifier used for `MergeByGroup`. |
| `Order` | `int` | Order within the bar or group. |
| `IsSeparator` | `bool` | Render as a separator. |

Example using `Content` for a toolbar host:

```csharp
var bar = new DockCommandBarDefinition("EditorRibbon", DockCommandBarKind.Ribbon)
{
    Content = new EditorRibbonControl()
};
```

## Host properties

`DockCommandBarHost` exposes the merged controls for each command bar kind:

- `MenuBars` - List of menu bar controls created by the adapter.
- `ToolBars` - List of tool bar controls created by the adapter.
- `RibbonBars` - List of ribbon bar controls created by the adapter.

The default template renders these lists with three `ItemsControl` instances. If you create a custom theme you can reorder or wrap them however you like:

```xaml
<ControlTheme x:Key="{x:Type controls:DockCommandBarHost}" TargetType="controls:DockCommandBarHost">
  <Setter Property="Template">
    <ControlTemplate>
      <DockPanel>
        <ItemsControl DockPanel.Dock="Top"
                      ItemsSource="{TemplateBinding MenuBars}" />
        <ItemsControl DockPanel.Dock="Top"
                      ItemsSource="{TemplateBinding ToolBars}" />
        <ItemsControl ItemsSource="{TemplateBinding RibbonBars}" />
      </DockPanel>
    </ControlTemplate>
  </Setter>
</ControlTheme>
```

## Custom adapters

The default adapter renders:

- `DockCommandBarKind.Menu` as `Menu`
- `DockCommandBarKind.ToolBar` as a horizontal `StackPanel` of `Button` controls
- `DockCommandBarKind.Ribbon` as a horizontal `StackPanel` of `ContentControl` items

To render your own controls, implement `IDockCommandBarAdapter` and use it in a custom host control or a forked `DockControl` that constructs a `DockCommandBarManager` with your adapter.

For related settings see [Dock settings](dock-settings.md).
