using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Converters;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockCapabilityBindingUpdateTests
{
    private static (RootDock Root, ToolDock Dock, Tool Tool) CreateHierarchy()
    {
        var root = new RootDock
        {
            VisibleDockables = new List<IDockable>()
        };

        var dock = new ToolDock
        {
            Owner = root,
            VisibleDockables = new List<IDockable>()
        };

        var tool = new Tool
        {
            Owner = dock
        };

        root.VisibleDockables!.Add(dock);
        dock.VisibleDockables!.Add(tool);
        root.ActiveDockable = dock;
        dock.ActiveDockable = tool;

        return (root, dock, tool);
    }

    [AvaloniaFact]
    public void CanCloseConverter_MultiBinding_Updates_When_Dockable_Property_Changes()
    {
        var (_, _, tool) = CreateHierarchy();
        var button = new Button
        {
            DataContext = tool
        };
        button.Bind(Control.IsVisibleProperty, new MultiBinding
        {
            Converter = DockCapabilityConverters.CanCloseMultiConverter,
            Bindings =
            {
                new Binding(),
                new Binding("CanClose"),
                new Binding("DockCapabilityOverrides.CanClose"),
                new Binding("Owner.DockCapabilityPolicy.CanClose")
            }
        });

        Dispatcher.UIThread.RunJobs();
        Assert.True(button.IsVisible);

        tool.CanClose = false;

        Dispatcher.UIThread.RunJobs();
        Assert.False(button.IsVisible);
    }

    [AvaloniaFact]
    public void CanCloseConverter_MultiBinding_Updates_When_Root_Policy_Reference_Changes()
    {
        var (root, _, tool) = CreateHierarchy();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = true
        };

        var button = new Button
        {
            DataContext = tool
        };
        button.Bind(Control.IsVisibleProperty, new MultiBinding
        {
            Converter = DockCapabilityConverters.CanCloseMultiConverter,
            Bindings =
            {
                new Binding(),
                new Binding("CanClose"),
                new Binding("DockCapabilityOverrides.CanClose"),
                new Binding("Owner.DockCapabilityPolicy.CanClose"),
                new Binding("Owner.Owner.RootDockCapabilityPolicy.CanClose")
            }
        });

        Dispatcher.UIThread.RunJobs();
        Assert.True(button.IsVisible);

        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };

        Dispatcher.UIThread.RunJobs();
        Assert.False(button.IsVisible);
    }
}
