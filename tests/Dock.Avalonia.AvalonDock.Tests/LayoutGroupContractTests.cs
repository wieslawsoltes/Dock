using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutGroupContractTests
{
    [AvaloniaTheory]
    [InlineData(-1, false)]
    [InlineData(1, false)]
    [InlineData(20, false)]
    [InlineData(-1, true)]
    [InlineData(1, true)]
    [InlineData(20, true)]
    public void ReplaceChildAt_Ignores_An_Invalid_Index_Before_Casting_The_Child(int index, bool incompatible)
    {
        LayoutAnchorable original = new(), candidate = new();
        LayoutAnchorablePane pane = new(original), owner = new(candidate);
        int changes = 0;
        pane.Children.CollectionChanged += (_, _) => changes++;
        ILayoutGroup group = pane;
        group.ReplaceChildAt(index, incompatible ? new LayoutDocument() : candidate);
        Assert.Same(original, Assert.Single(pane.Children));
        Assert.Same(candidate, Assert.Single(owner.Children));
        Assert.Same(owner, candidate.Parent);
        Assert.Same(original.DockModel, Assert.Single(((ToolDock)pane.DockModel).VisibleDockables!));
        Assert.Equal(0, changes);
    }

    [AvaloniaTheory]
    [InlineData("remove")]
    [InlineData("replace")]
    [InlineData("insert")]
    [InlineData("replaceAt")]
    public void An_Incompatible_Child_Preserves_The_Cast_Error_And_The_Existing_Layout(string operation)
    {
        LayoutAnchorable original = new();
        LayoutAnchorablePane pane = new(original);
        ILayoutGroup group = pane;
        LayoutDocument other = new();
        Assert.Throws<InvalidCastException>(() =>
        {
            switch (operation)
            {
                case "remove": group.RemoveChild(other); break;
                case "replace": group.ReplaceChild(other, original); break;
                case "insert": group.InsertChildAt(-1, other); break;
                case "replaceAt": group.ReplaceChildAt(0, other); break;
            }
        });
        Assert.Same(original, Assert.Single(pane.Children));
        Assert.Same(pane, original.Parent);
        Assert.Null(other.Parent);
    }

    [AvaloniaFact]
    public void Missing_And_Null_Children_Are_Ignored_Without_Casting_An_Unused_Replacement()
    {
        LayoutAnchorablePane pane = new(new LayoutAnchorable());
        ILayoutGroup group = pane;
        LayoutAnchorable absent = new();
        group.RemoveChild(absent); group.RemoveChild(null!);
        group.ReplaceChild(absent, new LayoutDocument());
        group.ReplaceChild(null!, new LayoutDocument());
        Assert.Equal(-1, group.IndexOfChild(null!));
        Assert.Single(pane.Children);
    }

    [AvaloniaFact]
    public void Index_Lookups_Use_The_Native_Objects_Identity_And_Reject_Unrelated_Types()
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane pane = new(tool);
        Assert.Equal(0, ((ILayoutGroup)pane).IndexOfChild(tool));
        Assert.Equal(0, ((ILayoutContentSelector)pane).IndexOf(tool));
        foreach (LayoutContent absent in new LayoutContent[] { new LayoutDocument(), new LayoutAnchorable() })
        {
            Assert.Equal(-1, ((ILayoutGroup)pane).IndexOfChild(absent));
            Assert.Equal(-1, ((ILayoutContentSelector)pane).IndexOf(absent));
        }
    }

    [AvaloniaTheory]
    [InlineData("add")]
    [InlineData("remove")]
    [InlineData("replace")]
    [InlineData("move")]
    [InlineData("clear")]
    public void Group_And_Ancestor_Tree_Callbacks_Precede_ChildrenCount_And_Public_Collection_Events(string operation)
    {
        LayoutPanel parent = new();
        LayoutAnchorablePane pane = new() { Children = { new LayoutAnchorable(), new LayoutAnchorable() } };
        parent.Children.Add(pane);
        List<string> trace = new();
        pane.ChildrenCollectionChanged += (_, _) => trace.Add("children");
        pane.ChildrenTreeChanged += (_, e) => trace.Add("pane:" + e.Change);
        parent.ChildrenTreeChanged += (_, e) => trace.Add("parent:" + e.Change);
        pane.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(pane.ChildrenCount)) trace.Add("count"); };
        pane.Children.CollectionChanged += (_, _) => trace.Add("collection");
        switch (operation)
        {
            case "add": pane.Children.Add(new LayoutAnchorable()); break;
            case "remove": pane.RemoveChildAt(0); break;
            case "replace": pane.ReplaceChildAt(0, new LayoutAnchorable()); break;
            case "move": pane.MoveChild(0, 1); break;
            case "clear": pane.Children.Clear(); break;
        }
        Assert.Equal(new[] { "children", "pane:DirectChildrenChanged", "parent:TreeChanged", "count", "collection" }, trace);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Earlier_Callback_Exception_Prevents_The_Later_Count_Notification(bool tree)
    {
        LayoutAnchorablePane pane = new(new LayoutAnchorable());
        List<string> trace = new();
        pane.ChildrenCollectionChanged += (_, _) => { trace.Add("children"); if (!tree) throw new CallbackException(); };
        pane.ChildrenTreeChanged += (_, _) => { trace.Add("tree"); throw new CallbackException(); };
        pane.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(pane.ChildrenCount)) trace.Add("count"); };
        Assert.Throws<CallbackException>(() => pane.Children.Add(new LayoutAnchorable()));
        Assert.Equal(tree ? new[] { "children", "tree" } : new[] { "children" }, trace);
        Assert.Equal(2, pane.ChildrenCount);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Remove_Callbacks_Observe_Detached_Ownership_And_Updated_Selection(bool rooted)
    {
        LayoutAnchorable removed = new(), retained = new();
        LayoutAnchorablePane pane = new() { Children = { removed, retained } };
        LayoutRoot? root = rooted ? new LayoutRoot { RootPanel = new LayoutPanel(pane) } : null;
        int observations = 0;
        pane.ChildrenCollectionChanged += Observe;
        pane.Children.CollectionChanged += (_, e) => { Assert.Equal(NotifyCollectionChangedAction.Remove, e.Action); Observe(null, EventArgs.Empty); };
        pane.RemoveChild(removed);
        Assert.Equal(2, observations);
        Assert.Same(retained.DockModel, Assert.Single(((ToolDock)pane.DockModel).VisibleDockables!));
        GC.KeepAlive(root);

        void Observe(object? sender, EventArgs args)
        {
            Assert.Null(removed.Parent);
            Assert.Null(removed.DockModel.Owner);
            Assert.False(removed.IsSelected);
            Assert.Same(retained, pane.SelectedContent);
            observations++;
        }
    }

    [AvaloniaFact]
    public void Valid_Interface_Replacement_And_Moves_Update_The_Native_Collection()
    {
        LayoutAnchorable first = new(), second = new(), replacement = new();
        LayoutAnchorablePane pane = new() { Children = { first, second } };
        ILayoutGroup group = pane;
        group.ReplaceChild(first, replacement);
        ((ILayoutPane)group).MoveChild(0, 1);
        Assert.Equal(new[] { second, replacement }, pane.Children);
        Assert.Equal(new[] { second.DockModel, replacement.DockModel }, ((ToolDock)pane.DockModel).VisibleDockables!);
        Assert.Null(first.Parent);
        Assert.Same(pane, replacement.Parent);
        int changes = 0;
        pane.Children.CollectionChanged += (_, _) => changes++;
        ((ILayoutPane)group).MoveChild(-1, -1);
        Assert.Equal(0, changes);
    }

    private sealed class CallbackException : Exception { }

    [AvaloniaFact]
    public void Reentrant_Removal_Preserves_The_Inner_Replacement_Without_Transient_Root_Deactivation()
    {
        LayoutDocument first = new(), second = new(), third = new();
        LayoutDocumentPane pane = new() { Children = { first, second, third } };
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        root.ActiveContent = first;
        second.LastActivationTimeStamp = DateTime.MaxValue;
        List<LayoutContent?> activated = new();
        root.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(root.ActiveContent)) activated.Add(root.ActiveContent); };
        bool removed = false;
        pane.ChildrenCollectionChanged += (_, _) =>
        {
            if (!removed) { removed = true; pane.RemoveChild(second); }
        };

        pane.RemoveChild(first);

        Assert.Equal(new LayoutContent?[] { second, third }, activated);
        Assert.Same(third, pane.SelectedContent);
        Assert.Same(third, root.ActiveContent);
        Assert.Null(first.Parent); Assert.Null(second.Parent);
        pane.RemoveChild(third);
        Assert.Null(root.ActiveContent);
        Assert.Null(root.LastFocusedDocument);
    }

    [AvaloniaFact]
    public void A_Throwing_Removal_Observer_Keeps_The_Selected_Replacement_And_Releases_Focus_Deferral()
    {
        LayoutDocument first = new(), second = new();
        LayoutDocumentPane pane = new() { Children = { first, second } };
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        root.ActiveContent = first;
        EventHandler throwing = (_, _) => throw new CallbackException();
        pane.ChildrenCollectionChanged += throwing;
        Assert.Throws<CallbackException>(() => pane.RemoveChild(first));
        Assert.Null(first.Parent);
        Assert.Same(second, root.ActiveContent);
        Assert.Same(second, pane.SelectedContent);
        pane.ChildrenCollectionChanged -= throwing;
        pane.RemoveChild(second);
        Assert.Null(root.ActiveContent);
        Assert.Null(root.LastFocusedDocument);
    }
}
