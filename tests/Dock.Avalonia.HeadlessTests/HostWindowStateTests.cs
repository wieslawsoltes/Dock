using Avalonia;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowStateTests
{
    private static HostWindowState CreateState(DockManager manager, HostWindow window)
    {
        return new HostWindowState(manager, window);
    }

    [AvaloniaFact]
    public void Process_CaptureLost_Resets_State()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);

        state.Process(new PixelPoint(0,0), EventType.Pressed);
        state.Process(new PixelPoint(10,10), EventType.Moved);
        state.Process(new PixelPoint(), EventType.CaptureLost);

        Assert.NotNull(window.HostWindowState);
    }

    [AvaloniaFact]
    public void Process_Released_Completes_Drag()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = CreateState(manager, window);

        state.Process(new PixelPoint(0,0), EventType.Pressed);
        state.Process(new PixelPoint(10,10), EventType.Moved);
        state.Process(new PixelPoint(), EventType.Released);

        Assert.NotNull(window.HostWindowState);
    }
}
