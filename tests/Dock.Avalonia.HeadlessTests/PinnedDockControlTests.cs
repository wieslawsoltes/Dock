using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PinnedDockControlTests
{
    [AvaloniaFact]
    public void PinnedDockControl_ReappliesStoredWidth_WhenColumnSizeChanged()
    {
        var previousUsePinnedWindow = DockSettings.UsePinnedDockWindow;
        DockSettings.UsePinnedDockWindow = false;

        try
        {
            var (window, control, _, grid) = CreatePinnedDockControl(200, 150);
            try
            {
                Assert.True(grid.ColumnDefinitions[0].Width.IsAbsolute);
                Assert.Equal(200, grid.ColumnDefinitions[0].Width.Value, 3);

                grid.ColumnDefinitions[0].Width = new GridLength(100, GridUnitType.Pixel);
                grid.InvalidateMeasure();
                control.UpdateLayout();

                Assert.Equal(200, grid.ColumnDefinitions[0].Width.Value, 3);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            DockSettings.UsePinnedDockWindow = previousUsePinnedWindow;
        }
    }

    [AvaloniaFact]
    public void PinnedDockControl_DragResize_UpdatesPinnedBounds()
    {
        var previousUsePinnedWindow = DockSettings.UsePinnedDockWindow;
        DockSettings.UsePinnedDockWindow = false;

        try
        {
            var (window, control, tool, grid) = CreatePinnedDockControl(200, 150);
            try
            {
                tool.GetPinnedBounds(out _, out _, out var width, out _);
                Assert.Equal(200, width, 3);

                InvokePrivateHandler(control, "OnPinnedDockSplitterDragStarted");

                grid.ColumnDefinitions[0].Width = new GridLength(300, GridUnitType.Pixel);
                grid.InvalidateMeasure();
                control.UpdateLayout();

                InvokePrivateHandler(control, "OnPinnedDockSplitterDragCompleted");

                tool.GetPinnedBounds(out _, out _, out var updatedWidth, out _);
                Assert.Equal(300, updatedWidth, 3);
            }
            finally
            {
                window.Close();
            }
        }
        finally
        {
            DockSettings.UsePinnedDockWindow = previousUsePinnedWindow;
        }
    }

    private static (Window window, PinnedDockControl control, Tool tool, Grid grid) CreatePinnedDockControl(double width, double height)
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;

        var pinnedDock = new ToolDock
        {
            Alignment = Alignment.Left,
            IsEmpty = false,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.PinnedDock = pinnedDock;

        var tool = new Tool();
        pinnedDock.VisibleDockables.Add(tool);
        tool.SetPinnedBounds(0, 0, width, height);

        var control = new PinnedDockControl { DataContext = root };
        var window = new Window
        {
            Width = 800,
            Height = 600,
            Content = control
        };

        window.Show();
        control.ApplyTemplate();
        window.UpdateLayout();
        control.UpdateLayout();

        var grid = control.GetVisualDescendants()
            .OfType<Grid>()
            .FirstOrDefault(candidate => candidate.Name == "PART_PinnedDockGrid");
        Assert.NotNull(grid);

        return (window, control, tool, grid!);
    }

    private static void InvokePrivateHandler(PinnedDockControl control, string methodName)
    {
        var method = typeof(PinnedDockControl).GetMethod(methodName, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Assert.NotNull(method);
        method!.Invoke(control, new object?[] { null, null });
    }
}
