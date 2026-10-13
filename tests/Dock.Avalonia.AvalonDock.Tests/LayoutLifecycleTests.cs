using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Headless.XUnit;
using AvalonDock;
using Dock.Model.Core;
using Dock.Model.Core.Events;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class LayoutLifecycleTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Direct_Parent_Changes_Notify_Before_And_After_Native_Ownership(bool native)
    {
        ProbeDocument document = new();
        LayoutDocumentPane first = new(document);
        LayoutDocumentPane second = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(first) };
        root.RootPanel.Children.Add(second);
        document.Transitions.Clear();
        int added = 0;
        int removed = 0;
        root.ElementAdded += (_, _) => added++;
        root.ElementRemoved += (_, _) => removed++;

        if (native) document.DockModel.Owner = second.DockModel;
        else document.Parent = second;

        Assert.Collection(document.Transitions,
            change => AssertTransition(change, "changing", first, second, first, root),
            change => AssertTransition(change, "changed", first, second, second, root));
        Assert.Equal(0, added);
        Assert.Equal(0, removed);
        Assert.Same(second, document.Parent);
    }

    [AvaloniaFact]
    public void Collection_Moves_Between_Roots_Report_The_Public_Node_And_Clear_Old_Focus()
    {
        ProbeDocument document = new();
        LayoutDocumentPane first = new(document);
        LayoutDocumentPane second = new();
        LayoutRoot oldRoot = new() { RootPanel = new LayoutPanel(first) };
        LayoutRoot newRoot = new() { RootPanel = new LayoutPanel(second) };
        oldRoot.ActiveContent = document;
        List<(string Kind, LayoutElement Element)> events = new();
        oldRoot.ElementRemoved += (sender, args) =>
        {
            Assert.Same(oldRoot, sender);
            // A leaf removal reports membership before the pane selects its
            // replacement. Upstream root cleanup only clears descendants here.
            Assert.Same(document, oldRoot.ActiveContent);
            Assert.Same(document, oldRoot.LastFocusedDocument);
            Assert.Null(args.Element.Parent);
            events.Add(("removed", args.Element));
        };
        newRoot.ElementAdded += (sender, args) =>
        {
            Assert.Same(newRoot, sender);
            Assert.Same(newRoot, args.Element.Root);
            Assert.Null(oldRoot.ActiveContent);
            Assert.Null(oldRoot.LastFocusedDocument);
            events.Add(("added", args.Element));
        };
        document.Transitions.Clear();
        document.RootTransitions.Clear();

        second.Children.Add(document);

        Assert.Equal(new[] { ("removed", (LayoutElement)document), ("added", (LayoutElement)document) }, events);
        Assert.Equal(new (ILayoutRoot?, ILayoutRoot?)[] { (oldRoot, null), (null, newRoot) }, document.RootTransitions);
        Assert.Collection(document.Transitions,
            change => AssertTransition(change, "changing", first, null, first, oldRoot),
            change => AssertTransition(change, "changed", first, null, null, null),
            change => AssertTransition(change, "changing", null, second, null, null),
            change => AssertTransition(change, "changed", null, second, second, newRoot));
        Assert.Same(newRoot.DockFactory, document.DockModel.Factory);
        Assert.Empty(first.Children);
        Assert.Same(document, Assert.Single(second.Children));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Throwing_Parent_Changing_Hooks_Leave_Ownership_Unchanged(bool native)
    {
        ProbeDocument document = new();
        LayoutDocumentPane first = new(document);
        LayoutDocumentPane second = new();
        document.Transitions.Clear();
        document.RejectChange = true;

        Assert.Throws<InvalidOperationException>(() =>
        {
            if (native) document.DockModel.Owner = second.DockModel;
            else document.Parent = second;
        });
        Assert.Same(first, document.Parent);
        Assert.Same(first.DockModel, document.DockModel.Owner);
        Assert.Single(document.Transitions);
        document.RejectChange = false;
        document.Transitions.Clear();
        document.Parent = second;
        Assert.Same(second, document.Parent);
        Assert.Equal(2, document.Transitions.Count);
    }

    [AvaloniaFact]
    public void Reassigning_The_Same_Parent_Does_Not_Invoke_Hooks_Or_Notifications()
    {
        ProbeDocument document = new();
        LayoutDocumentPane parent = new(document);
        document.Transitions.Clear();
        List<string?> notifications = new();
        document.PropertyChanging += (_, args) => notifications.Add(args.PropertyName);
        document.PropertyChanged += (_, args) => notifications.Add(args.PropertyName);
        document.Parent = parent;
        document.DockModel.Owner = parent.DockModel;
        Assert.Empty(document.Transitions);
        Assert.Empty(notifications);
    }

    [AvaloniaFact]
    public void Subtree_Attachment_And_Detachment_Report_The_Subtree_Root()
    {
        LayoutRoot root = new();
        LayoutDocument document = new();
        LayoutDocumentPane pane = new(document);
        List<LayoutElement> added = new();
        List<LayoutElement> removed = new();
        root.ElementAdded += (_, args) => added.Add(args.Element);
        root.ElementRemoved += (_, args) =>
        {
            Assert.Null(root.ActiveContent);
            Assert.Null(root.LastFocusedDocument);
            removed.Add(args.Element);
        };
        root.RootPanel.Children.Add(pane);
        Assert.Same(pane, Assert.Single(added));
        Assert.Same(root, document.Root);
        root.ActiveContent = document;
        root.RootPanel.Children.Remove(pane);
        Assert.Same(pane, Assert.Single(removed));
        Assert.Null(document.Root);
    }

    [AvaloniaFact]
    public void Floating_Document_Parent_Hooks_Do_Not_Expose_The_Internal_Native_Pane()
    {
        ProbeDocument document = new();
        LayoutDocumentPane pane = new(document);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        DockingManager manager = new() { Layout = root };
        document.Transitions.Clear();

        document.Float();

        LayoutDocumentFloatingWindow window = Assert.IsType<LayoutDocumentFloatingWindow>(Assert.Single(root.FloatingWindows));
        Assert.Same(window, document.Parent);
        foreach (Transition transition in document.Transitions)
        {
            Assert.True(transition.Previous is null || ReferenceEquals(transition.Previous, pane) || ReferenceEquals(transition.Previous, window));
            Assert.True(transition.Next is null || ReferenceEquals(transition.Next, pane) || ReferenceEquals(transition.Next, window));
        }
        document.Transitions.Clear();
        document.Dock();
        Assert.Same(pane, document.Parent);
        foreach (Transition transition in document.Transitions)
        {
            Assert.True(transition.Previous is null || ReferenceEquals(transition.Previous, pane) || ReferenceEquals(transition.Previous, window));
            Assert.True(transition.Next is null || ReferenceEquals(transition.Next, pane) || ReferenceEquals(transition.Next, window));
        }
    }

    [AvaloniaFact]
    public void Native_Owner_Assignment_Reuses_Existing_Wrappers_From_Separate_Trees()
    {
        ProbeDocument document = new();
        LayoutDocumentPane first = new(document);
        LayoutDocumentPane second = new();
        document.Transitions.Clear();

        document.DockModel.Owner = second.DockModel;

        Assert.Same(second, document.Parent);
        Assert.Collection(document.Transitions,
            change => AssertTransition(change, "changing", first, second, first, null),
            change => AssertTransition(change, "changed", first, second, second, null));
    }

    [AvaloniaFact]
    public void Native_Factory_Moves_Invoke_Hooks_With_Completed_Public_Parent_Values()
    {
        ProbeDocument document = new();
        LayoutDocumentPane first = new(document);
        LayoutDocumentPane second = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(first) };
        root.RootPanel.Children.Add(second);
        document.Transitions.Clear();

        root.DockFactory.MoveDockable((IDock)first.DockModel, (IDock)second.DockModel, document.DockModel, null);

        Assert.Same(second, document.Parent);
        Assert.NotEmpty(document.Transitions);
        foreach (Transition change in document.Transitions)
            Assert.Same(change.Phase == "changing" ? change.Previous : change.Next, change.ObservedParent);
        Assert.Same(second, document.Transitions[^1].Next);
    }

    [AvaloniaFact]
    public void Property_Hooks_Root_Events_And_Updated_Notifications_Follow_Parent_Transition_Order()
    {
        ProbeDocument document = new();
        LayoutDocumentPane pane = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        List<string> order = new();
        document.TransitionObserved = phase => order.Add(phase);
        document.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName == nameof(LayoutElement.Parent)) order.Add("property-changing");
        };
        document.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(LayoutElement.Parent)) order.Add("property-changed");
        };
        root.ElementAdded += (_, args) =>
        {
            Assert.Same(document, args.Element);
            order.Add("element-added");
        };
        root.Updated += (_, _) => order.Add("updated");

        document.Parent = pane;

        Assert.Equal(new[] { "property-changing", "changing", "changed", "root", "element-added", "property-changed", "updated" }, order);
    }

    [AvaloniaFact]
    public void A_Root_Hook_Override_Can_Replace_The_Default_Membership_Notifications()
    {
        ProbeDocument document = new() { SuppressRootNotification = true };
        LayoutDocumentPane pane = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
        int added = 0;
        root.ElementAdded += (_, _) => added++;
        pane.Children.Add(document);
        Assert.Same(root, document.Root);
        Assert.Equal(0, added);
        Assert.Equal(new (ILayoutRoot?, ILayoutRoot?)[] { (null, root) }, document.RootTransitions);
    }

    [AvaloniaFact]
    public void A_Rejected_Native_Move_Does_Not_Retain_The_Unrelated_Layout_Whose_Context_Was_Joined()
    {
        LayoutDocument document = new();
        LayoutRoot retained = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
        WeakReference<LayoutRoot> reference = JoinRejectedDestination(document);
        for (int attempt = 0; attempt < 3; attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        Assert.False(reference.TryGetTarget(out _));
        Assert.Same(retained, document.Root);
        GC.KeepAlive(retained);
    }

    [AvaloniaTheory]
    [InlineData("hide")]
    [InlineData("auto-hide")]
    [InlineData("float")]
    public void Tool_Lifecycle_Callbacks_Observe_Their_Public_Parent_In_Both_Directions(string operation)
    {
        ProbeAnchorable tool = new();
        LayoutAnchorablePane pane = new(tool);
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane()) };
        DockingManager manager = new() { Layout = root };
        root.RootPanel.Children.Add(pane);
        tool.Transitions.Clear();
        switch (operation)
        {
            case "hide": tool.Hide(); tool.Show(); break;
            case "auto-hide": tool.ToggleAutoHide(); tool.ToggleAutoHide(); break;
            case "float": tool.Float(); tool.Dock(); break;
        }

        Assert.Same(pane, tool.Parent);
        Assert.Same(root, tool.Root);
        Assert.NotEmpty(tool.Transitions);
        foreach (Transition change in tool.Transitions)
            Assert.Same(change.Phase == "changing" ? change.Previous : change.Next, change.ObservedParent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Public_Parent_Changes_Reject_Invalid_Thread_Access_Before_Callbacks_Or_Projection_Changes(bool floating)
    {
        ProbeDocument document = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
        DockingManager manager = new() { Layout = root };
        if (floating) document.Float();
        ILayoutContainer? parent = document.Parent;
        IDockable? owner = document.DockModel.Owner;
        document.Transitions.Clear();

        Exception? exception = await Task.Run(() => Record.Exception(() => document.Parent = null));

        Assert.IsType<InvalidOperationException>(exception);
        Assert.Same(parent, document.Parent);
        Assert.Same(owner, document.DockModel.Owner);
        Assert.Empty(document.Transitions);
        GC.KeepAlive(root);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference<LayoutRoot> JoinRejectedDestination(LayoutDocument document)
    {
        LayoutDocumentPane destination = new();
        LayoutRoot root = new() { RootPanel = new LayoutPanel(destination) };
        EventHandler<DockableOwnerChangingEventArgs> reject = (_, _) => throw new InvalidOperationException("Rejected");
        document.DockModel.OwnerChanging += reject;
        try { Assert.Throws<InvalidOperationException>(() => document.DockModel.Owner = destination.DockModel); }
        finally { document.DockModel.OwnerChanging -= reject; }
        return new(root);
    }

    private static void AssertTransition(Transition transition, string phase, ILayoutContainer? previous,
        ILayoutContainer? next, ILayoutContainer? observed, ILayoutRoot? root)
    {
        Assert.Equal(phase, transition.Phase);
        Assert.Same(previous, transition.Previous);
        Assert.Same(next, transition.Next);
        Assert.Same(observed, transition.ObservedParent);
        Assert.Same(root, transition.Root);
        Assert.Same((observed as LayoutElement)?.DockModel, transition.NativeOwner);
    }

    private sealed record Transition(string Phase, ILayoutContainer? Previous, ILayoutContainer? Next,
        ILayoutContainer? ObservedParent, ILayoutRoot? Root, IDockable? NativeOwner);

    private sealed class ProbeDocument : LayoutDocument
    {
        internal List<Transition> Transitions { get; } = new();
        internal List<(ILayoutRoot? Previous, ILayoutRoot? Next)> RootTransitions { get; } = new();
        internal bool RejectChange { get; set; }
        internal bool SuppressRootNotification { get; set; }
        internal Action<string>? TransitionObserved { get; set; }

        protected override void OnParentChanging(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            Transitions.Add(new("changing", oldValue, newValue, Parent, Root, DockModel.Owner));
            TransitionObserved?.Invoke("changing");
            if (RejectChange) throw new InvalidOperationException("Rejected parent change");
            base.OnParentChanging(oldValue, newValue);
        }

        protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            Transitions.Add(new("changed", oldValue, newValue, Parent, Root, DockModel.Owner));
            TransitionObserved?.Invoke("changed");
            base.OnParentChanged(oldValue, newValue);
        }

        protected override void OnRootChanged(ILayoutRoot? oldRoot, ILayoutRoot? newRoot)
        {
            RootTransitions.Add((oldRoot, newRoot));
            TransitionObserved?.Invoke("root");
            if (!SuppressRootNotification) base.OnRootChanged(oldRoot, newRoot);
        }
    }

    private sealed class ProbeAnchorable : LayoutAnchorable
    {
        internal List<Transition> Transitions { get; } = new();

        protected override void OnParentChanging(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            Transitions.Add(new("changing", oldValue, newValue, Parent, Root, DockModel.Owner));
            base.OnParentChanging(oldValue, newValue);
        }

        protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            Transitions.Add(new("changed", oldValue, newValue, Parent, Root, DockModel.Owner));
            base.OnParentChanged(oldValue, newValue);
        }
    }
}
