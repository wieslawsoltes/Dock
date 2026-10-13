using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class TreeExtensionTests
{
    [AvaloniaFact]
    public void Descendants_Use_Their_Own_Tree_And_Preserve_Depth_First_Child_Order()
    {
        TreeHost root = new();
        TreeHost visualParent = new(), logicalParent = new();
        Border first = new(), second = new(), last = new();
        root.AddVisual(visualParent);
        root.AddLogical(logicalParent);
        visualParent.AddVisual(first); visualParent.AddVisual(second);
        logicalParent.AddLogical(second); logicalParent.AddLogical(first);
        root.AddVisual(last); root.AddLogical(last);
        Assert.Equal(new Control[] { visualParent, first, second, last }, root.FindVisualChildren<Control>());
        Assert.Equal(new Control[] { logicalParent, second, first, last }, root.FindLogicalChildren<Control>());
        Assert.Equal(new[] { first, second, last }, root.FindVisualChildren<Border>());
        Assert.Equal(new[] { second, first, last }, root.FindLogicalChildren<Border>());
        Assert.DoesNotContain(root, root.FindVisualChildren<TreeHost>());
        Assert.DoesNotContain(root, root.FindLogicalChildren<TreeHost>());
    }

    [AvaloniaFact]
    public void Ancestors_Exclude_Self_And_Logical_Search_Prefers_The_Logical_Parent()
    {
        TreeHost root = new(), visualParent = new(), logicalParent = new();
        Border leaf = new();
        root.AddVisual(visualParent); root.AddLogical(logicalParent);
        visualParent.AddVisual(leaf); logicalParent.AddLogical(leaf);
        Assert.Same(visualParent, leaf.FindVisualAncestor<TreeHost>());
        Assert.Same(logicalParent, leaf.FindLogicalAncestor<TreeHost>());
        Assert.Same(visualParent, leaf.FindVisualAncestor<ITreeMarker>());
        Assert.Same(logicalParent, leaf.FindLogicalAncestor<ITreeMarker>());
        Assert.Null(root.FindVisualAncestor<TreeHost>());
        Assert.Null(root.FindLogicalAncestor<TreeHost>());
        Assert.Null(leaf.FindVisualAncestor<Window>());
        Assert.Equal(new AvaloniaObject[] { leaf, logicalParent, root }, leaf.FindLogicalAncestorsAndSelf());
        Assert.Same(root, leaf.FindVisualTreeRoot());
    }

    [AvaloniaFact]
    public void Logical_Ancestors_Fall_Back_To_Visual_Parents_For_Template_Children()
    {
        TreeHost root = new(), template = new();
        Border leaf = new();
        root.AddVisual(template); template.AddVisual(leaf);
        Assert.Null(leaf.Parent);
        Assert.Same(template, leaf.FindLogicalAncestor<TreeHost>());
        Assert.Equal(new AvaloniaObject[] { leaf, template, root }, leaf.FindLogicalAncestorsAndSelf());
    }

    [AvaloniaFact]
    public void A_Nonvisual_Logical_Node_Can_Lead_Back_To_The_Visual_Root()
    {
        TreeHost root = new(), parent = new();
        LogicalNode node = new();
        root.AddVisual(parent); parent.AddLogical(node);
        Assert.Same(root, node.FindVisualTreeRoot());
        Assert.Same(parent, node.FindLogicalAncestor<TreeHost>());
        Assert.Equal(new AvaloniaObject[] { node, parent, root }, node.FindLogicalAncestorsAndSelf());
        Assert.Same(node, Assert.Single(parent.FindLogicalChildren<LogicalNode>()));
        Assert.Empty(parent.FindVisualChildren<LogicalNode>());
        Assert.Throws<InvalidOperationException>(() => node.FindVisualChildren<Visual>().ToArray());
        Assert.Throws<InvalidOperationException>(() => node.FindVisualAncestor<TreeHost>());
    }

    [AvaloniaFact]
    public void Null_And_Nonvisual_Inputs_Preserve_The_Upstream_Empty_And_Error_Contracts()
    {
        AvaloniaObject? missing = null;
        Assert.Empty(missing.FindVisualChildren<Control>());
        Assert.Empty(missing.FindLogicalChildren<Control>());
        Assert.Empty(missing.FindLogicalAncestorsAndSelf());
        Assert.Null(missing.FindVisualTreeRoot());
        Assert.Throws<ArgumentNullException>(() => missing!.FindVisualAncestor<Control>());
        Assert.Throws<ArgumentNullException>(() => missing!.FindLogicalAncestor<Control>());
        AvaloniaObject nonvisual = new();
        Assert.Empty(nonvisual.FindLogicalChildren<AvaloniaObject>());
        Assert.Same(nonvisual, nonvisual.FindVisualTreeRoot());
        Assert.Throws<InvalidOperationException>(() => nonvisual.FindVisualChildren<Control>().ToArray());
        Assert.Throws<InvalidOperationException>(() => nonvisual.FindLogicalAncestor<Control>());
        using var enumerator = nonvisual.FindLogicalAncestorsAndSelf().GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Same(nonvisual, enumerator.Current);
        Assert.Throws<InvalidOperationException>(() => enumerator.MoveNext());
    }

    [AvaloniaFact]
    public void Searches_Observe_Reparenting_And_Enumeration_Is_Deferred()
    {
        TreeHost first = new(), second = new();
        Border leaf = new();
        var visuals = first.FindVisualChildren<Border>();
        var logicals = first.FindLogicalChildren<Border>();
        first.AddVisual(leaf); first.AddLogical(leaf);
        Assert.Same(leaf, Assert.Single(visuals));
        Assert.Same(leaf, Assert.Single(logicals));
        first.RemoveVisual(leaf); first.RemoveLogical(leaf);
        second.AddVisual(leaf); second.AddLogical(leaf);
        Assert.Empty(visuals); Assert.Empty(logicals);
        Assert.Same(second, leaf.FindVisualTreeRoot());
        Assert.Same(second, leaf.FindLogicalAncestor<TreeHost>());
        Assert.Same(second, leaf.FindVisualAncestor<TreeHost>());
    }

    [AvaloniaFact]
    public void Helpers_Reach_Real_Dock_Content_And_AutoHide_Without_Traversing_Layout_Models()
    {
        AnchorControlFixture window = new();
        DockingManager manager = window.FindControl<DockingManager>("Manager")!;
        try
        {
            window.Show(); Update();
            LayoutAnchorable tool = manager.Layout.LeftSide!.Children[0].Children[0];
            tool.IsSelected = true; Update();
            Control view = manager.GetLayoutItemFromModel(tool)!.View;
            Assert.Same(manager.AutoHideWindow, Assert.Single(manager.FindVisualChildren<LayoutAutoHideWindowControl>()));
            Control body = Assert.IsAssignableFrom<Control>(manager.AutoHideWindow!.Content);
            Assert.Contains(body, manager.AutoHideWindow.FindLogicalChildren<Control>());
            Assert.DoesNotContain(manager.AutoHideWindow, manager.FindLogicalChildren<Control>());
            Assert.Contains(view, manager.FindVisualChildren<Control>());
            Assert.Same(manager, view.FindLogicalAncestor<DockingManager>());
            Assert.Same(manager.AutoHideWindow, view.FindVisualAncestor<LayoutAutoHideWindowControl>());
            Visual host = window.GetVisualParent() ?? window;
            Assert.Null(host.GetVisualParent());
            Assert.Same(host, view.FindVisualTreeRoot());
            Assert.Same(window, view.FindVisualAncestor<Window>());
            Assert.Contains(manager, view.FindLogicalAncestorsAndSelf());
            Assert.Empty(manager.FindLogicalChildren<LayoutRoot>());
            manager.Layout = new LayoutRoot(); Update();
            Assert.DoesNotContain(view, manager.FindVisualChildren<Control>());
            window.Content = null; Update();
            Assert.Same(manager, manager.FindVisualTreeRoot());
        }
        finally { window.Close(); Dispatcher.UIThread.RunJobs(); }

        void Update() { Dispatcher.UIThread.RunJobs(); window.UpdateLayout(); }
    }

    private interface ITreeMarker { }
    private sealed class TreeHost : Control, ITreeMarker
    {
        public void AddVisual(Visual child) => VisualChildren.Add(child);
        public void RemoveVisual(Visual child) => VisualChildren.Remove(child);
        public void AddLogical(StyledElement child) => LogicalChildren.Add(child);
        public void RemoveLogical(StyledElement child) => LogicalChildren.Remove(child);
    }
    private sealed class LogicalNode : StyledElement { }
}
