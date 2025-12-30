# Dock Targets and Drop Indicators

Dock shows drop indicators using two controls: `DockTarget` (local docking) and `GlobalDockTarget` (global docking between windows). Both derive from `DockTargetBase` and are created by the docking pipeline during drag operations.

## How targets are created

When a drag starts, `DockControlState` locates a control marked with `DockProperties.IsDockTarget` and creates a target adorner for it. The adorner is hosted either in the visual tree or inside a floating window, depending on `DockSettings.UseFloatingDockAdorner`.

Local targets are attached to the drop control (or `DockProperties.DockAdornerHost` if set). Global targets are attached to the nearest `DockControl` and display global docking options when they are available.

Global targets are only shown when the effective `EnableGlobalDocking` setting is `true` for the target dock. This property is inherited through the dock hierarchy, so disabling it on a parent dock disables global targets for the entire subtree.

## Attached properties that drive targets

These `DockProperties` determine how targets behave:

| Property | Purpose |
| --- | --- |
| `IsDockTarget` | Marks the control as a docking surface. |
| `ShowDockIndicatorOnly` | Hide the selector icons and show indicators only. |
| `IndicatorDockOperation` | Assigns a `DockOperation` (Top/Bottom/Left/Right/Fill) to an indicator part. |
| `DockAdornerHost` | Redirects the adorner to a specific host control. |

Example of a drop surface that only shows indicators:

```xaml
<Border DockProperties.IsDockTarget="True"
        DockProperties.ShowDockIndicatorOnly="True"
        DockProperties.DockAdornerHost="{Binding #Root}"
        Background="Transparent" />
```

## DockTargetBase properties

`DockTargetBase` exposes properties used by the docking engine and styles:

- `ShowIndicatorsOnly` toggles selector icons on or off.
- `ShowHorizontalTargets` and `ShowVerticalTargets` enable or disable horizontal/vertical indicators.
- `IsGlobalDockAvailable` and `IsGlobalDockActive` control pseudo classes used to style global docking state.

These values are set by the docking pipeline based on the current drag operation and the layout configuration. You typically customize their visuals in styles instead of setting them manually.

## Styling targets

The default themes define template parts like `PART_TopIndicator` and `PART_TopSelector`. You can override them in a custom theme or add styles that react to pseudo classes:

```xaml
<Style Selector="DockTarget:global-available">
  <Setter Property="Opacity" Value="1" />
</Style>

<Style Selector="DockTarget:global-active">
  <Setter Property="Opacity" Value="1" />
</Style>
```

To customize global targets, edit the `GlobalDockTarget` template. Its indicator panels use `DockSettings.GlobalDockingProportion` to size the split areas.

## Adaptive global targets

`IRootDock.EnableAdaptiveGlobalDockTargets` reduces global docking indicators to only show options that change the layout. Enable it when you want to minimize drop choices in large dashboards:

```csharp
rootDock.EnableAdaptiveGlobalDockTargets = true;
```

## Related guides

- [Dock properties](dock-properties.md)
- [Floating dock adorners](dock-floating-adorners.md)
- [Dock settings](dock-settings.md)

For an overview of all guides see the [documentation index](README.md).
