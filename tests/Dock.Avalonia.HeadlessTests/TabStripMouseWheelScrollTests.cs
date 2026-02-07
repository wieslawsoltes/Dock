using System.Linq;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class TabStripMouseWheelScrollTests
{
    [AvaloniaFact]
    public void DocumentTabStrip_MouseWheelScrollsHorizontally_ByDefault()
    {
        var tabStrip = new DocumentTabStrip
        {
            Width = 180,
            Height = 32,
            ItemsSource = CreateItems(30, "Document")
        };

        var window = ShowInWindow(tabStrip, 180, 100);
        try
        {
            var scrollViewer = GetScrollViewer(tabStrip);
            Assert.True(scrollViewer.Extent.Width > scrollViewer.Viewport.Width);

            RaisePointerWheel(tabStrip, new Vector(0, -1));

            Assert.True(scrollViewer.Offset.X > 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolTabStrip_MouseWheelScrollsHorizontally_ByDefault()
    {
        var tabStrip = new ToolTabStrip
        {
            Width = 180,
            Height = 32,
            ItemsSource = CreateItems(30, "Tool")
        };

        var window = ShowInWindow(tabStrip, 180, 100);
        try
        {
            var scrollViewer = GetScrollViewer(tabStrip);
            Assert.True(scrollViewer.Extent.Width > scrollViewer.Viewport.Width);

            RaisePointerWheel(tabStrip, new Vector(0, -1));

            Assert.True(scrollViewer.Offset.X > 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentTabStrip_CanScrollVertically_WhenConfigured()
    {
        var tabStrip = new DocumentTabStrip
        {
            Width = 200,
            Height = 120,
            Orientation = Orientation.Vertical,
            MouseWheelScrollOrientation = Orientation.Vertical,
            ItemsSource = CreateItems(30, "Document")
        };

        var window = ShowInWindow(tabStrip, 240, 140);
        try
        {
            var scrollViewer = GetScrollViewer(tabStrip);
            Assert.True(scrollViewer.Extent.Height > scrollViewer.Viewport.Height);

            RaisePointerWheel(tabStrip, new Vector(0, -1));

            Assert.True(scrollViewer.Offset.Y > 0);
            Assert.Equal(0, scrollViewer.Offset.X);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentTabStrip_MouseWheelScrollOrientation_ChangesAtRuntime()
    {
        var tabStrip = new DocumentTabStrip
        {
            Width = 180,
            Height = 32,
            ItemsSource = CreateItems(30, "Document")
        };

        var window = ShowInWindow(tabStrip, 180, 100);
        try
        {
            var scrollViewer = GetScrollViewer(tabStrip);
            Assert.True(scrollViewer.Extent.Width > scrollViewer.Viewport.Width);

            tabStrip.MouseWheelScrollOrientation = Orientation.Vertical;
            RaisePointerWheel(tabStrip, new Vector(0, -1));
            Assert.Equal(0, scrollViewer.Offset.X);

            tabStrip.MouseWheelScrollOrientation = Orientation.Horizontal;
            RaisePointerWheel(tabStrip, new Vector(0, -1));
            Assert.True(scrollViewer.Offset.X > 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolTabStrip_MouseWheelScrollOrientation_ChangesAtRuntime()
    {
        var tabStrip = new ToolTabStrip
        {
            Width = 180,
            Height = 32,
            ItemsSource = CreateItems(30, "Tool")
        };

        var window = ShowInWindow(tabStrip, 180, 100);
        try
        {
            var scrollViewer = GetScrollViewer(tabStrip);
            Assert.True(scrollViewer.Extent.Width > scrollViewer.Viewport.Width);

            tabStrip.MouseWheelScrollOrientation = Orientation.Vertical;
            RaisePointerWheel(tabStrip, new Vector(0, -1));
            Assert.Equal(0, scrollViewer.Offset.X);

            tabStrip.MouseWheelScrollOrientation = Orientation.Horizontal;
            RaisePointerWheel(tabStrip, new Vector(0, -1));
            Assert.True(scrollViewer.Offset.X > 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DocumentTabStrip_MouseWheelScrolls_AfterDetachAndReattach()
    {
        var tabStrip = new DocumentTabStrip
        {
            Width = 180,
            Height = 32,
            ItemsSource = CreateItems(30, "Document")
        };

        var window = ShowInWindow(tabStrip, 180, 100);
        try
        {
            window.Content = null;
            window.UpdateLayout();
            window.Content = tabStrip;
            window.UpdateLayout();
            tabStrip.UpdateLayout();

            var scrollViewer = GetScrollViewer(tabStrip);
            Assert.True(scrollViewer.Extent.Width > scrollViewer.Viewport.Width);

            RaisePointerWheel(tabStrip, new Vector(0, -1));

            Assert.True(scrollViewer.Offset.X > 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void ToolTabStrip_MouseWheelScrolls_AfterDetachAndReattach()
    {
        var tabStrip = new ToolTabStrip
        {
            Width = 180,
            Height = 32,
            ItemsSource = CreateItems(30, "Tool")
        };

        var window = ShowInWindow(tabStrip, 180, 100);
        try
        {
            window.Content = null;
            window.UpdateLayout();
            window.Content = tabStrip;
            window.UpdateLayout();
            tabStrip.UpdateLayout();

            var scrollViewer = GetScrollViewer(tabStrip);
            Assert.True(scrollViewer.Extent.Width > scrollViewer.Viewport.Width);

            RaisePointerWheel(tabStrip, new Vector(0, -1));

            Assert.True(scrollViewer.Offset.X > 0);
        }
        finally
        {
            window.Close();
        }
    }

    private static AvaloniaList<string> CreateItems(int count, string prefix)
    {
        var items = new AvaloniaList<string>();
        for (var i = 0; i < count; i++)
        {
            items.Add($"{prefix} {i:00} - Very Long Tab Header");
        }

        return items;
    }

    private static Window ShowInWindow(Control control, double width, double height)
    {
        var window = new Window
        {
            Width = width,
            Height = height,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        return window;
    }

    private static ScrollViewer GetScrollViewer(Control control)
    {
        var scrollViewer = control.GetVisualDescendants().OfType<ScrollViewer>().FirstOrDefault();
        Assert.NotNull(scrollViewer);
        return scrollViewer!;
    }

    private static void RaisePointerWheel(Control control, Vector delta)
    {
        var source = control.GetVisualDescendants().OfType<TabStripItem>().OfType<Control>().FirstOrDefault() ?? control;
        var pointer = new Pointer(1, PointerType.Mouse, true);
        var x = source.Bounds.Width > 1 ? source.Bounds.Width / 2 : 1;
        var y = source.Bounds.Height > 1 ? source.Bounds.Height / 2 : 1;
        var properties = new PointerPointProperties(RawInputModifiers.None, PointerUpdateKind.Other);
        var args = new PointerWheelEventArgs(source, pointer, control, new Point(x, y), 0, properties, KeyModifiers.None, delta);
        source.RaiseEvent(args);
    }
}
