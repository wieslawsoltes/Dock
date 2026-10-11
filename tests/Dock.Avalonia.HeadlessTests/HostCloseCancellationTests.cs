using System;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostCloseCancellationTests
{
    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Factory_Veto_Keeps_The_Model_And_Host_Registered_Until_Retry(bool managed)
    {
        Factory factory = new();
        RootDock owner = new() { Factory = factory };
        RootDock layout = new() { Factory = factory };
        IHostWindow host = managed ? new ManagedHostWindow() : new HostWindow();
        factory.HostWindowLocator = new() { [nameof(IDockWindow)] = () => host };
        DockWindow window = new() { Layout = layout, Host = host };
        host.Window = window;
        layout.Window = window;
        factory.AddWindow(owner, window);
        IHostWindowCloseState closeState = Assert.IsAssignableFrom<IHostWindowCloseState>(host);
        Assert.False(closeState.IsClosed);
        window.Present(false);
        bool cancel = true;
        int closed = 0;
        int removed = 0;
        factory.WindowClosing += (_, args) => args.Cancel = cancel;
        factory.WindowClosed += (_, _) => closed++;
        factory.WindowRemoved += (_, _) => removed++;
        try
        {
            factory.RemoveWindow(window);
            Assert.False(closeState.IsClosed);
            Assert.True(host.IsTracked);
            Assert.Same(host, window.Host);
            Assert.Same(host, Assert.Single(factory.HostWindows));
            Assert.Same(window, Assert.Single(owner.Windows!));
            Assert.Same(owner, window.Owner);
            Assert.Same(factory, window.Factory);
            Assert.Same(layout, window.Layout);
            Assert.Same(window, layout.Window);
            Assert.Equal(0, closed);
            Assert.Equal(0, removed);

            cancel = false;
            factory.RemoveWindow(window);
            Assert.True(closeState.IsClosed);
            Assert.Null(window.Host);
            Assert.Empty(factory.HostWindows);
            Assert.Empty(owner.Windows!);
            Assert.Null(window.Owner);
            Assert.Null(window.Factory);
            Assert.Null(window.Layout);
            Assert.Null(layout.Window);
            Assert.Equal(1, closed);
            Assert.Equal(1, removed);
        }
        finally { cancel = false; host.Exit(); }
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Closing_Veto_Precedes_Model_And_Factory_Callbacks(bool viaModel)
    {
        Factory factory = new();
        ProbeDockWindow window = new() { Factory = factory };
        HostWindow host = new() { Window = window, IsTracked = true };
        window.Host = host;
        IHostWindowCloseState closeState = host;
        bool cancel = true;
        int factoryCalls = 0;
        factory.WindowClosing += (_, _) => factoryCalls++;
        host.Closing += (_, args) => args.Cancel = cancel;
        host.Show();
        try
        {
            if (viaModel) window.Exit();
            else host.Exit();
            Assert.True(host.IsVisible);
            Assert.True(host.IsTracked);
            Assert.False(closeState.IsClosed);
            Assert.Same(host, window.Host);
            Assert.Equal(0, window.CloseCalls);
            Assert.Equal(0, factoryCalls);

            cancel = false;
            if (viaModel) window.Exit();
            else host.Exit();
            Assert.True(closeState.IsClosed);
            Assert.False(host.IsVisible);
            Assert.Equal(1, window.CloseCalls);
            Assert.Equal(1, factoryCalls);
        }
        finally { cancel = false; host.Close(); }
    }

    [AvaloniaFact]
    public void Hidden_Hosts_Are_Live_And_Report_Closure_Before_Closed_Handlers()
    {
        HostWindow host = new();
        IHostWindowCloseState closeState = host;
        Assert.False(closeState.IsClosed);
        host.Show();
        host.Hide();
        Assert.False(closeState.IsClosed);
        bool? closedState = null;
        host.Closed += (_, _) => closedState = closeState.IsClosed;
        host.Close();
        Assert.True(closedState);
        Assert.True(closeState.IsClosed);
    }

    private sealed class ProbeDockWindow : DockWindow
    {
        public int CloseCalls { get; private set; }
        public override bool OnClose() { CloseCalls++; return true; }
    }
}
