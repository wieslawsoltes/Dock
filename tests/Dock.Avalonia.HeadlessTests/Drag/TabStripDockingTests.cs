using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests.Drag;

public class TabStripDockingTests
{
    private static void LayoutControl(Control control, double width, double height)
    {
        control.Measure(new Size(width, height));
        control.Arrange(new Rect(0, 0, width, height));
    }

    private static AdornerHelper<DockTarget> GetLocalHelper(HostWindowState state)
    {
        var prop = typeof(DockManagerState).GetProperty("LocalAdornerHelper", BindingFlags.Instance | BindingFlags.NonPublic);
        return (AdornerHelper<DockTarget>)prop!.GetValue(state)!;
    }

    private static void SetDropControl(HostWindowState state, Control control)
    {
        var prop = typeof(DockManagerState).GetProperty("DropControl", BindingFlags.Instance | BindingFlags.NonPublic);
        prop!.SetValue(state, control);
    }

    private static void InvokeOver(HostWindowState state, Point point, Visual relativeTo)
    {
        var method = typeof(HostWindowState).GetMethod("Over", BindingFlags.Instance | BindingFlags.NonPublic);
        method!.Invoke(state, new object[] { point, DragAction.Move, relativeTo });
    }

    [AvaloniaFact]
    public void Document_TabStrip_Drag_Shows_Fill_Indicator()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = new HostWindowState(manager, window);

        var host = new Border { Width = 200, Height = 100 };
        var tabStrip = new DocumentTabStrip();
        DockProperties.SetDockAdornerHost(tabStrip, host);

        var panel = new DockPanel();
        DockPanel.SetDock(tabStrip, Dock.Top);
        panel.Children.Add(tabStrip);
        panel.Children.Add(host);
        window.Content = panel;

        LayoutControl(host, 200, 100);
        LayoutControl(tabStrip, 200, 30);
        LayoutControl(panel, 200, 130);
        window.LayoutManager.ExecuteInitialLayoutPass();

        tabStrip.ApplyTemplate();
        host.ApplyTemplate();

        var borderFill = tabStrip.FindControl<Border>("PART_BorderFill");
        Assert.NotNull(borderFill);

        var dockTarget = new DockTarget();
        dockTarget.ApplyTemplate();

        GetLocalHelper(state).Adorner = dockTarget;
        SetDropControl(state, borderFill!);

        InvokeOver(state, new Point(10, 10), host);

        var center = dockTarget.FindControl<Panel>("PART_CenterIndicator");
        Assert.True(center.Opacity > 0);
    }

    [AvaloniaFact]
    public void Tool_TabStrip_Drag_Shows_Fill_Indicator()
    {
        var manager = new DockManager();
        var window = new HostWindow();
        var state = new HostWindowState(manager, window);

        var host = new Border { Width = 200, Height = 100 };
        var tabStrip = new ToolTabStrip();
        DockProperties.SetDockAdornerHost(tabStrip, host);

        var panel = new DockPanel();
        DockPanel.SetDock(tabStrip, Dock.Top);
        panel.Children.Add(tabStrip);
        panel.Children.Add(host);
        window.Content = panel;

        LayoutControl(host, 200, 100);
        LayoutControl(tabStrip, 200, 30);
        LayoutControl(panel, 200, 130);
        window.LayoutManager.ExecuteInitialLayoutPass();

        tabStrip.ApplyTemplate();
        host.ApplyTemplate();

        var borderFill = tabStrip.FindControl<Border>("PART_BorderFill");
        Assert.NotNull(borderFill);

        var dockTarget = new DockTarget();
        dockTarget.ApplyTemplate();

        GetLocalHelper(state).Adorner = dockTarget;
        SetDropControl(state, borderFill!);

        InvokeOver(state, new Point(10, 10), host);

        var center = dockTarget.FindControl<Panel>("PART_CenterIndicator");
        Assert.True(center.Opacity > 0);
    }
}
