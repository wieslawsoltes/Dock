using System;
using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class GroupBaseDerivationTests
{
    [AvaloniaFact]
    public void Application_Groups_Have_Independent_Native_Models_And_Notification_Hooks()
    {
        ApplicationGroup first = new();
        ApplicationGroup second = new();
        Assert.IsType<ProportionalDock>(first.DockModel);
        Assert.NotSame(first.DockModel, second.DockModel);
        Assert.Null(first.Parent);
        Assert.Null(first.Root);
        int calls = 0;
        first.ChildrenCollectionChanged += (sender, _) => { Assert.Same(first, sender); calls++; };
        second.ChildrenCollectionChanged += (_, _) => throw new Exception("A different group was notified.");
        first.NotifyCollection();
        Assert.Equal(1, calls);
        Assert.Equal(1, first.CollectionHooks);
    }

    [AvaloniaFact]
    public void An_External_Base_Group_Retains_Native_Parent_Identity_And_Bubbles_Tree_Changes()
    {
        LayoutRoot root = new();
        ApplicationGroup group = new() { Parent = root.RootPanel };
        Assert.Same(root.RootPanel, group.Parent);
        Assert.Same(root.RootPanel.DockModel, group.DockModel.Owner);
        Assert.Same(root, group.Root);
        List<string> changes = new();
        group.ChildrenTreeChanged += (sender, args) =>
        {
            Assert.Same(group, sender);
            changes.Add("group:" + args.Change);
        };
        root.RootPanel.ChildrenTreeChanged += (_, args) => changes.Add("parent:" + args.Change);
        group.NotifyTree();
        Assert.Equal(new[] { "group:DirectChildrenChanged", "parent:TreeChanged" }, changes);
        group.Parent = null;
        Assert.Null(group.Root);
        Assert.Null(group.DockModel.Owner);
    }

    [AvaloniaFact]
    public void A_Suppressed_Base_Hook_Still_Allows_The_Notification_Helper_To_Reach_The_Parent()
    {
        LayoutPanel parent = new();
        ApplicationGroup group = new() { Parent = parent, Suppress = true };
        int parents = 0;
        group.ChildrenCollectionChanged += (_, _) => throw new Exception("Collection hook was suppressed.");
        group.ChildrenTreeChanged += (_, _) => throw new Exception("Tree hook was suppressed.");
        parent.ChildrenTreeChanged += (_, args) => { Assert.Equal(ChildrenTreeChange.TreeChanged, args.Change); parents++; };
        group.NotifyCollection();
        group.NotifyTree();
        Assert.Equal(1, group.CollectionHooks);
        Assert.Equal(1, group.TreeHooks);
        Assert.Equal(1, parents);
    }

    [AvaloniaFact]
    public void A_Reentrant_Parent_Change_Redirects_The_Tree_Notification()
    {
        LayoutPanel first = new();
        LayoutPanel second = new();
        ApplicationGroup group = new() { Parent = first };
        List<string> changes = new();
        group.ChildrenTreeChanged += (_, _) => { changes.Add("child"); group.Parent = second; };
        first.ChildrenTreeChanged += (_, _) => changes.Add("old");
        second.ChildrenTreeChanged += (_, _) => changes.Add("new");
        group.NotifyTree();
        Assert.Equal(new[] { "child", "new" }, changes);
        Assert.Same(second.DockModel, group.DockModel.Owner);
    }

    [AvaloniaFact]
    public void A_Throwing_Group_Callback_Stops_Parent_Notification()
    {
        LayoutPanel parent = new();
        ApplicationGroup group = new() { Parent = parent };
        ApplicationException failure = new("application");
        group.ChildrenTreeChanged += (_, _) => throw failure;
        int calls = 0;
        parent.ChildrenTreeChanged += (_, _) => calls++;
        Assert.Same(failure, Assert.Throws<ApplicationException>(group.NotifyTree));
        Assert.Equal(0, calls);
        Assert.Same(parent, group.Parent);
    }

    private sealed class ApplicationGroup : LayoutGroupBase
    {
        public bool Suppress { get; init; }
        public int CollectionHooks { get; private set; }
        public int TreeHooks { get; private set; }
        public void NotifyCollection() => OnChildrenCollectionChanged();
        public void NotifyTree() => NotifyChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
        protected override void OnChildrenCollectionChanged()
        {
            CollectionHooks++;
            if (!Suppress) base.OnChildrenCollectionChanged();
        }
        protected override void OnChildrenTreeChanged(ChildrenTreeChange change)
        {
            TreeHooks++;
            if (!Suppress) base.OnChildrenTreeChanged(change);
        }
    }
}
