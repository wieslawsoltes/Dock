using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingBoundsTests
{
    [AvaloniaFact]
    public void Document_Geometry_Configures_Window_And_Tracks_Native_Changes()
    {
        LayoutDocument document = new()
        {
            FloatingLeft = 40, FloatingTop = 60, FloatingWidth = 520, FloatingHeight = 380, IsMaximized = true
        };
        LayoutDocumentFloatingWindow window = new() { RootDocument = document };
        Assert.Equal(40, window.DockWindow.X);
        Assert.Equal(60, window.DockWindow.Y);
        Assert.Equal(520, window.DockWindow.Width);
        Assert.Equal(380, window.DockWindow.Height);
        Assert.Equal(DockWindowState.Maximized, window.DockWindow.WindowState);

        window.DockWindow.X = 80;
        window.DockWindow.Y = 100;
        window.DockWindow.Width = 600;
        window.DockWindow.Height = 450;
        window.DockWindow.WindowState = DockWindowState.Normal;
        Assert.Equal(80, document.FloatingLeft);
        Assert.Equal(100, document.FloatingTop);
        Assert.Equal(600, document.FloatingWidth);
        Assert.Equal(450, document.FloatingHeight);
        Assert.False(document.IsMaximized);
        document.FloatingWidth = 720;
        Assert.Equal(720, window.DockWindow.Width);
    }

    [AvaloniaFact]
    public void Native_Tool_Float_Preserves_Content_Geometry_And_Maximized_State()
    {
        LayoutAnchorable tool = new()
        {
            FloatingLeft = 70, FloatingTop = 90, FloatingWidth = 540, FloatingHeight = 420, IsMaximized = true
        };
        LayoutPanel panel = new(new LayoutDocumentPane());
        panel.Children.Add(new LayoutAnchorablePane(tool));
        LayoutRoot root = new() { RootPanel = panel };
        tool.Float();
        LayoutAnchorableFloatingWindow window = Assert.IsType<LayoutAnchorableFloatingWindow>(Assert.Single(root.FloatingWindows));
        Assert.Equal(70, window.DockWindow.X);
        Assert.Equal(90, window.DockWindow.Y);
        Assert.Equal(540, window.DockWindow.Width);
        Assert.Equal(420, window.DockWindow.Height);
        Assert.Equal(DockWindowState.Maximized, window.DockWindow.WindowState);
        Assert.True(tool.IsMaximized);
        Assert.Equal(540, window.RootPanel!.FloatingWidth);
    }

    [AvaloniaFact]
    public void Tool_Geometry_Follows_Nested_Tree_Changes_And_Unsubscribes_Removed_Content()
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane pane = new(tool);
        LayoutAnchorablePaneGroup panel = new(pane)
        {
            FloatingLeft = 20, FloatingTop = 30, FloatingWidth = 480, FloatingHeight = 360
        };
        LayoutAnchorableFloatingWindow window = new() { RootPanel = panel };
        Assert.Equal(480, tool.FloatingWidth);
        Assert.Equal(360, pane.FloatingHeight);
        pane.FloatingLeft = 100;
        Assert.Equal(100, window.DockWindow.X);
        Assert.Equal(100, tool.FloatingLeft);
        Assert.Equal(100, panel.FloatingLeft);
        window.DockWindow.Width = 640;
        LayoutAnchorable added = new();
        pane.Children.Add(added);
        Assert.Equal(640, added.FloatingWidth);
        Assert.Equal(100, added.FloatingLeft);
        pane.Children.Remove(added);
        added.FloatingWidth = 800;
        Assert.Equal(640, window.DockWindow.Width);
        window.DockWindow.WindowState = DockWindowState.Maximized;
        Assert.True(tool.IsMaximized);
        Assert.True(pane.IsMaximized);
        Assert.True(panel.IsMaximized);
        Assert.False(added.IsMaximized);
    }

    [AvaloniaFact]
    public void Live_Host_And_Public_Geometry_Update_In_Both_Directions()
    {
        FloatingFixture view = new();
        DockingManager manager = view.FindControl<DockingManager>("Manager")!;
        LayoutDocumentFloatingWindow window = (LayoutDocumentFloatingWindow)manager.Layout.FloatingWindows[0];
        LayoutDocument document = window.RootDocument!;
        try
        {
            view.Show();
            Dispatcher.UIThread.RunJobs();
            HostWindow host = Assert.IsAssignableFrom<HostWindow>(window.DockWindow.Host);
            Assert.Equal(400, host.Width);
            Assert.Equal(300, host.Height);
            document.FloatingWidth = 540;
            document.FloatingHeight = 420;
            document.FloatingLeft = 70;
            document.FloatingTop = 90;
            Assert.Equal(540, host.Width);
            Assert.Equal(420, host.Height);
            Assert.Equal(new PixelPoint(70, 90), host.Position);

            host.Width = 620;
            host.Height = 460;
            host.Position = new PixelPoint(110, 130);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(620, document.FloatingWidth);
            Assert.Equal(460, document.FloatingHeight);
            Assert.Equal(110, document.FloatingLeft);
            Assert.Equal(130, document.FloatingTop);
            host.WindowState = WindowState.Maximized;
            Assert.True(document.IsMaximized);
            Assert.Equal(620, document.FloatingWidth);
            document.IsMaximized = false;
            Assert.Equal(WindowState.Normal, host.WindowState);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view.Close();
        }
    }
}
