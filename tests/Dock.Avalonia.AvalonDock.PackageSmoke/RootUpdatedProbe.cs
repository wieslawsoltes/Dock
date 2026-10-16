using System;
using System.Collections.Generic;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class RootUpdatedProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("protected tree hook", () =>
        {
            LayoutRoot root = new();
            ApplicationGroup group = new() { Parent = root.RootPanel };
            int updates = 0, trees = 0;
            root.Updated += (_, _) => updates++;
            root.RootPanel.ChildrenTreeChanged += (_, _) => trees++;
            group.Notify();
            Require(updates == 0 && trees == 1);
        });
        Check("root panel", () =>
        {
            LayoutRoot root = new();
            int updates = 0;
            root.Updated += (_, _) => updates++;
            root.RootPanel = new LayoutPanel();
            Require(updates == 1);
        });
        foreach (string action in new[] { "add", "remove", "move" })
            Check("floating " + action, () =>
            {
                LayoutRoot root = new();
                root.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() });
                root.FloatingWindows.Add(new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() });
                int updates = 0;
                root.Updated += (_, _) => updates++;
                if (action == "add") root.FloatingWindows.Add(new LayoutDocumentFloatingWindow());
                else if (action == "remove") root.FloatingWindows.RemoveAt(0);
                else root.FloatingWindows.Move(0, 1);
                Require(updates == (action == "add" ? 1 : 0));
            });
        foreach (bool tool in new[] { false, true })
        foreach (bool replace in new[] { false, true })
            Check($"floating child tool={tool} replace={replace}", () =>
            {
                LayoutFloatingWindow window = tool
                    ? new LayoutAnchorableFloatingWindow { RootPanel = new LayoutAnchorablePaneGroup() }
                    : new LayoutDocumentFloatingWindow { RootDocument = new LayoutDocument() };
                LayoutRoot root = new();
                root.FloatingWindows.Add(window);
                int updates = 0;
                root.Updated += (_, _) => updates++;
                if (window is LayoutAnchorableFloatingWindow tools) tools.RootPanel = replace ? new LayoutAnchorablePaneGroup() : null;
                else ((LayoutDocumentFloatingWindow)window).RootDocument = replace ? new LayoutDocument() : null;
                Require(updates == (replace ? 1 : 0));
            });
        Check("group order and commands", () =>
        {
            LayoutDocument document = new();
            LayoutDocumentPane first = new(document), second = new(new LayoutDocument());
            LayoutDocumentPaneGroup group = new(first) { Children = { second } };
            DockingManager manager = new() { Layout = new LayoutRoot { RootPanel = new LayoutPanel(group) } };
            try
            {
                LayoutItem item = manager.GetLayoutItemFromModel(document)!;
                Require(item.MoveToNextTabGroupCommand!.CanExecute(null));
                int updates = 0, changes = 0;
                manager.Layout.Updated += (_, _) => updates++;
                item.MoveToNextTabGroupCommand.CanExecuteChanged += (_, _) => changes++;
                group.Children.Move(0, 1);
                Require(updates == 0 && changes > 0 && !item.MoveToNextTabGroupCommand.CanExecute(null) &&
                    item.MoveToPreviousTabGroupCommand!.CanExecute(null));
            }
            finally { manager.Layout = new(); Dispatcher.UIThread.RunJobs(); }
        });
        Check("retained floating child lifetime", () =>
        {
            LayoutDocument document = new();
            SilentDocumentWindow window = new() { RootDocument = document };
            LayoutRoot root = new();
            root.FloatingWindows.Add(window);
            DockingManager manager = new() { Layout = root };
            try
            {
                Dispatcher.UIThread.RunJobs();
                LayoutItem item = manager.GetLayoutItemFromModel(document)!;
                int updates = 0;
                root.Updated += (_, _) => updates++;
                window.RootDocument = null;
                Dispatcher.UIThread.RunJobs();
                // Upstream keeps the removed child's Parent and removes an
                // item only when its public Root differs from the manager's.
                Require(updates == 0 && ReferenceEquals(document.Root, root) &&
                    ReferenceEquals(item.LayoutElement, document) && ReferenceEquals(manager.GetLayoutItemFromModel(document), item));
                manager.Layout = new();
                Dispatcher.UIThread.RunJobs();
                Require(item.LayoutElement is null && manager.GetLayoutItemFromModel(document) is null);
            }
            finally { manager.Layout = new(); Dispatcher.UIThread.RunJobs(); }
        });
        if (failures.Count != 0) throw new Exception("Packaged root-update failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); } catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Root update contract mismatch.");
    }

    private sealed class ApplicationGroup : LayoutGroupBase
    {
        internal void Notify() => NotifyChildrenTreeChanged(ChildrenTreeChange.DirectChildrenChanged);
    }

    private sealed class SilentDocumentWindow : LayoutDocumentFloatingWindow
    {
        protected override void RaisePropertyChanged(string? propertyName = null) { }
    }
}
