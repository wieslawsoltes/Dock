using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using System;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryPinStateTests
{
    [AvaloniaFact]
    public void PinDockable_Updates_ActiveDockable()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool1 = new Tool();
        var tool2 = new Tool();
        factory.AddDockable(toolDock, tool1);
        factory.AddDockable(toolDock, tool2);
        toolDock.ActiveDockable = tool1;

        factory.PinDockable(tool1);

        Assert.Same(tool2, toolDock.ActiveDockable);
    }

    [AvaloniaFact]
    public void UnpinDockable_Restores_ActiveDockable_And_State()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>(), IsExpanded = true, AutoHide = false };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);
        toolDock.ActiveDockable = tool;

        factory.PinDockable(tool);
        factory.UnpinDockable(tool);

        Assert.Same(tool, toolDock.ActiveDockable);
        Assert.True(toolDock.IsExpanded);
        Assert.False(toolDock.AutoHide);
    }

    [AvaloniaFact]
    public void PinDockable_Invalid_State_Throws()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool { Owner = toolDock };

        Assert.Throws<InvalidOperationException>(() => factory.PinDockable(tool));
    }
}
