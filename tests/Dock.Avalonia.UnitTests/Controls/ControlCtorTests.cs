using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Controls.Diagnostics;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class ControlCtorTests
{
    [AvaloniaFact]
    public void DockControl_Ctor()
    {
        var control = new DockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DockDockControl_Ctor()
    {
        var control = new DockDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DockTarget_Ctor()
    {
        var control = new DockTarget();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DockableControl_Ctor()
    {
        var control = new DockableControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DocumentContentControl_Ctor()
    {
        var control = new DocumentContentControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DocumentControl_Ctor()
    {
        var control = new DocumentControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DocumentDockControl_Ctor()
    {
        var control = new DocumentDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DocumentTabStrip_Ctor()
    {
        var control = new DocumentTabStrip();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DocumentTabStripItem_Ctor()
    {
        var control = new DocumentTabStripItem();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DragPreviewControl_Ctor()
    {
        var control = new DragPreviewControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DragPreviewWindow_Ctor()
    {
        var control = new DragPreviewWindow();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void GridDockControl_Ctor()
    {
        var control = new GridDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void HostWindow_Ctor()
    {
        var control = new HostWindow();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void HostWindowTitleBar_Ctor()
    {
        var control = new HostWindowTitleBar();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void PinnedDockControl_Ctor()
    {
        var control = new PinnedDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ProportionalDockControl_Ctor()
    {
        var control = new ProportionalDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void RootDockControl_Ctor()
    {
        var control = new RootDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void RootDockDebug_Ctor()
    {
        var control = new RootDockDebug();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void StackDockControl_Ctor()
    {
        var control = new StackDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolChromeControl_Ctor()
    {
        var control = new ToolChromeControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolContentControl_Ctor()
    {
        var control = new ToolContentControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolControl_Ctor()
    {
        var control = new ToolControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolDockControl_Ctor()
    {
        var control = new ToolDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolPinItemControl_Ctor()
    {
        var control = new ToolPinItemControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolPinnedControl_Ctor()
    {
        var control = new ToolPinnedControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolTabStrip_Ctor()
    {
        var control = new ToolTabStrip();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolTabStripItem_Ctor()
    {
        var control = new ToolTabStripItem();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void UniformGridDockControl_Ctor()
    {
        var control = new UniformGridDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void WrapDockControl_Ctor()
    {
        var control = new WrapDockControl();
        Assert.NotNull(control);
    }
}
