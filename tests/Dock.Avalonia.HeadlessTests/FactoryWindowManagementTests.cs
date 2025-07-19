using System.Collections.Generic;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryWindowManagementTests
{
    [AvaloniaFact]
    public void AddWindow_Adds_To_Root_Windows()
    {
        var factory = new Factory();
        var root = new RootDock { Windows = new List<IDockWindow>() };
        root.Factory = factory;

        var window = new DockWindow();
        factory.AddWindow(root, window);

        Assert.Single(root.Windows!);
        Assert.Contains(window, root.Windows);
        Assert.Equal(root, window.Owner);
    }

    [AvaloniaFact]
    public void InsertWindow_Inserts_Window_At_Index()
    {
        var factory = new Factory();
        var root = new RootDock { Windows = new List<IDockWindow>() };
        root.Factory = factory;

        var w1 = new DockWindow();
        var w2 = new DockWindow();
        factory.AddWindow(root, w1);
        factory.InsertWindow(root, w2, 0);

        Assert.Equal(2, root.Windows!.Count);
        Assert.Equal(w2, root.Windows[0]);
        Assert.Equal(w1, root.Windows[1]);
    }

    [AvaloniaFact]
    public void RemoveWindow_Removes_From_Root()
    {
        var factory = new Factory();
        var root = new RootDock { Windows = new List<IDockWindow>() };
        root.Factory = factory;

        var window = new DockWindow();
        factory.AddWindow(root, window);

        factory.RemoveWindow(window);

        Assert.Empty(root.Windows!);
    }
}
