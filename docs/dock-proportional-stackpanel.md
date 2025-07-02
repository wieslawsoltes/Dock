# Proportional StackPanel

`ProportionalStackPanel` is an Avalonia `Panel` included with Dock that sizes its children based on a `Proportion` value. The panel works horizontally or vertically and exposes attached properties to control layout behaviour.

## Key features

- **Orientation** – determines whether children are arranged horizontally or vertically.
- **ProportionProperty** – attached property that specifies the size ratio for each child. Values are normalized so that all proportions add up to `1`.
- **IsCollapsedProperty** – attached property that temporarily collapses a child without removing it from the layout.
- **ProportionalStackPanelSplitter** – interactive splitter used to adjust proportions at runtime. Insert between two children.

## Basic usage

```xaml
<psp:ProportionalStackPanel Orientation="Horizontal"
                           xmlns:psp="clr-namespace:Dock.Controls.ProportionalStackPanel">
  <Border psp:ProportionalStackPanel.Proportion="0.3" Background="Red"/>
  <psp:ProportionalStackPanelSplitter />
  <Border psp:ProportionalStackPanel.Proportion="0.7" Background="Green"/>
</psp:ProportionalStackPanel>
```

The example creates a horizontal panel with two regions separated by a splitter. Dragging the splitter resizes the regions while keeping their proportions within the available space.

`IsCollapsed` can be toggled to hide a region and redistribute its proportion among the remaining children.

## Notes

The control is used internally by Dock to implement `ProportionalDock` but can also be used in regular Avalonia views. See the unit tests under `tests/Dock.Avalonia.UnitTests` for additional examples.

For an overview of all guides see the [documentation index](README.md).
