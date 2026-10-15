using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Headless.XUnit;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class ParentTransitionContractTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Ancestor_Attachment_Does_Not_Advance_The_Childs_Reported_Root(bool native)
    {
        ProbeDocument document = new();
        LayoutDocumentPane first = new(document);
        LayoutDocumentPane second = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(first) { Children = { second } } };
        List<LayoutElement> added = new();
        root.ElementAdded += (_, e) => added.Add(e.Element);

        Assign(document, second, native);

        Assert.Equal(new (ILayoutRoot?, ILayoutRoot?)[] { (null, root) }, document.RootChanges);
        Assert.Same(document, Assert.Single(added));
        Assert.Same(second, document.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Moving_An_Ancestor_Retains_The_Childs_Previously_Reported_Root(bool native)
    {
        LayoutDocumentPane pane = new();
        LayoutRoot previous = new() { RootPanel = new LayoutPanel(pane) };
        ProbeDocument document = new();
        pane.Children.Add(document);
        LayoutDocumentPane destination = new();
        LayoutRoot current = new() { RootPanel = new LayoutPanel(destination) };
        current.RootPanel.Children.Add(pane);
        document.RootChanges.Clear();
        List<string> events = new();
        previous.ElementRemoved += (_, e) => { Assert.Same(document, e.Element); events.Add("removed"); };
        current.ElementAdded += (_, e) => { Assert.Same(document, e.Element); events.Add("added"); };

        Assign(document, destination, native);

        Assert.Equal(new (ILayoutRoot?, ILayoutRoot?)[] { (previous, current) }, document.RootChanges);
        Assert.Equal(new[] { "removed", "added" }, events);
    }

    public static IEnumerable<object[]> FailurePhases()
    {
        foreach (string phase in new[] { "parent-changing", "parent-changed", "root-changed", "property-changed" })
            foreach (bool native in new[] { false, true }) yield return new object[] { phase, native };
    }

    [AvaloniaTheory]
    [MemberData(nameof(FailurePhases))]
    public void A_Later_Transition_Uses_The_Root_Recorded_Before_The_Failing_Phase(string phase, bool native)
    {
        LayoutRoot first = new();
        LayoutRoot second = new();
        LayoutRoot third = new();
        ProbeDocument document = new() { Parent = first };
        InvalidOperationException failure = new(phase);
        document.Failure = failure;
        document.FailAt = phase;
        document.PropertyChanged += (_, e) =>
        {
            if (document.FailAt == "property-changed" && e.PropertyName == "Parent") throw failure;
        };

        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() => Assign(document, second, native)));

        Assert.Same(phase == "parent-changing" ? first : second, document.Parent);
        document.FailAt = null;
        document.RootChanges.Clear();
        Assign(document, third, native);
        ILayoutRoot expected = phase is "root-changed" or "property-changed" ? second : first;
        Assert.Equal(new (ILayoutRoot?, ILayoutRoot?)[] { (expected, third) }, document.RootChanges);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Root_Hook_Suppression_Does_Not_Suppress_The_Reported_Root_Update(bool native)
    {
        LayoutRoot first = new();
        LayoutRoot second = new();
        ProbeDocument document = new() { SuppressRootNotification = true };
        int added = 0;
        first.ElementAdded += (_, _) => added++;
        second.ElementAdded += (_, _) => added++;

        Assign(document, first, native);
        Assign(document, second, native);

        Assert.Equal(new (ILayoutRoot?, ILayoutRoot?)[] { (null, first), (first, second) }, document.RootChanges);
        Assert.Equal(0, added);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Parent_Notifications_Do_Not_Add_A_Root_Property_Notification(bool native)
    {
        LayoutPanel panel = new();
        LayoutRoot root = new();
        List<string> events = new();
        panel.PropertyChanging += (_, e) => { if (e.PropertyName is "Parent" or "Root") events.Add("before:" + e.PropertyName); };
        panel.PropertyChanged += (_, e) => { if (e.PropertyName is "Parent" or "Root") events.Add("after:" + e.PropertyName); };

        Assign(panel, root, native);

        Assert.Equal(new[] { "before:Parent", "after:Parent" }, events);
        Assert.Same(root, panel.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Updated_Uses_The_Current_Root_After_A_Property_Observer_Reparents_The_Element(bool native)
    {
        LayoutRoot first = new();
        LayoutRoot second = new();
        LayoutRoot third = new();
        LayoutPanel element = new() { Parent = first };
        List<LayoutRoot> updates = new();
        second.Updated += (_, _) => updates.Add(second);
        third.Updated += (_, _) => updates.Add(third);
        element.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Parent" && ReferenceEquals(element.Parent, second)) element.Parent = third;
        };

        Assign(element, second, native);

        Assert.Equal(new[] { third, third }, updates);
        Assert.Same(third, element.Parent);
    }

    [AvaloniaFact]
    public void A_Layout_Root_Reports_Only_An_Ancestor_Root()
    {
        LayoutRoot root = new();
        Assert.Null(root.Root);
        Assert.Null(((ILayoutElement)root).Root);
        Assert.Same(root, root.GetRoot());
        LayoutRoot parent = new();
        root.Parent = parent;
        Assert.Same(parent, root.Root);
    }

    [AvaloniaFact]
    public void An_Application_Container_Can_Be_A_Parent_Without_A_Native_Dock_Model()
    {
        LayoutRoot root = new();
        ExternalContainer parent = new() { Parent = root };
        LayoutDocument document = new();
        List<LayoutElement> added = new();
        root.ElementAdded += (_, e) => added.Add(e.Element);

        document.Parent = parent;

        Assert.Same(parent, document.Parent);
        Assert.Same(root, document.Root);
        Assert.Null(document.DockModel.Owner);
        Assert.Same(document, Assert.Single(added));
        LayoutDocumentPane nativeParent = new();
        document.Parent = nativeParent;
        Assert.Same(nativeParent.DockModel, document.DockModel.Owner);
        Assert.Null(document.Root);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Application_Root_Interfaces_Reach_The_Protected_Hook_And_Default_Concrete_Cast(bool suppress)
    {
        ExternalRoot root = new();
        ProbeDocument document = new() { SuppressRootNotification = suppress };
        Exception? error = Record.Exception(() => document.Parent = root);

        if (suppress) Assert.Null(error);
        else Assert.IsType<InvalidCastException>(error);
        Assert.Same(root, document.Parent);
        Assert.Same(root, document.Root);
        Assert.Equal(new (ILayoutRoot?, ILayoutRoot?)[] { (null, root) }, document.RootChanges);
        Assert.Null(document.DockModel.Owner);
    }

    [AvaloniaTheory]
    [InlineData(AnchorSide.Left, false)]
    [InlineData(AnchorSide.Top, false)]
    [InlineData(AnchorSide.Right, false)]
    [InlineData(AnchorSide.Bottom, false)]
    [InlineData(AnchorSide.Left, true)]
    [InlineData(AnchorSide.Top, true)]
    [InlineData(AnchorSide.Right, true)]
    [InlineData(AnchorSide.Bottom, true)]
    public void A_Null_Side_Parent_Fails_After_Assignment_And_Recovery_Keeps_Root_Membership(AnchorSide edge, bool native)
    {
        LayoutRoot root = new();
        LayoutAnchorSide side = ReadSide(root, edge);
        LayoutAnchorable tool = new();
        side.Children.Add(new LayoutAnchorGroup { Children = { tool } });
        List<string> events = new();
        root.ElementRemoved += (_, _) => events.Add("removed");
        root.ElementAdded += (_, _) => events.Add("added");
        side.PropertyChanging += (_, e) => { if (e.PropertyName == "Parent") events.Add("before"); };
        side.PropertyChanged += (_, e) => { if (e.PropertyName == "Parent") events.Add("after"); };

        Assert.Throws<NullReferenceException>(() => Assign(side, null, native));

        Assert.Equal(new[] { "before" }, events);
        Assert.Null(side.Parent);
        Assert.Null(side.Root);
        Assert.Null(tool.Root);
        Assert.Same(side, ReadSide(root, edge));
        Assert.Equal(0, Pinned(root, edge));
        events.Clear();
        Assign(side, root, native);
        Assert.Equal(new[] { "before", "after" }, events);
        Assert.Same(root, tool.Root);
        Assert.Equal(1, Pinned(root, edge));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Unrooted_Side_Parent_Remains_Assigned_After_The_Side_Hook_Fails(bool external)
    {
        LayoutRoot root = new();
        LayoutAnchorSide side = root.TopSide!;
        ILayoutContainer parent = external ? new ExternalContainer() : new LayoutPanel();

        Assert.Throws<NullReferenceException>(() => side.Parent = parent);

        Assert.Same(parent, side.Parent);
        Assert.Null(side.Root);
        Assert.Null(side.DockModel.Owner);
        side.Parent = root;
        Assert.Same(root, side.Parent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Direct_Side_Move_Without_A_New_Root_Slot_Releases_The_Previous_Pinned_List(bool native)
    {
        LayoutRoot previous = new();
        LayoutRoot current = new();
        LayoutAnchorSide side = previous.TopSide!;
        side.Children.Add(new LayoutAnchorGroup { Children = { new LayoutAnchorable() } });

        Assign(side, current, native);

        Assert.Same(side, previous.TopSide);
        Assert.Same(current, side.Parent);
        Assert.Null(side.DockModel.Owner);
        Assert.Equal(0, Pinned(previous, AnchorSide.Top));
        Assert.Equal(AnchorSide.Top, side.Side);
        current.LeftSide = side;
        Assert.Equal(AnchorSide.Top, side.Side);
        Assert.Equal(1, Pinned(current, AnchorSide.Left));
    }

    [AvaloniaFact]
    public void Suppressing_The_Side_Base_Hook_Allows_A_Null_Parent_And_Still_Releases_Native_Ownership()
    {
        SideWithoutBaseHook side = new();
        LayoutRoot root = new() { TopSide = side };
        LayoutAnchorable tool = new();
        side.Children.Add(new LayoutAnchorGroup { Children = { tool } });
        int removed = 0;
        root.ElementRemoved += (_, _) => removed++;
        side.Suppress = true;

        side.Parent = null;

        Assert.Null(side.Parent);
        Assert.Null(tool.Root);
        Assert.Equal(1, removed);
        Assert.Equal(0, Pinned(root, AnchorSide.Top));
    }

    private static void Assign(LayoutElement element, ILayoutContainer? parent, bool native)
    {
        if (native) element.DockModel.Owner = (parent as LayoutElement)?.DockModel;
        else element.Parent = parent;
    }

    private static LayoutAnchorSide ReadSide(LayoutRoot root, AnchorSide side) => (side switch
    {
        AnchorSide.Left => root.LeftSide,
        AnchorSide.Top => root.TopSide,
        AnchorSide.Right => root.RightSide,
        _ => root.BottomSide
    })!;

    private static int Pinned(LayoutRoot layout, AnchorSide side)
    {
        RootDock root = (RootDock)layout.DockModel;
        return (side switch
        {
            AnchorSide.Left => root.LeftPinnedDockables,
            AnchorSide.Top => root.TopPinnedDockables,
            AnchorSide.Right => root.RightPinnedDockables,
            _ => root.BottomPinnedDockables
        })?.Count ?? 0;
    }

    private sealed class ProbeDocument : LayoutDocument
    {
        internal List<(ILayoutRoot?, ILayoutRoot?)> RootChanges { get; } = new();
        internal bool SuppressRootNotification { get; set; }
        internal string? FailAt { get; set; }
        internal Exception? Failure { get; set; }
        protected override void OnParentChanging(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            if (FailAt == "parent-changing") throw Failure!;
            base.OnParentChanging(oldValue, newValue);
        }
        protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            if (FailAt == "parent-changed") throw Failure!;
            base.OnParentChanged(oldValue, newValue);
        }
        protected override void OnRootChanged(ILayoutRoot? oldRoot, ILayoutRoot? newRoot)
        {
            RootChanges.Add((oldRoot, newRoot));
            if (FailAt == "root-changed") throw Failure!;
            if (!SuppressRootNotification) base.OnRootChanged(oldRoot, newRoot);
        }
    }

    private sealed class SideWithoutBaseHook : LayoutAnchorSide
    {
        internal bool Suppress { get; set; }
        protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            if (!Suppress) base.OnParentChanged(oldValue, newValue);
        }
    }

    private class ExternalContainer : ILayoutContainer
    {
        public ILayoutContainer? Parent { get; set; }
        public ILayoutRoot? Root => throw new InvalidOperationException("Root traversal must follow Parent.");
        public IEnumerable<ILayoutElement> Children => Array.Empty<ILayoutElement>();
        public int ChildrenCount => 0;
        public void RemoveChild(ILayoutElement element) { }
        public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement) { }
        public event PropertyChangingEventHandler? PropertyChanging { add { } remove { } }
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
    }

    private sealed class ExternalRoot : ExternalContainer, ILayoutRoot
    {
        public DockingManager? Manager => null;
        public LayoutPanel RootPanel => throw new NotSupportedException();
        public LayoutAnchorSide? LeftSide => null;
        public LayoutAnchorSide? TopSide => null;
        public LayoutAnchorSide? RightSide => null;
        public LayoutAnchorSide? BottomSide => null;
        public LayoutContent? ActiveContent { get; set; }
        public ObservableCollection<LayoutAnchorable> Hidden { get; } = new();
        public ObservableCollection<LayoutFloatingWindow> FloatingWindows { get; } = new();
        public void CollectGarbage() { }
    }
}
