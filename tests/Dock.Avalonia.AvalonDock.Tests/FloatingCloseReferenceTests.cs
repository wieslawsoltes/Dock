using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class FloatingCloseReferenceTests
{
    public static IEnumerable<object[]> HookCases()
    {
        foreach (bool close in new[] { false, true })
        foreach (bool hide in new[] { false, true })
        foreach (bool initial in new[] { false, true })
        foreach (string response in new[] { "keep", "cancel", "clear" })
        foreach (bool suppress in new[] { false, true })
            yield return new object[] { close, hide, initial, response, suppress };
    }

    [AvaloniaTheory]
    [MemberData(nameof(HookCases))]
    public void Protected_Close_Hooks_Match_The_Original_WPF_Order(bool close, bool hide, bool initial, string response, bool suppress)
    {
        TraceHost host = new() { CloseAllowed = close, HideAllowed = hide, InitialCancel = initial, SuppressBase = suppress };
        host.Closing += (_, args) =>
        {
            if (!host.Capturing) return;
            host.Events.Add("event-enter:" + Flag(args.Cancel));
            if (response == "cancel") args.Cancel = true;
            else if (response == "clear") args.Cancel = false;
            host.Events.Add("event-exit:" + Flag(args.Cancel));
        };
        try
        {
            host.Show();
            host.Close();
            string id = $"hook-close{Flag(close)}-hide{Flag(hide)}-initial{Flag(initial)}-{response}-suppress{Flag(suppress)}";
            Assert.Equal(Observation(id, "after"), string.Join('\t', id, "after", Flag(host.Result), "-", "-", "-", "-", "-", "-", string.Join('>', host.Events)));
        }
        finally { host.Capturing = false; host.Close(); }
    }

    public static IEnumerable<object[]> HostCases()
    {
        foreach (bool tool in new[] { false, true })
        foreach (bool close in new[] { false, true })
        foreach (bool cancel in new[] { false, true })
        foreach (string route in new[] { "window", "model", "command", "replace", "unload" })
            yield return new object[] { tool, close, cancel, route };
    }

    [AvaloniaTheory]
    [MemberData(nameof(HostCases))]
    public void Host_Closure_And_Layout_Detachment_Match_The_Original_WPF_Result(bool tool, bool close, bool cancel, string route)
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
        Window owner = new() { Content = manager, Width = 640, Height = 400 };
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
            if (content is LayoutAnchorable toolModel) toolModel.Hiding += (_, _) => events.Add("content-hiding");
            host.Closing += (_, args) =>
            {
                events.Add("window-closing-enter:" + Flag(args.Cancel));
                if (cancelClose) args.Cancel = true;
                lastCancel = args.Cancel;
                events.Add("window-closing-exit:" + Flag(args.Cancel));
            };
            host.Closed += (_, _) => { closed = true; events.Add("window-closed"); };
            Assert.Equal(Observation(id, "before"), Take("before"));
            if (route == "window") host.Close();
            else if (route == "model") content.Close();
            else if (route == "command")
            {
                var command = manager.GetLayoutItemFromModel(content)!.CloseCommand!;
                if (command.CanExecute(null)) command.Execute(null);
                else events.Add("command-disabled");
            }
            else if (route == "replace") manager.Layout = new LayoutRoot();
            else owner.Content = null;
            Assert.Equal(Observation(id, "synchronous"), Take("synchronous"));
            Pump();
            Assert.Equal(Observation(id, "dispatched"), Take("dispatched"));
        }
        finally
        {
            cancelClose = false;
            if (host is not null && !closed)
            {
                host.Closing += (_, args) => args.Cancel = false;
                host.Close();
            }
            layout.FloatingWindows.Clear();
            owner.Close(); Pump();
        }

        string Take(string stage) => string.Join('\t', id, stage, Flag(lastCancel), Flag(host!.IsVisible), Flag(closed),
            content.Root is null ? "none" : ReferenceEquals(content.Root, layout) ? "original" : "other",
            content is LayoutAnchorable a ? Flag(a.IsHidden) : "-", layout.FloatingWindows.Count,
            manager.FloatingWindows.Count(), string.Join('>', events));
    }

    private static string Observation(string id, string stage) => File.ReadLines(Path.Combine(AppContext.BaseDirectory,
        "Assets", "WpfFloatingClose.tsv")).Single(row => row.StartsWith(id + "\t" + stage + "\t", StringComparison.Ordinal));
    private static string Flag(bool value) => value ? "1" : "0";
    private static void Pump() => Dispatcher.UIThread.RunJobs();

    private sealed class TraceHost() : LayoutFloatingWindowControl(new LayoutDocumentFloatingWindow())
    {
        public override ILayoutElement Model { get; } = new LayoutDocumentFloatingWindow();
        internal bool CloseAllowed { get; init; }
        internal bool HideAllowed { get; init; }
        internal bool InitialCancel { get; init; }
        internal bool SuppressBase { get; init; }
        internal bool Capturing { get; set; } = true;
        internal bool Result { get; private set; }
        internal List<string> Events { get; } = new();
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Capturing) { e.Cancel = false; base.OnClosing(e); e.Cancel = false; return; }
            // Avalonia constructs its native arguments internally. Enter through
            // Window.Close and hold the host open after recording the hook result.
            e.Cancel = InitialCancel;
            Events.Add("override-enter:" + Flag(e.Cancel));
            if (!SuppressBase) base.OnClosing(e);
            Events.Add("override-exit:" + Flag(e.Cancel));
            Result = e.Cancel;
            e.Cancel = true;
        }
        protected override bool CanClose(object? parameter = null) { if (Capturing) Events.Add("can-close"); return !Capturing || CloseAllowed; }
        protected override bool CanHide(object? parameter = null) { Events.Add("can-hide"); return HideAllowed; }
        protected override void DoHide() => Events.Add("do-hide");
    }
}
