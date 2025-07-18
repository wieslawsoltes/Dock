# EnableWindowDrag

`EnableWindowDrag` is an option on both `IDocumentDock` and `IToolDock`. When set to `true`, the empty area of the tab strip works as a drag handle for the host window. Tool docks enable this by default while document docks keep it disabled.

Enabling the property lets users reposition a floating or main window by dragging the tab bar instead of the standard title bar. This is useful when the window chrome is hidden or when you want the tabs to act as the natural drag area for the window.

## Usage

Set the property when creating your document docks:

```csharp
var documents = new DocumentDock
{
    EnableWindowDrag = true,
    // other options
};
```

Tool docks use the same property:

```csharp
var tools = new ToolDock
{
    // EnableWindowDrag is true by default
};
```

The same property is available on the Avalonia control `DocumentDock` and can also be set in XAML:

```xaml
<avaloniaDock:DocumentDock EnableWindowDrag="True" />
```

Tool docks bind the same property:

```xaml
<avaloniaDock:ToolDock />
```

With the property enabled, `DocumentTabStrip` listens for pointer events on its background and calls `BeginMoveDrag` on the surrounding `HostWindow`. The user can grab the tab area and drag the entire window.

## Scenarios

- **Floating windows** – Users can drag the tab bar of a floating document window to reposition it without relying on the window title bar.
- **Custom window chrome** – Applications that hide or replace the native title bar can still offer window dragging by enabling this option.
- **Tabbed layouts** – When multiple documents share a single window, the tab strip becomes the primary handle for moving that window.

See the [Advanced guide](dock-advanced.md) for details on customizing floating windows.
