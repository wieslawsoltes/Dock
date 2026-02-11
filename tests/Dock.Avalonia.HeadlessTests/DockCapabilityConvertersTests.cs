using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Headless.XUnit;
using Dock.Avalonia.Converters;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class DockCapabilityConvertersTests
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
    public void CanCloseConverter_Uses_Dock_Policy()
    {
        var (_, dock, tool) = CreateHierarchy();
        tool.CanClose = true;
        dock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };

        var result = DockCapabilityConverters.CanCloseConverter.Convert(tool, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [AvaloniaFact]
    public void CanCloseConverter_Uses_Dockable_Override_Over_Dock_Policy()
    {
        var (_, dock, tool) = CreateHierarchy();
        tool.CanClose = true;
        dock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanClose = false
        };
        tool.DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanClose = true
        };

        var result = DockCapabilityConverters.CanCloseConverter.Convert(tool, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }

    [AvaloniaFact]
    public void CanDropConverter_Uses_Root_Policy_For_Dock()
    {
        var (root, dock, _) = CreateHierarchy();
        root.RootDockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDrop = false
        };

        var result = DockCapabilityConverters.CanDropConverter.Convert(dock, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [AvaloniaFact]
    public void Converter_Returns_False_For_Non_Dockable_Value()
    {
        var result = DockCapabilityConverters.CanCloseConverter.Convert("not-dockable", typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(false, result);
    }

    [AvaloniaFact]
    public void ConvertBack_Throws_NotSupportedException()
    {
        var converter = DockCapabilityConverters.CanCloseConverter;

        Assert.Throws<NotSupportedException>(() => converter.ConvertBack(true, typeof(bool), null, CultureInfo.InvariantCulture));
    }

    [AvaloniaFact]
    public void CanDockAsDocumentConverter_Uses_Dockable_Override()
    {
        var (_, dock, tool) = CreateHierarchy();
        dock.DockCapabilityPolicy = new DockCapabilityPolicy
        {
            CanDockAsDocument = false
        };
        tool.DockCapabilityOverrides = new DockCapabilityOverrides
        {
            CanDockAsDocument = true
        };

        var result = DockCapabilityConverters.CanDockAsDocumentConverter.Convert(tool, typeof(bool), null, CultureInfo.InvariantCulture);

        Assert.Equal(true, result);
    }
}
