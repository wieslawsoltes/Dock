using System;
using System.Collections.Generic;
using AvalonDock;
using AvalonDock.Layout;
using Dock.Model;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class DuplicateContentProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        for (int kind = 0; kind < 3; kind++)
        {
            Check($"duplicate drop and live changes {kind}", () =>
            {
                (DockingManager manager, LayoutContent source, LayoutContent existing, LayoutElement pane, Action<bool> allow) = Create(kind);
                DockManager engine = new(new DockService());
                Require(engine.IsDockTargetVisible(source.DockModel, pane.DockModel, DockOperation.Fill));
                allow(false);
                ILayoutContainer? parent = source.Parent;
                Require(!engine.IsDockTargetVisible(source.DockModel, pane.DockModel, DockOperation.Fill));
                Require(!engine.ValidateDockable(source.DockModel, existing.DockModel, DragAction.Move, DockOperation.Fill, true));
                Require(ReferenceEquals(parent, source.Parent));
                source.ContentId = "changed";
                Require(engine.IsDockTargetVisible(source.DockModel, pane.DockModel, DockOperation.Fill));
                existing.ContentId = "changed";
                Require(!engine.IsDockTargetVisible(source.DockModel, pane.DockModel, DockOperation.Fill));
                allow(true);
                Require(engine.ValidateDockable(source.DockModel, pane.DockModel, DragAction.Move, DockOperation.Fill, true));
                Require(ReferenceEquals(pane, source.Parent));
                GC.KeepAlive(manager);
            });
            Check($"null duplicate key and ordinal strings {kind}", () =>
            {
                var (manager, source, existing, pane, allow) = Create(kind);
                IDockDropPolicy policy = (IDockDropPolicy)manager.DockFactory;
                source.Title = existing.Title = null!;
                source.ContentId = existing.ContentId = null;
                allow(false);
                Require(!policy.CanDrop(source.DockModel, pane.DockModel, DockOperation.Fill));
                source.Title = "";
                Require(policy.CanDrop(source.DockModel, pane.DockModel, DockOperation.Fill));
                source.Title = "same"; existing.Title = "Same";
                Require(policy.CanDrop(source.DockModel, pane.DockModel, DockOperation.Fill));
                existing.Title = "same";
                source.ContentId = "id"; existing.ContentId = "ID";
                Require(policy.CanDrop(source.DockModel, pane.DockModel, DockOperation.Fill));
                existing.ContentId = "id";
                Require(!policy.CanDrop(source.DockModel, pane.DockModel, DockOperation.Fill));
            });
            Check($"split targets and direct insertion {kind}", () =>
            {
                var (manager, source, _, pane, allow) = Create(kind);
                IDockDropPolicy policy = (IDockDropPolicy)manager.DockFactory;
                allow(false);
                Require(policy.CanDrop(source.DockModel, pane.DockModel, DockOperation.Left));
                Require(policy.CanDrop(source.DockModel, pane.DockModel, DockOperation.Window));
                if (pane is LayoutDocumentPane documents) documents.Children.Add(source);
                else ((LayoutAnchorablePane)pane).Children.Add((LayoutAnchorable)source);
                Require(ReferenceEquals(source.Parent, pane) && ((ILayoutContainer)pane).ChildrenCount == 2);
            });
        }
        Check("whole subtree duplicate", () =>
        {
            var (manager, source, _, pane, allow) = Create(1);
            LayoutAnchorablePane sourcePane = (LayoutAnchorablePane)source.Parent!;
            sourcePane.Children.Insert(0, new() { Title = "unique", ContentId = "unique", Content = new object() });
            sourcePane.SelectedContentIndex = 0;
            LayoutAnchorablePaneGroup group = new() { Children = { new LayoutAnchorablePaneGroup(sourcePane) } };
            manager.Layout.RootPanel.Children.Add(group);
            allow(false);
            Require(!((IDockDropPolicy)manager.DockFactory).CanDrop(group.DockModel, pane.DockModel, DockOperation.Fill));
            source.ContentId = "changed";
            Require(((IDockDropPolicy)manager.DockFactory).CanDrop(group.DockModel, pane.DockModel, DockOperation.Fill));
        });
        if (failures.Count != 0) throw new InvalidOperationException($"Packaged duplicate content: {failures.Count}/{checks} failed: " + string.Join("; ", failures));
        Console.WriteLine($"Duplicate content: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static (DockingManager, LayoutContent, LayoutContent, LayoutElement, Action<bool>) Create(int kind)
    {
        DockingManager manager = new();
        LayoutContent source = kind == 0 ? new LayoutDocument() : new LayoutAnchorable();
        LayoutContent existing = kind == 1 ? new LayoutAnchorable() : new LayoutDocument();
        source.Title = existing.Title = "same";
        source.ContentId = existing.ContentId = "same";
        source.Content = new object(); existing.Content = new object();
        LayoutElement sourcePane = kind == 0 ? new LayoutDocumentPane(source) : new LayoutAnchorablePane((LayoutAnchorable)source);
        LayoutElement destination = kind == 1 ? new LayoutAnchorablePane((LayoutAnchorable)existing) : new LayoutDocumentPane(existing);
        manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)sourcePane);
        manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)destination);
        return (manager, source, existing, destination, value =>
        {
            if (destination is LayoutDocumentPane documents) documents.AllowDuplicateContent = value;
            else ((LayoutAnchorablePane)destination).AllowDuplicateContent = value;
        });
    }

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Duplicate-content drop contract changed.");
    }
}
