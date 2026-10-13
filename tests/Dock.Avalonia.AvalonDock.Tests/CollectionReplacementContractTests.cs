using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class CollectionReplacementContractTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void A_Callback_Can_Add_A_Sibling_Without_Losing_Either_Native_Attachment(bool tools, bool countCallback)
    {
        LayoutContent incoming = tools ? new LayoutAnchorable() : new LayoutDocument();
        LayoutContent sibling = tools ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutGroup pane = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        bool added = false;
        void AddSibling()
        {
            if (added) return;
            added = true;
            pane.InsertChildAt(pane.ChildrenCount, sibling);
        }
        if (countCallback)
        {
            System.ComponentModel.INotifyPropertyChanged children = tools
                ? ((LayoutAnchorablePane)pane).Children : ((LayoutDocumentPane)pane).Children;
            children.PropertyChanged += (_, args) => { if (args.PropertyName == "Count") AddSibling(); };
        }
        else incoming.PropertyChanged += (_, args) => { if (args.PropertyName == "Parent") AddSibling(); };

        pane.InsertChildAt(0, incoming);

        Assert.True(added);
        Assert.Equal(new ILayoutElement[] { incoming, sibling }, pane.Children);
        Assert.Same(pane, incoming.Parent);
        Assert.Same(pane, sibling.Parent);
        IDock native = (IDock)((LayoutElement)pane).DockModel;
        Assert.Equal(new[] { incoming.DockModel, sibling.DockModel }, native.VisibleDockables!);
        Assert.Same(native, incoming.DockModel.Owner);
        Assert.Same(native, sibling.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Replacing_A_Floating_Window_With_Itself_Publishes_Replacement_Without_Closing_Its_Host(bool tools, bool shown)
    {
        DockingManager manager = new();
        LayoutFloatingWindow floating = tools
            ? new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable())) }
            : new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() };
        Window? view = shown ? new Window { Content = manager, Width = 800, Height = 500 } : null;
        view?.Show();
        manager.Layout.FloatingWindows.Add(floating);
        Dispatcher.UIThread.RunJobs();
        LayoutFloatingWindowControl? host = floating.DockWindow.Host as LayoutFloatingWindowControl;
        if (shown) Assert.NotNull(host);
        List<ILayoutContainer?> parents = new();
        List<NotifyCollectionChangedEventArgs> changes = new();
        int nativeChanges = 0;
        int closed = 0;
        floating.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(floating.Parent)) parents.Add(floating.Parent); };
        manager.Layout.FloatingWindows.CollectionChanged += (_, args) => changes.Add(args);
        manager.DockFactory.WindowAdded += (_, _) => nativeChanges++;
        manager.DockFactory.WindowRemoved += (_, _) => nativeChanges++;
        if (host is not null) host.Closed += (_, _) => closed++;
        try
        {
            manager.Layout.FloatingWindows[0] = floating;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(new ILayoutContainer?[] { null, manager.Layout }, parents);
            NotifyCollectionChangedEventArgs change = Assert.Single(changes);
            Assert.Equal(NotifyCollectionChangedAction.Replace, change.Action);
            Assert.Same(floating, Assert.Single(change.OldItems!));
            Assert.Same(floating, Assert.Single(change.NewItems!));
            Assert.Same(host, floating.DockWindow.Host);
            Assert.Same(manager.Layout, floating.Parent);
            Assert.Same(floating, Assert.Single(manager.Layout.FloatingWindows));
            Assert.Equal(0, nativeChanges);
            Assert.Equal(0, closed);
        }
        finally
        {
            manager.Layout.FloatingWindows.Clear();
            view?.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replacing_An_Active_Hidden_Or_Pinned_Tool_With_Itself_Keeps_Root_Activation(bool pinned)
    {
        LayoutRoot root = new();
        LayoutAnchorable child = new();
        LayoutAnchorGroup group = new();
        if (pinned)
        {
            group.Children.Add(child);
            root.LeftSide!.Children.Add(group);
        }
        else root.Hidden.Add(child);
        root.ActiveContent = child;
        Assert.True(child.IsActive);
        List<LayoutContent?> active = new();
        root.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(root.ActiveContent)) active.Add(root.ActiveContent);
        };

        if (pinned) group.Children[0] = child;
        else root.Hidden[0] = child;

        Assert.Same(child, root.ActiveContent);
        Assert.True(child.IsActive);
        Assert.Empty(active);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Incoming_Parent_Callbacks_See_Completed_Target_Membership(bool tools, bool replace)
    {
        LayoutContent incoming = tools ? new LayoutAnchorable() : new LayoutDocument();
        LayoutContent outgoing = tools ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutGroup previous = tools ? new LayoutAnchorablePane((LayoutAnchorable)incoming) : new LayoutDocumentPane(incoming);
        ILayoutGroup target = tools ? new LayoutAnchorablePane((LayoutAnchorable)outgoing) : new LayoutDocumentPane(outgoing);
        List<bool> membership = new();
        incoming.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(incoming.Parent))
            {
                membership.Add(target.IndexOfChild(incoming) == (replace ? 0 : 1));
                Assert.Equal(replace ? -1 : 0, target.IndexOfChild(outgoing));
            }
        };

        if (replace) target.ReplaceChildAt(0, incoming);
        else target.InsertChildAt(1, incoming);

        Assert.Equal(new[] { true, true }, membership);
        Assert.Empty(previous.Children);
        Assert.Same(target, incoming.Parent);
        Assert.Equal(replace ? 1 : 2, target.ChildrenCount);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Throwing_Detachment_Callback_Stops_Replacement_And_A_Later_Retry_Can_Attach(bool tools)
    {
        LayoutContent content = tools ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutGroup pane = tools ? new LayoutAnchorablePane((LayoutAnchorable)content) : new LayoutDocumentPane(content);
        System.InvalidOperationException error = new("parent callback");
        bool fail = true;
        int notifications = 0;
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(content.Parent) && content.Parent is null && fail) throw error;
        };
        pane.ChildrenCollectionChanged += (_, _) => notifications++;

        Assert.Same(error, Assert.Throws<System.InvalidOperationException>(() => pane.ReplaceChildAt(0, content)));

        Assert.Same(content, Assert.Single(pane.Children));
        Assert.Null(content.Parent);
        Assert.Null(content.DockModel.Owner);
        Assert.Equal(0, notifications);
        fail = false;
        pane.ReplaceChildAt(0, content);
        Assert.Same(pane, content.Parent);
        Assert.Equal(1, notifications);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void Replacing_A_Group_Child_With_Itself_Also_Detaches_And_Reattaches_Its_Public_Parent(int kind)
    {
        LayoutRoot root = new();
        LayoutElement child;
        ILayoutGroup group;
        if (kind == 0)
        {
            child = new LayoutDocumentPane(new LayoutDocument());
            group = root.RootPanel = new LayoutPanel((ILayoutPanelElement)child);
        }
        else if (kind == 1)
        {
            child = new LayoutDocumentPane(new LayoutDocument());
            group = new LayoutDocumentPaneGroup((LayoutDocumentPane)child);
            root.RootPanel.Children.Add((ILayoutPanelElement)group);
        }
        else if (kind == 2)
        {
            child = new LayoutAnchorablePane(new LayoutAnchorable());
            group = new LayoutAnchorablePaneGroup((LayoutAnchorablePane)child);
            root.RootPanel.Children.Add((ILayoutPanelElement)group);
        }
        else if (kind == 3)
        {
            LayoutAnchorGroup anchors = new() { Children = { new LayoutAnchorable() } };
            child = anchors;
            group = root.LeftSide!;
            group.InsertChildAt(0, child);
        }
        else
        {
            child = new LayoutAnchorable();
            LayoutAnchorGroup anchors = new() { Children = { (LayoutAnchorable)child } };
            root.LeftSide!.Children.Add(anchors);
            group = anchors;
        }
        List<ILayoutContainer?> parents = new();
        child.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(child.Parent)) parents.Add(child.Parent); };

        group.ReplaceChildAt(0, child);

        Assert.Equal(new ILayoutContainer?[] { null, group }, parents);
        Assert.Same(child, Assert.Single(group.Children));
        Assert.Same(group, child.Parent);
        Assert.Same(root, child.Root);
    }

    [AvaloniaFact]
    public void Replacing_A_Hidden_Tool_With_Itself_Publishes_The_Parent_Transitions()
    {
        LayoutRoot root = new();
        LayoutAnchorable child = new();
        root.Hidden.Add(child);
        List<ILayoutContainer?> parents = new();
        child.PropertyChanged += (_, args) => { if (args.PropertyName == nameof(child.Parent)) parents.Add(child.Parent); };
        root.Hidden[0] = child;
        Assert.Equal(new ILayoutContainer?[] { null, root }, parents);
        Assert.Same(root, child.Parent);
        Assert.Same(root.DockModel, child.DockModel.Owner);
        Assert.True(child.IsHidden);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reattachment_Removes_The_Child_From_A_Parent_Chosen_By_A_Detachment_Callback(bool tools)
    {
        LayoutContent content = tools ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutGroup pane = tools ? new LayoutAnchorablePane((LayoutAnchorable)content) : new LayoutDocumentPane(content);
        ILayoutGroup other = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        bool redirected = false;
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(content.Parent) || content.Parent is not null || redirected) return;
            redirected = true;
            other.InsertChildAt(0, content);
        };

        pane.ReplaceChildAt(0, content);

        Assert.True(redirected);
        Assert.Empty(other.Children);
        Assert.Empty(((IDock)((LayoutElement)other).DockModel).VisibleDockables!);
        Assert.Same(content, Assert.Single(pane.Children));
        Assert.Same(pane, content.Parent);
        Assert.Same(((LayoutElement)pane).DockModel, content.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false, false, 0)]
    [InlineData(false, false, 1)]
    [InlineData(false, false, 2)]
    [InlineData(false, true, 0)]
    [InlineData(false, true, 1)]
    [InlineData(false, true, 2)]
    [InlineData(true, false, 0)]
    [InlineData(true, false, 1)]
    [InlineData(true, false, 2)]
    [InlineData(true, true, 0)]
    [InlineData(true, true, 1)]
    [InlineData(true, true, 2)]
    public void Replacing_An_Item_With_Itself_Publishes_Replacement_And_Parent_Transitions(bool tools, bool rooted, int api)
    {
        LayoutContent content = tools ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutGroup pane = tools ? new LayoutAnchorablePane((LayoutAnchorable)content) : new LayoutDocumentPane(content);
        LayoutRoot? root = rooted ? new LayoutRoot { RootPanel = new LayoutPanel((ILayoutPanelElement)pane) } : null;
        List<ILayoutContainer?> parents = new();
        List<NotifyCollectionChangedEventArgs> changes = new();
        List<bool> selection = new();
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(content.Parent)) parents.Add(content.Parent);
        };
        content.IsSelectedChanged += (_, _) => selection.Add(content.IsSelected);
        if (tools) ((LayoutAnchorablePane)pane).Children.CollectionChanged += (_, args) => changes.Add(args);
        else ((LayoutDocumentPane)pane).Children.CollectionChanged += (_, args) => changes.Add(args);

        if (api == 0)
        {
            if (tools) ((LayoutAnchorablePane)pane).Children[0] = (LayoutAnchorable)content;
            else ((LayoutDocumentPane)pane).Children[0] = content;
        }
        else if (api == 1) pane.ReplaceChildAt(0, content);
        else pane.ReplaceChild(content, content);

        Assert.Equal(new ILayoutContainer?[] { null, pane }, parents);
        NotifyCollectionChangedEventArgs replacement = Assert.Single(changes);
        Assert.Equal(NotifyCollectionChangedAction.Replace, replacement.Action);
        Assert.Same(content, Assert.Single(replacement.OldItems!));
        Assert.Same(content, Assert.Single(replacement.NewItems!));
        Assert.Equal(0, replacement.OldStartingIndex);
        Assert.Equal(0, replacement.NewStartingIndex);
        Assert.Equal(new[] { false, true }, selection);
        Assert.Same(pane, content.Parent);
        Assert.Same(((LayoutElement)pane).DockModel, content.DockModel.Owner);
        Assert.Same(content.DockModel, Assert.Single(((IDock)((LayoutElement)pane).DockModel).VisibleDockables!));
        System.GC.KeepAlive(root);
    }
}
