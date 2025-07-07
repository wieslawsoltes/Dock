# Layout Panel Reference

Dock includes several dock types that arrange their children using familiar panel layouts. These complement `ProportionalDock` and are useful for static or grid based layouts.

## DockDock

`IDockDock` behaves like Avalonia's `DockPanel`. Set the `Dock` property on each
dockable to position it on the left, right, top or bottom edge. When
`LastChildFill` is `true` (the default) the final child stretches to fill any
remaining space. Dock docks are commonly used for toolbars or status bars that
surround a central document area.

```xaml
<dock:DockDock LastChildFill="True">
    <ToolDock dock:Dock="Left" />
    <DocumentDock />
</dock:DockDock>
```

## StackDock

`IStackDock` arranges its dockables in a line. The `Orientation` property chooses between horizontal and vertical layout and `Spacing` inserts a gap between the items.

```csharp
var stack = new StackDock
{
    Orientation = Orientation.Vertical,
    Spacing = 6
};
```

Stack docks work well for tool strips or areas with a fixed ordering of items.

## GridDock and GridDockSplitter

`IGridDock` exposes `ColumnDefinitions` and `RowDefinitions` strings that mirror Avalonia's `Grid` syntax. Use them to create complex grids with resizable cells. Place a `GridDockSplitter` between cells to let the user resize rows or columns at runtime.

```csharp
var grid = new GridDock
{
    ColumnDefinitions = "2*,*",
    RowDefinitions = "Auto,*"
};
```

```xaml
<GridDockSplitter ResizeDirection="Columns" />
```

## WrapDock

`IWrapDock` lays out items in sequence and wraps them to a new line when there is not enough space. The `Orientation` property defines whether wrapping occurs horizontally or vertically.

## UniformGridDock

`IUniformGridDock` divides the available space into equally sized cells. Specify `Rows` and `Columns` to control the grid dimensions. All dockables within the dock occupy a single cell.

```csharp
var uniform = new UniformGridDock { Rows = 2, Columns = 3 };
```

## When to use these panels

These dock types are best suited for static tool areas or dashboards where the layout seldom changes. They can be combined with other docks inside an `IRootDock` just like proportional docks. Refer to [Model Control Interfaces](dock-model-controls.md) for the complete list of available dock contracts.

For an overview of all guides see the [documentation index](README.md).
