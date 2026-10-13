using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls.Primitives;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace PackageConsumer;

internal static class ContentControlCallbackProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        int checks = 0;
        foreach (bool tool in new[] { false, true })
        {
            Check($"replayed old argument {tool}", () =>
            {
                var (_, first, second) = Create(tool);
                IProbe probe = NewProbe(tool);
                probe.Model = first;
                AvaloniaPropertyChangedEventArgs initial = probe.Last!;
                probe.Suppress = true;
                probe.Model = second;
                second.IsEnabled = false;
                probe.Apply(initial);
                probe.Control.IsEnabled = true;
                first.IsEnabled = false;
                Require(!probe.Control.IsEnabled && ReferenceEquals(probe.Model, second));
            });
            Check($"suppressed hook live model {tool}", () =>
            {
                var (_, first, second) = Create(tool);
                IProbe probe = NewProbe(tool);
                probe.Model = first;
                probe.Suppress = true;
                second.IsEnabled = false;
                probe.Model = second;
                first.IsEnabled = false;
                Require(!probe.Control.IsEnabled);
                probe.Model = null;
                probe.Control.IsEnabled = true;
                first.IsEnabled = true;
                first.IsEnabled = false;
                Require(probe.Control.IsEnabled);
            });
            Check($"repeated registrations {tool}", () =>
            {
                var (manager, first, second) = Create(tool);
                IProbe probe = NewProbe(tool);
                probe.Model = second;
                probe.Apply(probe.Last!);
                second.IsActive = true;
                ILayoutContentSelector selector = (ILayoutContentSelector)second.Parent!;
                List<int> changes = new();
                second.Parent!.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(ILayoutContentSelector.SelectedContentIndex)) changes.Add(selector.SelectedContentIndex);
                };
                second.IsEnabled = false;
                Require(string.Join(",", changes) == "-1,-1,0,-1,-1,0");
                Require(ReferenceEquals(selector.SelectedContent, first) && ReferenceEquals(manager.Layout.ActiveContent, second));
            });
            Check($"clear before fallback {tool}", () =>
            {
                var (_, first, second) = Create(tool);
                IProbe probe = NewProbe(tool);
                probe.Model = second;
                second.IsActive = true;
                ILayoutContentSelector selector = (ILayoutContentSelector)second.Parent!;
                second.Parent!.PropertyChanged += (_, args) =>
                {
                    if (args.PropertyName == nameof(ILayoutContentSelector.SelectedContentIndex) && selector.SelectedContentIndex == -1)
                        first.IsEnabled = false;
                };
                second.IsEnabled = false;
                Require(selector.SelectedContentIndex == -1 && !first.IsEnabled);
            });
            Check($"pane kind {tool}", () =>
            {
                LayoutAnchorable first = new() { Content = new object() };
                LayoutAnchorable active = new() { Content = new object() };
                ILayoutPanelElement pane = tool ? new LayoutDocumentPane { Children = { first, active } }
                    : new LayoutAnchorablePane { Children = { first, active } };
                DockingManager manager = new() { Layout = new() { RootPanel = new(pane) } };
                IProbe probe = NewProbe(tool);
                probe.Model = active;
                active.IsActive = true;
                active.IsEnabled = false;
                Require(!probe.Control.IsEnabled && ReferenceEquals(((ILayoutContentSelector)pane).SelectedContent, active));
                GC.KeepAlive(manager);
            });
        }
        foreach (bool clear in new[] { false, true })
            Check($"title container retention {clear}", () =>
            {
                var (manager, first, _) = Create(true);
                AnchorablePaneTitle title = new() { Model = (LayoutAnchorable)first };
                LayoutItem item = title.LayoutItem!;
                if (clear) title.ClearValue(AnchorablePaneTitle.ModelProperty);
                else title.Model = null;
                Require(title.Model is null && ReferenceEquals(title.LayoutItem, item));
                GC.KeepAlive(manager);
            });
        if (failures.Count != 0) throw new InvalidOperationException($"Packaged content callbacks: {failures.Count}/{checks} failed: " + string.Join("; ", failures));
        Console.WriteLine($"Content callbacks: {checks} packaged checks passed.");

        void Check(string name, Action action)
        {
            checks++;
            try { action(); }
            catch (Exception error) { failures.Add(name + ": " + error.Message); }
        }
    }

    private static (DockingManager, LayoutContent, LayoutContent) Create(bool tool)
    {
        LayoutContent first = tool ? new LayoutAnchorable() : new LayoutDocument();
        LayoutContent second = tool ? new LayoutAnchorable() : new LayoutDocument();
        first.Content = new object(); second.Content = new object();
        ILayoutPanelElement pane = tool ? new LayoutAnchorablePane { Children = { (LayoutAnchorable)first, (LayoutAnchorable)second } }
            : new LayoutDocumentPane { Children = { first, second } };
        return (new() { Layout = new() { RootPanel = new(pane) } }, first, second);
    }

    private static IProbe NewProbe(bool tool) => tool ? new ToolProbe() : new DocumentProbe();
    private static void Require(bool condition)
    {
        if (!condition) throw new InvalidOperationException("Content control callback contract changed.");
    }

    private interface IProbe
    {
        TemplatedControl Control { get; }
        LayoutContent? Model { get; set; }
        bool Suppress { get; set; }
        AvaloniaPropertyChangedEventArgs? Last { get; }
        void Apply(AvaloniaPropertyChangedEventArgs change);
    }

    private sealed class DocumentProbe : LayoutDocumentControl, IProbe
    {
        public TemplatedControl Control => this;
        public bool Suppress { get; set; }
        public AvaloniaPropertyChangedEventArgs? Last { get; private set; }
        public void Apply(AvaloniaPropertyChangedEventArgs change) => base.OnModelChanged(change);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Last = e;
            if (!Suppress) base.OnModelChanged(e);
        }
    }

    private sealed class ToolProbe : LayoutAnchorableControl, IProbe
    {
        public TemplatedControl Control => this;
        LayoutContent? IProbe.Model { get => Model; set => Model = (LayoutAnchorable?)value; }
        public bool Suppress { get; set; }
        public AvaloniaPropertyChangedEventArgs? Last { get; private set; }
        public void Apply(AvaloniaPropertyChangedEventArgs change) => base.OnModelChanged(change);
        protected override void OnModelChanged(AvaloniaPropertyChangedEventArgs e)
        {
            Last = e;
            if (!Suppress) base.OnModelChanged(e);
        }
    }
}
