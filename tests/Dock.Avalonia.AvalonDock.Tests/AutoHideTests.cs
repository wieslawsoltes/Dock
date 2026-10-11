using System.Linq;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Orientation = Avalonia.Layout.Orientation;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class AutoHideTests
{
    [AvaloniaFact]
    public void Declared_Side_Uses_Docks_Pinned_List_And_Public_Group_Parents()
    {
        LayoutAnchorable first = new() { ContentId = "agenda" };
        LayoutAnchorable second = new() { ContentId = "contacts" };
        LayoutAnchorGroup group = new();
        group.Children.Add(first);
        group.Children.Add(second);
        LayoutAnchorSide side = new();
        side.Children.Add(group);
        LayoutRoot root = new() { LeftSide = side };
        RootDock model = (RootDock)root.DockModel;
        Assert.Equal(new[] { first.DockModel, second.DockModel }, model.LeftPinnedDockables);
        Assert.Same(group, first.Parent);
        Assert.Same(side, group.Parent);
        Assert.Same(root, side.Parent);
        Assert.Same(root, first.Root);
        Assert.Equal(AnchorSide.Left, side.Side);
        Assert.True(first.IsAutoHidden);
        Assert.True(side.IsVisible);
        Assert.Equal(1, root.Descendents().Count(x => ReferenceEquals(x, first)));
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left)]
    [InlineData(AnchorSide.Top)]
    [InlineData(AnchorSide.Right)]
    [InlineData(AnchorSide.Bottom)]
    public void Native_Pin_And_Unpin_Preserve_Identity_And_Restore_The_Pane(AnchorSide side)
    {
        LayoutAnchorable tool = new() { ContentId = "tool" };
        LayoutAnchorablePane pane = new(tool);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        ((ToolDock)pane.DockModel).Alignment = side switch
        {
            AnchorSide.Left => Alignment.Left,
            AnchorSide.Top => Alignment.Top,
            AnchorSide.Right => Alignment.Right,
            _ => Alignment.Bottom
        };
        root.DockFactory.PinDockable(tool.DockModel);
        LayoutAnchorSide strip = GetSide(root, side);
        LayoutAnchorGroup group = Assert.Single(strip.Children);
        Assert.Same(tool, Assert.Single(group.Children));
        Assert.Same(group, tool.Parent);
        Assert.Empty(pane.Children);
        root.DockFactory.UnpinDockable(tool.DockModel);
        Assert.Empty(strip.Children);
        Assert.Same(tool, Assert.Single(pane.Children));
        Assert.Same(pane, tool.Parent);
        Assert.False(tool.IsAutoHidden);
    }

    [AvaloniaFact]
    public void Declared_Auto_Hidden_Tool_Can_Be_Restored_By_Dock()
    {
        LayoutAnchorable tool = new();
        LayoutAnchorGroup group = new();
        group.Children.Add(tool);
        LayoutRoot root = new();
        root.LeftSide.Children.Add(group);
        tool.ToggleAutoHide();
        Assert.False(tool.IsAutoHidden);
        LayoutAnchorablePane pane = Assert.IsType<LayoutAnchorablePane>(tool.Parent);
        Assert.Same(root, pane.Root);
        Assert.Contains(pane, root.Descendents());
        Assert.Empty(root.LeftSide.Children);
    }

    [AvaloniaFact]
    public void Public_Group_Changes_And_Native_List_Replacement_Are_Synchronized()
    {
        LayoutRoot root = new();
        LayoutAnchorGroup group = new();
        LayoutAnchorable first = new() { ContentId = "one" };
        LayoutAnchorable second = new() { ContentId = "two" };
        root.RightSide.Children.Add(group);
        group.Children.Add(first);
        group.Children.Add(second);
        group.Children.Move(1, 0);
        RootDock model = (RootDock)root.DockModel;
        Assert.Equal(new[] { second.DockModel, first.DockModel }, model.RightPinnedDockables);
        group.Children.Remove(first);
        Assert.Null(first.Parent);
        Assert.Same(second.DockModel, Assert.Single(model.RightPinnedDockables!));
        Tool native = new() { Id = "native", Owner = model, Factory = root.DockFactory };
        model.RightPinnedDockables = root.DockFactory.CreateList<IDockable>(native);
        LayoutAnchorable wrapper = Assert.Single(Assert.Single(root.RightSide.Children).Children);
        Assert.Same(native, wrapper.DockModel);
        Assert.Same(root, wrapper.Root);
    }

    [AvaloniaFact]
    public void Replacing_A_Side_Detaches_The_Old_Projection()
    {
        LayoutRoot root = new();
        LayoutAnchorSide old = root.TopSide;
        LayoutAnchorGroup group = new();
        LayoutAnchorable tool = new();
        group.Children.Add(tool);
        old.Children.Add(group);
        root.TopSide = new LayoutAnchorSide();
        Assert.Null(old.Parent);
        Assert.Same(group, tool.Parent);
        Assert.Null(tool.Root);
        Assert.Empty(((RootDock)root.DockModel).TopPinnedDockables!);
        group.Children.Add(new LayoutAnchorable());
        Assert.Empty(((RootDock)root.DockModel).TopPinnedDockables!);
    }

    [AvaloniaFact]
    public void Native_Reordering_Updates_Group_And_Tool_Order()
    {
        LayoutRoot root = new();
        LayoutAnchorGroup firstGroup = new();
        LayoutAnchorGroup secondGroup = new();
        LayoutAnchorable first = new();
        LayoutAnchorable second = new();
        LayoutAnchorable third = new();
        firstGroup.Children.Add(first);
        firstGroup.Children.Add(second);
        secondGroup.Children.Add(third);
        root.LeftSide.Children.Add(firstGroup);
        root.LeftSide.Children.Add(secondGroup);
        AvaloniaList<IDockable> pinned = (AvaloniaList<IDockable>)((RootDock)root.DockModel).LeftPinnedDockables!;
        pinned.Move(1, 0);
        Assert.Equal(new[] { second, first }, firstGroup.Children);
        pinned.Move(2, 0);
        Assert.Equal(new[] { secondGroup, firstGroup }, root.LeftSide.Children);
        Assert.Same(second.DockModel, pinned[1]);
        Assert.Same(firstGroup, second.Parent);
    }

    [AvaloniaFact]
    public void Native_Unpin_Keeps_Group_Referenced_By_Hidden_Tool()
    {
        LayoutAnchorable first = new();
        LayoutAnchorable second = new();
        LayoutAnchorablePane pane = new(first);
        pane.Children.Add(second);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        root.DockFactory.PinDockable(first.DockModel);
        root.DockFactory.PinDockable(second.DockModel);
        LayoutAnchorGroup group = Assert.IsType<LayoutAnchorGroup>(first.Parent);
        Assert.Same(group, second.Parent);
        first.Hide();
        root.DockFactory.UnpinDockable(second.DockModel);
        Assert.Empty(group.Children);
        Assert.Same(root, group.Root);
        root.CollectGarbage();
        first.Show();
        Assert.Same(group, first.Parent);
        Assert.True(first.IsAutoHidden);
    }

    [AvaloniaFact]
    public void Toggle_Uses_The_Upstream_Pane_Side_Algorithm()
    {
        LayoutAnchorable first = new();
        LayoutAnchorable second = new();
        LayoutAnchorablePane left = new(first);
        LayoutAnchorablePane right = new(second);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(left) };
        root.RootPanel.Children.Add(new LayoutDocumentPane());
        root.RootPanel.Children.Add(right);
        Assert.Equal(AnchorSide.Left, left.GetSide());
        Assert.Equal(AnchorSide.Right, right.GetSide());
        second.ToggleAutoHide();
        Assert.Same(second, Assert.Single(Assert.Single(root.RightSide.Children).Children));
        root.RootPanel.Orientation = Orientation.Vertical;
        first.ToggleAutoHide();
        Assert.Same(first, Assert.Single(Assert.Single(root.TopSide.Children).Children));
    }

    private static LayoutAnchorSide GetSide(LayoutRoot root, AnchorSide side) => side switch
    {
        AnchorSide.Left => root.LeftSide,
        AnchorSide.Top => root.TopSide,
        AnchorSide.Right => root.RightSide,
        _ => root.BottomSide
    };
}
