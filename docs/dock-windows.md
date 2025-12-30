# Floating Windows

Dock can detach dockables into separate floating windows. These windows are represented by the `IDockWindow` interface and hosted using `IHostWindow` implementations.

## Creating windows

`FactoryBase` exposes a `CreateWindowFrom` method which returns an `IDockWindow`. Override this method on your factory to customize the dock window that represents a floating layout:

```csharp
public override IDockWindow? CreateWindowFrom(IDockable dockable)
{
    var window = base.CreateWindowFrom(dockable);
    if (window is null)
    {
        return null;
    }

    window.Title = $"Dock - {dockable.Title}";
    return window;
}
```

Calling `FloatDockable` on the factory opens a dockable in a new window. The new `IDockWindow` is tracked by the root dock and stores its bounds and title so it can be serialized together with the layout.

To customize the platform window (`IHostWindow`) used by floating docks, use `HostWindowLocator` or `DefaultHostWindowLocator`. See [Host window locators](dock-host-window-locator.md) for details.

## IDockWindow model members

`IDockWindow` represents the floating window model and includes:

| Member | Type | Description |
| --- | --- | --- |
| `Id` | `string` | Window identifier. |
| `X`, `Y` | `double` | Window position. |
| `Width`, `Height` | `double` | Window size. |
| `Topmost` | `bool` | Keeps the window on top when `true`. |
| `Title` | `string` | Window title. |
| `Owner` | `IDockable?` | Dockable that owns the window. |
| `Factory` | `IFactory?` | Factory used by the window. |
| `Layout` | `IRootDock?` | Root layout hosted in the window. |
| `Host` | `IHostWindow?` | Platform host window. |
| `OnClose()` | `bool` | Close callback (return `false` to cancel). |
| `OnMoveDragBegin()` | `bool` | Drag begin callback (return `false` to cancel). |
| `OnMoveDrag()` | `void` | Drag in progress callback. |
| `OnMoveDragEnd()` | `void` | Drag end callback. |
| `Save()` | `void` | Persist size/position into the model. |
| `Present(bool)` | `void` | Show the window. |
| `Exit()` | `void` | Close the window. |
| `SetActive()` | `void` | Activate the window. |

## IHostWindow members

`IHostWindow` is the platform-facing host. It provides:

| Member | Type | Description |
| --- | --- | --- |
| `HostWindowState` | `IHostWindowState?` | Docking state for the host. |
| `IsTracked` | `bool` | Whether size/position is tracked. |
| `Window` | `IDockWindow?` | Backing dock window model. |
| `Present(bool)` | `void` | Show the host window. |
| `Exit()` | `void` | Close the host window. |
| `SetPosition/GetPosition` | `void` | Read/write the host position. |
| `SetSize/GetSize` | `void` | Read/write the host size. |
| `SetTitle` | `void` | Update the host title. |
| `SetLayout` | `void` | Assign the hosted layout. |
| `SetActive` | `void` | Activate the host window. |

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
This behavior is controlled by two settings on `DockSettings`:

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
