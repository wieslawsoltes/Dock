using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using AvalonDock;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class RootPropertyProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("null root panel", () =>
        {
            LayoutRoot root = new();
            LayoutPanel previous = root.RootPanel;
            root.RootPanel = null!;
            if (ReferenceEquals(previous, root.RootPanel) || previous.Parent is not null ||
                root.RootPanel.Children.Count != 1 || root.RootPanel.Children[0] is not LayoutDocumentPane)
                throw new Exception("Clearing the panel did not create the default document layout.");
        });
        foreach (string property in new[] { "LeftSide", "TopSide", "RightSide", "BottomSide" })
            Check("null " + property, () =>
            {
                LayoutRoot root = new();
                LayoutAnchorSide? previous = Read(root, property);
                List<string> events = Observe(root, property);
                Write(root, property, null);
                if (Read(root, property) is not null || root.Children.Contains(previous!) ||
                    root.Children.Count() != 4 || root.ChildrenCount != 5 || string.Join(" ", events) != "before after")
                    throw new Exception("Clearing the side did not retain the nullable-side and notification contracts.");
            });
        Check("panel notification phases", () =>
        {
            LayoutRoot root = new();
            List<string> events = Observe(root, nameof(root.RootPanel));
            root.RootPanel = new LayoutPanel(new LayoutDocumentPane());
            if (string.Join(" ", events) != "before after") throw new Exception("RootPanel did not publish both phases.");
        });
        Check("manager notification phases", () =>
        {
            LayoutRoot root = new();
            List<string> events = Observe(root, nameof(root.Manager));
            DockingManager manager = new() { Layout = root };
            manager.Layout = new LayoutRoot();
            if (string.Join(" ", events) != "before after before after") throw new Exception("Manager did not publish both attachment phases.");
        });
        Check("root child order", () =>
        {
            LayoutRoot root = new();
            LayoutDocumentFloatingWindow window = new() { RootDocument = new LayoutDocument() };
            root.FloatingWindows.Add(window);
            LayoutAnchorable hidden = new();
            root.Hidden.Add(hidden);
            ILayoutElement[] expected = { root.RootPanel, window, root.TopSide!, root.RightSide!, root.BottomSide!, root.LeftSide!, hidden };
            if (!expected.SequenceEqual(root.Children)) throw new Exception("Traversal did not visit floating windows before the sides.");
        });
        Check("hidden replacement notifications", () =>
        {
            LayoutRoot root = new();
            LayoutAnchorable old = new();
            root.Hidden.Add(old);
            List<NotifyCollectionChangedAction> events = new();
            root.Hidden.CollectionChanged += (_, e) => events.Add(e.Action);
            root.ReplaceChild(old, new LayoutAnchorable());
            if (!events.SequenceEqual(new[] { NotifyCollectionChangedAction.Remove, NotifyCollectionChangedAction.Add }))
                throw new Exception("ReplaceChild did not remove before inserting.");
        });
        Check("construction order", () =>
        {
            ProbeRoot root = new();
            if (string.Join(" ", root.Calls) != "before:RightSide after:RightSide before:LeftSide after:LeftSide before:TopSide after:TopSide before:BottomSide after:BottomSide before:RootPanel after:RootPanel")
                throw new Exception("The derived root did not observe constructor property order.");
        });
        foreach (string property in new[] { "RootPanel", "TopSide" })
            Check(property + " changing failure", () =>
            {
                LayoutRoot root = new();
                ILayoutElement previous = property == "RootPanel" ? root.RootPanel : root.TopSide!;
                InvalidOperationException expected = new("before root mutation");
                root.PropertyChanging += (_, e) => { if (e.PropertyName == property) throw expected; };
                Exception? actual = null;
                try
                {
                    if (property == "RootPanel") root.RootPanel = new LayoutPanel();
                    else root.TopSide = new LayoutAnchorSide();
                }
                catch (Exception error) { actual = error; }
                if (!ReferenceEquals(expected, actual) || !ReferenceEquals(previous.Parent, root))
                    throw new Exception("A changing observer could not stop the root mutation.");
            });
        Check("compiled null root slots", () =>
        {
            RootPropertiesProbeView view = new();
            foreach (string name in new[] { "DefaultManager", "PrefixedManager" })
            {
                DockingManager manager = view.FindControl<DockingManager>(name)!;
                if (manager.Layout.RootPanel.Children.Single() is not LayoutDocumentPane || manager.Layout.Children.Count() != 1)
                    throw new Exception("Compiled XAML did not clear the root slots.");
            }
        });
        if (failures.Count != 0) throw new Exception("Packaged root-property failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static List<string> Observe(LayoutRoot root, string property)
    {
        List<string> events = new();
        root.PropertyChanging += (_, e) => { if (e.PropertyName == property) events.Add("before"); };
        root.PropertyChanged += (_, e) => { if (e.PropertyName == property) events.Add("after"); };
        return events;
    }

    private static LayoutAnchorSide? Read(LayoutRoot root, string property) => property switch
    {
        "LeftSide" => root.LeftSide,
        "TopSide" => root.TopSide,
        "RightSide" => root.RightSide,
        _ => root.BottomSide
    };

    private static void Write(LayoutRoot root, string property, LayoutAnchorSide? value)
    {
        switch (property)
        {
            case "LeftSide": root.LeftSide = value; break;
            case "TopSide": root.TopSide = value; break;
            case "RightSide": root.RightSide = value; break;
            default: root.BottomSide = value; break;
        }
    }

    private sealed class ProbeRoot : LayoutRoot
    {
        private List<string>? _calls;
        internal IReadOnlyList<string> Calls => _calls!;
        protected override void RaisePropertyChanging(string? propertyName = null)
        {
            if (IsSlot(propertyName)) (_calls ??= new()).Add("before:" + propertyName);
            base.RaisePropertyChanging(propertyName);
        }
        protected override void RaisePropertyChanged(string? propertyName = null)
        {
            if (IsSlot(propertyName)) (_calls ??= new()).Add("after:" + propertyName);
            base.RaisePropertyChanged(propertyName);
        }
        private static bool IsSlot(string? name) => name is "RootPanel" or "LeftSide" or "TopSide" or "RightSide" or "BottomSide";
    }
}
