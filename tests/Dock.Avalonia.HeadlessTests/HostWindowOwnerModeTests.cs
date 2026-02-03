using Avalonia.Headless.XUnit;
using Dock.Avalonia.Controls;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Dock.Settings;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class HostWindowOwnerModeTests
{
    [AvaloniaFact]
    public void Present_Uses_ParentWindow_When_OwnerModeDefault_With_ParentWindow()
    {
        var previousSetting = DockSettings.UseOwnerForFloatingWindows;
        DockSettings.UseOwnerForFloatingWindows = false;
        try
        {
            var parentHost = new HostWindow();
            var parentWindow = new DockWindow { Layout = new RootDock() };
            parentHost.Window = parentWindow;
            parentWindow.Host = parentHost;

            parentHost.Present(false);

            var childHost = new HostWindow();
            var childWindow = new DockWindow
            {
                Layout = new RootDock(),
                OwnerMode = DockWindowOwnerMode.Default,
                ParentWindow = parentWindow
            };
            childHost.Window = childWindow;
            childWindow.Host = childHost;

            childHost.Present(false);

            Assert.Same(parentHost, childHost.Owner);

            childHost.Close();
            parentHost.Close();
        }
        finally
        {
            DockSettings.UseOwnerForFloatingWindows = previousSetting;
        }
    }

    [AvaloniaFact]
    public void Present_Uses_ParentWindow_When_OwnerModeParentWindow()
    {
        var parentHost = new HostWindow();
        var parentWindow = new DockWindow { Layout = new RootDock() };
        parentHost.Window = parentWindow;
        parentWindow.Host = parentHost;

        parentHost.Present(false);

        var childHost = new HostWindow();
        var childWindow = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = parentWindow
        };
        childHost.Window = childWindow;
        childWindow.Host = childHost;

        childHost.Present(false);

        Assert.Same(parentHost, childHost.Owner);

        childHost.Close();
        parentHost.Close();
    }

    [AvaloniaFact]
    public void Present_Uses_DockableWindow_When_ParentWindow_Set()
    {
        var parentHost = new HostWindow();
        var parentWindow = new DockWindow { Layout = new RootDock() };
        parentHost.Window = parentWindow;
        parentWindow.Host = parentHost;

        parentHost.Present(false);

        var childHost = new HostWindow();
        var childWindow = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.DockableWindow,
            ParentWindow = parentWindow
        };
        childHost.Window = childWindow;
        childWindow.Host = childHost;

        childHost.Present(false);

        Assert.Same(parentHost, childHost.Owner);

        childHost.Close();
        parentHost.Close();
    }

    [AvaloniaFact]
    public void Present_Ignores_SelfParentWindow()
    {
        var host = new HostWindow();
        var window = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.ParentWindow
        };
        window.ParentWindow = window;
        host.Window = window;
        window.Host = host;

        host.Present(false);

        Assert.Null(host.Owner);

        host.Close();
    }

    [AvaloniaFact]
    public void Present_Ignores_ParentWindow_When_OwnerModeNone()
    {
        var previousSetting = DockSettings.UseOwnerForFloatingWindows;
        DockSettings.UseOwnerForFloatingWindows = true;
        try
        {
            var parentHost = new HostWindow();
            var parentWindow = new DockWindow { Layout = new RootDock() };
            parentHost.Window = parentWindow;
            parentWindow.Host = parentHost;

            parentHost.Present(false);

            var childHost = new HostWindow();
            var childWindow = new DockWindow
            {
                Layout = new RootDock(),
                OwnerMode = DockWindowOwnerMode.None,
                ParentWindow = parentWindow
            };
            childHost.Window = childWindow;
            childWindow.Host = childHost;

            childHost.Present(false);

            Assert.Null(childHost.Owner);

            childHost.Close();
            parentHost.Close();
        }
        finally
        {
            DockSettings.UseOwnerForFloatingWindows = previousSetting;
        }
    }

    [AvaloniaFact]
    public void Present_Uses_RootWindow_When_OwnerModeRootWindow()
    {
        var root = new RootDock();

        var rootHost = new HostWindow();
        var rootWindow = new DockWindow { Layout = root };
        root.Window = rootWindow;
        rootHost.Window = rootWindow;
        rootWindow.Host = rootHost;

        rootHost.Present(false);

        var childHost = new HostWindow();
        var childWindow = new DockWindow
        {
            Layout = new RootDock(),
            Owner = root,
            OwnerMode = DockWindowOwnerMode.RootWindow
        };
        childHost.Window = childWindow;
        childWindow.Host = childHost;

        childHost.Present(false);

        Assert.Same(rootHost, childHost.Owner);

        childHost.Close();
        rootHost.Close();
    }

    [AvaloniaFact]
    public void Present_Uses_RootWindow_From_OwnerChain()
    {
        var factory = new Dock.Model.Avalonia.Factory();
        var tool = new Tool { Id = "Tool", Title = "Tool" };
        var toolDock = new ToolDock
        {
            Id = "ToolDock",
            Title = "Tools",
            VisibleDockables = factory.CreateList<IDockable>(tool),
            ActiveDockable = tool
        };
        var root = new RootDock
        {
            Id = "Root",
            Title = "Root",
            VisibleDockables = factory.CreateList<IDockable>(toolDock),
            ActiveDockable = toolDock
        };

        factory.InitLayout(root);

        var rootHost = new HostWindow();
        var rootWindow = new DockWindow { Layout = root, Factory = factory };
        root.Window = rootWindow;
        rootHost.Window = rootWindow;
        rootWindow.Host = rootHost;
        rootHost.Present(false);

        var childHost = new HostWindow();
        var childWindow = new DockWindow
        {
            Layout = new RootDock(),
            Owner = tool,
            OwnerMode = DockWindowOwnerMode.RootWindow,
            Factory = factory
        };
        childHost.Window = childWindow;
        childWindow.Host = childHost;

        childHost.Present(false);

        Assert.Same(rootHost, childHost.Owner);

        childHost.Close();
        rootHost.Close();
    }

    [AvaloniaFact]
    public void Present_ParentWindow_Hierarchy_Uses_Nearest_Parent()
    {
        var rootHost = new HostWindow();
        var rootWindow = new DockWindow { Layout = new RootDock() };
        rootHost.Window = rootWindow;
        rootWindow.Host = rootHost;
        rootHost.Present(false);

        var parentHost = new HostWindow();
        var parentWindow = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = rootWindow
        };
        parentHost.Window = parentWindow;
        parentWindow.Host = parentHost;
        parentHost.Present(false);

        var childHost = new HostWindow();
        var childWindow = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = parentWindow
        };
        childHost.Window = childWindow;
        childWindow.Host = childHost;
        childHost.Present(false);

        var grandHost = new HostWindow();
        var grandWindow = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = childWindow
        };
        grandHost.Window = grandWindow;
        grandWindow.Host = grandHost;
        grandHost.Present(false);

        Assert.Same(rootHost, parentHost.Owner);
        Assert.Same(parentHost, childHost.Owner);
        Assert.Same(childHost, grandHost.Owner);

        grandHost.Close();
        childHost.Close();
        parentHost.Close();
        rootHost.Close();
    }

    [AvaloniaFact]
    public void Present_Modal_Uses_Parent_Owner()
    {
        var parentHost = new HostWindow();
        var parentWindow = new DockWindow { Layout = new RootDock() };
        parentHost.Window = parentWindow;
        parentWindow.Host = parentHost;
        parentHost.Present(false);

        var childHost = new HostWindow();
        var childWindow = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.ParentWindow,
            ParentWindow = parentWindow
        };
        childHost.Window = childWindow;
        childWindow.Host = childHost;

        childHost.Present(true);

        Assert.Same(parentHost, childHost.Owner);

        childHost.Close();
        parentHost.Close();
    }

    [AvaloniaFact]
    public void Present_Applies_ShowInTaskbar()
    {
        var host = new HostWindow();
        var window = new DockWindow
        {
            Layout = new RootDock(),
            ShowInTaskbar = false
        };
        host.Window = window;
        window.Host = host;

        host.Present(false);

        Assert.False(host.ShowInTaskbar);

        host.Close();
    }

    [AvaloniaFact]
    public void Present_Modal_NoOwner_DoesNotThrow()
    {
        var host = new HostWindow();
        var window = new DockWindow
        {
            Layout = new RootDock(),
            OwnerMode = DockWindowOwnerMode.None,
            IsModal = true
        };
        host.Window = window;
        window.Host = host;

        host.Present(true);

        Assert.True(host.IsVisible);
        Assert.Null(host.Owner);

        host.Close();
    }
}
