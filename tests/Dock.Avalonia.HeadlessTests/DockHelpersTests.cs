using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Dock.Avalonia.Internal;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;

namespace Dock.Avalonia.HeadlessTests;

public class DockHelpersTests
{
    [AvaloniaFact]
    public void ToDockPoint_Returns_Correct_Point()
    {
        var point = new Point(10, 20);
        var dockPoint = DockHelpers.ToDockPoint(point);

        Assert.Equal(10, dockPoint.X);
        Assert.Equal(20, dockPoint.Y);
        Assert.Equal("10, 20", dockPoint.ToString());
    }

    [AvaloniaFact]
    public void FindProportionalDock_Finds_Child_Dock()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var docDock = new DocumentDock { Factory = factory, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, docDock);
        var proportional = new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(docDock, proportional);

        var result = DockHelpers.FindProportionalDock(docDock);

        Assert.Same(proportional, result);
    }

    [AvaloniaFact]
    public void GetControl_Finds_DropArea_ByBounds_When_Overlay_Is_HitTestVisible()
    {
        var target = new Border
        {
            Width = 200,
            Height = 120,
            Background = Brushes.Transparent
        };
        DockProperties.SetIsDropArea(target, true);
        DockProperties.SetIsDockTarget(target, true);

        var overlay = new Border
        {
            Width = 200,
            Height = 120,
            Background = Brushes.Transparent
        };

        var canvas = new Canvas
        {
            Width = 200,
            Height = 120
        };
        canvas.Children.Add(target);
        canvas.Children.Add(overlay);

        var window = new Window
        {
            Width = 200,
            Height = 120,
            Content = canvas
        };

        try
        {
            window.Show();
            window.UpdateLayout();

            var hit = DockHelpers.GetControl(canvas, new Point(10, 10), DockProperties.IsDropAreaProperty);

            Assert.Same(target, hit);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GetControl_Finds_DragArea_ByBounds_When_Overlay_Is_HitTestVisible()
    {
        var dockable = new Document { Title = "Document" };
        var target = new Border
        {
            Width = 200,
            Height = 120,
            Background = Brushes.Transparent,
            DataContext = dockable
        };
        DockProperties.SetIsDragArea(target, true);
        DockProperties.SetIsDragEnabled(target, true);

        var overlay = new Border
        {
            Width = 200,
            Height = 120,
            Background = Brushes.Transparent
        };

        var canvas = new Canvas
        {
            Width = 200,
            Height = 120
        };
        canvas.Children.Add(target);
        canvas.Children.Add(overlay);

        var window = new Window
        {
            Width = 200,
            Height = 120,
            Content = canvas
        };

        try
        {
            window.Show();
            window.UpdateLayout();

            var hit = DockHelpers.GetControl(canvas, new Point(10, 10), DockProperties.IsDragAreaProperty);

            Assert.Same(target, hit);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void GetControl_Does_Not_Find_DragArea_ByBounds_When_DataContext_Is_Not_Dockable()
    {
        var target = new Border
        {
            Width = 200,
            Height = 120,
            Background = Brushes.Transparent,
            DataContext = new object()
        };
        DockProperties.SetIsDragArea(target, true);
        DockProperties.SetIsDragEnabled(target, true);

        var overlay = new Border
        {
            Width = 200,
            Height = 120,
            Background = Brushes.Transparent
        };

        var canvas = new Canvas
        {
            Width = 200,
            Height = 120
        };
        canvas.Children.Add(target);
        canvas.Children.Add(overlay);

        var window = new Window
        {
            Width = 200,
            Height = 120,
            Content = canvas
        };

        try
        {
            window.Show();
            window.UpdateLayout();

            var hit = DockHelpers.GetControl(canvas, new Point(10, 10), DockProperties.IsDragAreaProperty);

            Assert.Null(hit);
        }
        finally
        {
            window.Close();
        }
    }
}
