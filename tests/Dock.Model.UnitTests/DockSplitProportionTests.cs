using System;
using Dock.Model.Avalonia;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockSplitProportionTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Invalid_share_does_not_mutate_the_layout(double share)
    {
        var factory = new Factory();
        var inserted = factory.CreateToolDock();
        var retained = factory.CreateToolDock();
        var parent = factory.CreateProportionalDock();
        parent.VisibleDockables = factory.CreateList<IDockable>(inserted, retained);
        inserted.Owner = parent;
        inserted.Proportion = 0.5;
        retained.Proportion = 0.5;
        Assert.False(DockSplitProportion.Apply(inserted, share));
        Assert.Equal(0.5, inserted.Proportion);
        Assert.Equal(0.5, retained.Proportion);
    }

    [Fact]
    public void Retained_sibling_ratios_are_preserved_and_splitters_are_ignored()
    {
        var factory = new Factory();
        var inserted = factory.CreateToolDock();
        var first = factory.CreateToolDock();
        var second = factory.CreateToolDock();
        var splitter = factory.CreateProportionalDockSplitter();
        var parent = factory.CreateProportionalDock();
        parent.VisibleDockables = factory.CreateList<IDockable>(inserted, splitter, first, second);
        inserted.Owner = parent;
        first.Proportion = 0.2;
        second.Proportion = 0.6;
        Assert.True(DockSplitProportion.Apply(inserted, 0.4));
        Assert.Equal(0.4, inserted.Proportion, 6);
        Assert.Equal(0.15, first.Proportion, 6);
        Assert.Equal(0.45, second.Proportion, 6);
        Assert.True(double.IsNaN(splitter.Proportion));
        Assert.Equal(first.Proportion, first.CollapsedProportion);
        Assert.Equal(second.Proportion, second.CollapsedProportion);
    }

    [Fact]
    public void Projected_layout_respects_operation_restrictions_without_touching_live_models()
    {
        var factory = new Factory();
        var root = factory.CreateRootDock();
        var dock = factory.CreateToolDock();
        var source = factory.CreateTool();
        var other = factory.CreateTool();
        dock.VisibleDockables = factory.CreateList<IDockable>(source, other);
        root.VisibleDockables = factory.CreateList<IDockable>(dock);
        factory.InitLayout(root);
        ((IDockableDockingRestrictions)source).AllowedDockOperations = DockOperationMask.Fill;
        Assert.Null(DockSplitPreview.Create(source, dock, DockOperation.Right));
        Assert.Same(dock, source.Owner);
        Assert.Equal(2, dock.VisibleDockables.Count);
    }
}
