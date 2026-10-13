using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using AvalonDock.Layout;
using Dock.Model.Avalonia.Controls;

namespace PackageConsumer;

internal static class FloatingCollectionProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (bool tools in new[] { false, true })
        {
            Check($"replacement phases, tools={tools}", () =>
            {
                LayoutRoot root = new();
                LayoutFloatingWindow previous = Create(tools);
                LayoutFloatingWindow incoming = Create(!tools);
                root.FloatingWindows.Add(previous);
                List<string> phases = new();
                ((INotifyPropertyChanged)root.FloatingWindows).PropertyChanged += (_, args) => phases.Add(args.PropertyName!);
                previous.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName != "Parent") return;
                    Require(ReferenceEquals(root.FloatingWindows[0], incoming));
                    phases.Add("detached");
                };
                incoming.PropertyChanged += (_, args) => { if (args.PropertyName == "Parent") phases.Add("attached"); };
                root.FloatingWindows.CollectionChanged += (_, args) =>
                {
                    Require(args.Action == NotifyCollectionChangedAction.Replace && ReferenceEquals(args.OldItems![0], previous) &&
                        ReferenceEquals(args.NewItems![0], incoming) && args.OldStartingIndex == 0 && args.NewStartingIndex == 0);
                    phases.Add("collection");
                };
                root.FloatingWindows[0] = incoming;
                Require(string.Join(",", phases) == "Item[],detached,attached,collection");
                Require(previous.Parent is null && ReferenceEquals(incoming.Parent, root) &&
                    ReferenceEquals(((RootDock)root.DockModel).Windows![0], incoming.DockWindow));
            });
            Check($"parent failure and retry, tools={tools}", () =>
            {
                LayoutRoot root = new();
                LayoutFloatingWindow previous = Create(tools);
                LayoutFloatingWindow incoming = Create(!tools);
                root.FloatingWindows.Add(previous);
                InvalidOperationException error = new("parent callback");
                bool fail = true;
                int published = 0;
                (tools ? incoming : previous).PropertyChanged += (_, args) =>
                {
                    if (fail && args.PropertyName == "Parent") throw error;
                };
                root.FloatingWindows.CollectionChanged += (_, _) => published++;
                Exception? caught = null;
                try { root.FloatingWindows[0] = incoming; }
                catch (Exception exception) { caught = exception; }
                Require(ReferenceEquals(caught, error) && published == 0 && root.FloatingWindows.Count == 1 &&
                    ReferenceEquals(root.FloatingWindows[0], incoming) && previous.Parent is null &&
                    ReferenceEquals(incoming.Parent, tools ? root : null));
                fail = false;
                root.FloatingWindows[0] = incoming;
                Require(ReferenceEquals(incoming.Parent, root) && published == 1);
            });
        }
        if (failures.Count != 0) throw new InvalidOperationException("Packaged floating collection failures: " + string.Join("; ", failures));
        Console.WriteLine($"Floating collection: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static LayoutFloatingWindow Create(bool tools) => tools
        ? new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) }
        : new LayoutDocumentFloatingWindow { RootDocument = new() };
    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Floating collection contract changed.");
    }
}
