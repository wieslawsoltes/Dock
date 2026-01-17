# MDI Layout Helpers and Defaults

Dock’s MDI layout commands are backed by helper methods in `Dock.Model.Core.MdiLayoutHelper`. These helpers update `IMdiDocument.MdiBounds`, `MdiState`, and `MdiZIndex` so MDI windows are repositioned consistently.

## Built-in helper methods

`MdiLayoutHelper` exposes four static methods that operate on an `IDocumentDock`:

- `CascadeDocuments` – Offsets windows with a cascading diagonal.
- `TileDocumentsHorizontal` – Stacks windows vertically.
- `TileDocumentsVertical` – Stacks windows horizontally.
- `RestoreDocuments` – Sets all windows back to `Normal` state.

These are the methods used by the `CascadeDocuments`, `TileDocumentsHorizontal`,
`TileDocumentsVertical`, and `RestoreDocuments` commands exposed on `IDocumentDock`
implementations.

## Default layout constants

`MdiLayoutDefaults` defines the constants used by the helper:

- `CascadeOffset`
- `MinimizedHeight`
- `MinimizedWidth`
- `MinimizedSpacing`
- `DefaultWidthRatio`
- `DefaultHeightRatio`
- `MinimumWidth`
- `MinimumHeight`

These values are constants; override layout behavior by supplying your own layout logic instead of changing the defaults.

## Calling the helpers directly

You can invoke the helper methods whenever you need to enforce a layout:

```csharp
using Dock.Model.Core;

MdiLayoutHelper.CascadeDocuments(documentDock);
```

`MdiLayoutHelper` relies on `IDocumentDock.GetVisibleBounds` to compute layout bounds, so ensure your dockables are being tracked in the visual tree when you call it.

## Custom layout managers

MDI layouts in the Avalonia UI are arranged by `MdiLayoutPanel`. It uses
`ClassicMdiLayoutManager` by default but can be replaced:

```csharp
var mdiDocumentControl = new MdiDocumentControl
{
    LayoutManager = new MyMdiLayoutManager()
};
```

You can also set it in XAML:

```xaml
<controls:MdiDocumentControl>
  <controls:MdiDocumentControl.LayoutManager>
    <local:MyMdiLayoutManager />
  </controls:MdiDocumentControl.LayoutManager>
</controls:MdiDocumentControl>
```

Your layout manager receives `MdiLayoutEntry` instances and is responsible for arranging each document window.

## Related types

`IMdiDocument.MdiBounds` uses the `DockRect` struct to store bounds. In the Avalonia control layer, `DockRect` is converted to `Rect` as windows are arranged.

See [MDI document layout](dock-mdi.md) for the user-facing behavior and commands.
