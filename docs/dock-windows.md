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

Host windows inherit from Avalonia `Window`, which allows you to subclass them
and add custom behaviour. One approach is to fade the window after a period of
no pointer activity. The example below starts a timer whenever the pointer moves
and sets `Opacity` to `0` once the delay elapses:

```csharp
public class FadingHostWindow : HostWindow
{
    private readonly DispatcherTimer _timer;

    public FadingHostWindow()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
        _timer.Tick += (_, _) => Opacity = 0;

        PointerMoved += ResetFade;
        PointerEntered += ResetFade;
    }

    private void ResetFade(object? sender, PointerEventArgs e)
    {
        Opacity = 1;
        _timer.Stop();
        _timer.Start();
    }
}
```

Register `FadingHostWindow` through `HostWindowLocator` or
`DefaultHostWindowLocator` to host floating docks with this fade-out behaviour.
