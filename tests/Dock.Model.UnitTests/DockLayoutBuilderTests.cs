using System.Linq;
using Dock.Model.Builder;
using Dock.Model.Core;
using Dock.Model.Mvvm;
using Dock.Model.Mvvm.Controls;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockLayoutBuilderTests
{
    [Fact]
    public void Build_SimpleLayout_ReturnsRootDock()
    {
        var factory = new Factory();
        var doc = new Document { Id = "Doc1", Title = "Doc" };

        var root = new DockLayoutBuilder(factory)
            .WithDocument(doc)
            .Build();

        var layout = Assert.IsType<RootDock>(root);
        var main = Assert.IsType<ProportionalDock>(layout.VisibleDockables?.First());
        var documentDock = Assert.IsType<DocumentDock>(main.VisibleDockables?.First());
        Assert.Equal(doc, documentDock.ActiveDockable);
    }

    [Fact]
    public void Build_ComplexLayout_CreatesSplitHierarchy()
    {
        var factory = new Factory();
        var doc1 = new Document { Id = "D1", Title = "One" };
        var doc2 = new Document { Id = "D2", Title = "Two" };
        var tool = new Tool { Id = "T1", Title = "Tool" };

        var root = new DockLayoutBuilder(factory)
            .SplitHorizontally()
            .WithDocument(doc1)
            .WithDocument(doc2)
            .WithTool(tool, Alignment.Right)
            .Build();

        var main = Assert.IsType<ProportionalDock>(root.VisibleDockables?.First());
        Assert.Equal(Orientation.Horizontal, main.Orientation);
        Assert.Equal(5, main.VisibleDockables?.Count);
    }
}

