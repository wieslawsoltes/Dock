using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockServiceSplitProportionTests
{
    [Fact]
    public void SplitDockable_AfterBottomResize_TopSplitKeepsVerticalProportions()
    {
        // Arrange
        var factory = new Factory();
        var dockService = new DockService();

        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Factory = factory
        };

        var horizontalLayout = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        factory.AddDockable(root, horizontalLayout);

        var initialDocumentDock = new DocumentDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            Proportion = 0.7
        };
        factory.AddDockable(horizontalLayout, initialDocumentDock);

        var existingDocument = new Document { Id = "Document1", Title = "Document 1" };
        var documentToBottom = new Document { Id = "Document2", Title = "Document 2" };
        factory.AddDockable(initialDocumentDock, existingDocument);
        factory.AddDockable(initialDocumentDock, documentToBottom);
        initialDocumentDock.ActiveDockable = existingDocument;

        factory.InitLayout(root);

        // Act 1: Split to bottom.
        var firstSplit = dockService.SplitDockable(
            documentToBottom,
            initialDocumentDock,
            initialDocumentDock,
            DockOperation.Bottom,
            bExecute: true);

        // Assert 1: We now have a vertical nested layout with top + bottom docks.
        Assert.True(firstSplit);
        Assert.Single(horizontalLayout.VisibleDockables!);

        var verticalLayout = Assert.IsType<ProportionalDock>(horizontalLayout.VisibleDockables[0]);
        Assert.Equal(Orientation.Vertical, verticalLayout.Orientation);
        Assert.Equal(3, verticalLayout.VisibleDockables!.Count);

        var topDocumentDock = Assert.IsType<DocumentDock>(verticalLayout.VisibleDockables[0]);
        var bottomDocumentDock = Assert.IsType<DocumentDock>(verticalLayout.VisibleDockables[2]);

        // Simulate splitter resize by updating proportions.
        topDocumentDock.Proportion = 0.72;
        bottomDocumentDock.Proportion = 0.28;

        // Add a new source document to the top dock and dock it to Top of the resized bottom dock.
        var documentToTop = new Document { Id = "Document3", Title = "Document 3" };
        factory.AddDockable(topDocumentDock, documentToTop);
        topDocumentDock.ActiveDockable = documentToTop;

        // Act 2: Split resized bottom dock to top.
        var secondSplit = dockService.SplitDockable(
            documentToTop,
            topDocumentDock,
            bottomDocumentDock,
            DockOperation.Top,
            bExecute: true);

        // Assert 2: Operation remains vertical and proportionally splits only the resized bottom area.
        Assert.True(secondSplit);
        Assert.Equal(5, verticalLayout.VisibleDockables.Count);
        Assert.Same(topDocumentDock, verticalLayout.VisibleDockables[0]);
        Assert.IsType<ProportionalDockSplitter>(verticalLayout.VisibleDockables[1]);

        var insertedTopDock = Assert.IsType<DocumentDock>(verticalLayout.VisibleDockables[2]);
        Assert.IsType<ProportionalDockSplitter>(verticalLayout.VisibleDockables[3]);
        Assert.Same(bottomDocumentDock, verticalLayout.VisibleDockables[4]);

        Assert.Equal(0.72, topDocumentDock.Proportion, 3);
        Assert.Equal(0.14, insertedTopDock.Proportion, 3);
        Assert.Equal(0.14, bottomDocumentDock.Proportion, 3);
    }
}
