using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Headless.XUnit;
using Avalonia.Layout;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutExtensionTests
{
    [Fact]
    public void Traversal_Is_Deferred_Depth_First_And_Excludes_The_Starting_Node()
    {
        Container root = new(), branch = new();
        Node first = new(), second = new(), last = new();
        IEnumerable<ILayoutElement> descendants = root.Descendents();
        root.Items.Add(branch); root.Items.Add(last);
        branch.Items.Add(first); branch.Items.Add(second);
        Assert.Equal(new ILayoutElement[] { branch, first, second, last }, descendants);
        branch.Items.Remove(first);
        Assert.Equal(new ILayoutElement[] { branch, second, last }, descendants);
        Assert.Empty(first.Descendents());
    }

    [Fact]
    public void Parent_Search_Uses_The_Nearest_Matching_Ancestor_And_Observes_Reparenting()
    {
        Container root = new(), branch = new() { Parent = root };
        Container leaf = new() { Parent = branch };
        Assert.Same(branch, leaf.FindParent<Container>());
        Assert.Same(branch, leaf.FindParent<ILayoutContainer>());
        Assert.Null(root.FindParent<ILayoutElement>());
        leaf.Parent = root;
        Assert.Same(root, leaf.FindParent<Container>());
    }

    [Fact]
    public void An_Unmatched_Nonnullable_Value_Type_Parent_Retains_The_Cast_Error()
    {
        Node node = new() { Parent = new Container() };
        Assert.Throws<NullReferenceException>(() => node.FindParent<int>());
        Assert.Null(node.FindParent<int?>());
        Assert.Null(node.FindParent<ILayoutRoot>());
    }

    [AvaloniaFact]
    public void Root_Search_Walks_Parents_Without_Reading_The_Reported_Root()
    {
        LayoutRoot root = new();
        Container branch = new() { Parent = root };
        Node node = new() { Parent = branch };
        Assert.Same(root, node.GetRoot());
        Assert.Null(new Node().GetRoot());
        branch.Parent = null;
        Assert.Null(node.GetRoot());
    }

    [AvaloniaFact]
    public void Root_Search_Returns_The_Starting_Root_Before_Reading_Any_Properties()
    {
        ILayoutElement root = new RootWithExplicitContract();
        Assert.Same(root, root.GetRoot());
    }

    [Fact]
    public void Type_Helpers_Use_Descendants_And_Accept_Interface_Types()
    {
        Container root = new(), branch = new();
        Node leaf = new();
        root.Items.Add(branch); branch.Items.Add(leaf);
        Assert.True(root.ContainsChildOfType<ILayoutContainer>());
        Assert.True(root.ContainsChildOfType<ILayoutRoot, Node>());
        Assert.False(root.ContainsChildOfType<ILayoutRoot>());
        Assert.False(root.ContainsChildOfType<ILayoutRoot, LayoutContent>());
        Assert.False(branch.ContainsChildOfType<Container>());
        Assert.True(branch.IsOfType<ILayoutContainer, ILayoutRoot>());
        Assert.False(branch.IsOfType<LayoutContent, ILayoutRoot>());
    }

    [Fact]
    public void Null_Inputs_Preserve_The_Public_Empty_And_Error_Contracts()
    {
        ILayoutContainer missing = null!;
        Assert.Empty(missing.Descendents());
        Assert.False(missing.ContainsChildOfType<ILayoutElement>());
        Assert.False(missing.ContainsChildOfType<ILayoutElement, ILayoutRoot>());
        Assert.False(missing.IsOfType<ILayoutElement, ILayoutRoot>());
        Assert.Throws<NullReferenceException>(() => missing.FindParent<ILayoutElement>());
        Assert.Throws<NullReferenceException>(() => missing.GetRoot());
        Assert.Throws<NullReferenceException>(() => missing.GetSide());
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal, 1)]
    [InlineData(Orientation.Horizontal, 2)]
    [InlineData(Orientation.Horizontal, 3)]
    [InlineData(Orientation.Vertical, 1)]
    [InlineData(Orientation.Vertical, 2)]
    [InlineData(Orientation.Vertical, 3)]
    public void Side_Search_Uses_The_Nearest_Panel_And_Its_Ordered_Halves(Orientation orientation, int count)
    {
        LayoutPanel panel = new() { Orientation = orientation };
        for (int i = 0; i < count; i++) panel.Children.Add(new LayoutAnchorablePane(new LayoutAnchorable()));
        for (int i = 0; i < count; i++)
        {
            AnchorSide expected = orientation == Orientation.Horizontal
                ? i < count / 2d ? AnchorSide.Left : AnchorSide.Right
                : i < count / 2d ? AnchorSide.Top : AnchorSide.Bottom;
            Assert.Equal(expected, panel.Children[i].GetSide());
        }
        LayoutPanel outer = new() { Orientation = orientation == Orientation.Horizontal ? Orientation.Vertical : Orientation.Horizontal };
        outer.Children.Add(new LayoutDocumentPane()); outer.Children.Add(panel);
        Assert.Equal(orientation == Orientation.Horizontal ? AnchorSide.Left : AnchorSide.Top, panel.Children[0].GetSide());
        LayoutAnchorablePane absent = new() { Parent = panel };
        Assert.Equal(orientation == Orientation.Horizontal ? AnchorSide.Right : AnchorSide.Bottom, absent.GetSide());
        Assert.Equal(AnchorSide.Right, new LayoutAnchorablePane().GetSide());
        Assert.Equal(AnchorSide.Right, new LayoutAnchorablePane { Parent = new LayoutPanel() }.GetSide());
    }

    [AvaloniaTheory]
    [InlineData(Orientation.Horizontal)]
    [InlineData(Orientation.Vertical)]
    public void Side_Search_Finds_Nested_Group_Descendants_But_Not_Absent_Children(Orientation orientation)
    {
        LayoutAnchorablePane first = new();
        LayoutAnchorablePaneGroup group = new() { Children = { first } };
        LayoutPanel panel = new(group) { Orientation = orientation };
        panel.Children.Add(new LayoutDocumentPane());
        LayoutAnchorablePane alias = new() { Parent = group };
        Assert.Equal(orientation == Orientation.Horizontal ? AnchorSide.Left : AnchorSide.Top, first.GetSide());
        Assert.Equal(orientation == Orientation.Horizontal ? AnchorSide.Right : AnchorSide.Bottom, alias.GetSide());
    }

    private class Node : ILayoutElement
    {
        public ILayoutContainer? Parent { get; set; }
        public ILayoutRoot? Root => throw new InvalidOperationException("Traversal must use Parent.");
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
        public event PropertyChangingEventHandler? PropertyChanging { add { } remove { } }
    }

    private sealed class Container : Node, ILayoutContainer
    {
        public List<ILayoutElement> Items { get; } = new();
        public IEnumerable<ILayoutElement> Children => Items;
        public int ChildrenCount => Items.Count;
        public void RemoveChild(ILayoutElement element) => Items.Remove(element);
        public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement) => Items[Items.IndexOf(oldElement)] = newElement;
    }

    private sealed class RootWithExplicitContract : LayoutRoot, ILayoutElement
    {
        ILayoutContainer? ILayoutElement.Parent => throw new InvalidOperationException("A root is returned before parent lookup.");
        ILayoutRoot? ILayoutElement.Root => throw new InvalidOperationException("Root lookup does not read the Root property.");
    }

}
