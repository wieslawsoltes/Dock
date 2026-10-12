using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using Dock.Controls.ProportionalStackPanel;
using Xunit;

namespace Dock.Avalonia.UnitTests.Controls;

public class ProportionalSplitterHostTests
{
    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal, false, false)]
    [InlineData(Orientation.Vertical, false, false)]
    [InlineData(Orientation.Horizontal, true, false)]
    [InlineData(Orientation.Vertical, true, false)]
    [InlineData(Orientation.Horizontal, false, true)]
    [InlineData(Orientation.Vertical, false, true)]
    [InlineData(Orientation.Horizontal, true, true)]
    [InlineData(Orientation.Vertical, true, true)]
    public void Grid_Host_Receives_Native_Pointer_Resizes(Orientation orientation, bool preview, bool wrapped)
    {
        using Workspace workspace = new(orientation, preview, wrapped);
        Point point = workspace.Pointer;
        Vector delta = orientation == Orientation.Horizontal ? new Vector(30, 0) : new Vector(0, 30);
        workspace.Window.MouseDown(point, MouseButton.Left);
        workspace.Window.MouseMove(point + delta);
        Assert.Equal(preview ? 0 : 1, workspace.Host.Resizes);
        workspace.Window.MouseUp(point + delta, MouseButton.Left);
        Assert.Equal(1, workspace.Host.Resizes);
        Assert.Same(workspace.Host.Children[0], workspace.Host.Target);
        Assert.Same(workspace.Host.Children[2], workspace.Host.Neighbor);
        Assert.Equal(30, workspace.Host.Delta);
        Assert.Equal(25, workspace.Host.Minimum);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Orientation_Change_Or_Detach_Cancels_The_Pending_Preview(bool detach)
    {
        using Workspace workspace = new(Orientation.Horizontal, true, false);
        Point point = workspace.Pointer;
        workspace.Window.MouseDown(point, MouseButton.Left);
        workspace.Window.MouseMove(point + new Vector(30, 0));
        Assert.Contains(":preview", workspace.Splitter.Classes);
        if (detach) workspace.Window.Content = null;
        else workspace.Host.SetOrientation(Orientation.Vertical);
        workspace.Window.MouseUp(point + new Vector(30, 0), MouseButton.Left);
        Assert.Equal(0, workspace.Host.Resizes);
        Assert.DoesNotContain(":preview", workspace.Splitter.Classes);
        if (detach)
        {
            Assert.Equal(0, workspace.Host.SubscriberCount);
            workspace.Window.Content = workspace.Host;
            workspace.Update();
            Assert.Equal(1, workspace.Host.SubscriberCount);
        }
        else
        {
            Assert.Contains(":vertical", workspace.Splitter.Classes);
            Assert.DoesNotContain(":horizontal", workspace.Splitter.Classes);
            Assert.Equal(10, workspace.Splitter.Height);
            Assert.True(double.IsNaN(workspace.Splitter.Width));
        }
    }

    [AvaloniaFact]
    public void Disabled_Splitter_Does_Not_Invoke_The_Host()
    {
        using Workspace workspace = new(Orientation.Horizontal, false, false);
        workspace.Splitter.IsResizingEnabled = false;
        Point point = workspace.Pointer;
        workspace.Window.MouseDown(point, MouseButton.Left);
        workspace.Window.MouseMove(point + new Vector(30, 0));
        workspace.Window.MouseUp(point + new Vector(30, 0), MouseButton.Left);
        Assert.Equal(0, workspace.Host.Resizes);
    }

    private sealed class Workspace : IDisposable
    {
        public Host Host { get; } = new();
        public ProportionalStackPanelSplitter Splitter { get; }
        public Window Window { get; }
        public Point Pointer => Splitter.TranslatePoint(new Point(Splitter.Bounds.Width / 2, Splitter.Bounds.Height / 2), Window)!.Value;
        public Workspace(Orientation orientation, bool preview, bool wrapped)
        {
            Host.SetOrientation(orientation);
            Splitter = new()
            {
                Thickness = 10, PreviewResize = preview,
                Template = new FuncControlTemplate<ProportionalStackPanelSplitter>((_, _) => new Border { Background = Brushes.Gray })
            };
            ProportionalStackPanelSplitter.SetMinimumProportionSize(Splitter, 25);
            Host.Children.Add(new Border { Background = Brushes.White });
            Host.Children.Add(wrapped ? new ContentPresenter { Content = Splitter } : Splitter);
            Host.Children.Add(new Border { Background = Brushes.White });
            Host.SetOrientation(orientation);
            Window = new() { Content = Host, Width = 600, Height = 500 };
            Window.Show(); Update();
        }
        public void Update() { Dispatcher.UIThread.RunJobs(); Window.UpdateLayout(); }
        public void Dispose() { Window.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private sealed class Host : Grid, IProportionalSplitterHost
    {
        private EventHandler? _orientationChanged;
        public Panel Panel => this;
        public Orientation Orientation { get; private set; }
        public int SubscriberCount { get; private set; }
        public event EventHandler? OrientationChanged
        {
            add { _orientationChanged += value; SubscriberCount++; }
            remove { _orientationChanged -= value; SubscriberCount--; }
        }
        public int Resizes { get; private set; }
        public Control? Target { get; private set; }
        public Control? Neighbor { get; private set; }
        public double Delta { get; private set; }
        public double Minimum { get; private set; }
        public void Resize(Control target, Control neighbor, double dragDelta, double minimumSize)
        {
            Resizes++; Target = target; Neighbor = neighbor; Delta = dragDelta; Minimum = minimumSize;
        }
        public void SetOrientation(Orientation value)
        {
            bool changed = Orientation != value;
            Orientation = value;
            ColumnDefinitions = value == Orientation.Horizontal ? new ColumnDefinitions("*,10,*") : new ColumnDefinitions();
            RowDefinitions = value == Orientation.Vertical ? new RowDefinitions("*,10,*") : new RowDefinitions();
            for (int i = 0; i < Children.Count; i++)
            {
                SetColumn(Children[i], value == Orientation.Horizontal ? i : 0);
                SetRow(Children[i], value == Orientation.Vertical ? i : 0);
            }
            if (changed) _orientationChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
