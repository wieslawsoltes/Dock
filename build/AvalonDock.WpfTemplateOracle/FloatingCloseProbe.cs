using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

internal static class FloatingCloseProbe
{
    internal static void Run(string output)
    {
        Application application = new() { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        List<string> rows = new()
        {
            "case\tstage\tcancel\thost-visible\thost-closed\tcontent-root\tcontent-hidden\tmodel-count\thost-count\tevents"
        };
        foreach (bool close in new[] { false, true })
        foreach (bool hide in new[] { false, true })
        foreach (bool initial in new[] { false, true })
        foreach (string response in new[] { "keep", "cancel", "clear" })
        foreach (bool suppress in new[] { false, true })
        {
            TraceHost host = new(new LayoutDocumentFloatingWindow())
                { CloseAllowed = close, HideAllowed = hide, SuppressBase = suppress };
            bool capturing = true;
            host.Closing += (_, args) =>
            {
                if (!capturing) return;
                host.Events.Add("event-enter:" + Flag(args.Cancel));
                if (response == "cancel") args.Cancel = true;
                else if (response == "clear") args.Cancel = false;
                host.Events.Add("event-exit:" + Flag(args.Cancel));
            };
            CancelEventArgs result = host.InvokeClosing(initial);
            rows.Add(string.Join('\t', $"hook-close{Flag(close)}-hide{Flag(hide)}-initial{Flag(initial)}-{response}-suppress{Flag(suppress)}",
                "after", Flag(result.Cancel), "-", "-", "-", "-", "-", "-", string.Join('>', host.Events)));
            capturing = false;
            host.CloseAllowed = true;
            host.SuppressBase = false;
            host.Close();
        }

        foreach (bool tool in new[] { false, true })
        foreach (bool close in new[] { false, true })
        foreach (bool cancel in new[] { false, true })
        foreach (string route in new[] { "window", "model", "command", "replace", "unload" })
            ObserveWindow(rows, tool, close, cancel, route);

        File.WriteAllLines(output, rows);
        Console.WriteLine($"Recorded {rows.Count - 1} WPF floating-close observations.");
        application.Shutdown();
    }

    private static void ObserveWindow(List<string> rows, bool tool, bool close, bool cancel, string route)
    {
        LayoutContent content = tool
            ? new LayoutAnchorable { Title = "Tool", Content = "Tool content", CanClose = close }
            : new LayoutDocument { Title = "Document", Content = "Document content", CanClose = close };
        LayoutRoot layout = new()
        {
            RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { Title = "Other", Content = "Other" }))
        };
        if (content is LayoutAnchorable anchorable) layout.RootPanel.Children.Add(new LayoutAnchorablePane(anchorable));
        else ((LayoutDocumentPane)layout.RootPanel.Children[0]).Children.Add((LayoutDocument)content);
        DockingManager manager = new() { Layout = layout };
        Window owner = new() { Content = manager, Width = 640, Height = 400, ShowInTaskbar = false };
        List<string> events = new();
        bool closed = false;
        bool lastCancel = false;
        bool cancelClose = cancel;
        LayoutFloatingWindowControl? host = null;
        string id = $"{(tool ? "tool" : "document")}-close{Flag(close)}-cancel{Flag(cancel)}-{route}";
        try
        {
            owner.Show(); Pump();
            content.Float(); Pump();
            host = manager.FloatingWindows.Single();
            content.Closing += (_, _) => events.Add("content-closing");
            content.Closed += (_, _) => events.Add("content-closed");
            if (content is LayoutAnchorable toolModel)
            {
                toolModel.Hiding += (_, _) => events.Add("content-hiding");
            }
            host.Closing += (_, args) =>
            {
                events.Add("window-closing-enter:" + Flag(args.Cancel));
                if (cancelClose) args.Cancel = true;
                lastCancel = args.Cancel;
                events.Add("window-closing-exit:" + Flag(args.Cancel));
            };
            host.Closed += (_, _) => { closed = true; events.Add("window-closed"); };
            Take("before");
            if (route == "window") host.Close();
            else if (route == "model") content.Close();
            else if (route == "command")
            {
                var command = manager.GetLayoutItemFromModel(content).CloseCommand;
                if (command.CanExecute(null)) command.Execute(null);
                else events.Add("command-disabled");
            }
            else if (route == "replace") manager.Layout = new LayoutRoot();
            else owner.Content = null;
            Take("synchronous");
            Pump();
            Take("dispatched");
        }
        finally
        {
            cancelClose = false;
            if (host is not null && !closed)
            {
                // Cleanup must also handle an orphaned or capability-blocked host.
                host.Closing += (_, args) => args.Cancel = false;
                host.Close();
            }
            owner.Close();
            Pump();
        }

        void Take(string stage) => rows.Add(string.Join('\t', id, stage, Flag(lastCancel),
            Flag(host!.IsVisible), Flag(closed), content.Root is null ? "none" : ReferenceEquals(content.Root, layout) ? "original" : "other",
            content is LayoutAnchorable a ? Flag(a.IsHidden) : "-", layout.FloatingWindows.Count,
            manager.FloatingWindows.Count(), string.Join('>', events)));
    }

    private static string Flag(bool value) => value ? "1" : "0";

    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));

    private sealed class TraceHost(ILayoutElement model) : LayoutFloatingWindowControl(model)
    {
        public override ILayoutElement Model { get; } = model;
        internal bool CloseAllowed { get; set; }
        internal bool HideAllowed { get; set; }
        internal bool SuppressBase { get; set; }
        internal List<string> Events { get; } = new();
        internal CancelEventArgs InvokeClosing(bool cancel)
        {
            CancelEventArgs args = new(cancel);
            OnClosing(args);
            return args;
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Events.Add("override-enter:" + Flag(e.Cancel));
            if (!SuppressBase) base.OnClosing(e);
            Events.Add("override-exit:" + Flag(e.Cancel));
        }
        protected override bool CanClose(object? parameter = null) { Events.Add("can-close"); return CloseAllowed; }
        protected override bool CanHide(object? parameter = null) { Events.Add("can-hide"); return HideAllowed; }
        protected override void DoHide() => Events.Add("do-hide");
    }
}
