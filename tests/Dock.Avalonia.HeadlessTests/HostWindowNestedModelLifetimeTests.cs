using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Themes.Fluent;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowNestedModelLifetimeTests
{
    [AvaloniaTheory]
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public async Task Retaining_A_Nested_Layout_Does_Not_Retain_Its_Closed_Host(bool tools, bool preserveLayout, bool localTheme)
    {
        var (layout, host) = Close(tools, preserveLayout, localTheme);
        for (int i = 0; i < 20; i++)
        {
            Pump();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (!IsAlive(host)) break;
            await Task.Delay(10);
        }
        Assert.False(IsAlive(host));
        GC.KeepAlive(layout);
    }

    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Closing_Releases_Nested_Models_And_Allows_A_Replacement_Host(bool tools, bool cancelFirst)
    {
        var (layout, panel, pane, document, model, host, factory) = Create(tools, true, true);
        bool cancel = cancelFirst;
        host.Closing += (_, args) => args.Cancel = cancel;
        HostWindow? replacement = null;
        host.Closed += (_, _) =>
        {
            Assert.Null(layout.Parent);
            Assert.Null(panel.Parent);
            Assert.Null(pane.Parent);
            Assert.Null(document.Parent);
            replacement = new() { Content = layout };
            factory.InitDockWindow(model, model.Owner!, replacement);
            model.Present(false);
        };
        try
        {
            Assert.NotNull(panel.Parent);
            host.Close();
            if (cancelFirst)
            {
                Assert.Same(host, layout.Parent);
                Assert.NotNull(panel.Parent);
                Assert.Null(replacement);
                cancel = false;
                host.Close();
            }
            Pump();
            Assert.NotNull(replacement);
            Assert.True(replacement.IsVisible);
            Assert.Same(replacement, layout.Parent);
            Assert.NotNull(panel.Parent);
            Assert.Same(layout, panel.Owner);
            Assert.Same(panel, pane.Owner);
            Assert.Same(pane, document.Owner);
            Assert.Same(layout, host.Content);
        }
        finally { cancel = false; host.Close(); replacement?.Close(); factory.RemoveWindow(model); }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (RootDock Layout, WeakReference<HostWindow> Host) Close(bool tools, bool preserveLayout, bool localTheme)
    {
        var (layout, _, _, _, _, host, _) = Create(tools, preserveLayout, localTheme);
        host.Close();
        Pump();
        return (layout, new(host));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsAlive(WeakReference<HostWindow> host) => host.TryGetTarget(out _);

    private static void Pump()
    {
        AvaloniaHeadlessPlatform.ForceRenderTimerTick();
        Dispatcher.UIThread.RunJobs();
    }

    private static (RootDock Layout, ProportionalDock Panel, DockBase Pane, DockableBase Document,
        DockWindow Model, HostWindow Host, Factory Factory) Create(bool tools, bool preserveLayout, bool localTheme)
    {
        Factory factory = new LifetimeFactory(preserveLayout);
        RootDock owner = new() { Factory = factory };
        DockableBase document = tools ? new Tool { Content = new TextBlock { Text = "Tool" } }
            : new Document { Content = new TextBlock { Text = "Document" } };
        DockBase pane = tools ? new ToolDock() : new DocumentDock();
        ProportionalDock panel = new() { Factory = factory, VisibleDockables = factory.CreateList<IDockable>(pane), ActiveDockable = pane };
        RootDock layout = new() { Factory = factory, VisibleDockables = factory.CreateList<IDockable>(panel), ActiveDockable = panel };
        document.Factory = factory;
        document.Owner = pane;
        pane.Factory = factory;
        pane.Owner = panel;
        pane.VisibleDockables = factory.CreateList<IDockable>(document);
        pane.ActiveDockable = document;
        panel.Owner = layout;
        DockWindow model = new() { Layout = layout, Width = 640, Height = 400 };
        layout.Window = model;
        HostWindow host = new() { Content = layout };
        if (localTheme) host.Styles.Add(new DockFluentTheme());
        factory.AddWindow(owner, model);
        factory.InitDockWindow(model, owner, host);
        model.Present(false);
        Pump();
        return (layout, panel, pane, document, model, host, factory);
    }

    private sealed class LifetimeFactory(bool preserveLayout) : Factory, IDockWindowClosePolicy
    {
        public bool PreserveLayoutOnHostClose(IDockWindow window) => preserveLayout;
    }
}
