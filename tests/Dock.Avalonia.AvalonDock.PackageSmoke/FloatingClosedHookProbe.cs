using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using AvalonDock.Themes;
using Dock.Model.Core;

namespace PackageConsumer;

internal static class FloatingClosedHookProbe
{
    internal static void Verify()
    {
        List<string> failures = new();
        Check("document subscription in Closed", () => CheckSubscription(false));
        Check("tool subscription in Closed", () => CheckSubscription(true));
        Check("cleanup before Closed", () =>
        {
            using Workspace workspace = new();
            LayoutAnchorablePane empty = new();
            workspace.Root.RootPanel.Children.Add(empty);
            bool collected = false;
            workspace.Host.Closed += (_, _) => collected = empty.Parent is null;
            workspace.Host.Close();
            if (!collected) throw new Exception("Closed ran before empty-pane collection.");
        });
        Check("no second cleanup after Closed", () =>
        {
            using Workspace workspace = new();
            LayoutAnchorablePane added = new();
            workspace.Host.Closed += (_, _) => workspace.Root.RootPanel.Children.Add(added);
            workspace.Host.Close();
            if (!ReferenceEquals(added.Parent, workspace.Root.RootPanel))
                throw new Exception("A second collection removed a pane added by Closed.");
        });
        Check("typed bindings after Closed", () =>
        {
            ToolHost host = new(new LayoutAnchorableFloatingWindow { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) });
            using BehaviorSubject<LayoutItem?> source = new(new LayoutDocumentItem());
            using IDisposable binding = host.Bind(LayoutAnchorableFloatingWindowControl.SingleContentLayoutItemProperty, source.ToBinding());
            bool observed = false;
            host.Closed += (_, _) => observed = source.HasObservers;
            host.Show();
            host.Close();
            if (!observed || source.HasObservers) throw new Exception("The local tool binding did not clear after the base event.");
        });
        Check("empty model before Closed", () =>
        {
            using Workspace workspace = new();
            bool removed = false;
            int notifications = 0;
            workspace.Manager.DockFactory.WindowClosed += (_, _) => notifications++;
            workspace.Host.Closed += (_, _) => removed = workspace.Model.Root is null;
            workspace.Model.RootDocument = null;
            Dispatcher.UIThread.RunJobs();
            if (!removed || notifications != 1) throw new Exception("Empty-model collection or native closure notification occurred out of order.");
        });
        Check("failed Closed retains observation", () =>
        {
            LayoutDocumentFloatingWindow model = new() { RootDocument = new() };
            DocumentHost host = new(model);
            InvalidOperationException failure = new("expected callback failure");
            bool fail = true;
            host.Closed += (_, _) => { if (fail) throw failure; };
            host.Show();
            try
            {
                // Exercise the protected hook directly so the deliberate error
                // cannot interrupt Avalonia's separate lifetime teardown.
                try { host.InvokeClosed(); throw new Exception("Closed did not propagate the callback error."); }
                catch (InvalidOperationException e) when (ReferenceEquals(e, failure)) { }
                model.RootDocument = null;
                if (host.UserOrigin) throw new Exception("The failed base call removed the document subscription.");
            }
            finally { fail = false; host.Close(); Dispatcher.UIThread.RunJobs(); }
        });
        if (failures.Count != 0) throw new Exception("Packaged Closed-hook failures: " + string.Join("; ", failures));

        void Check(string name, Action action)
        {
            try { action(); }
            catch (Exception e) { failures.Add(name + ": " + e); }
        }
    }

    private static void CheckSubscription(bool tools)
    {
        LayoutDocumentFloatingWindow document = new() { RootDocument = new() };
        LayoutAnchorableFloatingWindow tool = new() { RootPanel = new(new LayoutAnchorablePane(new LayoutAnchorable())) };
        DocumentHost? documentHost = tools ? null : new(document);
        ToolHost? toolHost = tools ? new(tool) : null;
        LayoutFloatingWindowControl host = (LayoutFloatingWindowControl?)documentHost ?? toolHost!;
        bool internalOrigin = false;
        host.Closed += (_, _) =>
        {
            if (tools) tool.RootPanel = null;
            else document.RootDocument = null;
            internalOrigin = !(documentHost?.UserOrigin ?? toolHost!.UserOrigin);
        };
        host.Show();
        host.Close();
        if (!internalOrigin) throw new Exception("Typed model observation ended before Closed callbacks.");
    }

    private sealed class DocumentHost(LayoutDocumentFloatingWindow model) : LayoutDocumentFloatingWindowControl(model)
    {
        internal bool UserOrigin => CloseInitiatedByUser;
        internal void InvokeClosed() => base.OnClosed(EventArgs.Empty);
        protected override bool CanClose(object? parameter = null) => true;
    }
    private sealed class ToolHost(LayoutAnchorableFloatingWindow model) : LayoutAnchorableFloatingWindowControl(model)
    {
        internal bool UserOrigin => CloseInitiatedByUser;
        protected override bool CanClose(object? parameter = null) => true;
    }
    private sealed class Workspace : IDisposable
    {
        internal LayoutRoot Root { get; } = new() { RootPanel = new(new LayoutDocumentPane(new LayoutDocument())) };
        internal LayoutDocumentFloatingWindow Model { get; } = new() { RootDocument = new() };
        internal DockingManager Manager { get; }
        internal DocumentHost Host { get; private set; } = null!;
        private readonly Window _view;
        internal Workspace()
        {
            Manager = new() { Layout = Root };
            _view = new() { Content = Manager, Width = 640, Height = 400 };
            _view.Styles.Add(new AvalonDockTheme());
            _view.Show();
            _view.UpdateLayout();
            Dispatcher.UIThread.RunJobs();
            Manager.DockFactory.HostWindowLocator = new()
            {
                [nameof(IDockWindow)] = () => Host = new(Model)
            };
            Root.FloatingWindows.Add(Model);
            Dispatcher.UIThread.RunJobs();
        }
        public void Dispose()
        {
            Host.Close();
            Root.FloatingWindows.Clear();
            _view.Close();
            Dispatcher.UIThread.RunJobs();
        }
    }
}
