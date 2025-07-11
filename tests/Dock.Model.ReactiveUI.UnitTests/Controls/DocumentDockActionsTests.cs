using Dock.Model.Core;
using Dock.Model.ReactiveUI.Controls;
using Xunit;

namespace Dock.Model.ReactiveUI.UnitTests.Controls;

public class DocumentDockActionsTests
{
    private class RecordingFactory : Factory
    {
        public IDock? AddedDock;
        public IDockable? AddedDockable;
        public IDockable? ActiveDockable;
        public (IDock dock, IDockable? dockable)? Focused;

        public override void AddDockable(IDock dock, IDockable dockable)
        {
            AddedDock = dock;
            AddedDockable = dockable;
        }

        public override void SetActiveDockable(IDockable dockable)
        {
            ActiveDockable = dockable;
        }

        public override void SetFocusedDockable(IDock dock, IDockable? dockable)
        {
            Focused = (dock, dockable);
        }
    }

    [Fact]
    public void AddDocument_Uses_Factory_Methods()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var document = new Document();

        dock.AddDocument(document);

        Assert.Same(dock, factory.AddedDock);
        Assert.Same(document, factory.AddedDockable);
        Assert.Same(document, factory.ActiveDockable);
        Assert.NotNull(factory.Focused);
        Assert.Same(dock, factory.Focused?.dock);
        Assert.Same(document, factory.Focused?.dockable);
    }

    [Fact]
    public void AddTool_Uses_Factory_Methods()
    {
        var factory = new RecordingFactory();
        var dock = new DocumentDock { Factory = factory };
        var tool = new Tool();

        dock.AddTool(tool);

        Assert.Same(dock, factory.AddedDock);
        Assert.Same(tool, factory.AddedDockable);
        Assert.Same(tool, factory.ActiveDockable);
        Assert.NotNull(factory.Focused);
        Assert.Same(dock, factory.Focused?.dock);
        Assert.Same(tool, factory.Focused?.dockable);
    }
}
