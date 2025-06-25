# Diagonal Resizing

Dock layouts use `ProportionalStackPanelSplitter` controls to let the user resize groups of dockable controls. Normally each splitter only affects a single orientation – vertical or horizontal.

When two splitters meet you can add a `ProportionalCornerSplitter` to let the user resize both directions at once. The corner splitter exposes `HorizontalSplitter` and `VerticalSplitter` properties which should reference the participating `ProportionalStackPanelSplitter` instances.

```xaml
<ProportionalCornerSplitter
    HorizontalSplitter="{Binding ElementName=HorizontalSplit}"
    VerticalSplitter="{Binding ElementName=VerticalSplit}"/>
```

Dragging the corner splitter moves both underlying splitters simultaneously, allowing diagonal adjustments of the surrounding panels.
