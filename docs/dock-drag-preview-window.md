# Drag preview window

Dock displays a floating window when a dockable is dragged. The window shows the tab header and a live preview of the content. The preview updates when the source control is re‑rendered or its layout changes.

The helper sets a `VisualBrush` on the preview control that references the dragged control's visual. This avoids bitmap conversion and updates automatically when the source re-renders or its layout changes. Applications can customize `IDragOffsetCalculator` to control where the window appears.
