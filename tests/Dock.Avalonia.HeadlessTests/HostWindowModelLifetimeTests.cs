using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowModelLifetimeTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Successful_Close_Detaches_The_Layout_From_The_Logical_Tree(bool preserveLayout, bool cancelFirst)
    {
        var (layout, model, host, factory) = Create(preserveLayout);
        bool cancel = cancelFirst;
        int closed = 0;
        host.Closing += (_, args) => args.Cancel = cancel;
        host.Closed += (_, _) => { Assert.Null(layout.Parent); closed++; };
        try
        {
            Assert.Same(host, layout.Parent);
            host.Close();
            if (cancelFirst)
            {
                Assert.Same(host, layout.Parent);
                Assert.Same(layout, host.Content);
                Assert.Equal(0, closed);
                cancel = false;
                host.Close();
            }
            Assert.Null(layout.Parent);
            Assert.Same(layout, host.Content);
            Assert.Equal(1, closed);
            Assert.Empty(factory.HostWindows);
            if (preserveLayout) Assert.Same(layout, model.Layout);
        }
        finally { cancel = false; host.Close(); factory.RemoveWindow(model); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task A_Retained_Layout_Does_Not_Retain_Its_Closed_Host(bool preserveLayout)
    {
        var (layout, host) = Close(preserveLayout);
        for (int i = 0; i < 20; i++)
        {
            AvaloniaHeadlessPlatform.ForceRenderTimerTick();
            Dispatcher.UIThread.RunJobs();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            if (!IsAlive(host)) break;
            await Task.Delay(10);
        }
        Assert.False(IsAlive(host));
        GC.KeepAlive(layout);
    }

    [AvaloniaFact]
    public void Closing_Preserves_Application_Control_Content()
    {
        TextBlock content = new() { Text = "Custom content" };
        HostWindow host = new() { Content = content };
        host.Show();
        host.Close();
        Assert.Same(content, host.Content);
        Assert.Same(host, content.Parent);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (RootDock Layout, WeakReference<HostWindow> Host) Close(bool preserveLayout)
    {
        var (layout, _, host, _) = Create(preserveLayout);
        host.Close();
        Dispatcher.UIThread.RunJobs();
        return (layout, new(host));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static bool IsAlive(WeakReference<HostWindow> host) => host.TryGetTarget(out _);

    private static (RootDock Layout, DockWindow Model, HostWindow Host, Factory Factory) Create(bool preserveLayout)
    {
        Factory factory = new LifetimeFactory(preserveLayout);
        RootDock owner = new() { Factory = factory };
        RootDock layout = new() { Factory = factory, VisibleDockables = factory.CreateList<IDockable>() };
        DockWindow model = new() { Layout = layout };
        layout.Window = model;
        HostWindow host = new() { Content = layout };
        factory.AddWindow(owner, model);
        factory.InitDockWindow(model, owner, host);
        model.Present(false);
        Dispatcher.UIThread.RunJobs();
        return (layout, model, host, factory);
    }

    private sealed class LifetimeFactory(bool preserveLayout) : Factory, IDockWindowClosePolicy
    {
        public bool PreserveLayoutOnHostClose(IDockWindow window) => preserveLayout;
    }
}
