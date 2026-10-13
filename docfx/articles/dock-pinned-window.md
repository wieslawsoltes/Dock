# Pinned dock window

`DockSettings` exposes an option to display auto-hidden tools in a transparent floating window. This works around native control airspace issues just like floating dock adorners.

```csharp
// Enable floating auto-hide windows
DockSettings.UsePinnedDockWindow = true;
```

When enabled the `PinnedDockControl` places the preview content inside a lightweight `PinnedDockWindow`. The window follows the host layout and closes automatically when the tool is hidden.

Activation can move between the owner and preview without closing the preview,
so its editors can receive keyboard focus. Leaving both windows closes the
preview through the factory. `KeepPinnedDockableVisible` preserves it across
those activation changes.

In managed window mode (`DockSettings.UseManagedWindows = true`), pinned windows render inside `ManagedWindowLayer` instead of a native window.

## Pinned preview layout

Pinned previews can either overlay the main content or take real layout space. Configure the root dock with `PinnedDockDisplayMode` (`Overlay` or `Inline`) to pick the default behavior. Individual dockables can override this by setting `PinnedDockDisplayModeOverride` to force a specific mode for that tool or document.

`PinnedDockControl.MinimumRemainingContentSize` reserves at least 50 layout units
beside an overlay preview by default. Set it through a style or custom template
to change that region; values must be finite and nonnegative. A custom resize
behavior can set zero and enforce its own preview limits. Changes update an open
preview immediately and do not affect inline layout or the stored pinned size.

This is a minimum for the remaining grid region, not a maximum on pinned content.
For example, a right-side preview 769 units wide with a 6-unit splitter needs a
25-unit remaining region inside an 800-unit host. Setting the minimum to 25 or
zero keeps that preview aligned to the host's right edge; retaining 50 makes the
combined regions wider than the host.

Layout normally initializes missing pinned bounds from the arranged content.
Custom resizers can set `InitializePinnedBoundsOnLayout` to `False` to keep an
unset model size until their own commit. This also preserves unset sizes through
layout and canceled custom drags. Native `GridSplitter` input still records
sizes, and setting the option back to `True` initializes an already open preview.

## Alignment and orientation

`PinnedDockControl` exposes `PinnedDockAlignment` to control which edge the pinned preview occupies. The default templates bind it to the owning `ToolDock.Alignment`, but you can override it when building custom templates.

Pinned tab strips use `ToolPinnedControl.Orientation` (propagated to `ToolPinItemControl.Orientation`) to arrange pinned tabs vertically or horizontally.

## Pinned tab behavior

Pinned tab items toggle the preview for the selected tool. Clicking the same pinned tab again hides the preview instead of closing the dockable. Use the pinned tab context menu (or `IFactory.CloseDockable`) when you want to close or remove a pinned tool.
