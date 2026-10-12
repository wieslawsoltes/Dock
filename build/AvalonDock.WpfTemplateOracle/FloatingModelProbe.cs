using System.IO;
using System.Windows;
using System.Windows.Threading;
using Xceed.Wpf.AvalonDock;
using Xceed.Wpf.AvalonDock.Controls;
using Xceed.Wpf.AvalonDock.Layout;

internal static class FloatingModelProbe
{
    internal static void Run(string output)
    {
        Application app = new() { ShutdownMode = ShutdownMode.OnExplicitShutdown };
        List<string> rows = new()
        {
            "case\tstage\tvisible\tclosed\told-parent\told-root\tnew-parent\tnew-root\tmodels\thosts\tevents"
        };
        foreach (bool tools in new[] { false, true })
        foreach (string suppression in new[] { "none", "child", "validity", "visibility", "all" })
        foreach (string operation in new[] { "clear", "replace", "clear-restore" })
        foreach (bool cancel in new[] { false, true })
            Observe(rows, tools, suppression, operation, cancel);
        File.WriteAllLines(output, rows);
        Console.WriteLine($"Recorded {rows.Count - 1} WPF floating-model observations.");
        app.Shutdown();
    }

    private static void Observe(List<string> rows, bool tools, string suppression, string operation, bool cancel)
    {
        LayoutElement original = CreateChild(tools, "Original");
        LayoutElement replacement = CreateChild(tools, "Replacement");
        LayoutFloatingWindow model = tools
            ? new ToolWindow { RootPanel = (LayoutAnchorablePaneGroup)original, Suppression = suppression }
            : new DocumentWindow { RootDocument = (LayoutDocument)original, Suppression = suppression };
        LayoutRoot root = new() { RootPanel = new LayoutPanel(new LayoutDocumentPane(new LayoutDocument { Title = "Other" })) };
        root.FloatingWindows.Add(model);
        DockingManager manager = new() { Layout = root };
        Window owner = new() { Content = manager, Width = 640, Height = 400, ShowInTaskbar = false };
        LayoutFloatingWindowControl? host = null;
        bool closed = false;
        bool veto = cancel;
        List<string> events = new();
        string id = $"{(tools ? "tool" : "document")}-{suppression}-{operation}-cancel{Flag(cancel)}";
        try
        {
            owner.Show(); Pump();
            host = manager.FloatingWindows.Single();
            host.Closing += (_, args) =>
            {
                events.Add("closing-enter:" + Flag(args.Cancel));
                if (veto) args.Cancel = true;
                events.Add("closing-exit:" + Flag(args.Cancel));
            };
            host.Closed += (_, _) => { closed = true; events.Add("closed"); };
            if (model is LayoutDocumentFloatingWindow document)
                document.RootDocumentChanged += (_, _) => events.Add("document-changed");
            Take("before");
            if (operation != "replace") SetChild(null);
            if (operation != "clear") SetChild(replacement);
            Take("synchronous");
            Pump();
            Take("dispatched");
        }
        finally
        {
            veto = false;
            if (host is not null && !closed)
            {
                host.Closing += (_, args) => args.Cancel = false;
                host.Close();
            }
            owner.Close(); Pump();
        }

        void SetChild(LayoutElement? child)
        {
            if (model is LayoutAnchorableFloatingWindow tool) tool.RootPanel = (LayoutAnchorablePaneGroup?)child;
            else ((LayoutDocumentFloatingWindow)model).RootDocument = (LayoutDocument?)child;
        }

        void Take(string stage) => rows.Add(string.Join('\t', id, stage, Flag(host!.IsVisible), Flag(closed),
            Parent(original), Root(original), Parent(replacement), Root(replacement), root.FloatingWindows.Count,
            manager.FloatingWindows.Count(), string.Join('>', events)));
        string Parent(ILayoutElement element) => element.Parent is null ? "none" : ReferenceEquals(element.Parent, model) ? "floating" : "other";
        string Root(ILayoutElement element) => element.Root is null ? "none" : ReferenceEquals(element.Root, root) ? "original" : "other";
    }

    private static LayoutElement CreateChild(bool tools, string title) => tools
        ? new LayoutAnchorablePaneGroup(new LayoutAnchorablePane(new LayoutAnchorable { Title = title, Content = title, CanClose = true }))
        : new LayoutDocument { Title = title, Content = title };

    private static bool Suppresses(string suppression, string property, string child) => suppression == "all" ||
        (suppression == "child" && property == child) || (suppression == "validity" && property == "IsValid") ||
        (suppression == "visibility" && property == "IsVisible");

    private static string Flag(bool value) => value ? "1" : "0";
    private static void Pump() => Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.ApplicationIdle, new Action(() => { }));

    private sealed class DocumentWindow : LayoutDocumentFloatingWindow
    {
        internal string Suppression { get; init; } = "none";
        protected override void RaisePropertyChanged(string propertyName)
        {
            if (!Suppresses(Suppression, propertyName, nameof(RootDocument))) base.RaisePropertyChanged(propertyName);
        }
    }

    private sealed class ToolWindow : LayoutAnchorableFloatingWindow
    {
        internal string Suppression { get; init; } = "none";
        protected override void RaisePropertyChanged(string propertyName)
        {
            if (!Suppresses(Suppression, propertyName, nameof(RootPanel))) base.RaisePropertyChanged(propertyName);
        }
    }
}
