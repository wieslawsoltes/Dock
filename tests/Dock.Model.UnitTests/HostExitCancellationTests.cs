using System;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class HostExitCancellationTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_Canceled_Exit_Preserves_The_Host_And_Previous_Tracking_State(bool tracked)
    {
        ClosingHost host = new() { IsTracked = tracked };
        DockWindow window = CreateWindow(host);
        window.Exit();
        Assert.Same(host, window.Host);
        Assert.Equal(tracked, host.IsTracked);
        Assert.Equal(17, window.X);
        Assert.Equal(29, window.Y);
        Assert.Equal(420, window.Width);
        Assert.Equal(280, window.Height);
        Assert.Equal(1, host.ExitCalls);
        host.AllowClose = true;
        window.Exit();
        Assert.Null(window.Host);
        Assert.False(host.IsTracked);
        Assert.Equal(2, host.ExitCalls);
    }

    [Fact]
    public void A_Legacy_Host_Keeps_Unconditional_Exit_Behavior()
    {
        Host host = new() { IsTracked = true };
        DockWindow window = CreateWindow(host);
        window.Exit();
        Assert.Null(window.Host);
        Assert.False(host.IsTracked);
        Assert.Equal(1, host.ExitCalls);
    }

    [Fact]
    public void An_Already_Closed_Host_Is_Detached_Without_Another_Exit_Callback()
    {
        ClosingHost host = new() { IsClosed = true, IsTracked = true };
        DockWindow window = CreateWindow(host);
        window.Exit();
        Assert.Null(window.Host);
        Assert.False(host.IsTracked);
        Assert.Equal(0, host.ExitCalls);
    }

    [Fact]
    public void A_Throwing_Exit_Restores_Tracking_And_Can_Be_Retried()
    {
        ClosingHost host = new() { IsTracked = true, OnExit = () => throw new InvalidOperationException("Close failed") };
        DockWindow window = CreateWindow(host);
        Assert.Throws<InvalidOperationException>(window.Exit);
        Assert.Same(host, window.Host);
        Assert.True(host.IsTracked);
        host.OnExit = null;
        host.AllowClose = true;
        window.Exit();
        Assert.Null(window.Host);
        Assert.Equal(2, host.ExitCalls);
    }

    [Fact]
    public void A_Reentrant_Exit_Does_Not_Invoke_The_Host_Twice()
    {
        ClosingHost host = new() { IsTracked = true, AllowClose = true };
        DockWindow window = CreateWindow(host);
        host.OnExit = window.Exit;
        window.Exit();
        Assert.Null(window.Host);
        Assert.Equal(1, host.ExitCalls);
    }

    [Fact]
    public void A_Replacement_Host_Installed_By_The_Close_Callback_Is_Preserved()
    {
        ClosingHost host = new() { IsTracked = true, AllowClose = true };
        ClosingHost replacement = new() { IsTracked = true };
        DockWindow window = CreateWindow(host);
        host.OnExit = () => window.Host = replacement;
        window.Exit();
        Assert.Same(replacement, window.Host);
        Assert.True(replacement.IsTracked);
        Assert.Equal(0, replacement.ExitCalls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Factory_Removal_Preserves_Ownership_Until_The_Host_Closes(bool reenterAfterClosing)
    {
        ClosingHost host = new() { IsTracked = true };
        DockWindow window = CreateWindow(host);
        Factory factory = new();
        RootDock owner = new() { Windows = factory.CreateList<IDockWindow>(window) };
        DockWindow parent = new();
        window.Owner = owner;
        window.ParentWindow = parent;
        window.Factory = factory;
        RootDock layout = (RootDock)window.Layout!;
        int removed = 0;
        factory.WindowRemoved += (_, _) => removed++;
        factory.RemoveWindow(window);
        Assert.Same(window, Assert.Single(owner.Windows));
        Assert.Same(host, window.Host);
        Assert.Same(owner, window.Owner);
        Assert.Same(parent, window.ParentWindow);
        Assert.Same(factory, window.Factory);
        Assert.Same(layout, window.Layout);
        Assert.Same(window, layout.Window);
        Assert.Equal(0, removed);

        host.AllowClose = true;
        // A close callback can itself ask the factory to remove this window.
        if (reenterAfterClosing) host.OnClosed = () => factory.RemoveWindow(window);
        else host.OnExit = () => factory.RemoveWindow(window);
        factory.RemoveWindow(window);
        Assert.Empty(owner.Windows);
        Assert.Null(window.Host);
        Assert.Null(window.Owner);
        Assert.Null(window.ParentWindow);
        Assert.Null(window.Factory);
        Assert.Null(window.Layout);
        Assert.Null(layout.Window);
        Assert.Equal(1, removed);
        Assert.Equal(2, host.ExitCalls);
    }

    private static DockWindow CreateWindow(Host host)
    {
        RootDock layout = new();
        DockWindow window = new() { Host = host, Layout = layout };
        layout.Window = window;
        host.Window = window;
        return window;
    }

    private sealed class ClosingHost : Host, IHostWindowCloseState
    {
        public bool AllowClose { get; set; }
        public bool IsClosed { get; set; }
        public Action? OnClosed { get; set; }
        public override void Exit()
        {
            base.Exit();
            IsClosed = AllowClose;
            if (IsClosed) OnClosed?.Invoke();
        }
    }

    private class Host : IHostWindow
    {
        public IHostWindowState? HostWindowState => null;
        public bool IsTracked { get; set; }
        public IDockWindow? Window { get; set; }
        public int ExitCalls { get; private set; }
        public Action? OnExit { get; set; }
        public void Present(bool isDialog) { }
        public virtual void Exit() { ExitCalls++; OnExit?.Invoke(); }
        public void SetLayout(IDock layout) { }
        public void SetPosition(double x, double y) { }
        public void GetPosition(out double x, out double y) { x = 17; y = 29; }
        public void SetSize(double width, double height) { }
        public void GetSize(out double width, out double height) { width = 420; height = 280; }
        public void SetWindowState(DockWindowState windowState) { }
        public DockWindowState GetWindowState() => DockWindowState.Normal;
        public void SetTitle(string? title) { }
        public void SetActive() { }
    }
}
