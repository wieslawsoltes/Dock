using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Dock.Model.Mvvm.Core;
using Xunit;
using System.Collections.Generic;
using System.Windows.Input;

namespace Dock.Model.Mvvm.UnitTests;

public class RecordingFactory : Factory
{
    public List<(IDock Dock, IDockable Dockable)> AddDockableCalls { get; } = new();
    public List<IDockable> SetActiveDockableCalls { get; } = new();
    public List<(IDock Dock, IDockable? Dockable)> SetFocusedDockableCalls { get; } = new();

    public override void AddDockable(IDock dock, IDockable dockable)
    {
        AddDockableCalls.Add((dock, dockable));
    }

    public override void SetActiveDockable(IDockable dockable)
    {
        SetActiveDockableCalls.Add(dockable);
    }

    public override void SetFocusedDockable(IDock dock, IDockable? dockable)
    {
        SetFocusedDockableCalls.Add((dock, dockable));
    }
}

public class DocumentDockTests
{
    [Fact]
    public void DocumentDock_Default_Values()
    {
        var dock = new DocumentDock();

        Assert.False(dock.CanCreateDocument);
        Assert.False(dock.EnableWindowDrag);
        Assert.Equal(DocumentTabLayout.Top, dock.TabsLayout);
        Assert.NotNull(dock.CreateDocument);
    }

    [Fact]
    public void AddDocument_Calls_Factory_Methods()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var document = new Document();

        dock.AddDocument(document);

        Assert.Single(factory.AddDockableCalls);
        Assert.Same(dock, factory.AddDockableCalls[0].Dock);
        Assert.Same(document, factory.AddDockableCalls[0].Dockable);
        Assert.Single(factory.SetActiveDockableCalls);
        Assert.Same(document, factory.SetActiveDockableCalls[0]);
        Assert.Single(factory.SetFocusedDockableCalls);
        Assert.Same(dock, factory.SetFocusedDockableCalls[0].Dock);
        Assert.Same(document, factory.SetFocusedDockableCalls[0].Dockable);
    }

    [Fact]
    public void AddTool_Calls_Factory_Methods()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var tool = new Tool();

        dock.AddTool(tool);

        Assert.Single(factory.AddDockableCalls);
        Assert.Same(dock, factory.AddDockableCalls[0].Dock);
        Assert.Same(tool, factory.AddDockableCalls[0].Dockable);
        Assert.Single(factory.SetActiveDockableCalls);
        Assert.Same(tool, factory.SetActiveDockableCalls[0]);
        Assert.Single(factory.SetFocusedDockableCalls);
        Assert.Same(dock, factory.SetFocusedDockableCalls[0].Dock);
        Assert.Same(tool, factory.SetFocusedDockableCalls[0].Dockable);
    }

    [Fact]
    public void CreateDocument_Command_Uses_DocumentFactory()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var document = new Document();
        dock.DocumentFactory = () => document;

        ICommand command = dock.CreateDocument!;
        command.Execute(null);

        Assert.Single(factory.AddDockableCalls);
        Assert.Same(document, factory.AddDockableCalls[0].Dockable);
    }
}

public class ToolDockTests
{
    [Fact]
    public void ToolDock_Default_Values()
    {
        var dock = new ToolDock();
        Assert.Equal(Alignment.Unset, dock.Alignment);
        Assert.False(dock.IsExpanded);
        Assert.True(dock.AutoHide);
        Assert.Equal(GripMode.Visible, dock.GripMode);
    }

    [Fact]
    public void AddTool_Calls_Factory_Methods()
    {
        var factory = new RecordingFactory();
        var dock = new ToolDock { Factory = factory };
        var tool = new Tool();

        dock.AddTool(tool);

        Assert.Single(factory.AddDockableCalls);
        Assert.Same(dock, factory.AddDockableCalls[0].Dock);
        Assert.Same(tool, factory.AddDockableCalls[0].Dockable);
    }
}

public class DockWindowTests
{
    [Fact]
    public void DockWindow_Default_Values()
    {
        var window = new DockWindow();
        Assert.Equal(nameof(IDockWindow), window.Id);
        Assert.Equal(nameof(IDockWindow), window.Title);
    }
}
