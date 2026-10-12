using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
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
    public void Hidden_Window_Does_Not_Expose_Its_Retained_Drop_Area()
    {
        Border dropArea = new() { Background = Brushes.Transparent };
        DockProperties.SetIsDropArea(dropArea, true);
        Window window = new() { Width = 400, Height = 300, Content = dropArea };
        try
        {
            window.Show();
            window.UpdateLayout();
            Point point = new(20, 20);
            window.MouseMove(point);
            Assert.Same(dropArea, DockHelpers.GetControl(dropArea, point, DockProperties.IsDropAreaProperty));

            window.Hide();
            Assert.True(dropArea.IsVisible);
            Assert.False(dropArea.IsEffectivelyVisible);
            Assert.Null(DockHelpers.GetControl(dropArea, point, DockProperties.IsDropAreaProperty));

            window.Show();
            window.MouseMove(point);
            Assert.Same(dropArea, DockHelpers.GetControl(dropArea, point, DockProperties.IsDropAreaProperty));
        }
        finally { window.Close(); }
    }

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
}
