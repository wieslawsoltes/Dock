using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Internal;
using Dock.Controls.ProportionalStackPanel;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockDropPreviewTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Local_drop_matches_preview_when_source_pane_collapses(bool keepSourcePane)
    {
        var (factory, root, panes) = CreateLayout(0.4, 0.3, 0.3);
        var source = panes[0].ActiveDockable!;
        if (keepSourcePane) factory.AddDockable(panes[0], factory.CreateTool());
        var before = Measure(root);
        var target = before[panes[1]];
        var preview = new Rect(target.X + target.Width / 2, target.Y, target.Width / 2, target.Height);
        var manager = new DockManager(new DockService());
        Assert.True(manager.ValidateDockable(source, panes[1], DragAction.Move, DockOperation.Right, false));
        Assert.True(manager.ValidateDockable(source, panes[1], DragAction.Move, DockOperation.Right, true));
        var actual = Measure(root)[source.Owner!];
        AssertBounds(preview, actual);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Global_share_is_independent_of_intermediate_arrange(bool arrangeBeforeProportion)
    {
        var (_, targetRoot, targets) = CreateLayout(0.5, 0.5);
        var (_, sourceRoot, sources) = CreateLayout(1.0);
        var source = sources[0].ActiveDockable!;
        Measure(targetRoot);
        var manager = new DockManager(new DockService());
        Assert.True(manager.ValidateDockable(source, targets[0].Owner!, DragAction.Move, DockOperation.Left, true));
        if (arrangeBeforeProportion) Measure(targetRoot);
        Assert.True(GlobalDockingService.Instance.TryApplyGlobalDockingProportion(source, sourceRoot, targetRoot, 0.33));
        Assert.Equal(330, Measure(targetRoot)[source.Owner!].Width, 0);
    }

    [AvaloniaFact]
    public void Global_container_drag_sizes_the_inserted_pane()
    {
        var (_, targetRoot, targets) = CreateLayout(0.5, 0.5);
        var (_, sourceRoot, sources) = CreateLayout(1.0);
        var dragDock = sources[0];
        var source = dragDock.ActiveDockable!;
        Measure(targetRoot);
        // DockControlState.Execute resolves a dock drag to its active dockable.
        var manager = new DockManager(new DockService());
        Assert.True(manager.ValidateDockable(source, targets[0].Owner!, DragAction.Move, DockOperation.Left, true));
        Assert.True(GlobalDockingService.Instance.TryApplyGlobalDockingProportion(dragDock, sourceRoot, targetRoot, 0.33));
        Assert.Equal(330, Measure(targetRoot)[source.Owner!].Width, 0);
    }

    internal static (Factory factory, IRootDock root, IToolDock[] panes) CreateLayout(params double[] shares)
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        root.IsCollapsable = false;
        root.VisibleDockables = factory.CreateList<IDockable>();
        var row = factory.CreateProportionalDock();
        row.Orientation = Orientation.Horizontal;
        row.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, row);
        var panes = new IToolDock[shares.Length];
        for (var i = 0; i < shares.Length; i++)
        {
            if (i > 0) factory.AddDockable(row, factory.CreateProportionalDockSplitter());
            var pane = factory.CreateToolDock();
            pane.Id = "pane-" + i;
            pane.Proportion = pane.CollapsedProportion = shares[i];
            pane.VisibleDockables = factory.CreateList<IDockable>();
            factory.AddDockable(row, pane);
            var tool = factory.CreateTool();
            tool.Id = "tool-" + i;
            factory.AddDockable(pane, tool);
            pane.ActiveDockable = tool;
            panes[i] = pane;
        }
        factory.InitLayout(root);
        return (factory, root, panes);
    }

    internal static Dictionary<IDockable, Rect> Measure(IDock root)
    {
        var controls = new Dictionary<IDockable, Control>();
        Control Build(IDockable model)
        {
            Control control;
            if (model is IProportionalDock row)
            {
                var panel = new ProportionalStackPanel
                {
                    Orientation = row.Orientation == Orientation.Horizontal
                        ? global::Avalonia.Layout.Orientation.Horizontal : global::Avalonia.Layout.Orientation.Vertical
                };
                foreach (var child in row.VisibleDockables!)
                    if (child is not IProportionalDockSplitter) panel.Children.Add(Build(child));
                control = panel;
            }
            else if (model is IRootDock dock) return Build(dock.VisibleDockables![0]);
            else control = new Border();
            controls[model] = control;
            ProportionalStackPanel.SetProportion(control, model.Proportion);
            ProportionalStackPanel.SetCollapsedProportion(control, model.CollapsedProportion);
            return control;
        }
        var visual = Build(root);
        visual.Measure(new Size(1000, 600));
        visual.Arrange(new Rect(0, 0, 1000, 600));
        var result = new Dictionary<IDockable, Rect>();
        foreach (var pair in controls)
        {
            var position = pair.Value.TranslatePoint(default, visual)!.Value;
            result[pair.Key] = new Rect(position, pair.Value.Bounds.Size);
            // Mirror the production panel's two-way proportion binding without reflection bindings.
            pair.Key.Proportion = ProportionalStackPanel.GetProportion(pair.Value);
            pair.Key.CollapsedProportion = ProportionalStackPanel.GetCollapsedProportion(pair.Value);
        }
        return result;
    }

    internal static void AssertBounds(Rect expected, Rect actual)
    {
        Assert.InRange(Math.Abs(expected.X - actual.X), 0, 1);
        Assert.InRange(Math.Abs(expected.Y - actual.Y), 0, 1);
        Assert.InRange(Math.Abs(expected.Width - actual.Width), 0, 1);
        Assert.InRange(Math.Abs(expected.Height - actual.Height), 0, 1);
    }
}
