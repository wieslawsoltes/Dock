using System;
using System.Collections.Generic;
using System.IO;
#if WPF_REFERENCE
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Layout;
#else
using Avalonia.Threading;
using AvalonDock.Layout;
#endif

namespace AvalonDock.ReferenceProbes;

internal static class ContentActivationHookProbe
{
    internal static IEnumerable<string> Operations() => new[]
    {
        "detached-activate", "detached-deactivate", "content", "root", "manager", "repeat", "deactivate",
        "clear-root", "manager-redirect", "root-redirect", "hook-redirect", "hook-self", "event-redirect",
        "event-self", "changing-self", "changed-redirect", "manager-throw", "hook-throw", "after-hook-throw",
        "event-throw", "changing-throw", "changed-throw"
    };

    internal static void Run(string output)
    {
        List<string> rows = new() { "scenario\tstage\tstate\terror\tevents" };
        foreach (bool tools in new[] { false, true })
        foreach (bool suppress in new[] { false, true })
        foreach (string operation in Operations()) rows.AddRange(Observe(tools, suppress, operation));
        File.WriteAllLines(output, rows);
        Console.WriteLine($"Recorded {rows.Count - 1} content-activation hook observations.");
    }

    internal static string[] Observe(bool tools, bool suppress, string operation)
    {
        Hooks hooks = new() { Suppress = suppress };
        LayoutContent first = tools ? new LayoutAnchorable() : new LayoutDocument();
        LayoutContent content = tools ? new TraceTool(hooks) : new TraceDocument(hooks);
        first.ContentId = "first"; first.Content = new object();
        content.ContentId = "second"; content.Content = new object();
        LayoutRoot? root = null;
        DockingManager? manager = null;
        if (!operation.StartsWith("detached-", StringComparison.Ordinal))
        {
            ILayoutPanelElement pane = tools ? new LayoutAnchorablePane((LayoutAnchorable)first) { Children = { (LayoutAnchorable)content } }
                : new LayoutDocumentPane((LayoutDocument)first) { Children = { (LayoutDocument)content } };
            root = new() { RootPanel = new LayoutPanel(pane) };
            manager = new() { Layout = root };
            first.IsActive = true;
        }
        if (operation is "repeat" or "deactivate" or "clear-root" or "detached-deactivate") content.IsActive = true;
        DateTime previousTime = new(2000, 1, 1);
        content.LastActivationTimeStamp = previousTime;
        List<string> events = new();
        List<string> rows = new();
        string scenario = (tools ? "tool-" : "document-") + (suppress ? "override-" : "base-") + operation;
        Action? changing = null;
        Action? changed = null;
        Action? activeEvent = null;
        Action? managerEvent = null;
        Action? rootEvent = null;
        hooks.Record = stage => events.Add(stage + "@" + State());
        content.PropertyChanging += (_, args) =>
        {
            if (args.PropertyName != nameof(LayoutContent.IsActive)) return;
            events.Add("changing@" + State());
            Invoke(ref changing);
        };
        content.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName != nameof(LayoutContent.IsActive)) return;
            events.Add("changed@" + State());
            Invoke(ref changed);
        };
        content.IsActiveChanged += (_, _) =>
        {
            events.Add("event-enter@" + State());
            Invoke(ref activeEvent);
            events.Add("event-exit@" + State());
        };
        if (manager is not null) manager.ActiveContentChanged += (_, _) => Invoke(ref managerEvent);
        if (root is not null) root.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(LayoutRoot.ActiveContent)) Invoke(ref rootEvent);
        };

        Take("before", "none");
        string error = "none";
        try
        {
            switch (operation)
            {
                case "detached-activate": case "content": case "repeat": content.IsActive = true; break;
                case "detached-deactivate": case "deactivate": content.IsActive = false; break;
                case "root": root!.ActiveContent = content; break;
                case "manager": manager!.ActiveContent = content; break;
                case "clear-root": root!.ActiveContent = null; break;
                case "manager-redirect": managerEvent = () => first.IsActive = true; content.IsActive = true; break;
                case "root-redirect": rootEvent = () => first.IsActive = true; content.IsActive = true; break;
                case "hook-redirect": hooks.Before = () => first.IsActive = true; content.IsActive = true; break;
                case "hook-self": hooks.Before = () => content.IsActive = false; content.IsActive = true; break;
                case "event-redirect": activeEvent = () => first.IsActive = true; content.IsActive = true; break;
                case "event-self": activeEvent = () => content.IsActive = false; content.IsActive = true; break;
                case "changing-self": changing = () => content.IsActive = true; content.IsActive = true; break;
                case "changed-redirect": changed = () => first.IsActive = true; content.IsActive = true; break;
                case "manager-throw": managerEvent = Fail; content.IsActive = true; break;
                case "hook-throw": hooks.Before = Fail; content.IsActive = true; break;
                case "after-hook-throw": hooks.After = Fail; content.IsActive = true; break;
                case "event-throw": activeEvent = Fail; content.IsActive = true; break;
                case "changing-throw": changing = Fail; content.IsActive = true; break;
                case "changed-throw": changed = Fail; content.IsActive = true; break;
                default: throw new ArgumentOutOfRangeException(nameof(operation));
            }
        }
        catch (Exception exception) { error = exception.GetType().Name; }
        Take("after", error);
        return rows.ToArray();

        string State() => $"active={Token(root?.ActiveContent)};last={Token(root?.LastFocusedDocument)};value={Token(manager?.ActiveContent)};flags={Flag(first.IsActive)}{Flag(content.IsActive)};selected={Flag(first.IsSelected)}{Flag(content.IsSelected)};time={(content.LastActivationTimeStamp == previousTime ? "old" : "new")}";
        string Token(object? value) => value is null ? "null" : ReferenceEquals(value, first) ? "first" : ReferenceEquals(value, content) ? "second" : "other";
        void Take(string stage, string failure)
        {
#if WPF_REFERENCE
            Dispatcher.CurrentDispatcher.Invoke(static () => { }, DispatcherPriority.Background);
#else
            Dispatcher.UIThread.RunJobs();
#endif
            rows.Add(string.Join('\t', scenario, stage, State(), failure,
                events.Count == 0 ? "none" : string.Join('>', events)));
            events.Clear();
        }
    }

    private static int Flag(bool value) => value ? 1 : 0;
    private static void Fail() => throw new InvalidOperationException("Probe callback");
    private static void Invoke(ref Action? callback)
    {
        Action? action = callback;
        callback = null;
        action?.Invoke();
    }

    private sealed class Hooks
    {
        internal bool Suppress;
        internal Action<string>? Record;
        internal Action? Before;
        internal Action? After;
        internal void Enter(bool oldValue, bool newValue)
        {
            Record?.Invoke($"hook-enter:{Flag(oldValue)}{Flag(newValue)}");
            Invoke(ref Before);
        }
        internal void Exit(bool oldValue, bool newValue)
        {
            Invoke(ref After);
            Record?.Invoke($"hook-exit:{Flag(oldValue)}{Flag(newValue)}");
        }
    }

    private sealed class TraceDocument(Hooks hooks) : LayoutDocument
    {
        protected override void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            hooks.Enter(oldValue, newValue);
            if (!hooks.Suppress) base.OnIsActiveChanged(oldValue, newValue);
            hooks.Exit(oldValue, newValue);
        }
    }

    private sealed class TraceTool(Hooks hooks) : LayoutAnchorable
    {
        protected override void OnIsActiveChanged(bool oldValue, bool newValue)
        {
            hooks.Enter(oldValue, newValue);
            if (!hooks.Suppress) base.OnIsActiveChanged(oldValue, newValue);
            hooks.Exit(oldValue, newValue);
        }
    }
}
