using System.Collections.ObjectModel;
using Dock.Model.Core;
using Dock.Model.CaliburMicro.Controls;
using Dock.Model.CaliburMicro.Core;
using Xunit;

namespace Dock.Model.CaliburMicro.UnitTests;

public class FactoryTests
{
    [Fact]
    public void TestFactory_Ctor()
    {
        var actual = new TestFactory();
        Assert.NotNull(actual);
    }

    [Fact]
    public void CreateList_Creates_ObservableCollection_Empty()
    {
        var factory = new TestFactory();
        var actual = factory.CreateList<IDockable>();
        Assert.NotNull(actual);
        Assert.IsType<ObservableCollection<IDockable>>(actual);
        Assert.Empty(actual);
    }

    [Fact]
    public void CreateRootDock_Creates_RootDock_Type()
    {
        var factory = new TestFactory();
        var actual = factory.CreateRootDock();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }

    [Fact]
    public void CreateProportionalDock_Creates_ProportionalDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDock();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDock>(actual);
    }

    [Fact]
    public void CreateDockDock_Creates_DockDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockDock();
        Assert.NotNull(actual);
        Assert.IsType<DockDock>(actual);
    }

    [Fact]
    public void CreateStackDock_Creates_StackDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateStackDock();
        Assert.NotNull(actual);
        Assert.IsType<StackDock>(actual);
    }

    [Fact]
    public void CreateGridDock_Creates_GridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDock();
        Assert.NotNull(actual);
        Assert.IsType<GridDock>(actual);
    }

    [Fact]
    public void CreateWrapDock_Creates_WrapDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateWrapDock();
        Assert.NotNull(actual);
        Assert.IsType<WrapDock>(actual);
    }

    [Fact]
    public void CreateUniformGridDock_Creates_UniformGridDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateUniformGridDock();
        Assert.NotNull(actual);
        Assert.IsType<UniformGridDock>(actual);
    }

    [Fact]
    public void CreateProportionalDockSplitter_Creates_ProportionalDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateProportionalDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<ProportionalDockSplitter>(actual);
    }

    [Fact]
    public void CreateGridDockSplitter_Creates_GridDockSplitter()
    {
        var factory = new TestFactory();
        var actual = factory.CreateGridDockSplitter();
        Assert.NotNull(actual);
        Assert.IsType<GridDockSplitter>(actual);
    }

    [Fact]
    public void CreateToolDock_Creates_ToolDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateToolDock();
        Assert.NotNull(actual);
        Assert.IsType<ToolDock>(actual);
    }

    [Fact]
    public void CreateDocumentDock_Creates_DocumentDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDocumentDock();
        Assert.NotNull(actual);
        Assert.IsType<DocumentDock>(actual);
    }

    [Fact]
    public void CreateDockWindow_Creates_DockWindow()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDockWindow();
        Assert.NotNull(actual);
        Assert.IsType<DockWindow>(actual);
    }

    [Fact]
    public void CreateDocument_Creates_Document()
    {
        var factory = new TestFactory();
        var actual = factory.CreateDocument();
        Assert.NotNull(actual);
        Assert.IsType<Document>(actual);
        Assert.True(double.IsNaN(actual.MinWidth));
        Assert.True(double.IsNaN(actual.MaxWidth));
        Assert.True(double.IsNaN(actual.MinHeight));
        Assert.True(double.IsNaN(actual.MaxHeight));
    }

    [Fact]
    public void CreateTool_Creates_Tool()
    {
        var factory = new TestFactory();
        var actual = factory.CreateTool();
        Assert.NotNull(actual);
        Assert.IsType<Tool>(actual);
        Assert.True(double.IsNaN(actual.MinWidth));
        Assert.True(double.IsNaN(actual.MaxWidth));
        Assert.True(double.IsNaN(actual.MinHeight));
        Assert.True(double.IsNaN(actual.MaxHeight));
    }

    [Fact]
    public void CreateLayout_Creates_RootDock()
    {
        var factory = new TestFactory();
        var actual = factory.CreateLayout();
        Assert.NotNull(actual);
        Assert.IsType<RootDock>(actual);
    }
}

public class TestFactory : Factory
{
}
