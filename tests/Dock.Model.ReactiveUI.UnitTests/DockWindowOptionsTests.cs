using Dock.Model.Controls;
using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Dock.Model.ReactiveUI.Core;
using System.Collections.Generic;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests;

public class DockWindowOptionsTests
{
    private sealed class TestHostWindow : IHostWindow
    {
        public IHostWindowState? HostWindowState { get; }
        public bool IsTracked { get; set; }
        public IDockWindow? Window { get; set; }
        public DockWindowState WindowState { get; private set; } = DockWindowState.Normal;
        public bool PresentedAsDialog { get; private set; }

        public void Present(bool isDialog)
        {
            PresentedAsDialog = isDialog;
        }

        public void Exit()
        {
        }

        public void SetPosition(double x, double y)
        {
        }

        public void GetPosition(out double x, out double y)
        {
            x = 0;
            y = 0;
        }

        public void SetSize(double width, double height)
        {
        }

        public void GetSize(out double width, out double height)
        {
            width = 0;
            height = 0;
        }

        public void SetWindowState(DockWindowState windowState)
        {
            WindowState = windowState;
        }

        public DockWindowState GetWindowState()
        {
            return WindowState;
        }

        public void SetTitle(string? title)
        {
        }

        public void SetLayout(IDock layout)
        {
        }

        public void SetActive()
        {
        }
    }

    private sealed class HostFactory : Factory
    {
        public TestHostWindow Host { get; } = new();

        public override IHostWindow? GetHostWindow(string id)
        {
            return Host;
        }
    }

    private sealed class TrackingFactory : Factory
    {
        public List<TestHostWindow> Hosts { get; } = new();

        public override IHostWindow? GetHostWindow(string id)
        {
            var host = new TestHostWindow();
            Hosts.Add(host);
            return host;
        }
    }

    [Fact]
    public void CreateWindowFrom_Applies_Options()
    {
        var factory = new HostFactory();
        var parentWindow = factory.CreateDockWindow();
        var options = new DockWindowOptions
        {
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = parentWindow,
            IsModal = true
        };

        var tool = factory.CreateTool();
        var window = factory.CreateWindowFrom(tool, options);

        Assert.NotNull(window);
        Assert.Equal(options.OwnerMode, window!.OwnerMode);
        Assert.Same(parentWindow, window.ParentWindow);
        Assert.True(window.IsModal);
    }

    [Fact]
    public void CreateWindowFrom_NullOptions_Uses_Defaults()
    {
        var factory = new HostFactory();
        var tool = factory.CreateTool();

        var window = factory.CreateWindowFrom(tool, null);

        Assert.NotNull(window);
        Assert.Equal(DockWindowOwnerMode.Default, window!.OwnerMode);
        Assert.Null(window.ParentWindow);
        Assert.False(window.IsModal);
    }

    [Fact]
    public void CreateWindowFrom_Applies_ShowInTaskbar()
    {
        var factory = new HostFactory();
        var tool = factory.CreateTool();
        var options = new DockWindowOptions { ShowInTaskbar = false };

        var window = factory.CreateWindowFrom(tool, options);

        Assert.NotNull(window);
        Assert.Equal(false, window!.ShowInTaskbar);
    }

    [Fact]
    public void FloatDockable_Uses_Modal_Option()
    {
        var factory = new HostFactory();
        var root = factory.CreateRootDock();
        var toolDock = factory.CreateToolDock();
        var tool = factory.CreateTool();

        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(toolDock, tool);

        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, toolDock);
        root.ActiveDockable = toolDock;

        factory.InitLayout(root);

        factory.FloatDockable(tool, new DockWindowOptions { IsModal = true });

        Assert.True(factory.Host.PresentedAsDialog);
    }

    [Fact]
    public void FloatDockable_DockableWindow_Assigns_ParentWindow()
    {
        var factory = new TrackingFactory();
        var root = factory.CreateRootDock();
        var toolDock = factory.CreateToolDock();
        var tool = factory.CreateTool();

        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(toolDock, tool);

        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, toolDock);
        root.ActiveDockable = toolDock;

        factory.InitLayout(root);

        var parentWindow = factory.CreateDockWindow();
        parentWindow.Layout = root;
        root.Window = parentWindow;

        factory.FloatDockable(tool, new DockWindowOptions { OwnerMode = DockWindowOwnerMode.DockableWindow });

        Assert.NotNull(root.Windows);
        Assert.Single(root.Windows!);
        Assert.Same(parentWindow, root.Windows![0].ParentWindow);
    }

    [Fact]
    public void FloatDockable_Assigns_Options_To_Window()
    {
        var factory = new TrackingFactory();
        var root = factory.CreateRootDock();
        var toolDock = factory.CreateToolDock();
        var tool = factory.CreateTool();

        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(toolDock, tool);

        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, toolDock);
        root.ActiveDockable = toolDock;

        factory.InitLayout(root);

        var parentWindow = factory.CreateDockWindow();
        var options = new DockWindowOptions
        {
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = parentWindow,
            IsModal = false
        };

        factory.FloatDockable(tool, options);

        Assert.NotNull(root.Windows);
        Assert.Single(root.Windows!);
        var window = root.Windows![0];
        Assert.Equal(DockWindowOwnerMode.ParentWindow, window.OwnerMode);
        Assert.Same(parentWindow, window.ParentWindow);
    }

    [Fact]
    public void SplitToWindow_Uses_Modal_Option()
    {
        var factory = new TrackingFactory();
        var root = factory.CreateRootDock();
        var toolDock = factory.CreateToolDock();
        var tool = factory.CreateTool();

        toolDock.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(toolDock, tool);

        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, toolDock);
        root.ActiveDockable = toolDock;

        factory.InitLayout(root);

        factory.SplitToWindow(toolDock, tool, 0, 0, 200, 120, new DockWindowOptions { IsModal = true });

        Assert.NotEmpty(factory.Hosts);
        Assert.True(factory.Hosts[^1].PresentedAsDialog);
    }

    [Fact]
    public void FloatAllDockables_Uses_Modal_Option()
    {
        var factory = new TrackingFactory();
        var root = factory.CreateRootDock();
        var toolDock = factory.CreateToolDock();
        var tool1 = factory.CreateTool();
        var tool2 = factory.CreateTool();

        toolDock.VisibleDockables = factory.CreateList<IDockable>(tool1, tool2);
        toolDock.ActiveDockable = tool1;

        root.VisibleDockables = factory.CreateList<IDockable>();
        factory.AddDockable(root, toolDock);
        root.ActiveDockable = toolDock;

        factory.InitLayout(root);

        factory.FloatAllDockables(tool1, new DockWindowOptions { IsModal = true });

        Assert.NotEmpty(factory.Hosts);
        Assert.True(factory.Hosts[^1].PresentedAsDialog);
    }
}
