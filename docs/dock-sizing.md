# Dock Sizing Guide

This guide explains how to control the size of dockable items beyond the default proportional layout. Dock positions are normally calculated from each dockable's `Proportion` value, but you can override this behaviour with pixel based limits and by disabling splitters.

## Proportions vs fixed sizes

`ProportionalDock` arranges its children according to their `Proportion` property. This is ideal when panes should resize together. For pixel perfect layouts assign `MinWidth`, `MaxWidth`, `MinHeight` or `MaxHeight` on the dockable view model. When these limits are present the layout engine honours them before applying proportions.

Setting both the minimum and maximum to the same value effectively locks the dimension:

```csharp
var errorsTool = new ToolViewModel
{
    Id = "Errors",
    Title = "Errors",
    MinWidth = 300,
    MaxWidth = 300,
};
```

With identical values the pane cannot be resized by the user and will always occupy `300` pixels.

## Pixel sizing with limits

You can also mix proportions and absolute limits. For example a sidebar might be at least `200` pixels wide but grow with the window until it reaches `400` pixels:

```csharp
var explorer = new ToolViewModel
{
    Id = "Explorer",
    Title = "Explorer",
    MinWidth = 200,
    MaxWidth = 400,
};
```

The dock splits space proportionally and then clamps the result within the specified range.

## Disabling splitter interaction

`ProportionalDockSplitter` exposes a `CanResize` property. When `false` the user cannot drag that splitter which keeps adjacent panes at their current sizes:

```xaml
<ProportionalDockSplitter x:Name="LeftSplitter" CanResize="False" />
```

This is useful when combining fixed size panes with resizable ones or when you want to lock down a layout entirely.
Setting `ResizePreview="True"` shows only a drag indicator while moving and applies the size when released.

## Common scenarios

- **Fixed sidebar** – Set equal `MinWidth` and `MaxWidth` on the tool dock and disable the neighbouring splitter.
- **Resizable panel with limits** – Specify `MinWidth` and `MaxWidth` to constrain how small or large the panel becomes.
- **Non-resizable document area** – Place a splitter with `CanResize="False"` between the document dock and other panes.

By combining these properties you can achieve precise control over the dimensions of each dockable.

For an overview of all guides see the [documentation index](README.md).
