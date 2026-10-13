using System.Linq;
using Dock.Model.Avalonia;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockSplitInitializationTests
{
    [Theory]
    [InlineData(DockOperation.Left, false)]
    [InlineData(DockOperation.Right, false)]
    [InlineData(DockOperation.Top, false)]
    [InlineData(DockOperation.Bottom, false)]
    [InlineData(DockOperation.Left, true)]
    [InlineData(DockOperation.Right, true)]
    [InlineData(DockOperation.Top, true)]
    [InlineData(DockOperation.Bottom, true)]
    public void Splitting_into_existing_row_initializes_divider_and_allows_next_preview(DockOperation operation, bool documents)
    {
        var factory = new Factory();
        IDockable source = documents ? factory.CreateDocument() : factory.CreateTool();
        IDockable retained = documents ? factory.CreateDocument() : factory.CreateTool();
        IDockable target = documents ? factory.CreateDocument() : factory.CreateTool();
        IDock sourcePane = documents ? factory.CreateDocumentDock() : factory.CreateToolDock();
        IDock targetPane = documents ? factory.CreateDocumentDock() : factory.CreateToolDock();
        sourcePane.VisibleDockables = factory.CreateList(source, retained);
        sourcePane.ActiveDockable = source;
        targetPane.VisibleDockables = factory.CreateList(target);
        targetPane.ActiveDockable = target;
        var row = factory.CreateProportionalDock();
        row.Orientation = operation is DockOperation.Left or DockOperation.Right ? Orientation.Horizontal : Orientation.Vertical;
        var originalSplitter = factory.CreateProportionalDockSplitter();
        row.VisibleDockables = factory.CreateList<IDockable>(sourcePane, originalSplitter, targetPane);
        var root = factory.CreateRootDock();
        root.VisibleDockables = factory.CreateList<IDockable>(row);
        root.ActiveDockable = row;
        factory.InitLayout(root);

        Assert.True(new DockManager(new DockService()).ValidateDockable(source, targetPane, DragAction.Move, operation, true));

        var insertedPane = Assert.IsAssignableFrom<IDock>(source.Owner);
        Assert.NotSame(sourcePane, insertedPane);
        Assert.Same(row, insertedPane.Owner);
        Assert.Same(retained, Assert.Single(sourcePane.VisibleDockables));
        var divider = Assert.Single(row.VisibleDockables.OfType<IProportionalDockSplitter>(), x => !ReferenceEquals(x, originalSplitter));
        Assert.Same(row, divider.Owner);
        Assert.Same(factory, divider.Factory);
        Assert.NotNull(DockSplitPreview.Create(source, targetPane, operation));
    }
}
