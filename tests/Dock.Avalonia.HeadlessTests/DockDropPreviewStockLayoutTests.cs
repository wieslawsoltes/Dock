using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;
using Xunit.Abstractions;

namespace Dock.Avalonia.HeadlessTests;

public class DockDropPreviewStockLayoutTests(ITestOutputHelper output)
{
    [AvaloniaTheory]
    [InlineData(false, DockOperation.Left, Alignment.Left)]
    [InlineData(false, DockOperation.Right, Alignment.Left)]
    [InlineData(false, DockOperation.Top, Alignment.Left)]
    [InlineData(false, DockOperation.Bottom, Alignment.Left)]
    [InlineData(true, DockOperation.Left, Alignment.Left)]
    [InlineData(true, DockOperation.Right, Alignment.Left)]
    [InlineData(true, DockOperation.Top, Alignment.Left)]
    [InlineData(true, DockOperation.Bottom, Alignment.Left)]
    [InlineData(false, DockOperation.Left, Alignment.Right)]
    [InlineData(false, DockOperation.Right, Alignment.Right)]
    [InlineData(false, DockOperation.Top, Alignment.Right)]
    [InlineData(false, DockOperation.Bottom, Alignment.Right)]
    [InlineData(true, DockOperation.Left, Alignment.Right)]
    [InlineData(true, DockOperation.Right, Alignment.Right)]
    [InlineData(true, DockOperation.Top, Alignment.Right)]
    [InlineData(true, DockOperation.Bottom, Alignment.Right)]
    [InlineData(false, DockOperation.Left, Alignment.Top)]
    [InlineData(false, DockOperation.Right, Alignment.Top)]
    [InlineData(false, DockOperation.Top, Alignment.Top)]
    [InlineData(false, DockOperation.Bottom, Alignment.Top)]
    [InlineData(true, DockOperation.Left, Alignment.Top)]
    [InlineData(true, DockOperation.Right, Alignment.Top)]
    [InlineData(true, DockOperation.Top, Alignment.Top)]
    [InlineData(true, DockOperation.Bottom, Alignment.Top)]
    [InlineData(false, DockOperation.Left, Alignment.Bottom)]
    [InlineData(false, DockOperation.Right, Alignment.Bottom)]
    [InlineData(false, DockOperation.Top, Alignment.Bottom)]
    [InlineData(false, DockOperation.Bottom, Alignment.Bottom)]
    [InlineData(true, DockOperation.Left, Alignment.Bottom)]
    [InlineData(true, DockOperation.Right, Alignment.Bottom)]
    [InlineData(true, DockOperation.Top, Alignment.Bottom)]
    [InlineData(true, DockOperation.Bottom, Alignment.Bottom)]
    public void Pinned_sidebar_is_excluded_from_projected_geometry(bool global, DockOperation operation, Alignment sidebar)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(0.5, 0.5);
        var pinned = factory.CreateTool();
        pinned.Title = "Pinned tool";
        var pinnedTools = factory.CreateList<IDockable>(pinned);
        switch (sidebar)
        {
            case Alignment.Left: root.LeftPinnedDockables = pinnedTools; break;
            case Alignment.Right: root.RightPinnedDockables = pinnedTools; break;
            case Alignment.Top: root.TopPinnedDockables = pinnedTools; break;
            case Alignment.Bottom: root.BottomPinnedDockables = pinnedTools; break;
        }
        factory.InitLayout(root);
        var (_, _, sources) = DockDropPreviewTests.CreateLayout(1.0);
        var source = sources[0].ActiveDockable!;
        var control = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.Contains(control.GetVisualDescendants().OfType<ToolPinnedControl>(), x => x.IsVisible && x.Bounds.Width > 0);
            var target = global ? panes[1].Owner! : panes[1];
            var proportion = global ? 0.33 : double.NaN;
            var preview = new DockDropPreviewService().GetBounds(source, target, operation, control, proportion)!.Value;
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, target, DragAction.Move, operation, true));
            if (global) Assert.True(DockSplitProportion.Apply(Assert.IsAssignableFrom<IDock>(source.Owner), proportion));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var actual = new Rect(realized.TranslatePoint(default, control)!.Value, realized.Bounds.Size);
            output.WriteLine($"Preview={preview}; actual={actual}");
            DockDropPreviewTests.AssertBounds(preview, actual);
        }
        finally { window.Close(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void First_split_uses_the_styled_splitter_thickness(bool changeAfterHover)
    {
        var (factory, root, panes) = DockDropPreviewTests.CreateLayout(1.0);
        var (_, _, sources) = DockDropPreviewTests.CreateLayout(1.0);
        var source = sources[0].ActiveDockable!;
        var control = new DockControl { Layout = root };
        var window = new Window { Width = 1000, Height = 600, Content = control };
        var style = new Style(x => x.OfType<ProportionalStackPanelSplitter>())
        {
            Setters = { new Setter(ProportionalStackPanelSplitter.ThicknessProperty, 20d) }
        };
        if (!changeAfterHover) window.Styles.Add(style);
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.Empty(control.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>());
            var service = new DockDropPreviewService();
            if (changeAfterHover)
            {
                Assert.NotNull(service.GetBounds(source, panes[0], DockOperation.Right, control, double.NaN));
                window.Styles.Add(style);
                Dispatcher.UIThread.RunJobs();
                window.UpdateLayout();
                Assert.False(service.IsCurrent(source, panes[0], DockOperation.Right, control, double.NaN));
            }
            var preview = service.GetBounds(source, panes[0], DockOperation.Right, control, double.NaN)!.Value;
            Assert.True(new DockManager(new DockService()).ValidateDockable(source, panes[0], DragAction.Move, DockOperation.Right, true));
            Dispatcher.UIThread.RunJobs();
            window.UpdateLayout();
            Assert.Equal(20, Assert.Single(control.GetVisualDescendants().OfType<ProportionalStackPanelSplitter>()).Thickness);
            var realized = Assert.IsAssignableFrom<Control>(factory.VisibleDockableControls[source.Owner!]);
            var actual = new Rect(realized.TranslatePoint(default, control)!.Value, realized.Bounds.Size);
            output.WriteLine($"Preview={preview}; actual={actual}");
            DockDropPreviewTests.AssertBounds(preview, actual);
        }
        finally { window.Close(); }
    }
}
