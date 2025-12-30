# DockableControl and TrackingMode

`DockableControl` is a lightweight wrapper used by Dock templates to track the visual state of an `IDockable`. It registers the control in the factory tracking dictionaries and records bounds/pointer positions for drag operations.

## What DockableControl does

When its `DataContext` is an `IDockable`, `DockableControl`:

- Registers itself in the appropriate factory dictionary (`VisibleDockableControls`, `PinnedDockableControls`, or `TabDockableControls`).
- Stores bounds via `SetVisibleBounds`, `SetPinnedBounds`, or `SetTabBounds`.
- Updates pointer positions during drag operations.

The tracking behavior depends on the `TrackingMode` property.

## TrackingMode values

| Value | Registers in | Updates |
| --- | --- | --- |
| `Visible` | `VisibleDockableControls`, `VisibleRootControls` | Visible bounds |
| `Pinned` | `PinnedDockableControls`, `PinnedRootControls` | Pinned bounds |
| `Tab` | `TabDockableControls`, `TabRootControls` | Tab bounds |

## Using DockableControl in templates

If you override templates for document content, tool content, tab items, or pinned items, keep `DockableControl` in place and set the matching `TrackingMode`.

Document content (visible):

```xaml
<DockableControl TrackingMode="Visible">
  <ContentPresenter Content="{Binding}" />
</DockableControl>
```

Tab item (tab tracking):

```xaml
<DockableControl TrackingMode="Tab">
  <Border>
    <TextBlock Text="{Binding Title}" />
  </Border>
</DockableControl>
```

Pinned item (pinned tracking):

```xaml
<DockableControl TrackingMode="Pinned">
  <ContentPresenter Content="{Binding}" />
</DockableControl>
```

Without `DockableControl`, factory tracking dictionaries stay empty and Dock loses bounds/pointer history for that dockable.

## Related guides

- [Tracking controls](dock-tracking-controls.md)
- [Dock properties](dock-properties.md)
- [Styling and theming](dock-styling.md)

For an overview of all guides see the [documentation index](README.md).
