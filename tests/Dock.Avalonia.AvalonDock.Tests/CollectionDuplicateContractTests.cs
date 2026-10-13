using System.Collections.Generic;
using System.Collections.Specialized;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model.Core;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class CollectionDuplicateContractTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Adding_The_Same_Content_Twice_Publishes_Both_Occurrences_With_One_Native_Node(bool tools)
    {
        var (pane, child, children) = Create(tools);
        List<NotifyCollectionChangedAction> actions = new();
        children.CollectionChanged += (_, e) => actions.Add(e.Action);
        int parents = 0;
        child.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(child.Parent)) parents++; };

        pane.InsertChildAt(1, child);

        Assert.Equal(new ILayoutElement[] { child, child }, pane.Children);
        Assert.Equal(new[] { NotifyCollectionChangedAction.Add }, actions);
        Assert.Equal(0, parents);
        Assert.Same(pane, child.Parent);
        Assert.Same(child.DockModel, Assert.Single(Native(pane).VisibleDockables!));

        pane.RemoveChildAt(0);

        Assert.Same(child, Assert.Single(pane.Children));
        Assert.Equal(new[] { NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Remove }, actions);
        Assert.Equal(1, parents);
        Assert.Null(child.Parent);
        Assert.Null(child.DockModel.Owner);
        Assert.Empty(Native(pane).VisibleDockables!);

        pane.InsertChildAt(0, child);
        Assert.Equal(new ILayoutElement[] { child, child }, pane.Children);
        Assert.Same(pane, child.Parent);
        Assert.Same(child.DockModel, Assert.Single(Native(pane).VisibleDockables!));
        Assert.Equal(2, parents);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Replacing_With_An_Existing_Child_Preserves_ObservableCollection_Parent_Callbacks(bool tools)
    {
        var (pane, first, _) = Create(tools);
        LayoutContent second = tools ? new LayoutAnchorable() : new LayoutDocument();
        pane.InsertChildAt(1, second);
        List<ILayoutContainer?> firstParents = new();
        List<ILayoutContainer?> secondParents = new();
        first.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(first.Parent)) firstParents.Add(first.Parent); };
        second.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(second.Parent)) secondParents.Add(second.Parent); };

        pane.ReplaceChildAt(0, second);

        Assert.Equal(new ILayoutElement[] { second, second }, pane.Children);
        Assert.Equal(new ILayoutContainer?[] { null }, firstParents);
        Assert.Empty(secondParents);
        Assert.Same(second.DockModel, Assert.Single(Native(pane).VisibleDockables!));

        pane.ReplaceChildAt(0, second);

        Assert.Equal(new ILayoutContainer?[] { null, pane }, secondParents);
        Assert.Equal(new ILayoutElement[] { second, second }, pane.Children);
        Assert.Same(second.DockModel, Assert.Single(Native(pane).VisibleDockables!));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Moving_And_Refreshing_Duplicate_Occurrences_Keep_Native_Membership_Unique(bool tools)
    {
        var (pane, first, _) = Create(tools);
        LayoutContent second = tools ? new LayoutAnchorable() : new LayoutDocument();
        pane.InsertChildAt(1, second);
        pane.InsertChildAt(2, first);

        if (tools) ((LayoutAnchorablePane)pane).MoveChild(2, 0);
        else ((LayoutDocumentPane)pane).MoveChild(2, 0);

        Assert.Equal(new ILayoutElement[] { first, first, second }, pane.Children);
        Assert.Equal(new[] { first.DockModel, second.DockModel }, Native(pane).VisibleDockables!);
        LayoutContent third = tools ? new LayoutAnchorable() : new LayoutDocument();
        third.DockModel.Owner = ((LayoutElement)pane).DockModel;
        Native(pane).VisibleDockables!.Add(third.DockModel);
        Assert.Equal(new ILayoutElement[] { first, first, second, third }, pane.Children);

        pane.RemoveChild(first);
        Assert.Null(first.Parent);
        LayoutContent fourth = tools ? new LayoutAnchorable() : new LayoutDocument();
        fourth.DockModel.Owner = ((LayoutElement)pane).DockModel;
        Native(pane).VisibleDockables!.Add(fourth.DockModel);
        Assert.Equal(new ILayoutElement[] { first, second, third, fourth }, pane.Children);
        Assert.Equal(new[] { second.DockModel, third.DockModel, fourth.DockModel }, Native(pane).VisibleDockables!);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Duplicates_Keep_One_Native_Window_And_Detach_On_First_Removal(bool tools)
    {
        LayoutRoot root = new();
        LayoutFloatingWindow child = Floating(tools);
        root.FloatingWindows.Add(child);
        int parents = 0;
        child.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(child.Parent)) parents++; };

        root.FloatingWindows.Add(child);

        Assert.Equal(new[] { child, child }, root.FloatingWindows);
        Assert.Same(child.DockWindow, Assert.Single(((RootDock)root.DockModel).Windows!));
        Assert.Equal(0, parents);
        root.FloatingWindows.RemoveAt(0);
        Assert.Same(child, Assert.Single(root.FloatingWindows));
        Assert.Null(child.Parent);
        Assert.Null(child.DockWindow.Owner);
        Assert.Empty(((RootDock)root.DockModel).Windows!);
        Assert.Equal(1, parents);

        root.FloatingWindows.Add(child);
        Assert.Equal(new[] { child, child }, root.FloatingWindows);
        Assert.Same(root, child.Parent);
        Assert.Same(child.DockWindow, Assert.Single(((RootDock)root.DockModel).Windows!));
        root.FloatingWindows.Clear();
        Assert.Same(root, child.Parent);
        Assert.Empty(((RootDock)root.DockModel).Windows!);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Duplicate_Replacement_And_Move_Preserve_Unique_Native_Order(bool tools)
    {
        LayoutRoot root = new();
        LayoutFloatingWindow first = Floating(tools);
        LayoutFloatingWindow second = Floating(tools);
        root.FloatingWindows.Add(first);
        root.FloatingWindows.Add(second);
        root.FloatingWindows.Add(first);
        root.FloatingWindows.Move(2, 0);
        Assert.Equal(new[] { first, first, second }, root.FloatingWindows);
        Assert.Equal(new[] { first.DockWindow, second.DockWindow }, ((RootDock)root.DockModel).Windows!);

        root.FloatingWindows[2] = first;

        Assert.Equal(new[] { first, first, first }, root.FloatingWindows);
        Assert.Null(second.Parent);
        Assert.Same(first.DockWindow, Assert.Single(((RootDock)root.DockModel).Windows!));
        root.FloatingWindows.RemoveAt(0);
        root.FloatingWindows[0] = first;
        Assert.Same(root, first.Parent);
        Assert.Same(first.DockWindow, Assert.Single(((RootDock)root.DockModel).Windows!));
        root.FloatingWindows.Clear();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Hidden_And_Pinned_Duplicates_Project_Once_And_Respect_The_Remaining_Occurrence_Parent(bool pinned)
    {
        LayoutRoot root = new();
        LayoutAnchorable child = new();
        LayoutAnchorGroup group = new();
        if (pinned) root.LeftSide!.Children.Add(group);
        var children = pinned ? group.Children : root.Hidden;
        children.Add(child);
        children.Add(child);
        RootDock native = (RootDock)root.DockModel;
        Assert.Equal(2, children.Count);
        Assert.Same(child.DockModel, Assert.Single(pinned ? native.LeftPinnedDockables! : native.HiddenDockables!));

        children.RemoveAt(0);

        Assert.Single(children);
        Assert.Null(child.Parent);
        Assert.Null(child.DockModel.Owner);
        Assert.Empty(pinned ? native.LeftPinnedDockables! : native.HiddenDockables!);
        children.Add(child);
        Assert.Equal(2, children.Count);
        Assert.Same(child.DockModel, Assert.Single(pinned ? native.LeftPinnedDockables! : native.HiddenDockables!));
    }

    [AvaloniaFact]
    public void Removing_One_Document_Occurrence_Releases_Its_Container_When_Its_Root_Is_Cleared()
    {
        LayoutDocument child = new();
        LayoutDocumentPane pane = new(child);
        pane.Children.Add(child);
        DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
        Assert.NotNull(manager.GetLayoutItemFromModel(child));

        pane.Children.RemoveAt(0);
        Dispatcher.UIThread.RunJobs();

        Assert.Same(child, Assert.Single(pane.Children));
        Assert.Null(child.Root);
        Assert.Null(manager.GetLayoutItemFromModel(child));
    }

    private static LayoutFloatingWindow Floating(bool tools) => tools
        ? new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) }
        : new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() };

    private static IDock Native(ILayoutGroup pane) => (IDock)((LayoutElement)pane).DockModel;

    private static (ILayoutGroup Pane, LayoutContent Content, INotifyCollectionChanged Children) Create(bool tools)
    {
        if (tools)
        {
            LayoutAnchorable content = new();
            LayoutAnchorablePane pane = new(content);
            return (pane, content, pane.Children);
        }
        LayoutDocument document = new();
        LayoutDocumentPane documents = new(document);
        return (documents, document, documents.Children);
    }
}
