using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using Dock.Model.Avalonia.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class CollectionResetContractTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Reset_Retains_The_Public_Parent_And_Root_Without_Keeping_Native_Membership(bool tools, bool rooted)
    {
        var (pane, content, clear) = Create(tools);
        LayoutRoot? root = rooted ? new() { RootPanel = new((ILayoutPanelElement)pane) } : null;
        List<string> parentChanges = new();
        content.PropertyChanging += (_, e) => { if (e.PropertyName == nameof(content.Parent)) parentChanges.Add("changing"); };
        content.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(content.Parent)) parentChanges.Add("changed"); };
        int resets = 0;
        INotifyCollectionChanged children = tools ? ((LayoutAnchorablePane)pane).Children : ((LayoutDocumentPane)pane).Children;
        children.CollectionChanged += (_, e) =>
        {
            Assert.Equal(NotifyCollectionChangedAction.Reset, e.Action);
            Assert.Null(e.OldItems);
            Assert.Same(pane, content.Parent);
            Assert.Same(root, content.Root);
            resets++;
        };

        clear();

        Assert.Equal(1, resets);
        Assert.Empty(parentChanges);
        Assert.Empty(pane.Children);
        Assert.Same(pane, content.Parent);
        Assert.Same(root, content.Root);
        Assert.Null(content.DockModel.Owner);
        Assert.Empty(((IDock)((LayoutElement)pane).DockModel).VisibleDockables!);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Readding_A_Reset_Child_Uses_The_Retained_Parent_For_The_Next_Transition(bool tools, bool sameParent)
    {
        var (previous, content, clear) = Create(tools);
        ILayoutGroup destination = sameParent ? previous : tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        clear();
        List<ILayoutContainer?> parents = new();
        content.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(content.Parent)) parents.Add(content.Parent); };

        destination.InsertChildAt(0, content);

        Assert.Equal(sameParent ? Array.Empty<ILayoutContainer?>() : new ILayoutContainer?[] { destination }, parents);
        Assert.Same(destination, content.Parent);
        Assert.Same(((LayoutElement)destination).DockModel, content.DockModel.Owner);
        destination.RemoveChild(content);
        Assert.Null(content.Parent);
        Assert.Null(content.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_Parent_Clear_After_Reset_Publishes_The_Deferred_Detachment(bool tools)
    {
        var (_, content, clear) = Create(tools);
        clear();
        int changes = 0;
        content.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(content.Parent)) changes++; };

        content.Parent = null;

        Assert.Equal(1, changes);
        Assert.Null(content.Parent);
        Assert.Null(content.Root);
        Assert.Null(content.DockModel.Owner);
    }

    [AvaloniaFact]
    public void Resetting_Hidden_Content_Retains_Its_Public_Root_Parent()
    {
        LayoutRoot root = new();
        LayoutAnchorable content = new();
        root.Hidden.Add(content);
        int parents = 0;
        content.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(content.Parent)) parents++; };
        NotifyCollectionChangedEventArgs? change = null;
        root.Hidden.CollectionChanged += (_, e) => change = e;

        root.Hidden.Clear();

        Assert.Equal(NotifyCollectionChangedAction.Reset, change!.Action);
        Assert.Equal(0, parents);
        Assert.Same(root, content.Parent);
        Assert.Same(root, content.Root);
        Assert.Null(content.DockModel.Owner);
        root.Hidden.Add(content);
        Assert.Equal(0, parents);
        root.Hidden.Remove(content);
        Assert.Null(content.Parent);
        Assert.Equal(1, parents);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Resetting_Floating_Windows_Publishes_One_Reset_And_Retains_Public_Parents(bool tools, bool shown)
    {
        DockingManager manager = new();
        LayoutFloatingWindow floating = tools
            ? new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) }
            : new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() };
        Window? view = shown ? new() { Content = manager, Width = 800, Height = 500 } : null;
        view?.Show();
        manager.Layout.FloatingWindows.Add(floating);
        Dispatcher.UIThread.RunJobs();
        List<NotifyCollectionChangedAction> actions = new();
        int parents = 0;
        manager.Layout.FloatingWindows.CollectionChanged += (_, e) => actions.Add(e.Action);
        floating.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(floating.Parent)) parents++; };
        try
        {
            manager.Layout.FloatingWindows.Clear();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new[] { NotifyCollectionChangedAction.Reset }, actions);
            Assert.Equal(0, parents);
            Assert.Same(manager.Layout, floating.Parent);
            Assert.Empty(manager.Layout.FloatingWindows);
            Assert.Empty(((RootDock)manager.Layout.DockModel).Windows!);
            Assert.Null(floating.DockWindow.Owner);
            manager.Layout.FloatingWindows.Add(floating);
            Assert.Same(manager.Layout, floating.Parent);
            Assert.Equal(0, parents);
            manager.Layout.FloatingWindows.Remove(floating);
            Assert.Null(floating.Parent);
        }
        finally { view?.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Resetting_Pinned_Groups_Or_Their_Contents_Retains_Parents_And_Detaches_Native_Pins(bool sideReset)
    {
        LayoutRoot root = new();
        LayoutAnchorable child = new();
        LayoutAnchorGroup group = new();
        group.Children.Add(child);
        root.LeftSide!.Children.Add(group);
        int changes = 0;
        child.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(child.Parent)) changes++; };
        group.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(group.Parent)) changes++; };

        if (sideReset) root.LeftSide.Children.Clear();
        else group.Children.Clear();

        Assert.Equal(0, changes);
        Assert.Same(group, child.Parent);
        Assert.Same(root.LeftSide, group.Parent);
        Assert.Same(root, child.Root);
        Assert.Null(child.DockModel.Owner);
        Assert.Empty(((RootDock)root.DockModel).LeftPinnedDockables!);
    }

    [AvaloniaFact]
    public void Reset_Retains_A_Documents_Container_While_Its_Public_Root_Remains_Current()
    {
        LayoutDocument content = new();
        LayoutDocumentPane pane = new(content);
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        var item = manager.GetLayoutItemFromModel(content);
        Assert.NotNull(item);

        pane.Children.Clear();
        Dispatcher.UIThread.RunJobs();

        Assert.Same(item, manager.GetLayoutItemFromModel(content));
        content.Parent = null;
        Dispatcher.UIThread.RunJobs();
        Assert.Null(manager.GetLayoutItemFromModel(content));
    }

    private static (ILayoutGroup Pane, LayoutContent Content, Action Clear) Create(bool tools)
    {
        if (tools)
        {
            LayoutAnchorable content = new();
            LayoutAnchorablePane pane = new(content);
            return (pane, content, pane.Children.Clear);
        }
        else
        {
            LayoutDocument content = new();
            LayoutDocumentPane pane = new(content);
            return (pane, content, pane.Children.Clear);
        }
    }
}
