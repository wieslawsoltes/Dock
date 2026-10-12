using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockSplitPolicyTests
{
    [Theory]
    [InlineData(-1, DockOperation.Left)]
    [InlineData(-1, DockOperation.Top)]
    [InlineData(-1, DockOperation.Right)]
    [InlineData(-1, DockOperation.Bottom)]
    [InlineData(0, DockOperation.Left)]
    [InlineData(0, DockOperation.Top)]
    [InlineData(0, DockOperation.Right)]
    [InlineData(0, DockOperation.Bottom)]
    [InlineData(1, DockOperation.Left)]
    [InlineData(1, DockOperation.Top)]
    [InlineData(1, DockOperation.Right)]
    [InlineData(1, DockOperation.Bottom)]
    public void Tool_Splits_Use_The_Optional_Destination_Factory_Policy(int mode, DockOperation operation)
    {
        Factory factory = mode < 0 ? new Factory() : new SplitFactory(mode == 1);
        Tool tool = new() { Id = "tool", CanDockAsDocument = true };
        ToolDock source = new() { VisibleDockables = factory.CreateList<IDockable>(tool) };
        DocumentDock target = new() { VisibleDockables = factory.CreateList<IDockable>(new Document { Id = "document" }) };
        ProportionalDock panel = new() { VisibleDockables = factory.CreateList<IDockable>(source, target) };
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(panel) };
        factory.InitLayout(root);
        DockManager engine = new(new DockService());

        Assert.True(engine.ValidateDockable(tool, target, DragAction.Move, operation, false));
        Assert.Same(source, tool.Owner);
        if (factory is SplitFactory before) Assert.Equal(0, before.Calls);
        Assert.True(engine.ValidateDockable(tool, target, DragAction.Move, operation, true));
        if (mode == 1) Assert.IsType<DocumentDock>(tool.Owner);
        else Assert.IsType<ToolDock>(tool.Owner);
        Assert.NotSame(source, tool.Owner);
        Assert.NotSame(target, tool.Owner);
        Assert.Same(factory, tool.Factory);
        Assert.Same(tool, Assert.Single(((IDock)tool.Owner!).VisibleDockables!));
        if (factory is SplitFactory policy)
        {
            Assert.Equal(1, policy.Calls);
            Assert.Same(tool, policy.Source);
            Assert.Same(target, policy.Target);
            Assert.Equal(operation, policy.Operation);
        }
    }

    [Fact]
    public void Split_Policy_Does_Not_Bypass_Document_Docking_Capability()
    {
        SplitFactory factory = new(true);
        Tool tool = new() { CanDockAsDocument = false };
        ToolDock source = new() { VisibleDockables = factory.CreateList<IDockable>(tool) };
        DocumentDock target = new() { VisibleDockables = factory.CreateList<IDockable>(new Document()) };
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(
            new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(source, target) }) };
        factory.InitLayout(root);
        DockManager engine = new(new DockService());
        Assert.False(engine.ValidateDockable(tool, target, DragAction.Move, DockOperation.Left, true));
        Assert.Same(source, tool.Owner);
        Assert.Equal(0, factory.Calls);
    }

    [Theory]
    [InlineData(false, DockOperation.Left)]
    [InlineData(false, DockOperation.Top)]
    [InlineData(false, DockOperation.Right)]
    [InlineData(false, DockOperation.Bottom)]
    [InlineData(true, DockOperation.Left)]
    [InlineData(true, DockOperation.Top)]
    [InlineData(true, DockOperation.Right)]
    [InlineData(true, DockOperation.Bottom)]
    public void Pane_Splits_Preserve_All_Tab_Identities_And_Order(bool document, DockOperation operation)
    {
        Factory factory = document ? new SplitFactory(true) : new Factory();
        Tool[] tools = { new() { Id = "first" }, new() { Id = "second" }, new() { Id = "third" }, new() { Id = "fourth" } };
        ToolDock source = new() { VisibleDockables = factory.CreateList<IDockable>(tools) };
        DocumentDock target = new() { VisibleDockables = factory.CreateList<IDockable>(new Document()) };
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(
            new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(source, target) }) };
        factory.InitLayout(root);
        DockManager engine = new(new DockService());

        Assert.True(engine.ValidateDockable(source, target, DragAction.Move, operation, false));
        Assert.Equal(tools, source.VisibleDockables);
        Assert.True(engine.ValidateDockable(source, target, DragAction.Move, operation, true));
        IDock created = Assert.IsAssignableFrom<IDock>(tools[0].Owner);
        if (document) Assert.IsType<DocumentDock>(created);
        else Assert.IsType<ToolDock>(created);
        Assert.Equal(tools, created.VisibleDockables);
        foreach (Tool tool in tools) Assert.Same(created, tool.Owner);
        Assert.Empty(source.VisibleDockables!);
    }

    private sealed class SplitFactory(bool useDocumentDock) : Factory, IDockSplitPolicy
    {
        public int Calls { get; private set; }
        public IDockable? Source { get; private set; }
        public IDock? Target { get; private set; }
        public DockOperation Operation { get; private set; }
        public bool UseDocumentDock(IDockable source, IDock target, DockOperation operation)
        {
            Calls++;
            Source = source;
            Target = target;
            Operation = operation;
            return useDocumentDock;
        }
    }
}
