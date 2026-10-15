using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Infrastructure;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingResizeBorderTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Both_Native_Host_Types_Use_Ten_Unit_Resize_Edges_By_Default(bool tool)
    {
        using Workspace workspace = new();
        LayoutFloatingWindowControl host = workspace.Float(tool);
        Assert.Equal(new Thickness(10), host.ResizeBorderThickness);
        VerifyEdges(host, new Thickness(10));
        host.ResizeBorderThickness = new(4, 6, 8, 12);
        Pump(host);
        VerifyEdges(host, host.ResizeBorderThickness);
        Assert.Same(host, ((LayoutFloatingWindow)host.Model).DockWindow.Host);
    }

    [AvaloniaFact]
    public void Literal_And_Compiled_Style_Bindings_Update_The_Resize_Hit_Areas()
    {
        FloatingResizeFixture host = new();
        try
        {
            host.Show();
            Pump(host);
            Assert.Equal(new Thickness(4, 6, 8, 12), host.ResizeBorderThickness);
            VerifyEdges(host, host.ResizeBorderThickness);
            host.ClearValue(LayoutFloatingWindowControl.ResizeBorderThicknessProperty);
            host.BorderThickness = new(7, 9, 11, 13);
            host.Classes.Add("bound-resize");
            Pump(host);
            Assert.Equal(host.BorderThickness, host.ResizeBorderThickness);
            VerifyEdges(host, host.BorderThickness);
            host.BorderThickness = new(12, 14, 16, 18);
            Pump(host);
            Assert.Equal(host.BorderThickness, host.ResizeBorderThickness);
            VerifyEdges(host, host.BorderThickness);
        }
        finally { host.Close(); }
    }

    [AvaloniaFact]
    public void Resize_Input_Uses_The_Grip_Without_Taking_Center_Content_Input()
    {
        FloatingResizeFixture host = new();
        try
        {
            host.Show();
            Pump(host);
            bool handled = false;
            object? source = null;
            host.AddHandler(InputElement.PointerPressedEvent, (_, args) =>
            {
                handled = args.Handled;
                source = args.Source;
            }, RoutingStrategies.Bubble, handledEventsToo: true);
            Point edge = new(1, host.Bounds.Height / 2);
            host.MouseDown(edge, MouseButton.Left);
            host.MouseUp(edge, MouseButton.Left);
            Assert.True(handled);
            Assert.IsType<LayoutWindowResizeGrip>(source);
            host.MouseDown(edge, MouseButton.Right);
            host.MouseUp(edge, MouseButton.Right);
            Assert.False(handled);

            Button button = host.FindControl<Button>("ContentButton")!;
            int clicks = 0;
            button.Click += (_, _) => clicks++;
            Point center = button.TranslatePoint(new(button.Bounds.Width / 2, button.Bounds.Height / 2), host)!.Value;
            Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(center));
            host.MouseDown(center, MouseButton.Left);
            host.MouseUp(center, MouseButton.Left);
            Assert.Equal(1, clicks);
        }
        finally { host.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Fixed_Size_And_Maximized_Hosts_Do_Not_Expose_Resize_Grips(bool tool)
    {
        using Workspace workspace = new();
        LayoutFloatingWindowControl host = workspace.Float(tool);
        Point edge = new(1, host.Bounds.Height / 2);
        Assert.IsType<LayoutWindowResizeGrip>(host.InputHitTest(edge));
        host.CanResize = false;
        Pump(host);
        Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(edge));
        host.CanResize = true;
        foreach (WindowState state in new[] { WindowState.Maximized, WindowState.FullScreen })
        {
            host.WindowState = state;
            Pump(host);
            Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(edge));
        }
        host.WindowState = WindowState.Normal;
        Pump(host);
        Assert.IsType<LayoutWindowResizeGrip>(host.InputHitTest(edge));
        host.ResizeBorderThickness = default;
        Pump(host);
        Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(edge));
    }

    [AvaloniaFact]
    public void Hidden_Restored_Hosts_Keep_Their_Resize_Configuration_And_Are_Independent()
    {
        using Workspace workspace = new();
        LayoutFloatingWindowControl document = workspace.Float(false);
        LayoutFloatingWindowControl tool = workspace.Float(true);
        tool.ResizeBorderThickness = new(14);
        workspace.Tool.Hide();
        workspace.Tool.Show();
        Pump(tool);
        Assert.Same(tool, workspace.Tool.FindParent<LayoutFloatingWindow>()!.DockWindow.Host);
        Assert.Equal(new Thickness(10), document.ResizeBorderThickness);
        VerifyEdges(tool, new(14));
    }

    private static void VerifyEdges(LayoutFloatingWindowControl host, Thickness thickness)
    {
        double width = host.Bounds.Width;
        double height = host.Bounds.Height;
        (Point Point, WindowDecorationsElementRole Role)[] probes =
        {
            (new(thickness.Left / 2, height / 2), WindowDecorationsElementRole.ResizeW),
            (new(width - thickness.Right / 2, height / 2), WindowDecorationsElementRole.ResizeE),
            (new(width / 2, thickness.Top / 2), WindowDecorationsElementRole.ResizeN),
            (new(width / 2, height - thickness.Bottom / 2), WindowDecorationsElementRole.ResizeS),
            (new(thickness.Left / 2, thickness.Top / 2), WindowDecorationsElementRole.ResizeNW),
            (new(width - thickness.Right / 2, thickness.Top / 2), WindowDecorationsElementRole.ResizeNE),
            (new(thickness.Left / 2, height - thickness.Bottom / 2), WindowDecorationsElementRole.ResizeSW),
            (new(width - thickness.Right / 2, height - thickness.Bottom / 2), WindowDecorationsElementRole.ResizeSE)
        };
        Assert.Equal(8, host.GetVisualDescendants().OfType<LayoutWindowResizeGrip>().Count());
        foreach ((Point point, WindowDecorationsElementRole role) in probes)
        {
            IInputElement? hit = host.InputHitTest(point);
            Assert.True(hit is LayoutWindowResizeGrip, $"Expected {role} at {point}, host {host.Bounds}, hit {hit}.");
            LayoutWindowResizeGrip grip = (LayoutWindowResizeGrip)hit!;
            Assert.Equal(role, WindowDecorationProperties.GetElementRole(grip));
            Assert.NotNull(grip.Cursor);
        }
        Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(new(thickness.Left + 1, height / 2)));
        Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(new(width - thickness.Right - 1, height / 2)));
        Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(new(width / 2, thickness.Top + 1)));
        Assert.IsNotType<LayoutWindowResizeGrip>(host.InputHitTest(new(width / 2, height - thickness.Bottom - 1)));
    }

    private static void Pump(Window host)
    {
        Dispatcher.UIThread.RunJobs();
        host.UpdateLayout();
        host.MouseMove(new(host.Bounds.Width / 2, host.Bounds.Height / 2));
    }

    private sealed class Workspace : IDisposable
    {
        internal DockingManager Manager { get; } = new();
        internal LayoutDocument Document { get; } = new() { Title = "Document", Content = new TextBox { Text = "Document" } };
        internal LayoutAnchorable Tool { get; } = new() { Title = "Tool", Content = new TextBox { Text = "Tool" } };
        private readonly Window _view;

        internal Workspace()
        {
            LayoutPanel panel = new(new LayoutDocumentPane(Document));
            panel.Children.Add(new LayoutAnchorablePane(Tool));
            Manager.Layout = new() { RootPanel = panel };
            _view = new() { Width = 800, Height = 500, Content = Manager };
            _view.Show();
            Pump(_view);
        }

        internal LayoutFloatingWindowControl Float(bool tool)
        {
            LayoutContent content = tool ? Tool : Document;
            content.Float();
            LayoutFloatingWindowControl host = (LayoutFloatingWindowControl)content.FindParent<LayoutFloatingWindow>()!.DockWindow.Host!;
            host.Width = 420;
            host.Height = 280;
            Pump(host);
            return host;
        }

        public void Dispose()
        {
            Manager.Layout.FloatingWindows.Clear();
            _view.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
