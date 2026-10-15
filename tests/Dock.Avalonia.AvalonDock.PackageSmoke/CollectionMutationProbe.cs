using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class CollectionMutationProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (bool tools in new[] { false, true })
        {
            Check($"nested count callback, tools={tools}", () =>
            {
                LayoutContent incoming = Content(tools);
                LayoutContent sibling = Content(tools);
                ILayoutGroup pane = tools ? new LayoutAnchorablePane() : new LayoutDocumentPane();
                System.ComponentModel.INotifyPropertyChanged children = tools
                    ? ((LayoutAnchorablePane)pane).Children : ((LayoutDocumentPane)pane).Children;
                bool added = false;
                children.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName != "Count" || added) return;
                    added = true;
                    pane.InsertChildAt(pane.ChildrenCount, sibling);
                };
                pane.InsertChildAt(0, incoming);
                Dock.Model.Core.IDock native = (Dock.Model.Core.IDock)((LayoutElement)pane).DockModel;
                Require(pane.ChildrenCount == 2 && native.VisibleDockables?.Count == 2 &&
                    ReferenceEquals(incoming.Parent, pane) && ReferenceEquals(sibling.Parent, pane) &&
                    ReferenceEquals(incoming.DockModel.Owner, native) && ReferenceEquals(sibling.DockModel.Owner, native));
            });
            foreach (bool rooted in new[] { false, true })
            {
                Check($"same content, tools={tools}, rooted={rooted}", () =>
                {
                    LayoutContent content = Content(tools);
                    ILayoutGroup pane = Pane(content);
                    LayoutRoot? root = rooted ? new() { RootPanel = new((ILayoutPanelElement)pane) } : null;
                    List<ILayoutContainer?> parents = new();
                    NotifyCollectionChangedEventArgs? change = null;
                    content.PropertyChanged += (_, args) => { if (args.PropertyName == "Parent") parents.Add(content.Parent); };
                    if (tools) ((LayoutAnchorablePane)pane).Children.CollectionChanged += (_, args) => change = args;
                    else ((LayoutDocumentPane)pane).Children.CollectionChanged += (_, args) => change = args;
                    pane.ReplaceChildAt(0, content);
                    Require(parents.Count == 2 && parents[0] is null && ReferenceEquals(parents[1], pane));
                    Require(change?.Action == NotifyCollectionChangedAction.Replace && ReferenceEquals(change.OldItems![0], content) && ReferenceEquals(change.NewItems![0], content));
                    GC.KeepAlive(root);
                });
            }
            foreach (bool replace in new[] { false, true })
            {
                Check($"incoming parent phase, tools={tools}, replace={replace}", () =>
                {
                    LayoutContent incoming = Content(tools);
                    ILayoutGroup previous = Pane(incoming);
                    ILayoutGroup target = Pane(Content(tools));
                    int observed = 0;
                    incoming.PropertyChanged += (_, args) =>
                    {
                        if (args.PropertyName != "Parent") return;
                        Require(target.IndexOfChild(incoming) == (replace ? 0 : 1));
                        observed++;
                    };
                    if (replace) target.ReplaceChildAt(0, incoming);
                    else target.InsertChildAt(1, incoming);
                    Require(observed == 2 && previous.ChildrenCount == 0 && ReferenceEquals(incoming.Parent, target));
                });
            }
            Check($"same floating window, tools={tools}", () =>
            {
                LayoutRoot root = new();
                LayoutFloatingWindow floating = tools
                    ? new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) }
                    : new LayoutDocumentFloatingWindow { RootDocument = new() };
                root.FloatingWindows.Add(floating);
                List<ILayoutContainer?> parents = new();
                NotifyCollectionChangedEventArgs? change = null;
                floating.PropertyChanged += (_, args) => { if (args.PropertyName == "Parent") parents.Add(floating.Parent); };
                root.FloatingWindows.CollectionChanged += (_, args) => change = args;
                root.FloatingWindows[0] = floating;
                Require(parents.Count == 2 && parents[0] is null && ReferenceEquals(parents[1], root));
                Require(change?.Action == NotifyCollectionChangedAction.Replace && ReferenceEquals(change.NewItems![0], floating));
                root.FloatingWindows.Clear();
            });
        }
        foreach (bool pinned in new[] { false, true })
        {
            Check($"active replacement, pinned={pinned}", () =>
            {
                LayoutRoot root = new();
                LayoutAnchorable content = new();
                LayoutAnchorGroup group = new();
                if (pinned) { group.Children.Add(content); root.LeftSide!.Children.Add(group); }
                else root.Hidden.Add(content);
                root.ActiveContent = content;
                List<ILayoutContainer?> parents = new();
                content.PropertyChanged += (_, args) => { if (args.PropertyName == "Parent") parents.Add(content.Parent); };
                if (pinned) group.Children[0] = content;
                else root.Hidden[0] = content;
                Require(content.IsActive && ReferenceEquals(root.ActiveContent, content) && parents.Count == 2 && parents[0] is null);
            });
        }
        if (failures.Count != 0) throw new InvalidOperationException("Packaged collection mutation failures: " + string.Join("; ", failures));
        Console.WriteLine($"Collection mutation: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static LayoutContent Content(bool tools) => tools ? new LayoutAnchorable() : new LayoutDocument();
    private static ILayoutGroup Pane(LayoutContent content) => content is LayoutAnchorable tool ? new LayoutAnchorablePane(tool) : new LayoutDocumentPane(content);
    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Collection mutation contract changed.");
    }
}
