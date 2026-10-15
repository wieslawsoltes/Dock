using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

namespace PackageConsumer;

internal static class GarbageCollectionProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        Check("parent-root reference validation", () =>
        {
            LayoutDocument content = new();
            LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) };
            content.PreviousContainer = root.RootPanel;
            content.PreviousContainerIndex = 7;
            root.CollectGarbage();
            Require(content.PreviousContainer is null && content.PreviousContainerIndex == 7);
        });
        Check("saved return identifier", () =>
        {
            LayoutDocument content = new();
            LayoutAnchorablePane empty = new();
            LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) { Children = { empty } } };
            content.PreviousContainer = empty;
            content.PreviousContainerId = "saved";
            root.CollectGarbage();
            Require(content.PreviousContainer is null && content.PreviousContainerId == "saved" && content.PreviousContainerIndex == -1);
        });
        Check("non-pane return reference", () =>
        {
            LayoutDocument content = new();
            LayoutPanel empty = new();
            LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(content)) { Children = { empty } } };
            content.PreviousContainer = empty;
            root.CollectGarbage();
            Require(ReferenceEquals(content.PreviousContainer, empty));
        });
        Check("container pruning requires a pane removal", () =>
        {
            LayoutRoot root = new();
            LayoutPanel empty = new();
            root.RootPanel.Children.Add(empty);
            root.CollectGarbage();
            Require(root.RootPanel.Children.Contains(empty));
        });
        Check("last document pane survives", () =>
        {
            LayoutDocumentPane first = new();
            LayoutDocumentPane last = new();
            LayoutRoot root = new() { RootPanel = new LayoutPanel(first) { Children = { last } } };
            root.CollectGarbage();
            Require(root.RootPanel.Children.Count == 1 && ReferenceEquals(root.RootPanel.Children[0], last));
        });
        Check("empty root panel resets", () =>
        {
            LayoutPanel panel = new(new LayoutAnchorablePane());
            LayoutRoot root = new() { RootPanel = panel };
            root.CollectGarbage();
            Require(!ReferenceEquals(root.RootPanel, panel) && root.RootPanel.Children[0] is LayoutDocumentPane);
        });
        Check("group cleanup retains focused content", () =>
        {
            LayoutDocument content = new();
            LayoutDocumentPane populated = new(content);
            LayoutDocumentPane empty = new();
            LayoutPanel panel = new(populated) { Children = { empty } };
            LayoutRoot root = new() { RootPanel = panel };
            ((Dock.Model.Core.IDock)panel.DockModel).ActiveDockable = empty.DockModel;
            content.IsActive = true;
            root.CollectGarbage();
            Require(ReferenceEquals(root.ActiveContent, content) && content.IsActive);
        });
        foreach (bool tools in new[] { false, true })
        {
            Check($"live collapse children, tools={tools}", () =>
            {
                ILayoutGroup inner = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
                ILayoutGroup outer = tools ? new LayoutAnchorablePaneGroup() : new LayoutDocumentPaneGroup();
                inner.InsertChildAt(0, Pane(tools));
                outer.InsertChildAt(0, inner);
                LayoutRoot root = new() { RootPanel = new LayoutPanel((ILayoutPanelElement)outer) };
                ILayoutElement added = Pane(tools);
                void Changed(object? sender, NotifyCollectionChangedEventArgs args)
                {
                    if (args.Action == NotifyCollectionChangedAction.Remove) inner.InsertChildAt(inner.ChildrenCount, added);
                }
                if (tools) ((LayoutAnchorablePaneGroup)outer).Children.CollectionChanged += Changed;
                else ((LayoutDocumentPaneGroup)outer).Children.CollectionChanged += Changed;
                root.CollectGarbage();
                Require(outer.ChildrenCount == 2 && outer.IndexOfChild(added) == 1);
            });
        }
        foreach (bool individualReference in new[] { false, true })
        {
            Check($"floating return XML callback, individual={individualReference}", () =>
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
                Require(floatingInCallback == true && tool.IsFloating && tool.PreviousContainer?.FindParent<LayoutAnchorableFloatingWindow>() is not null);
            });
        }
        if (failures.Count != 0) throw new InvalidOperationException("Packaged garbage collection failures: " + string.Join("; ", failures));
        Console.WriteLine($"Garbage collection: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static ILayoutElement Pane(bool tools) => tools
        ? new LayoutAnchorablePane(new LayoutAnchorable()) : new LayoutDocumentPane(new LayoutDocument());

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Layout collection contract changed.");
    }
}
