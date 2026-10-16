using System;
using System.Collections.Generic;
using System.ComponentModel;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class LayoutInterfaceProbe
{
    internal static void Verify()
    {
        LayoutRoot root = new();
        ExternalNode node = new() { Parent = root };
        if (!ReferenceEquals(root, node.GetRoot()))
            throw new Exception("The packaged layout helper did not traverse the public parent contract.");
        try { node.FindParent<int>(); throw new Exception("An absent value-type parent did not retain its cast error."); }
        catch (NullReferenceException) { }

        LayoutAnchorable removed = new(), retained = new();
        LayoutAnchorablePane pane = new() { Children = { removed, retained } };
        root.RootPanel = new LayoutPanel(pane);
        ILayoutGroup group = pane;
        group.ReplaceChildAt(-1, new LayoutDocument());
        group.ReplaceChildAt(2, new LayoutDocument());
        if (pane.ChildrenCount != 2) throw new Exception("An invalid replacement index changed the packaged group.");
        try { group.RemoveChild(new LayoutDocument()); throw new Exception("An incompatible group removal did not raise its cast error."); }
        catch (InvalidCastException) { }

        List<string> trace = new();
        group.ChildrenCollectionChanged += (_, _) =>
        {
            if (removed.Parent is not null || removed.DockModel.Owner is not null || removed.IsSelected ||
                !ReferenceEquals(pane.SelectedContent, retained))
                throw new Exception("A packaged group callback observed stale removal ownership or selection.");
            trace.Add("children");
        };
        pane.ChildrenTreeChanged += (_, _) => trace.Add("tree");
        pane.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(pane.ChildrenCount)) trace.Add("count"); };
        pane.Children.CollectionChanged += (_, _) => trace.Add("collection");
        group.RemoveChild(removed);
        if (string.Join(",", trace) != "children,tree,count,collection")
            throw new Exception("The packaged group changed callback delivery order.");
    }

    private sealed class ExternalNode : ILayoutElement
    {
        public ILayoutContainer? Parent { get; set; }
        public ILayoutRoot? Root => throw new InvalidOperationException("GetRoot must not read this property.");
        public event PropertyChangedEventHandler? PropertyChanged { add { } remove { } }
        public event PropertyChangingEventHandler? PropertyChanging { add { } remove { } }
    }
}
