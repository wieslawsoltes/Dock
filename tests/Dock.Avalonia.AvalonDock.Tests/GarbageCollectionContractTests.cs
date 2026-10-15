using System.Collections.Specialized;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class GarbageCollectionContractTests
{
    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Removing_An_Empty_Active_Group_Child_Preserves_The_Focused_Content(int kind)
    {
        LayoutContent content = kind == 1 ? new LayoutAnchorable() : new LayoutDocument();
        ILayoutPanelElement populated = kind == 1 ? new LayoutAnchorablePane((LayoutAnchorable)content) : new LayoutDocumentPane(content);
        ILayoutPanelElement empty = kind == 1 ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        ILayoutGroup group = kind switch { 1 => new LayoutAnchorablePaneGroup(), 2 => new LayoutDocumentPaneGroup(), _ => new LayoutPanel() };
        group.InsertChildAt(0, populated);
        group.InsertChildAt(1, empty);
        LayoutRoot root = new() { RootPanel = group as LayoutPanel ?? new LayoutPanel((ILayoutPanelElement)group) };
        IDock native = (IDock)((LayoutElement)group).DockModel;
        native.ActiveDockable = ((LayoutElement)empty).DockModel;
        content.IsActive = true;
        Assert.Same(content, root.ActiveContent);

        root.CollectGarbage();

        Assert.Same(content, root.ActiveContent);
        Assert.True(content.IsActive);
        Assert.Same(content.DockModel, ((global::Dock.Model.Avalonia.Controls.RootDock)root.DockModel).FocusedDockable);
        Assert.Same(((LayoutElement)populated).DockModel, native.ActiveDockable);
        Assert.Null(empty.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Return_Locations_Are_Resolved_Before_Xml_Content_Callbacks_And_Collection(bool individualReference)
    {
        DockingManager manager = new();
        XmlLayoutSerializer serializer = new(manager);
        bool? floatingInCallback = null;
        serializer.LayoutSerializationCallback += (_, args) =>
        {
            if (args.Model is LayoutAnchorable) floatingInCallback = args.Model.IsFloating;
            args.Content = new object();
        };
        string reference = individualReference ? " PreviousContainerId='return'" : "";
        serializer.Deserialize(new StringReader("<LayoutRoot><RootPanel><LayoutDocumentPane /></RootPanel>" +
            "<RightSide><LayoutAnchorGroup PreviousContainerId='return'><LayoutAnchorable ContentId='tool' IsFloating='true'" + reference +
            " /></LayoutAnchorGroup></RightSide><FloatingWindows><LayoutAnchorableFloatingWindow><LayoutAnchorablePaneGroup>" +
            "<LayoutAnchorablePane Id='return' /></LayoutAnchorablePaneGroup></LayoutAnchorableFloatingWindow></FloatingWindows></LayoutRoot>"));

        LayoutAnchorable tool = manager.Layout.Descendents().OfType<LayoutAnchorable>().Single();
        Assert.True(floatingInCallback);
        Assert.True(tool.IsFloating);
        Assert.True((tool.DockModel.DockingState & DockingWindowState.Floating) != 0);
        Assert.IsType<LayoutAnchorableFloatingWindow>(tool.PreviousContainer!.FindParent<LayoutFloatingWindow>());
        Assert.Same(Assert.IsType<LayoutAnchorablePane>(tool.PreviousContainer).DockModel, tool.DockModel.OriginalOwner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_NonPane_Containers_Are_Pruned_Only_After_A_Pane_Is_Removed(bool removePane)
    {
        LayoutDocumentPane documents = new(new LayoutDocument());
        LayoutPanel emptyPanel = new();
        LayoutAnchorGroup emptyAnchors = new();
        LayoutDocumentFloatingWindow emptyWindow = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(documents) { Children = { emptyPanel } } };
        root.RightSide!.Children.Add(emptyAnchors);
        root.FloatingWindows.Add(emptyWindow);
        if (removePane) root.RootPanel.Children.Add(new LayoutAnchorablePane());

        root.CollectGarbage();

        Assert.Equal(!removePane, root.RootPanel.Children.Contains(emptyPanel));
        Assert.Equal(!removePane, root.RightSide.Children.Contains(emptyAnchors));
        Assert.Equal(!removePane, root.FloatingWindows.Contains(emptyWindow));
    }

    [AvaloniaFact]
    public void Empty_Document_Panes_Are_Removed_In_Tree_Order_Leaving_The_Last_One()
    {
        LayoutDocumentPane first = new();
        LayoutDocumentPane second = new();
        LayoutDocumentPane last = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(first) { Children = { second, last } } };
        List<ILayoutElement> removed = new();
        root.RootPanel.Children.CollectionChanged += (_, args) =>
        {
            if (args.Action == NotifyCollectionChangedAction.Remove) removed.Add((ILayoutElement)args.OldItems![0]!);
        };

        root.CollectGarbage();

        Assert.Equal(new ILayoutElement[] { first, second }, removed);
        Assert.Same(last, Assert.Single(root.RootPanel.Children));
    }

    [AvaloniaFact]
    public void Removing_The_Last_Tool_Pane_Replaces_An_Empty_Root_Panel_With_The_Default_Document_Layout()
    {
        LayoutPanel panel = new(new LayoutAnchorablePane());
        LayoutRoot root = new() { RootPanel = panel };

        root.CollectGarbage();

        Assert.NotSame(panel, root.RootPanel);
        Assert.Null(panel.Parent);
        Assert.IsType<LayoutDocumentPane>(Assert.Single(root.RootPanel.Children));
    }

    [AvaloniaFact]
    public void Each_Pane_Removal_Precedes_One_Panel_Window_And_Anchor_Group_Removal()
    {
        LayoutAnchorablePane firstPane = new();
        LayoutAnchorablePane secondPane = new();
        LayoutPanel firstPanel = new();
        LayoutPanel secondPanel = new();
        LayoutDocumentFloatingWindow firstWindow = new();
        LayoutDocumentFloatingWindow secondWindow = new();
        LayoutAnchorGroup firstAnchors = new();
        LayoutAnchorGroup secondAnchors = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument()))
        { Children = { firstPanel, secondPanel, firstPane, secondPane } } };
        root.FloatingWindows.Add(firstWindow);
        root.FloatingWindows.Add(secondWindow);
        root.RightSide!.Children.Add(firstAnchors);
        root.RightSide.Children.Add(secondAnchors);
        List<ILayoutElement> removed = new();
        void Track(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.Action == NotifyCollectionChangedAction.Remove) removed.Add((ILayoutElement)args.OldItems![0]!);
        }
        root.RootPanel.Children.CollectionChanged += Track;
        root.FloatingWindows.CollectionChanged += Track;
        root.RightSide.Children.CollectionChanged += Track;

        root.CollectGarbage();

        Assert.Equal(new ILayoutElement[] { firstPane, firstPanel, firstWindow, firstAnchors,
            secondPane, secondPanel, secondWindow, secondAnchors }, removed);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Return_References_Are_Validated_Through_The_Containers_Parent(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) };
        content.PreviousContainer = root.RootPanel;
        content.PreviousContainerIndex = 17;
        content.PreviousContainerId = "saved-parent";

        root.CollectGarbage();

        Assert.Null(content.PreviousContainer);
        Assert.Equal(17, content.PreviousContainerIndex);
        Assert.Equal("saved-parent", content.PreviousContainerId);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Clearing_An_Empty_Return_Pane_Retains_Its_Saved_Identifier(bool tool, bool floatingReturn)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) };
        LayoutAnchorablePane previous = new();
        if (floatingReturn)
            root.FloatingWindows.Add(new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup(previous) });
        else root.RootPanel.Children.Add(previous);
        content.PreviousContainer = previous;
        content.PreviousContainerIndex = 17;
        content.PreviousContainerId = "saved-empty-pane";

        root.CollectGarbage();

        Assert.Null(content.PreviousContainer);
        Assert.Equal(-1, content.PreviousContainerIndex);
        Assert.Equal("saved-empty-pane", content.PreviousContainerId);
        Assert.Null(previous.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Empty_Referenced_Panel_Is_Not_Treated_As_An_Empty_Return_Pane(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) };
        LayoutPanel previous = new();
        root.RootPanel.Children.Add(previous);
        content.PreviousContainer = previous;
        content.PreviousContainerIndex = 17;
        content.PreviousContainerId = "saved-panel";

        root.CollectGarbage();

        Assert.Same(previous, content.PreviousContainer);
        Assert.Same(root.RootPanel, previous.Parent);
        Assert.Equal(17, content.PreviousContainerIndex);
        Assert.Equal("saved-panel", content.PreviousContainerId);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Collapsing_Groups_Includes_Children_Added_By_Removal_Or_Insertion_Callbacks(bool tools, bool onInsertion)
    {
        ILayoutGroup outer = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
        ILayoutGroup inner = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
        ILayoutElement original = CreatePane(tools);
        ILayoutElement added = CreatePane(tools);
        inner.InsertChildAt(0, original);
        outer.InsertChildAt(0, inner);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)outer) };
        bool changed = false;
        void OnChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (changed || args.Action != (onInsertion ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove)) return;
            changed = true;
            inner.InsertChildAt(inner.ChildrenCount, added);
        }
        if (tools) ((LayoutAnchorablePaneGroup)outer).Children.CollectionChanged += OnChanged;
        else ((LayoutDocumentPaneGroup)outer).Children.CollectionChanged += OnChanged;

        root.CollectGarbage();

        Assert.True(changed);
        Assert.Equal(2, outer.ChildrenCount);
        Assert.Equal(0, outer.IndexOfChild(original));
        Assert.Equal(1, outer.IndexOfChild(added));
        Assert.Equal(0, inner.ChildrenCount);
        Assert.Null(inner.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Collapsing_A_Group_Uses_The_Children_That_Remain_After_A_Removal_Callback(bool tools)
    {
        ILayoutGroup outer = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
        ILayoutGroup inner = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
        ILayoutElement original = CreatePane(tools);
        ILayoutElement replacement = CreatePane(tools);
        inner.InsertChildAt(0, original);
        outer.InsertChildAt(0, inner);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)outer) };
        bool changed = false;
        void OnChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            if (changed || args.Action != NotifyCollectionChangedAction.Remove) return;
            changed = true;
            inner.RemoveChild(original);
            inner.InsertChildAt(0, replacement);
        }
        if (tools) ((LayoutAnchorablePaneGroup)outer).Children.CollectionChanged += OnChanged;
        else ((LayoutDocumentPaneGroup)outer).Children.CollectionChanged += OnChanged;

        root.CollectGarbage();

        Assert.Equal(1, outer.ChildrenCount);
        Assert.Equal(0, outer.IndexOfChild(replacement));
        Assert.Null(original.Parent);
        Assert.Equal(0, inner.ChildrenCount);
    }

    [AvaloniaFact]
    public void Tool_Groups_Collapse_Before_Document_Groups_Regardless_Of_Their_Tree_Order()
    {
        LayoutDocumentPaneGroup documents = (LayoutDocumentPaneGroup)CreateNestedGroup(false);
        LayoutAnchorablePaneGroup tools = (LayoutAnchorablePaneGroup)CreateNestedGroup(true);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(documents) { Children = { tools } } };
        List<string> order = new();
        documents.Children.CollectionChanged += (_, args) => { if (args.Action == NotifyCollectionChangedAction.Remove) order.Add("documents"); };
        tools.Children.CollectionChanged += (_, args) => { if (args.Action == NotifyCollectionChangedAction.Remove) order.Add("tools"); };

        root.CollectGarbage();

        Assert.Equal(new[] { "tools", "documents" }, order);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Each_Collapse_Phase_Rescans_For_New_Groups_Added_By_A_Callback(bool tools)
    {
        ILayoutGroup initial = CreateNestedGroup(tools);
        ILayoutGroup added = CreateNestedGroup(tools);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)initial) };
        bool changed = false;
        initial.ChildrenCollectionChanged += (_, _) =>
        {
            if (changed) return;
            changed = true;
            root.RootPanel.Children.Add((ILayoutPanelElement)added);
        };

        root.CollectGarbage();

        Assert.True(changed);
        Assert.Single(added.Children);
        Assert.IsType(tools ? typeof(LayoutAnchorablePane) : typeof(LayoutDocumentPane), Assert.Single(added.Children));
    }

    [AvaloniaFact]
    public void The_Document_Phase_Does_Not_Restart_The_Completed_Tool_Phase()
    {
        ILayoutGroup documents = CreateNestedGroup(false);
        ILayoutGroup tools = CreateNestedGroup(true);
        LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)documents) };
        bool changed = false;
        documents.ChildrenCollectionChanged += (_, _) =>
        {
            if (changed) return;
            changed = true;
            root.RootPanel.Children.Add((ILayoutPanelElement)tools);
        };

        root.CollectGarbage();

        Assert.IsType<LayoutAnchorablePaneGroup>(Assert.Single(tools.Children));
    }

    private static ILayoutGroup CreateNestedGroup(bool tools)
    {
        ILayoutGroup outer = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
        ILayoutGroup inner = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
        inner.InsertChildAt(0, CreatePane(tools));
        outer.InsertChildAt(0, inner);
        return outer;
    }

    private static ILayoutElement CreatePane(bool tools) => tools
        ? new LayoutAnchorablePane(new LayoutAnchorable())
        : new LayoutDocumentPane(new LayoutDocument());
}
