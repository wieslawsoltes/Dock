# MDI Document Layout

Dock can display documents as classic MDI windows inside a document dock. In this mode documents become movable, resizable windows with minimize, maximize, and restore states.

## Enable MDI mode

Set the layout mode on a document dock:

```csharp
var documents = new DocumentDock
{
    LayoutMode = DocumentLayoutMode.Mdi
};
```

If you prefer fluent setup:

```csharp
factory.DocumentDock(out var documents)
       .WithLayoutMode(DocumentLayoutMode.Mdi);
```

## Arrange documents

Document docks expose commands for common MDI layout operations:

```csharp
documents.CascadeDocuments?.Execute(null);
documents.TileDocumentsHorizontal?.Execute(null);
documents.TileDocumentsVertical?.Execute(null);
documents.RestoreDocuments?.Execute(null);
```

The commands use the dock’s current visible bounds to compute layouts.

## Document state and bounds

Documents implement `IMdiDocument` and store their window bounds and state:

- `MdiBounds` stores the normal window rectangle.
- `MdiState` tracks `Normal`, `Minimized`, and `Maximized` states.
- `MdiZIndex` controls stacking order.

These values are serialized with the layout so windows restore to their previous positions.

## MDI control customization

The Avalonia controls expose additional properties for theming:

- `MdiDocumentControl` forwards `IconTemplate`, `HeaderTemplate`, `ModifiedTemplate`, `CloseTemplate`, and `CloseButtonTheme` to each window.
- `MdiDocumentWindow` exposes `DocumentContextMenu`, `MdiState`, and `IsActive` for styling or interaction.

To customize the layout algorithm, provide an `IMdiLayoutManager` implementation. It can be assigned on `MdiDocumentControl.LayoutManager` (which the default template forwards to `MdiLayoutPanel`):

```csharp
var mdiLayoutManager = new MyMdiLayoutManager();
mdiDocumentControl.LayoutManager = mdiLayoutManager;
```

For details on the built-in layout helpers and defaults see
[MDI layout helpers](dock-mdi-layout-helpers.md).

## Notes

- The document header contains a drag handle that starts docking operations when you drag it outside the MDI surface.
- When a document is maximized the stored bounds remain unchanged so restore returns to the previous size.

For an overview of all guides see the [documentation index](README.md).
