using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PinnedDockRemainingContentTests
{
    [AvaloniaTheory]
    [InlineData(Alignment.Left, false)]
    [InlineData(Alignment.Right, false)]
    [InlineData(Alignment.Top, false)]
    [InlineData(Alignment.Bottom, false)]
    [InlineData(Alignment.Left, true)]
    [InlineData(Alignment.Right, true)]
    [InlineData(Alignment.Top, true)]
    [InlineData(Alignment.Bottom, true)]
    public void Overlay_Reserve_Updates_Layout_Without_Changing_Stored_Or_Rendered_Content_Size(Alignment alignment, bool fullExtent)
    {
        using Workspace workspace = new(alignment, PinnedDockDisplayMode.Overlay, fullExtent);
        Assert.Equal(50, workspace.Control.MinimumRemainingContentSize);
        foreach (double minimum in new[] { 50d, 0d, 25d, 50d })
        {
            workspace.Control.MinimumRemainingContentSize = minimum;
            workspace.Window.UpdateLayout();
            Assert.Equal(workspace.RequestedSize, workspace.ContentSize, 1);
            double remaining = Math.Max(minimum, workspace.AvailableSize - workspace.RequestedSize - 6);
            Assert.Equal(remaining, workspace.RemainingSize, 1);
            Assert.Equal(workspace.Trailing ? remaining + 6 : 0, workspace.ContentOrigin, 1);
            if (!fullExtent && minimum <= 25 && workspace.Trailing)
                Assert.Equal(workspace.AvailableSize, workspace.ContentOrigin + workspace.ContentSize, 1);
            workspace.Tool.GetPinnedBounds(out _, out _, out double width, out double height);
            Assert.Equal(workspace.RequestedSize, workspace.Horizontal ? width : height);
        }
    }

    [AvaloniaTheory]
    [InlineData(Alignment.Left)]
    [InlineData(Alignment.Right)]
    [InlineData(Alignment.Top)]
    [InlineData(Alignment.Bottom)]
    public void Inline_Layout_Does_Not_Reserve_An_Overlay_Region(Alignment alignment)
    {
        using Workspace workspace = new(alignment, PinnedDockDisplayMode.Inline, false);
        Rect originalContent = workspace.Content.Bounds;
        Rect originalHost = workspace.Control.Bounds;
        workspace.Control.MinimumRemainingContentSize = 200;
        workspace.Window.UpdateLayout();
        Assert.Equal(originalContent, workspace.Content.Bounds);
        Assert.Equal(originalHost, workspace.Control.Bounds);
        Assert.Equal(2, workspace.Horizontal ? workspace.Grid.ColumnDefinitions.Count : workspace.Grid.RowDefinitions.Count);
    }

    [AvaloniaTheory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    [InlineData(-1)]
    public void Remaining_Minimum_Rejects_Invalid_Values(double value)
    {
        PinnedDockControl control = new();
        Assert.Throws<ArgumentException>(() => control.MinimumRemainingContentSize = value);
        Assert.Equal(50, control.MinimumRemainingContentSize);
        control.MinimumRemainingContentSize = 0;
        Assert.Equal(0, control.MinimumRemainingContentSize);
    }

    private sealed class Workspace : IDisposable
    {
        private readonly bool _previousWindowSetting = DockSettings.UsePinnedDockWindow;
        public bool Horizontal { get; }
        public bool Trailing { get; }
        public double AvailableSize => Horizontal ? 800 : 600;
        public double RequestedSize { get; }
        public Window Window { get; }
        public PinnedDockControl Control { get; }
        public Tool Tool { get; } = new();
        public Grid Grid { get; } = new() { Name = "PART_PinnedDockGrid" };
        public ContentControl Content { get; } = new() { Name = "PART_PinnedDock", Content = "Preview" };
        public double ContentSize => Horizontal ? Content.Bounds.Width : Content.Bounds.Height;
        public double ContentOrigin => Horizontal ? Content.Bounds.X : Content.Bounds.Y;
        public double RemainingSize => Horizontal
            ? Grid.ColumnDefinitions.Single(definition => definition.Width.IsStar).ActualWidth
            : Grid.RowDefinitions.Single(definition => definition.Height.IsStar).ActualHeight;

        public Workspace(Alignment alignment, PinnedDockDisplayMode mode, bool fullExtent)
        {
            DockSettings.UsePinnedDockWindow = false;
            Horizontal = alignment is Alignment.Left or Alignment.Right;
            Trailing = alignment is Alignment.Right or Alignment.Bottom;
            RequestedSize = fullExtent ? AvailableSize : AvailableSize - 31;
            Tool.SetPinnedBounds(0, 0, Horizontal ? RequestedSize : 100, Horizontal ? 100 : RequestedSize);
            Factory factory = new();
            RootDock root = new()
            {
                Factory = factory,
                PinnedDock = new ToolDock
                {
                    Alignment = alignment, IsEmpty = false,
                    VisibleDockables = factory.CreateList<IDockable>(Tool), ActiveDockable = Tool
                }
            };
            Control = new PinnedDockControl { DataContext = root, PinnedDockAlignment = alignment, PinnedDockDisplayMode = mode };
            Control.Template = new FuncControlTemplate<PinnedDockControl>((_, scope) =>
            {
                Thumb splitter = new()
                {
                    Name = "PART_PinnedDockSplitter",
                    Width = Horizontal ? 6 : double.NaN,
                    Height = Horizontal ? double.NaN : 6
                };
                scope.Register(Grid.Name!, Grid);
                scope.Register(Content.Name!, Content);
                scope.Register(splitter.Name, splitter);
                Grid.Children.Add(Content);
                Grid.Children.Add(splitter);
                return Grid;
            });
            Window = new Window
            {
                Width = 800, Height = 600,
                Content = new PinnedDockHostPanel
                {
                    DataContext = root, PinnedDockDisplayMode = mode, PinnedDockAlignment = alignment,
                    Children = { new Border(), Control }
                }
            };
            Window.Show();
            Window.UpdateLayout();
        }

        public void Dispose()
        {
            Window.Close();
            DockSettings.UsePinnedDockWindow = _previousWindowSetting;
        }
    }
}
