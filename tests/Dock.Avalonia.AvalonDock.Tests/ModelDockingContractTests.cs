using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ModelDockingContractTests
{
    public static IEnumerable<object[]> MissingContextCases()
    {
        foreach (bool tool in new[] { false, true })
            foreach (string context in new[] { "none", "pane", "root" })
                foreach (string operation in new[] { "dock", "float", "document", "internal" })
                    yield return new object[] { tool, context, operation };
    }

    [AvaloniaTheory]
    [MemberData(nameof(MissingContextCases))]
    public void Missing_Manager_Context_Fails_Without_Moving_Or_Activating_Content(bool tool, string context, string operation)
    {
        LayoutContent content = tool ? new ExposedTool() : new ExposedDocument();
        ILayoutGroup pane = tool ? new LayoutAnchorablePane() : new LayoutDocumentPane();
        LayoutRoot? root = null;
        if (context != "none") pane.InsertChildAt(0, content);
        if (context == "root")
        {
            root = new LayoutRoot();
            if (tool) root.RootPanel.Children.Add((LayoutAnchorablePane)pane);
            else root.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = (LayoutDocument)content });
        }
        content.IsSelected = false;
        content.IsActive = false;
        ILayoutContainer? parent = content.Parent;
        int windows = root?.FloatingWindows.Count ?? 0;
        List<string> events = new();
        content.IsSelectedChanged += (_, _) => events.Add("selected");
        content.IsActiveChanged += (_, _) => events.Add("active");
        Action action = operation switch
        {
            "dock" => content.Dock,
            "float" => content.Float,
            "document" => content.DockAsDocument,
            _ => ((IExposeDock)content).InvokeBaseDock
        };

        if (operation == "document" && root is null) Assert.Throws<InvalidOperationException>(action);
        else Assert.Throws<NullReferenceException>(action);

        Assert.Same(parent, content.Parent);
        Assert.Same(root, content.Root);
        Assert.False(content.IsSelected);
        Assert.False(content.IsActive);
        Assert.Empty(events);
        Assert.Equal(windows, root?.FloatingWindows.Count ?? 0);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void DockAsDocument_Already_In_A_Document_Pane_Does_Not_Require_A_Manager(bool tool)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane pane = new(content);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        content.IsSelected = false;

        content.DockAsDocument();

        Assert.Same(pane, content.Parent);
        Assert.Same(root, content.Root);
        Assert.False(content.IsSelected);
        Assert.False(content.IsActive);
    }

    [AvaloniaTheory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, int.MaxValue)]
    [InlineData(true, int.MaxValue)]
    public void Restoring_A_Previous_Pane_Publishes_The_Final_Index_In_Its_Add_Notification(bool tool, int requestedIndex)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane origin = new(content);
        origin.Children.Add(new LayoutDocument());
        LayoutDocumentPane target = new(new LayoutDocument());
        target.Children.Add(new LayoutDocument());
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { origin, target } } } };
        content.PreviousContainer = target;
        content.PreviousContainerIndex = requestedIndex;
        object model = content.DockModel;
        List<(NotifyCollectionChangedAction Action, int Index)> changes = new();
        target.Children.CollectionChanged += (_, e) => changes.Add((e.Action, e.NewStartingIndex));

        content.Dock();

        Assert.Equal(new[] { (NotifyCollectionChangedAction.Add, Math.Min(requestedIndex, 2)) }, changes);
        Assert.Same(content, target.Children[Math.Min(requestedIndex, 2)]);
        Assert.Same(origin, content.PreviousContainer);
        Assert.Equal(0, content.PreviousContainerIndex);
        Assert.Same(model, content.DockModel);
        Assert.Same(target.DockModel, content.DockModel.Owner);
        Assert.Same(content, manager.Layout.ActiveContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Previous_Panes_In_Another_Root_Use_The_Destination_Manager_For_Completion(bool asDocument)
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane origin = new(tool);
        DockingManager source = new() { Layout = new() { RootPanel = new LayoutPanel(origin) } };
        LayoutDocumentPane target = new(new LayoutDocument());
        DockingManager destination = new() { Layout = new() { RootPanel = new LayoutPanel(target) } };
        tool.PreviousContainer = target;
        tool.PreviousContainerIndex = 0;
        List<string> events = new();
        source.PreviewDock += (_, _) => events.Add("source:preview");
        source.Docked += (_, _) => events.Add("source:docked");
        destination.PreviewDock += (_, _) => events.Add("destination:preview");
        destination.Docked += (_, _) =>
        {
            Assert.Same(origin, tool.PreviousContainer);
            events.Add("destination:docked");
        };

        if (asDocument) tool.DockAsDocument();
        else tool.Dock();

        Assert.Same(destination.Layout, tool.Root);
        Assert.Same(target, tool.Parent);
        Assert.Same(destination.DockFactory, tool.DockModel.Factory);
        Assert.Null(tool.PreviousContainer); // Destination collection clears references to another root.
        Assert.Equal(0, tool.PreviousContainerIndex);
        Assert.Equal(new[] { "source:preview", "destination:docked" }, events);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Invalid_Previous_Index_Fails_Before_Collection_Mutation(bool floating)
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane origin = new(tool);
        LayoutAnchorablePane target = new(new LayoutAnchorable());
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(origin) } };
        if (floating) manager.Layout.FloatingWindows.Add(new LayoutAnchorableFloatingWindow
        {
            RootPanel = new LayoutAnchorablePaneGroup(target)
        });
        else manager.Layout.RootPanel.Children.Add(target);
        tool.PreviousContainer = target;
        tool.PreviousContainerIndex = -1;
        List<string> events = new();
        manager.PreviewDock += (_, _) => events.Add("preview");
        manager.Docked += (_, _) => events.Add("docked");
        int changes = 0;
        origin.Children.CollectionChanged += (_, _) => changes++;
        target.Children.CollectionChanged += (_, _) => changes++;

        ArgumentOutOfRangeException error = Assert.Throws<ArgumentOutOfRangeException>(() => { if (floating) tool.Float(); else tool.Dock(); });

        Assert.Equal("index", error.ParamName);
        Assert.Equal(0, changes);
        Assert.Same(origin, tool.Parent);
        Assert.Same(target, tool.PreviousContainer);
        Assert.Equal(floating ? Array.Empty<string>() : new[] { "preview" }, events);
        manager.Layout.FloatingWindows.Clear();
    }

    [AvaloniaFact]
    public void An_Invalid_Previous_Container_Does_Not_Fall_Back_To_InternalDock()
    {
        ExposedDocument document = new();
        LayoutDocumentPane pane = new(document);
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        document.PreviousContainer = manager.Layout;

        Assert.Throws<NullReferenceException>(document.Dock);

        Assert.Equal(0, document.DockCalls);
        Assert.Same(pane, document.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Suppressed_InternalDock_Does_Not_Activate_Content_And_Clears_Floating_Only_After_Completion(bool tool)
    {
        LayoutContent content = tool ? new ExposedTool { SuppressDock = true } : new ExposedDocument { SuppressDock = true };
        LayoutDocument other = new();
        LayoutDocumentPane pane = new(other);
        pane.Children.Add(content);
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
        other.IsActive = true;
        using XmlReader reader = XmlReader.Create(new StringReader("<Content IsFloating='true' />"));
        reader.MoveToContent();
        content.ReadXml(reader);
        List<string> events = new();
        manager.PreviewDock += (_, _) => events.Add("preview");
        manager.Docked += (_, _) =>
        {
            Assert.True(content.IsFloating);
            Assert.False(content.IsSelected);
            Assert.False(content.IsActive);
            events.Add("docked");
        };
        content.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(content.IsFloating)) events.Add("floating"); };

        content.Dock();

        Assert.Equal(new[] { "preview", "docked", "floating" }, events);
        Assert.Equal(1, ((IExposeDock)content).DockCalls);
        Assert.Same(other, manager.Layout.ActiveContent);
        Assert.False(content.IsFloating);
        Assert.False(content.IsActive);
        Assert.False(content.IsSelected);
    }

    [AvaloniaFact]
    public void DockAsDocument_Completes_Before_Selecting_And_Activating_The_Moved_Tool()
    {
        LayoutDocument other = new();
        LayoutAnchorable tool = new();
        LayoutDocumentPane target = new(other);
        LayoutAnchorablePane origin = new(tool);
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { target, origin } } } };
        other.IsActive = true;
        tool.IsSelected = false;
        bool completed = false;
        manager.Docked += (_, _) =>
        {
            Assert.Same(target, tool.Parent);
            Assert.Same(other, target.SelectedContent);
            Assert.Same(other, manager.Layout.ActiveContent);
            Assert.False(tool.IsSelected);
            Assert.False(tool.IsActive);
            completed = true;
        };

        tool.DockAsDocument();

        Assert.True(completed);
        Assert.Same(tool, target.SelectedContent);
        Assert.Same(tool, manager.Layout.ActiveContent);
    }

    [AvaloniaFact]
    public void Docked_Observers_See_The_Empty_Floating_Model_Before_Garbage_Collection()
    {
        LayoutDocument document = new();
        LayoutDocumentPane target = new(new LayoutDocument());
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(target) } };
        LayoutDocumentFloatingWindow window = new() { RootDocument = document };
        manager.Layout.FloatingWindows.Add(window);
        document.PreviousContainer = target;
        document.PreviousContainerIndex = 0;
        bool completed = false;
        manager.Docked += (_, _) =>
        {
            Assert.Contains(window, manager.Layout.FloatingWindows);
            Assert.Null(window.RootDocument);
            Assert.Same(manager.Layout, window.Parent);
            completed = true;
        };

        document.Dock();

        Assert.True(completed);
        Assert.DoesNotContain(window, manager.Layout.FloatingWindows);
        Assert.Null(document.PreviousContainer);
        Assert.Equal(0, document.PreviousContainerIndex);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Removing_The_Manager_During_Preview_Stops_At_The_Expected_Model_Phase(bool asDocument)
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane origin = new(tool);
        LayoutDocumentPane target = new(new LayoutDocument());
        DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { origin, target } } } };
        LayoutRoot original = manager.Layout;
        manager.PreviewDock += (_, _) => manager.Layout = new LayoutRoot();
        bool completed = false;
        manager.Docked += (_, _) => completed = true;

        Assert.Throws<NullReferenceException>(() => { if (asDocument) tool.DockAsDocument(); else tool.Dock(); });

        Assert.Same(asDocument ? (ILayoutContainer)target : origin, tool.Parent);
        Assert.Same(original, tool.Root);
        Assert.False(completed);
    }

    [AvaloniaTheory]
    [InlineData(false, "null")]
    [InlineData(true, "null")]
    [InlineData(false, "detached")]
    [InlineData(true, "detached")]
    [InlineData(false, "foreign")]
    [InlineData(true, "foreign")]
    public void Garbage_Collection_Clears_Stale_Return_References_Without_Rewriting_Their_Metadata(bool tool, string previous)
    {
        LayoutContent content = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutDocumentPane pane = new(content);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        LayoutDocumentPane reference = new();
        LayoutRoot? foreign = previous == "foreign" ? new() { RootPanel = new LayoutPanel(reference) } : null;
        if (previous != "null") content.PreviousContainer = reference;
        content.PreviousContainerIndex = 17;
        content.PreviousContainerId = "saved-location";

        root.CollectGarbage();

        Assert.Null(content.PreviousContainer);
        Assert.Equal(17, content.PreviousContainerIndex);
        Assert.Equal("saved-location", content.PreviousContainerId);
        Assert.Same(pane, content.Parent);
        GC.KeepAlive(foreign);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Float_Can_Restore_An_Existing_Pane_Without_A_Manager_In_Either_Layout(bool differentRoot)
    {
        LayoutAnchorable tool = new();
        LayoutAnchorablePane origin = new(tool);
        origin.Children.Add(new LayoutAnchorable());
        LayoutRoot source = new() { RootPanel = new LayoutPanel(origin) };
        LayoutRoot target = differentRoot ? new() : source;
        LayoutAnchorablePane pane = new(new LayoutAnchorable());
        target.FloatingWindows.Add(new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup(pane) });
        tool.PreviousContainer = pane;
        tool.PreviousContainerIndex = 0;

        tool.Float();

        Assert.Same(pane, tool.Parent);
        Assert.Same(target, tool.Root);
        Assert.Same(target.DockFactory, tool.DockModel.Factory);
        Assert.Same(tool, pane.Children[0]);
        Assert.True(tool.IsActive);
        Assert.True(tool.IsSelected);
        Assert.True(tool.IsFloating);
        Assert.Same(differentRoot ? null : origin, tool.PreviousContainer);
        Assert.Equal(0, tool.PreviousContainerIndex);
    }

    [AvaloniaFact]
    public void A_Last_Focused_Floating_Document_Without_A_Pane_Creates_A_New_Document_Pane()
    {
        LayoutDocument document = new();
        LayoutDocumentPane original = new(new LayoutDocument());
        DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(original) } };
        manager.Layout.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = document });
        document.IsActive = true;
        Assert.Same(document, manager.Layout.LastFocusedDocument);

        document.DockAsDocument();

        Assert.IsType<LayoutDocumentPane>(document.Parent);
        Assert.NotSame(original, document.Parent);
        Assert.Same(manager.Layout, original.Root);
        Assert.Same(document, manager.Layout.ActiveContent);
        Assert.Empty(manager.Layout.FloatingWindows);
    }

    private interface IExposeDock
    {
        int DockCalls { get; }
        void InvokeBaseDock();
    }

    private sealed class ExposedDocument : LayoutDocument, IExposeDock
    {
        internal bool SuppressDock { get; init; }
        public int DockCalls { get; private set; }
        public void InvokeBaseDock() => base.InternalDock();
        protected override void InternalDock() { DockCalls++; if (!SuppressDock) base.InternalDock(); }
    }

    private sealed class ExposedTool : LayoutAnchorable, IExposeDock
    {
        internal bool SuppressDock { get; init; }
        public int DockCalls { get; private set; }
        public void InvokeBaseDock() => base.InternalDock();
        protected override void InternalDock() { DockCalls++; if (!SuppressDock) base.InternalDock(); }
    }
}
