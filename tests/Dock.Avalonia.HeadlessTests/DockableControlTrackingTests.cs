using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockableControlTrackingTests
{
    [AvaloniaFact]
    public void DockableControl_UpdatesPinnedBounds_WhenNotInToolPinItemControl()
    {
        var tool = new Tool();
        var dockableControl = new DockableControl
        {
            TrackingMode = TrackingMode.Pinned,
            Width = 120,
            Height = 80
        };

        var host = new Grid
        {
            Width = 200,
            Height = 200,
            Children = { dockableControl }
        };

        var window = new Window
        {
            Width = 300,
            Height = 300,
            Content = host
        };

        try
        {
            dockableControl.DataContext = tool;
            window.Show();
            window.UpdateLayout();
            dockableControl.InvalidateMeasure();
            window.UpdateLayout();

            tool.GetPinnedBounds(out _, out _, out var width, out var height);
            Assert.Equal(120, width, 3);
            Assert.Equal(80, height, 3);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void DockableControl_DoesNotUpdatePinnedBounds_WhenInsideToolPinItemControl()
    {
        var tool = new Tool();
        var dockableControl = new DockableControl
        {
            TrackingMode = TrackingMode.Pinned,
            Width = 120,
            Height = 80
        };

        var pinItem = new ToolPinItemControl
        {
            Width = 200,
            Height = 200,
            Template = new FuncControlTemplate<ToolPinItemControl>((parent, scope) =>
                new Grid
                {
                    Width = 200,
                    Height = 200,
                    Children = { dockableControl }
                })
        };

        var window = new Window
        {
            Width = 300,
            Height = 300,
            Content = pinItem
        };

        try
        {
            dockableControl.DataContext = tool;
            window.Show();
            pinItem.ApplyTemplate();
            window.UpdateLayout();
            dockableControl.InvalidateMeasure();
            window.UpdateLayout();

            tool.GetPinnedBounds(out _, out _, out var width, out var height);
            Assert.True(double.IsNaN(width));
            Assert.True(double.IsNaN(height));
        }
        finally
        {
            window.Close();
        }
    }
}
