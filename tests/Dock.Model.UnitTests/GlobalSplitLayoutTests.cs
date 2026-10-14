using System.Collections.Generic;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class GlobalSplitLayoutTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Window_Edge_Roundtrips_Keep_A_Bounded_Tree_And_The_Main_Region(bool document)
    {
        Factory factory = new();
        IDockable source = document ? new Document() : new Tool();
        IDock pane = document ? new DocumentDock() : new ToolDock { Alignment = Alignment.Bottom };
        pane.VisibleDockables = factory.CreateList(source);
        DocumentDock documents = new() { VisibleDockables = factory.CreateList<IDockable>(new Document()) };
        ProportionalDock main = new()
        {
            Id = "MainLayout", IsCollapsable = false, Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(documents, new ProportionalDockSplitter(), pane),
            ActiveDockable = documents, DefaultDockable = documents
        };
        RootDock root = new()
        {
            IsCollapsable = false, VisibleDockables = factory.CreateList<IDockable>(main),
            ActiveDockable = main, DefaultDockable = main
        };
        factory.InitLayout(root);
        DockService service = new();
        DockOperation[] operations = { DockOperation.Right, DockOperation.Bottom, DockOperation.Left, DockOperation.Top };

        for (int iteration = 0; iteration < 80; iteration++)
        {
            IDock target = Assert.IsAssignableFrom<IDock>(Assert.Single(root.VisibleDockables!));
            DockOperation operation = operations[iteration % operations.Length];
            Assert.True(service.SplitDockable(source, Assert.IsAssignableFrom<IDock>(source.Owner), target, operation, true));

            HashSet<IDockable> visited = new();
            AssertTree(root, visited);
            Assert.Contains(main, visited);
            Assert.Contains(source, visited);
            Assert.Same(main, documents.Owner);
            Assert.Same(documents, main.DefaultDockable);
            Assert.Equal(Orientation.Vertical, main.Orientation);
            int proportionalCount = 0;
            foreach (IDockable dockable in visited)
            {
                if (dockable is IProportionalDock) proportionalCount++;
            }
            Assert.InRange(proportionalCount, 1, 2);
            IDock inserted = Assert.IsAssignableFrom<IDock>(source.Owner);
            IProportionalDock split = Assert.IsAssignableFrom<IProportionalDock>(inserted.Owner);
            Assert.Equal(operation is DockOperation.Left or DockOperation.Right ? Orientation.Horizontal : Orientation.Vertical, split.Orientation);
            Assert.Same(inserted, split.VisibleDockables![operation is DockOperation.Left or DockOperation.Top ? 0 : split.VisibleDockables.Count - 1]);
            Assert.Same(Assert.Single(root.VisibleDockables!), root.ActiveDockable);
            Assert.Same(root.ActiveDockable, root.DefaultDockable);
            if (inserted is IToolDock tool) Assert.Equal(operation switch
            {
                DockOperation.Left => Alignment.Left, DockOperation.Right => Alignment.Right,
                DockOperation.Top => Alignment.Top, _ => Alignment.Bottom
            }, tool.Alignment);
        }
    }

    [Theory]
    [InlineData(DockOperation.Left, 0.6, 0.4, 0.3, 0.2)]
    [InlineData(DockOperation.Right, 0.6, double.NaN, 0.3, 0.2)]
    [InlineData(DockOperation.Top, double.NaN, double.NaN, 0.25, 0.25)]
    [InlineData(DockOperation.Bottom, 0.0, 0.4, 0.3, 0.2)]
    [InlineData(DockOperation.Bottom, 0.8, 0.4, 1.0 / 3.0, 1.0 / 6.0)]
    public void Matching_Target_Is_Reused_With_Existing_Relative_Shares(DockOperation operation,
        double firstShare, double secondShare, double expectedFirst, double expectedSecond)
    {
        Factory factory = new();
        DocumentDock first = new() { Proportion = firstShare };
        ToolDock second = new() { Proportion = secondShare };
        ProportionalDockSplitter existingSplitter = new();
        ProportionalDock main = new()
        {
            IsCollapsable = false, Proportion = 0.7,
            Orientation = operation is DockOperation.Left or DockOperation.Right ? Orientation.Horizontal : Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(first, existingSplitter, second),
            ActiveDockable = first, DefaultDockable = first
        };
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(main), ActiveDockable = main, DefaultDockable = main };
        factory.InitLayout(root);
        ToolDock incoming = new() { Proportion = 0.9, CollapsedProportion = 0.9 };

        factory.SplitToDock(main, incoming, operation);

        Assert.Same(main, Assert.Single(root.VisibleDockables!));
        Assert.Same(main, root.ActiveDockable);
        Assert.Same(main, root.DefaultDockable);
        Assert.Same(first, main.DefaultDockable);
        Assert.Same(incoming, main.ActiveDockable);
        Assert.Equal(0.7, main.Proportion);
        Assert.Equal(expectedFirst, first.Proportion, 10);
        Assert.Equal(expectedSecond, second.Proportion, 10);
        Assert.Equal(first.Proportion, first.CollapsedProportion);
        Assert.Equal(second.Proportion, second.CollapsedProportion);
        Assert.Equal(0.5, incoming.Proportion);
        Assert.Equal(0.5, incoming.CollapsedProportion);
        Assert.Contains(existingSplitter, main.VisibleDockables!);
        Assert.Equal(5, main.VisibleDockables!.Count);
        AssertTree(root, new HashSet<IDockable>());
    }

    [Fact]
    public void Matching_Nested_Target_Reuses_Its_Region_And_Wraps_Content_With_Its_Group()
    {
        Factory factory = new();
        DocumentDock documents = new() { Proportion = 1.0 };
        ProportionalDock region = new()
        {
            Orientation = Orientation.Horizontal, Proportion = 0.7,
            VisibleDockables = factory.CreateList<IDockable>(documents), ActiveDockable = documents, DefaultDockable = documents
        };
        ToolDock sibling = new() { Proportion = 0.3 };
        ProportionalDock outer = new()
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(region, new ProportionalDockSplitter(), sibling),
            ActiveDockable = region, DefaultDockable = region
        };
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(outer) };
        factory.InitLayout(root);
        Document incoming = new() { DockGroup = "editor" };

        factory.SplitToDock(region, incoming, DockOperation.Right);

        Assert.Same(outer, region.Owner);
        Assert.Same(region, outer.ActiveDockable);
        Assert.Same(region, outer.DefaultDockable);
        Assert.Equal(0.7, region.Proportion);
        Assert.Equal(0.3, sibling.Proportion);
        IDock inserted = Assert.IsAssignableFrom<IDock>(incoming.Owner);
        Assert.Same(region, inserted.Owner);
        Assert.Same(incoming, inserted.ActiveDockable);
        Assert.Equal("editor", inserted.DockGroup);
        Assert.Equal(0.5, inserted.Proportion);
        Assert.Equal(0.5, documents.Proportion);
        AssertTree(root, new HashSet<IDockable>());
    }

    [Fact]
    public void Existing_Root_Wrapper_Chain_Is_Normalized_Before_Splitting()
    {
        Factory factory = new();
        DocumentDock documents = new();
        ProportionalDock main = new()
        {
            IsCollapsable = false, Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(documents), ActiveDockable = documents, DefaultDockable = documents
        };
        List<ProportionalDock> wrappers = new();
        IDock child = main;
        for (int index = 0; index < 20; index++)
        {
            ProportionalDock wrapper = new()
            {
                VisibleDockables = factory.CreateList<IDockable>(child), ActiveDockable = child, DefaultDockable = child,
                Proportion = 0.7, CollapsedProportion = 0.8
            };
            wrappers.Add(wrapper);
            child = wrapper;
        }
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(child), ActiveDockable = child, DefaultDockable = child };
        factory.InitLayout(root);

        factory.SplitToDock(child, new ToolDock(), DockOperation.Bottom);

        Assert.Same(main, Assert.Single(root.VisibleDockables!));
        Assert.Same(main, root.ActiveDockable);
        Assert.Same(main, root.DefaultDockable);
        Assert.Equal(0.7, main.Proportion);
        Assert.Equal(0.8, main.CollapsedProportion);
        Assert.Same(documents, main.DefaultDockable);
        foreach (ProportionalDock wrapper in wrappers)
        {
            Assert.Null(wrapper.Owner);
            Assert.Null(wrapper.ActiveDockable);
            Assert.Null(wrapper.DefaultDockable);
            Assert.Empty(wrapper.VisibleDockables!);
        }
        AssertTree(root, new HashSet<IDockable>());
    }

    private static void AssertTree(IDock dock, HashSet<IDockable> visited)
    {
        Assert.True(visited.Add(dock), "A dock must occur only once in the layout.");
        if (dock.VisibleDockables is not { } children) return;
        if (dock.ActiveDockable is { } active) Assert.Contains(active, children);
        if (dock.DefaultDockable is { } defaultDockable) Assert.Contains(defaultDockable, children);
        for (int index = 0; index < children.Count; index++)
        {
            IDockable child = children[index];
            Assert.Same(dock, child.Owner);
            if (child is ISplitter)
            {
                Assert.InRange(index, 1, children.Count - 2);
                Assert.IsNotAssignableFrom<ISplitter>(children[index - 1]);
                Assert.IsNotAssignableFrom<ISplitter>(children[index + 1]);
            }
            if (child is IDock childDock) AssertTree(childDock, visited);
            else Assert.True(visited.Add(child));
        }
    }
}
