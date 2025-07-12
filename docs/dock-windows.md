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

For more advanced scenarios see [Adapter Classes](dock-adapters.md) and the [Advanced Guide](dock-advanced.md).

## Fading windows on inactivity

`HostWindow` exposes `FadeOnInactive` and `CloseOnFadeOut` to automatically fade
floating windows when the pointer is idle. When enabled the window opacity is
animated using a style transition and restored on pointer movement. You can also
close the window once the fade completes.

```csharp
public override IHostWindow CreateWindowFrom(IDockWindow source)
{
    var window = base.CreateWindowFrom(source);
    window.FadeOnInactive = true;
    window.CloseOnFadeOut = false; // set to true to auto-close
    return window;
}
```

The same properties exist on `IDockWindow` so the behaviour can be serialized
with the layout.
