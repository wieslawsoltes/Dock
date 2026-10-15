using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;

namespace PackageConsumer;

internal static class ParentTransitionProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("ancestor attachment", () =>
        {
            ProbeDocument child = new();
            LayoutDocumentPane pane = new(child);
            LayoutRoot root = new() { RootPanel = new LayoutPanel(pane) };
            child.Parent = root;
            Require(child.Changes.SequenceEqual(new[] { ((ILayoutRoot?)null, (ILayoutRoot?)root) }));
        });
        Check("ancestor move", () =>
        {
            LayoutDocumentPane pane = new();
            LayoutRoot first = new() { RootPanel = new LayoutPanel(pane) };
            ProbeDocument child = new();
            pane.Children.Add(child);
            LayoutRoot second = new();
            second.RootPanel.Children.Add(pane);
            child.Changes.Clear();
            child.Parent = second;
            Require(child.Changes.SequenceEqual(new[] { ((ILayoutRoot?)first, (ILayoutRoot?)second) }));
        });
        Check("root ancestor lookup", () => Require(new LayoutRoot().Root is null));
        Check("parent property notifications", () =>
        {
            LayoutDocument child = new();
            List<string?> properties = new();
            child.PropertyChanged += (_, e) => properties.Add(e.PropertyName);
            child.Parent = new LayoutRoot();
            Require(properties.Contains("Parent") && !properties.Contains("Root"));
        });
        Check("updated current root", () =>
        {
            LayoutRoot second = new();
            LayoutRoot third = new();
            LayoutPanel child = new();
            List<LayoutRoot> updates = new();
            second.Updated += (_, _) => updates.Add(second);
            third.Updated += (_, _) => updates.Add(third);
            child.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == "Parent" && ReferenceEquals(child.Parent, second)) child.Parent = third;
            };
            child.Parent = second;
            Require(updates.SequenceEqual(new[] { third, third }));
        });
        Check("application parent", () =>
        {
            LayoutRoot root = new();
            ApplicationContainer parent = new() { Parent = root };
            LayoutDocument child = new() { Parent = parent };
            Require(ReferenceEquals(child.Parent, parent) && ReferenceEquals(child.Root, root) && child.DockModel.Owner is null);
        });
        foreach (bool suppress in new[] { false, true })
            Check("application root " + suppress, () =>
            {
                ApplicationRoot root = new();
                ProbeDocument child = new() { SuppressRoot = suppress };
                Exception? failure = null;
                try { child.Parent = root; } catch (Exception error) { failure = error; }
                Require(suppress ? failure is null : failure is InvalidCastException);
                Require(ReferenceEquals(child.Root, root) && child.Changes.Count == 1);
            });
        foreach (bool rootPhase in new[] { false, true })
            Check("failed hook recovery " + rootPhase, () =>
            {
                LayoutRoot first = new();
                LayoutRoot second = new();
                LayoutRoot third = new();
                ProbeDocument child = new() { Parent = first, FailParent = !rootPhase, FailRoot = rootPhase };
                Exception? failure = null;
                try { child.Parent = second; } catch (Exception error) { failure = error; }
                Require(failure is InvalidOperationException);
                child.FailParent = child.FailRoot = false;
                child.Changes.Clear();
                child.Parent = third;
                Require(child.Changes.SequenceEqual(new[] { ((ILayoutRoot?)(rootPhase ? second : first), (ILayoutRoot?)third) }));
            });
        foreach (AnchorSide edge in new[] { AnchorSide.Left, AnchorSide.Top, AnchorSide.Right, AnchorSide.Bottom })
            Check("null side parent " + edge, () =>
            {
                LayoutRoot root = new();
                LayoutAnchorSide side = ReadSide(root, edge);
                side.Children.Add(new LayoutAnchorGroup { Children = { new LayoutAnchorable() } });
                int membership = 0;
                root.ElementAdded += (_, _) => membership++;
                root.ElementRemoved += (_, _) => membership++;
                Exception? failure = null;
                try { side.Parent = null; } catch (Exception error) { failure = error; }
                Require(failure is NullReferenceException && side.Parent is null && membership == 0 && Pinned(root, edge) == 0);
                side.Parent = root;
                Require(membership == 0 && Pinned(root, edge) == 1);
            });
        Check("direct side move", () =>
        {
            LayoutRoot previous = new();
            LayoutRoot current = new();
            LayoutAnchorSide side = previous.TopSide!;
            side.Children.Add(new LayoutAnchorGroup { Children = { new LayoutAnchorable() } });
            side.Parent = current;
            Require(ReferenceEquals(previous.TopSide, side) && ReferenceEquals(side.Parent, current) &&
                side.DockModel.Owner is null && Pinned(previous, AnchorSide.Top) == 0);
        });
        Check("suppressed side hook", () =>
        {
            SuppressedSide side = new();
            LayoutRoot root = new() { TopSide = side };
            side.Children.Add(new LayoutAnchorGroup { Children = { new LayoutAnchorable() } });
            side.Suppress = true;
            side.Parent = null;
            Require(side.Parent is null && Pinned(root, AnchorSide.Top) == 0);
        });
        if (failures.Count != 0) throw new Exception("Packaged parent-transition failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); } catch (Exception error) { failures.Add(name + ": " + error.GetType().Name + " " + error.Message); }
        }
    }

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Parent transition contract mismatch.");
    }

    private static LayoutAnchorSide ReadSide(LayoutRoot root, AnchorSide side) => (side switch
    {
        AnchorSide.Left => root.LeftSide, AnchorSide.Top => root.TopSide,
        AnchorSide.Right => root.RightSide, _ => root.BottomSide
    })!;

    private static int Pinned(LayoutRoot layout, AnchorSide side)
    {
        RootDock root = (RootDock)layout.DockModel;
        return (side switch
        {
            AnchorSide.Left => root.LeftPinnedDockables, AnchorSide.Top => root.TopPinnedDockables,
            AnchorSide.Right => root.RightPinnedDockables, _ => root.BottomPinnedDockables
        })?.Count ?? 0;
    }

    private sealed class ProbeDocument : LayoutDocument
    {
        internal List<(ILayoutRoot?, ILayoutRoot?)> Changes { get; } = new();
        internal bool FailParent { get; set; }
        internal bool FailRoot { get; set; }
        internal bool SuppressRoot { get; set; }
        protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            if (FailParent) throw new InvalidOperationException("parent hook");
            base.OnParentChanged(oldValue, newValue);
        }
        protected override void OnRootChanged(ILayoutRoot? oldRoot, ILayoutRoot? newRoot)
        {
            Changes.Add((oldRoot, newRoot));
            if (FailRoot) throw new InvalidOperationException("root hook");
            if (!SuppressRoot) base.OnRootChanged(oldRoot, newRoot);
        }
    }

    private sealed class SuppressedSide : LayoutAnchorSide
    {
        internal bool Suppress { get; set; }
        protected override void OnParentChanged(ILayoutContainer? oldValue, ILayoutContainer? newValue)
        {
            if (!Suppress) base.OnParentChanged(oldValue, newValue);
        }
    }

    private class ApplicationContainer : ILayoutContainer
    {
        public ILayoutContainer? Parent { get; set; }
        public ILayoutRoot? Root => throw new InvalidOperationException("Follow the Parent chain.");
        public IEnumerable<ILayoutElement> Children => Array.Empty<ILayoutElement>();
        public int ChildrenCount => 0;
        public void RemoveChild(ILayoutElement element) { }
        public void ReplaceChild(ILayoutElement oldElement, ILayoutElement newElement) { }
        public event PropertyChangingEventHandler? PropertyChanging { add { } remove { } }
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
    }

    private sealed class ApplicationRoot : ApplicationContainer, ILayoutRoot
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
