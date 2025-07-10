using System.Collections.Generic;
using System.Windows.Input;
using Dock.Model.Controls;
using Dock.Model.Prism.Controls;
using Dock.Model.Prism.Core;
using Xunit;

namespace Dock.Model.Prism.UnitTests;

public class DocumentDockTests
{
    private class RecordingFactory : Factory
    {
        public readonly List<(IDock Dock, IDockable Dockable)> Added = new();
        public readonly List<IDockable> Active = new();
        public readonly List<(IDock Dock, IDockable? Dockable)> Focused = new();

        public override void AddDockable(IDock dock, IDockable dockable)
        {
            Added.Add((dock, dockable));
        }

        public override void SetActiveDockable(IDockable dockable)
        {
            Active.Add(dockable);
        }

        public override void SetFocusedDockable(IDock dock, IDockable? dockable)
        {
            Focused.Add((dock, dockable));
        }
    }

    [Fact]
    public void AddDocument_UsesFactory()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var document = new Document();

        dock.AddDocument(document);

        Assert.Single(factory.Added);
        Assert.Same(dock, factory.Added[0].Dock);
        Assert.Same(document, factory.Added[0].Dockable);
        Assert.Single(factory.Active);
        Assert.Same(document, factory.Active[0]);
        Assert.Single(factory.Focused);
        Assert.Same(dock, factory.Focused[0].Dock);
        Assert.Same(document, factory.Focused[0].Dockable);
    }

    [Fact]
    public void AddTool_UsesFactory()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var tool = new Tool();

        dock.AddTool(tool);

        Assert.Single(factory.Added);
        Assert.Same(tool, factory.Added[0].Dockable);
        Assert.Single(factory.Active);
        Assert.Same(tool, factory.Active[0]);
        Assert.Single(factory.Focused);
        Assert.Same(dock, factory.Focused[0].Dock);
        Assert.Same(tool, factory.Focused[0].Dockable);
    }

    [Fact]
    public void CreateDocument_Command_UsesFactoryFunction()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var created = new Document();
        dock.DocumentFactory = () => created;

        ICommand? command = dock.CreateDocument;
        command?.Execute(null);

        Assert.Single(factory.Added);
        Assert.Same(created, factory.Added[0].Dockable);
    }
}
