using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml;
using AvalonDock;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class ModelDockingProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        foreach (bool tool in new[] { false, true })
        {
            Check($"{tool}: detached float", () => RequiresNullReference(Create(tool).Float));
            Check($"{tool}: detached dock", () => RequiresNullReference(Create(tool).Dock));
            Check($"{tool}: detached protected dock", () =>
            {
                if (tool) RequiresNullReference(new ProbeTool().InvokeBaseDock);
                else RequiresNullReference(new ProbeDocument().InvokeBaseDock);
            });
            Check($"{tool}: suppressed protected dock", () =>
            {
                LayoutContent content = tool ? new ProbeTool() : new ProbeDocument();
                LayoutDocument other = new();
                LayoutDocumentPane pane = new(other);
                pane.Children.Add(content);
                DockingManager manager = new() { Layout = new() { RootPanel = new LayoutPanel(pane) } };
                other.IsActive = true;
                using XmlReader reader = XmlReader.Create(new StringReader("<Content IsFloating='true' />"));
                reader.MoveToContent();
                content.ReadXml(reader);
                bool completed = false;
                manager.Docked += (_, _) =>
                {
                    if (!content.IsFloating || content.IsActive || content.IsSelected)
                        throw new Exception("Completion observed an early activation or floating reset.");
                    completed = true;
                };
                content.Dock();
                if (!completed || content.IsFloating || !ReferenceEquals(manager.Layout.ActiveContent, other))
                    throw new Exception("A suppressed fallback did not retain activation and complete the metadata change.");
            });
        }
        foreach (int index in new[] { 0, 1 })
            Check($"return insertion {index}", () =>
            {
                LayoutAnchorable tool = new();
                LayoutAnchorablePane source = new(tool);
                LayoutDocumentPane target = new(new LayoutDocument());
                target.Children.Add(new LayoutDocument());
                DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { source, target } } } };
                tool.PreviousContainer = target;
                tool.PreviousContainerIndex = index;
                List<(NotifyCollectionChangedAction Action, int Index)> changes = new();
                target.Children.CollectionChanged += (_, e) => changes.Add((e.Action, e.NewStartingIndex));
                tool.Dock();
                if (changes.Count != 1 || changes[0] != (NotifyCollectionChangedAction.Add, index))
                    throw new Exception("The insertion observer saw an appended tab followed by a reorder.");
                GC.KeepAlive(manager);
            });
        foreach (bool asDocument in new[] { false, true })
            Check($"cross-root return {asDocument}", () =>
            {
                LayoutAnchorable tool = new();
                LayoutAnchorablePane source = new(tool);
                DockingManager first = new() { Layout = new() { RootPanel = new LayoutPanel(source) } };
                LayoutDocumentPane target = new(new LayoutDocument());
                DockingManager second = new() { Layout = new() { RootPanel = new LayoutPanel(target) } };
                tool.PreviousContainer = target;
                tool.PreviousContainerIndex = 0;
                List<string> events = new();
                first.PreviewDock += (_, _) => events.Add("preview");
                first.Docked += (_, _) => events.Add("stale");
                second.Docked += (_, _) => events.Add("docked");
                if (asDocument) tool.DockAsDocument();
                else tool.Dock();
                if (!ReferenceEquals(tool.Root, second.Layout) || string.Join(" ", events) != "preview docked")
                    throw new Exception("A return pane in another layout did not receive the content and completion.");
            });
        Check("document completion before activation", () =>
        {
            LayoutAnchorable tool = new();
            LayoutDocument other = new();
            LayoutDocumentPane target = new(other);
            DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { new LayoutAnchorablePane(tool), target } } } };
            other.IsActive = true;
            tool.IsSelected = false;
            bool completed = false;
            manager.Docked += (_, _) =>
            {
                if (tool.IsSelected || tool.IsActive || !ReferenceEquals(tool.Parent, target))
                    throw new Exception("DockAsDocument activated before completion.");
                completed = true;
            };
            tool.DockAsDocument();
            if (!completed || !tool.IsSelected || !tool.IsActive) throw new Exception("DockAsDocument did not activate after completion.");
        });
        Check("managerless document docking", () =>
        {
            LayoutAnchorable tool = new();
            LayoutRoot root = new() { RootPanel = new() { Children = { new LayoutAnchorablePane(tool), new LayoutDocumentPane() } } };
            RequiresNullReference(tool.DockAsDocument);
            if (!ReferenceEquals(tool.Root, root)) throw new Exception("The failed operation moved the content.");
        });
        Check("unreferenced return metadata", () =>
        {
            LayoutDocument document = new() { PreviousContainerIndex = 13, PreviousContainerId = "metadata" };
            LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(document)) };
            root.CollectGarbage();
            if (document.PreviousContainerIndex != 13 || document.PreviousContainerId != "metadata")
                throw new Exception("Collection rewrote metadata without a referenced container.");
        });
        Check("negative return index", () =>
        {
            LayoutAnchorable tool = new();
            LayoutAnchorablePane source = new(tool);
            LayoutAnchorablePane target = new(new LayoutAnchorable());
            DockingManager manager = new() { Layout = new() { RootPanel = new() { Children = { source, target } } } };
            tool.PreviousContainer = target;
            tool.PreviousContainerIndex = -1;
            Exception? actual = null;
            try { tool.Dock(); }
            catch (Exception error) { actual = error; }
            if (actual is not ArgumentOutOfRangeException { ParamName: "index" } || !ReferenceEquals(tool.Parent, source))
                throw new Exception("A negative return index did not fail before movement.");
            GC.KeepAlive(manager);
        });
        if (failures.Count != 0) throw new Exception("Packaged model-docking failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static LayoutContent Create(bool tool) => tool ? new LayoutAnchorable() : new LayoutDocument();

    private static void RequiresNullReference(Action action)
    {
        Exception? actual = null;
        try { action(); }
        catch (Exception error) { actual = error; }
        if (actual is not NullReferenceException) throw new Exception("The operation did not report its missing manager context.");
    }

    private sealed class ProbeDocument : LayoutDocument
    {
        internal void InvokeBaseDock() => base.InternalDock();
        protected override void InternalDock() { }
    }

    private sealed class ProbeTool : LayoutAnchorable
    {
        internal void InvokeBaseDock() => base.InternalDock();
        protected override void InternalDock() { }
    }
}
