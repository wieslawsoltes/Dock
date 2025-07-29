using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Diagnostics.Controls;
using Dock.Model.Core;
using Xunit;
using LayoutOrientation = Avalonia.Layout.Orientation;

namespace Dock.Avalonia.Diagnostics.UnitTests;

public class DockControlInstantiationTests
{
    [AvaloniaFact]
    public void DocumentControl_Default_TabsLayout_Top()
    {
        var control = new DocumentControl();
        Assert.Equal(DocumentTabLayout.Top, control.TabsLayout);
    }

    [AvaloniaFact]
    public void DocumentTabStrip_Default_Orientation_Horizontal()
    {
        var control = new DocumentTabStrip();
        Assert.Equal(LayoutOrientation.Horizontal, control.Orientation);
    }

    [AvaloniaFact]
    public void DockableControl_Default_TrackingMode_Visible()
    {
        var control = new DockableControl();
        Assert.Equal(TrackingMode.Visible, control.TrackingMode);
    }

    [AvaloniaFact]
    public void PinnedDockControl_Default_Alignment_Unset()
    {
        var control = new PinnedDockControl();
        Assert.Equal(Alignment.Left, control.PinnedDockAlignment);
    }

    [AvaloniaFact]
    public void HostWindow_Defaults()
    {
        var window = new HostWindow();
        Assert.False(window.IsToolWindow);
        Assert.False(window.ToolChromeControlsWholeWindow);
        Assert.False(window.DocumentChromeControlsWholeWindow);
        Assert.NotNull(window.HostWindowState);
    }

    [AvaloniaFact]
    public void HostWindowTitleBar_Can_Instantiate()
    {
        var bar = new HostWindowTitleBar();
        Assert.NotNull(bar);
    }

    [AvaloniaFact]
    public void DockTarget_Can_Instantiate()
    {
        var target = new DockTarget();
        Assert.NotNull(target);
    }

    [AvaloniaFact]
    public void DockAdornerWindow_Can_Instantiate()
    {
        var window = new DockAdornerWindow();
        Assert.NotNull(window);
    }

    [AvaloniaFact]
    public void DragPreviewWindow_Can_Instantiate()
    {
        var window = new DragPreviewWindow();
        Assert.NotNull(window);
    }

    [AvaloniaFact]
    public void DragPreviewControl_Can_Instantiate()
    {
        var control = new DragPreviewControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void GlobalDockTarget_Can_Instantiate()
    {
        var target = new GlobalDockTarget();
        Assert.NotNull(target);
    }

    [AvaloniaFact]
    public void DockDockControl_Can_Instantiate()
    {
        var control = new DockDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DocumentDockControl_Can_Instantiate()
    {
        var control = new DocumentDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void DocumentContentControl_Can_Instantiate()
    {
        var control = new DocumentContentControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void GridDockControl_Can_Instantiate()
    {
        var control = new GridDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ProportionalDockControl_Can_Instantiate()
    {
        var control = new ProportionalDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void RootDockControl_Can_Instantiate()
    {
        var control = new RootDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void RootDockDebug_Can_Instantiate()
    {
        var debug = new RootDockDebug();
        Assert.NotNull(debug);
    }

    [AvaloniaFact]
    public void StackDockControl_Can_Instantiate()
    {
        var control = new StackDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolChromeControl_Defaults()
    {
        var control = new ToolChromeControl();
        Assert.False(control.IsActive);
        Assert.False(control.IsPinned);
        Assert.False(control.IsFloating);
        Assert.False(control.IsMaximized);
    }

    [AvaloniaFact]
    public void ToolContentControl_Can_Instantiate()
    {
        var control = new ToolContentControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolControl_Can_Instantiate()
    {
        var control = new ToolControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolDockControl_Can_Instantiate()
    {
        var control = new ToolDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolPinItemControl_Can_Instantiate()
    {
        var control = new ToolPinItemControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolPinnedControl_Can_Instantiate()
    {
        var control = new ToolPinnedControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolTabStrip_Can_Instantiate()
    {
        var control = new ToolTabStrip();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void ToolTabStripItem_Can_Instantiate()
    {
        var item = new ToolTabStripItem();
        Assert.NotNull(item);
    }

    [AvaloniaFact]
    public void UniformGridDockControl_Can_Instantiate()
    {
        var control = new UniformGridDockControl();
        Assert.NotNull(control);
    }

    [AvaloniaFact]
    public void WrapDockControl_Can_Instantiate()
    {
        var control = new WrapDockControl();
        Assert.NotNull(control);
    }
}
