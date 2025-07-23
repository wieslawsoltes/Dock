# Floating Windows

Dock can detach dockables into separate floating windows. These windows are represented by the `IDockWindow` interface and hosted using `IHostWindow` implementations.

## Creating windows

`FactoryBase` exposes a `CreateWindowFrom` method which returns an `IHostWindow`. Override this method on your factory to customize the platform window used for floating docks:

```csharp
public override IHostWindow CreateWindowFrom(IDockWindow source)
{
    var window = base.CreateWindowFrom(source);
    window.Title = $"Dock - {source.Id}";
    return window;
}
```

Calling `FloatDockable` on the factory opens a dockable in a new window. The returned `IDockWindow` stores its bounds and title and can be serialized together with the layout.

## Window chrome options

`HostWindow` provides two boolean properties that control how its chrome behaves:

- `ToolChromeControlsWholeWindow`
- `DocumentChromeControlsWholeWindow`

When enabled these toggle the `:toolchromecontrolswindow` or
`:documentchromecontrolswindow` pseudo class respectively. Styles can use these
states to remove the standard system chrome and let the Dock chrome occupy the
entire window.

## Presenting and closing

`IDockWindow` exposes `Present(bool isDialog)` to show the window and `Exit()` to close it programmatically. The default implementation uses `HostAdapter` to bridge between the interface and the underlying Avalonia `Window`.

Use the `WindowOpened` and `WindowClosed` events on the factory to track when floating windows appear or disappear.

```csharp
factory.WindowClosed += (_, e) =>
    Console.WriteLine($"Closed {e.Window?.Title}");
```

## Window magnetism

Floating windows can optionally snap to the edges of other floating windows while being dragged.
This behaviour is controlled by two settings on `DockSettings`:

- `EnableWindowMagnetism` enables or disables the feature.
- `WindowMagnetDistance` specifies how close windows must be before they snap together.

When magnetism is enabled, `HostWindow` compares its position against other tracked windows during a drag
and adjusts the position if it falls within the snap distance. This makes it easy to align multiple floating
windows.

## Bringing windows to front

If `DockSettings.BringWindowsToFrontOnDrag` is enabled, initiating a drag will activate
all floating windows and any main window hosting a `DockControl` so they stay above other
applications until the drag completes.

For more advanced scenarios see [Adapter Classes](dock-adapters.md) and the [Advanced Guide](dock-advanced.md).
