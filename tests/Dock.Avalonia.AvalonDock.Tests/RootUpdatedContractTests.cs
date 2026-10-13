using System;
using System.Collections.Generic;
using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class RootUpdatedContractTests
{
    [AvaloniaTheory]
    [InlineData("add", 1)]
    [InlineData("remove", 0)]
    [InlineData("replace", 1)]
    [InlineData("move", 0)]
    [InlineData("clear", 0)]
    public void Group_Collection_Changes_Only_Report_Completed_Parent_Attachments(string operation, int updates)
    {
        LayoutDocumentPane pane = new();
        LayoutDocumentPaneGroup group = new(pane);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(group) };
        pane.Children.Add(new LayoutDocument());
        pane.Children.Add(new LayoutDocument());
        List<string> trace = new();
        root.Updated += (sender, args) =>
        {
            Assert.Same(root, sender);
            Assert.Same(EventArgs.Empty, args);
            trace.Add("updated");
        };
        pane.ChildrenCollectionChanged += (_, _) => trace.Add("children");
        pane.ChildrenTreeChanged += (_, _) => trace.Add("pane");
        group.ChildrenTreeChanged += (_, _) => trace.Add("group");
        root.RootPanel.ChildrenTreeChanged += (_, _) => trace.Add("panel");
        pane.PropertyChanged += (_, args) => { if (args.PropertyName == "ChildrenCount") trace.Add("count"); };
        pane.Children.CollectionChanged += (_, _) => trace.Add("collection");

        switch (operation)
        {
            case "add": pane.Children.Add(new LayoutDocument()); break;
            case "remove": pane.Children.RemoveAt(0); break;
            case "replace": pane.Children[0] = new LayoutDocument(); break;
            case "move": pane.MoveChild(0, 1); break;
            case "clear": pane.Children.Clear(); break;
        }

        List<string> expected = new();
        if (updates != 0) expected.Add("updated");
        expected.AddRange(new[] { "children", "pane", "group", "panel", "count", "collection" });
        Assert.Equal(expected, trace);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Public_And_Native_Tab_Reordering_Do_Not_Invoke_Throwing_Updated_Observers(bool native)
    {
        LayoutAnchorable first = new(), second = new();
        LayoutAnchorablePane pane = new() { Children = { first, second } };
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        root.Updated += (_, _) => throw new InvalidOperationException("No parent changed.");

        if (native) ((AvaloniaList<IDockable>)((ToolDock)pane.DockModel).VisibleDockables!).Move(0, 1);
        else pane.MoveChild(0, 1);

        Assert.Equal(new[] { second, first }, pane.Children);
        Assert.Same(pane, first.Parent);
        Assert.Same(pane.DockModel, first.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Protected_Tree_Notifications_Preserve_Propagation_Without_Updating_The_Root(bool suppress)
    {
        LayoutRoot root = new();
        ApplicationGroup group = new() { Parent = root.RootPanel, Suppress = suppress };
        int own = 0, parent = 0;
        group.ChildrenTreeChanged += (_, _) => own++;
        root.RootPanel.ChildrenTreeChanged += (_, _) => parent++;
        root.Updated += (_, _) => throw new InvalidOperationException("Only the tree hook ran.");

        group.Notify();

        Assert.Equal(suppress ? 0 : 1, own);
        Assert.Equal(1, parent);
    }

    [AvaloniaFact]
    public void Reparenting_During_A_Tree_Callback_Updates_Only_The_New_Root_Once()
    {
        LayoutRoot previous = new(), current = new();
        ApplicationGroup group = new() { Parent = previous.RootPanel };
        List<string> trace = new();
        previous.Updated += (_, _) => trace.Add("previous");
        current.Updated += (_, _) => trace.Add("current");
        group.ChildrenTreeChanged += (_, _) => { trace.Add("child"); group.Parent = current.RootPanel; };
        current.RootPanel.ChildrenTreeChanged += (_, _) => trace.Add("parent");

        group.Notify();

        Assert.Equal(new[] { "child", "current", "parent" }, trace);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Root_Panel_Replacement_Updates_After_Parent_Attachment_Before_The_Root_Property(bool reset)
    {
        LayoutRoot root = new();
        List<string> trace = new();
        root.PropertyChanging += (_, args) => { if (args.PropertyName == "RootPanel") trace.Add("changing"); };
        root.Updated += (_, _) => trace.Add("updated");
        root.PropertyChanged += (_, args) => { if (args.PropertyName == "RootPanel") trace.Add("changed"); };

        root.RootPanel = reset ? null : new LayoutPanel();

        Assert.Equal(new[] { "changing", "updated", "changed" }, trace);
    }

    [AvaloniaTheory]
    [InlineData("add", 1)]
    [InlineData("remove", 0)]
    [InlineData("replace", 1)]
    [InlineData("move", 0)]
    [InlineData("clear", 0)]
    public void Floating_Collection_Changes_Only_Report_New_Parent_Attachments(string operation, int expected)
    {
        LayoutRoot root = new();
        root.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() });
        root.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() });
        int updates = 0;
        root.Updated += (_, _) => updates++;

        switch (operation)
        {
            case "add": root.FloatingWindows.Add(new LayoutDocumentFloatingWindow()); break;
            case "remove": root.FloatingWindows.RemoveAt(0); break;
            case "replace": root.FloatingWindows[0] = new LayoutDocumentFloatingWindow(); break;
            case "move": root.FloatingWindows.Move(0, 1); break;
            case "clear": root.FloatingWindows.Clear(); break;
        }

        Assert.Equal(expected, updates);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Floating_Child_Changes_Only_Update_When_The_Public_Parent_Changes(bool tool, bool replace)
    {
        LayoutRoot root = new();
        LayoutFloatingWindow window = tool
            ? new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup() }
            : new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() };
        root.FloatingWindows.Add(window);
        int updates = 0;
        root.Updated += (_, _) => updates++;

        if (window is LayoutAnchorableFloatingWindow tools) tools.RootPanel = replace ? new LayoutAnchorablePaneGroup() : null;
        else ((LayoutDocumentFloatingWindow)window).RootDocument = replace ? new LayoutDocument() : null;

        Assert.Equal(replace ? 1 : 0, updates);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Floating_Subtree_Reordering_Does_Not_Emit_Extra_Updated_Events(bool extension)
    {
        LayoutAnchorablePane pane = new() { Children = { new LayoutAnchorable(), new LayoutAnchorable() } };
        LayoutFloatingWindow window = extension
            ? new LayoutGroupFloatingWindow { RootPanels = { pane } }
            : new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup(pane) };
        LayoutRoot root = new();
        root.FloatingWindows.Add(window);
        int singlePaneChanges = 0;
        window.PropertyChanged += (_, args) => { if (args.PropertyName == "IsSinglePane") singlePaneChanges++; };
        root.Updated += (_, _) => throw new InvalidOperationException("Reordering retained the parent.");

        pane.MoveChild(0, 1);

        Assert.Equal(extension ? 0 : 1, singlePaneChanges);
    }

    [AvaloniaFact]
    public void Group_Reordering_Refreshes_Commands_Without_Public_Updated_Events()
    {
        LayoutDocument document = new();
        LayoutDocumentPane first = new(document), second = new(new LayoutDocument());
        LayoutDocumentPaneGroup group = new(first) { Children = { second } };
        DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(group) } };
        LayoutItem item = manager.GetLayoutItemFromModel(document)!;
        Assert.True(item.MoveToNextTabGroupCommand!.CanExecute(null));
        Assert.False(item.MoveToPreviousTabGroupCommand!.CanExecute(null));
        int changes = 0;
        item.MoveToNextTabGroupCommand.CanExecuteChanged += (_, _) => changes++;
        manager.Layout.Updated += (_, _) => throw new InvalidOperationException("Group order changed.");

        group.Children.Move(0, 1);

        Assert.False(item.MoveToNextTabGroupCommand.CanExecute(null));
        Assert.True(item.MoveToPreviousTabGroupCommand.CanExecute(null));
        Assert.True(changes > 0);
        manager.Layout = new();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Clearing_A_Floating_Child_Releases_Its_Item_Even_With_Suppressed_Property_Notifications(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutFloatingWindow window = tool
            ? new SilentToolWindow { RootPanel = new LayoutAnchorablePaneGroup(new LayoutAnchorablePane((LayoutAnchorable)content)) }
            : new SilentDocumentWindow { RootDocument = (LayoutDocument)content };
        LayoutRoot root = new();
        root.FloatingWindows.Add(window);
        DockingManager manager = new() { Layout = root };
        Dispatcher.UIThread.RunJobs();
        LayoutItem item = manager.GetLayoutItemFromModel(content)!;
        Assert.Same(content, item.LayoutElement);
        root.Updated += (_, _) => throw new InvalidOperationException("The removed child retains its public parent.");

        if (window is LayoutAnchorableFloatingWindow tools) tools.RootPanel = null;
        else ((LayoutDocumentFloatingWindow)window).RootDocument = null;
        Dispatcher.UIThread.RunJobs();

        Assert.Null(manager.GetLayoutItemFromModel(content));
        Assert.Null(item.LayoutElement);
        manager.Layout = new();
        Dispatcher.UIThread.RunJobs();
    }

    private sealed class ApplicationGroup : LayoutGroupBase
    {
        internal bool Suppress { get; init; }
        internal void Notify() => NotifyChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
        protected override void OnChildrenTreeChanged(ChildrenTreeChange change)
        {
            if (!Suppress) base.OnChildrenTreeChanged(change);
        }
    }

    private sealed class SilentDocumentWindow : LayoutDocumentFloatingWindow
    {
        protected override void RaisePropertyChanged(string? propertyName = null) { }
    }

    private sealed class SilentToolWindow : LayoutAnchorableFloatingWindow
    {
        protected override void RaisePropertyChanged(string? propertyName = null) { }
    }
}
