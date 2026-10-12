# Proportional StackPanel

`ProportionalStackPanel` is an Avalonia `Panel` included with Dock that sizes its children based on a `Proportion` value. The panel works horizontally or vertically and exposes attached properties to control layout behavior.

## Key features

- **Orientation** – determines whether children are arranged horizontally or vertically.
- **ProportionProperty** – attached property that specifies the size ratio for each child. Values are normalized so that all proportions add up to `1`.
- **IsCollapsedProperty** – attached property that temporarily collapses a child without removing it from the layout.
- **CollapsedProportionProperty** – attached property that remembers the last non-collapsed proportion so a panel can restore its size.
- **ProportionalStackPanelSplitter** – interactive splitter used to adjust proportions at runtime. Insert between two children.

## Basic usage

```xaml
<psp:ProportionalStackPanel Orientation="Horizontal"
                           xmlns:psp="clr-namespace:Dock.Controls.ProportionalStackPanel;assembly=Dock.Controls.ProportionalStackPanel">
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

## Custom layout hosts

A panel that implements `IProportionalSplitterHost` can contain native
`ProportionalStackPanelSplitter` controls while retaining its own layout policy.
Return the containing panel from `Panel`, expose its `Orientation`, and raise
`OrientationChanged` when the axis changes. Implement `Resize` to apply the
requested delta and minimum size to the two neighboring panes. Dock retains
pointer handling, capture cancellation, preview adorners, cursor updates and
collapsed-pane navigation. A splitter may also be inside a child content presenter.
Changing the axis or removing the splitter cancels an unfinished preview.

For native docking bounds and pointer tracking, a custom control can implement
`IDockableControl` and own a `DockableControlTracker(control, trackingMode)`.
The tracker registers the current `IDockable` data context while attached,
unregisters it on detachment or replacement, and supports later reattachment.
Dispose the tracker when replacing it. `DockableControl` uses this same service.
Custom splitter hosts retain legacy drop indicators when the detached native
preview cannot reproduce their sizing policy.
