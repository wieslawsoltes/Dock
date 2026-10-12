using System;
using Dock.Model.Adapters;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockWindowHostFactoryTests
{
    [Fact]
    public void Initialization_Provides_The_Window_Model_To_The_Optional_Factory()
    {
        WindowFactory factory = new();
        DockWindow window = new() { Layout = new RootDock() };
        RootDock owner = new();
        factory.InitDockWindow(window, owner);
        Assert.Same(window, factory.Requested);
        Assert.Same(factory.Host, window.Host);
        Assert.Same(window, factory.Host.Window);
        Assert.Same(owner, window.Owner);
        Assert.Same(factory, window.Factory);
        Assert.Equal(1, factory.Calls);
    }

    [Fact]
    public void Present_Reuses_A_Host_And_Recreates_It_With_The_Same_Model_After_Exit()
    {
        WindowFactory factory = new();
        DockWindow window = new() { Layout = new RootDock(), Factory = factory };
        HostAdapter adapter = new(window);
        adapter.Present(false);
        Assert.Same(window, factory.Requested);
        Assert.Same(window.Layout, factory.Host.Layout);
        Assert.True(factory.Host.IsTracked);
        adapter.Present(true);
        Assert.Equal(1, factory.Calls);
        Assert.Equal(2, factory.Host.Presentations);
        Assert.True(factory.Host.Modal);
        adapter.Exit();
        Assert.Null(window.Host);
        Assert.False(factory.Host.IsTracked);
        adapter.Present(false);
        Assert.Equal(2, factory.Calls);
        Assert.Same(window, factory.Requested);
    }

    [Fact]
    public void Explicit_Host_Initialization_Does_Not_Invoke_The_Optional_Factory()
    {
        WindowFactory factory = new();
        DockWindow window = new() { Layout = new RootDock() };
        Host host = new();
        factory.InitDockWindow(window, new RootDock(), host);
        Assert.Same(host, window.Host);
        Assert.Equal(0, factory.Calls);
    }

    [Fact]
    public void Ordinary_Factories_Keep_Their_Id_Based_Locator_For_Both_Paths()
    {
        Factory factory = new();
        Host host = new();
        int calls = 0;
        factory.HostWindowLocator = new() { ["special"] = () => { calls++; return host; } };
        DockWindow window = new() { Id = "special", Layout = new RootDock() };
        factory.InitDockWindow(window, new RootDock());
        Assert.Same(host, window.Host);
        HostAdapter adapter = new(window);
        adapter.Exit();
        adapter.Present(false);
        Assert.Same(host, window.Host);
        Assert.Equal(2, calls);
    }

    [Fact]
    public void Null_Result_Leaves_The_Model_Unhosted_Without_Using_A_Second_Locator()
    {
        WindowFactory factory = new() { OmitHost = true };
        factory.DefaultHostWindowLocator = () => throw new InvalidOperationException("Unexpected fallback");
        DockWindow window = new() { Layout = new RootDock() };
        factory.InitDockWindow(window, new RootDock());
        new HostAdapter(window).Present(false);
        Assert.Null(window.Host);
        Assert.Equal(2, factory.Calls);
        Assert.Equal(0, factory.Host.Presentations);
    }

    private sealed class WindowFactory : Factory, IDockWindowHostFactory
    {
        public Host Host { get; } = new();
        public IDockWindow? Requested { get; private set; }
        public int Calls { get; private set; }
        public bool OmitHost { get; set; }

        public IHostWindow? CreateHostWindow(IDockWindow window)
        {
            Requested = window;
            Calls++;
            return OmitHost ? null : Host;
        }
    }

    private sealed class Host : IHostWindow
    {
        public IHostWindowState? HostWindowState => null;
        public bool IsTracked { get; set; }
        public IDockWindow? Window { get; set; }
        public IDock? Layout { get; private set; }
        public int Presentations { get; private set; }
        public bool Modal { get; private set; }
        public void Present(bool isDialog) { Presentations++; Modal = isDialog; }
        public void Exit() { }
        public void SetLayout(IDock layout) => Layout = layout;
        public void SetPosition(double x, double y) { }
        public void GetPosition(out double x, out double y) { x = 0; y = 0; }
        public void SetSize(double width, double height) { }
        public void GetSize(out double width, out double height) { width = 400; height = 300; }
        public void SetWindowState(DockWindowState windowState) { }
        public DockWindowState GetWindowState() => DockWindowState.Normal;
        public void SetTitle(string? title) { }
        public void SetActive() { }
    }
}
