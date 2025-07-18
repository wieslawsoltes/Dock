# Chrome-style tab dragging

`DocumentTabStrip` supports chrome-like tab dragging and reordering. With `ChromeTabDrag` enabled (the default), dragging a tab shows it following the pointer. Once the pointer crosses half of an adjacent tab, the items swap. Dragging further than a small vertical threshold detaches the document into a floating window.

Use the property on the control to disable the behaviour:

```xaml
<DocumentTabStrip ChromeTabDrag="False" />
```

The logic uses `DockSettings.MinimumHorizontalDragDistance` and `DockSettings.MinimumVerticalDragDistance` to decide when the drag starts.


