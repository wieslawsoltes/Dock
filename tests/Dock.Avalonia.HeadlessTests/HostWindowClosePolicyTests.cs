using System;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Avalonia.Internal;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowClosePolicyTests
{
    [AvaloniaTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Preserved_Layout_Retains_Its_Model_And_Can_Be_Presented_Again(bool cancelFirst, bool managed)
    {
        PolicyFactory factory = new();
        RootDock owner = new() { Factory = factory, Windows = factory.CreateList<IDockWindow>() };
        Document document = new();
        RootDock layout = new() { Factory = factory, VisibleDockables = factory.CreateList<IDockable>(document) };
        DockWindow model = new() { Layout = layout };
        IHostWindow host = CreateHost(managed);
        layout.Window = model;
        document.Owner = layout;
        document.Factory = factory;
        factory.AddWindow(owner, model);
        factory.InitDockWindow(model, owner, host);
        int closed = 0;
        bool cancel = cancelFirst;
        factory.WindowClosing += (_, args) => args.Cancel = cancel;
        factory.WindowClosed += (_, _) => closed++;
        model.Present(false);
        try
        {
            host.Exit();
            if (cancelFirst)
            {
                if (host is HostWindow native) Assert.True(native.IsVisible);
                Assert.True(host.IsTracked);
                Assert.Same(host, model.Host);
                Assert.Equal(0, closed);
                cancel = false;
                host.Exit();
            }
            Assert.True(((IHostWindowCloseState)host).IsClosed);
            Assert.False(host.IsTracked);
            Assert.Null(model.Host);
            Assert.Empty(factory.HostWindows);
            Assert.Same(model, Assert.Single(owner.Windows!));
            Assert.Same(layout, model.Layout);
            Assert.Same(model, layout.Window);
            Assert.Same(layout, document.Owner);
            Assert.Equal(0, factory.ContentCloseCalls);
            Assert.Equal(1, closed);
            if (managed) Assert.Empty(ManagedWindowRegistry.GetOrCreateDock(factory).VisibleDockables!);
            factory.DefaultHostWindowLocator = () => CreateHost(managed);
            model.Present(false);
            Assert.NotSame(host, model.Host);
            Assert.True(model.Host!.IsTracked);
        }
        finally { cancel = false; model.Exit(); factory.RemoveWindow(model); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Preserved_Host_Is_Released_Before_The_Factory_Closed_Callback(bool managed)
    {
        PolicyFactory factory = new();
        RootDock owner = new() { Factory = factory };
        RootDock layout = new() { Factory = factory };
        DockWindow model = new() { Layout = layout };
        IHostWindow host = CreateHost(managed);
        IHostWindow replacement = CreateHost(managed);
        factory.DefaultHostWindowLocator = () => host;
        factory.AddWindow(owner, model);
        model.Present(false);
        bool recreate = true;
        factory.WindowClosed += (_, args) =>
        {
            if (!recreate) return;
            recreate = false;
            Assert.Same(model, args.Window);
            Assert.True(((IHostWindowCloseState)host).IsClosed);
            Assert.Null(model.Host);
            Assert.DoesNotContain(host, factory.HostWindows);
            if (managed) Assert.Empty(ManagedWindowRegistry.GetOrCreateDock(factory).VisibleDockables!);
            factory.DefaultHostWindowLocator = () => replacement;
            model.Present(false);
        };
        try
        {
            host.Exit();
            Assert.False(recreate);
            Assert.Same(replacement, model.Host);
            Assert.Same(replacement, Assert.Single(factory.HostWindows));
            Assert.True(replacement.IsTracked);
            Assert.Same(layout, model.Layout);
            Assert.Same(model, Assert.Single(owner.Windows!));
            if (managed) Assert.Single(ManagedWindowRegistry.GetOrCreateDock(factory).VisibleDockables!);
        }
        finally { recreate = false; model.Exit(); factory.RemoveWindow(model); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Managed_Exit_Rejects_Reentrant_Close_And_Allows_Retry_After_Cancellation_Or_Exception(bool throwFirst)
    {
        PolicyFactory factory = new();
        RootDock owner = new() { Factory = factory };
        DockWindow model = new() { Layout = new RootDock { Factory = factory } };
        ManagedHostWindow host = new();
        factory.DefaultHostWindowLocator = () => host;
        factory.AddWindow(owner, model);
        model.Present(false);
        bool reject = true;
        int closing = 0;
        factory.WindowClosing += (_, args) =>
        {
            closing++;
            host.Exit();
            if (reject && throwFirst) throw new InvalidOperationException("close rejected");
            args.Cancel = reject;
        };
        try
        {
            if (throwFirst) Assert.Throws<InvalidOperationException>(host.Exit);
            else host.Exit();
            Assert.Equal(1, closing);
            Assert.False(((IHostWindowCloseState)host).IsClosed);
            Assert.Same(host, model.Host);
            Assert.True(host.IsTracked);
            Assert.Single(ManagedWindowRegistry.GetOrCreateDock(factory).VisibleDockables!);
            reject = false;
            host.Exit();
            Assert.Equal(2, closing);
            Assert.True(((IHostWindowCloseState)host).IsClosed);
            Assert.Null(model.Host);
            Assert.Empty(ManagedWindowRegistry.GetOrCreateDock(factory).VisibleDockables!);
            Assert.Same(model, Assert.Single(owner.Windows!));
        }
        finally { reject = false; model.Exit(); factory.RemoveWindow(model); }
    }

    [AvaloniaFact]
    public void A_Closed_Callback_Can_Install_A_Replacement_Host()
    {
        PolicyFactory factory = new();
        DockWindow model = new() { Factory = factory };
        HostWindow host = new() { Window = model, IsTracked = true };
        HostWindow replacement = new() { Window = model };
        model.Host = host;
        host.Closed += (_, _) => model.Host = replacement;
        host.Show();
        host.Close();
        Assert.Same(replacement, model.Host);
        Assert.DoesNotContain(host, factory.HostWindows);
        replacement.Close();
    }

    private sealed class PolicyFactory : Factory, IDockWindowClosePolicy
    {
        public int ContentCloseCalls { get; private set; }
        public bool PreserveLayoutOnHostClose(IDockWindow window) => true;
        public override void CloseWindow(IDockWindow window) { ContentCloseCalls++; base.CloseWindow(window); }
    }

    private static IHostWindow CreateHost(bool managed) => managed ? new ManagedHostWindow() : new HostWindow();
}
