using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
    public void Generated_Root_Wrapper_Chain_Is_Normalized_Before_Splitting()
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
            ProportionalDock wrapper = CreateSingletonWrapper(factory, child);
            wrapper.Proportion = 0.7;
            wrapper.CollapsedProportion = 0.8;
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

    [Fact]
    public void Normalizing_Root_Wrappers_Preserves_Inherited_Docking_Group()
    {
        Factory factory = new();
        Document document = new();
        DocumentDock documents = new() { VisibleDockables = factory.CreateList<IDockable>(document) };
        ProportionalDock wrapper = CreateSingletonWrapper(factory, documents);
        wrapper.DockGroup = "editors";
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(wrapper), ActiveDockable = wrapper };
        factory.InitLayout(root);
        Document sameGroup = new() { DockGroup = "editors" };
        Document otherGroup = new() { DockGroup = "tools" };
        Assert.True(DockGroupValidator.ValidateDockingGroups(sameGroup, document));
        Assert.False(DockGroupValidator.ValidateDockingGroups(otherGroup, document));

        factory.SplitToDock(wrapper, new ToolDock(), DockOperation.Bottom);

        Assert.NotNull(wrapper.Owner);
        Assert.Same(wrapper, documents.Owner);
        Assert.Equal("editors", DockGroupValidator.GetEffectiveDockGroup(document));
        Assert.True(DockGroupValidator.ValidateDockingGroups(sameGroup, document));
        Assert.False(DockGroupValidator.ValidateDockingGroups(otherGroup, document));
        AssertTree(root, new HashSet<IDockable>());
    }

    [Theory]
    [InlineData(DockOperation.Left)]
    [InlineData(DockOperation.Bottom)]
    public void Reusing_Inactive_Root_Region_Activates_The_Split_Region(DockOperation operation)
    {
        Factory factory = new();
        DocumentDock documents = new();
        ProportionalDock target = new()
        {
            IsCollapsable = false,
            Orientation = operation == DockOperation.Left ? Orientation.Horizontal : Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(documents), ActiveDockable = documents
        };
        DocumentDock previous = new();
        RootDock root = new()
        {
            VisibleDockables = factory.CreateList<IDockable>(previous, target),
            ActiveDockable = previous, DefaultDockable = previous
        };
        factory.InitLayout(root);
        ToolDock incoming = new();

        factory.SplitToDock(target, incoming, operation);

        Assert.Same(target, root.ActiveDockable);
        Assert.Same(previous, root.DefaultDockable);
        Assert.Same(incoming, target.ActiveDockable);
        AssertTree(root, new HashSet<IDockable>());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    public void Root_Regions_With_Constraints_Are_Not_Discarded(int policyKind)
    {
        Factory factory = new();
        DocumentDock documents = new();
        ProportionalDock region = CreateSingletonWrapper(factory, documents);
        if (policyKind == 0) region.DockCapabilityPolicy = new DockCapabilityPolicy { CanDrop = false };
        else if (policyKind == 1) region.DockCapabilityOverrides = new DockCapabilityOverrides { CanFloat = false };
        else if (policyKind == 2) region.EnableGlobalDocking = false;
        else if (policyKind == 3) region.MinWidth = 400;
        else if (policyKind == 4) region.MaxWidth = 500;
        else if (policyKind == 5) region.MinHeight = 300;
        else if (policyKind == 6) region.MaxHeight = 400;
        else if (policyKind == 7) region.CanDrop = false;
        else if (policyKind == 8) region.CanDrag = false;
        else if (policyKind == 9) region.AllowedDockOperations = DockOperationMask.Bottom;
        else region.AllowedDropOperations = DockOperationMask.Bottom;
        RootDock root = new() { VisibleDockables = factory.CreateList<IDockable>(region), ActiveDockable = region };
        factory.InitLayout(root);

        factory.SplitToDock(region, new ToolDock(), DockOperation.Bottom);

        Assert.Same(region, documents.Owner);
        Assert.NotNull(region.Owner);
        AssertTree(root, new HashSet<IDockable>());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Grouped_Window_Edge_Roundtrips_Keep_Inherited_Groups_And_A_Bounded_Tree(bool groupOnRoot)
    {
        Factory factory = new();
        DocumentDock documents = new() { VisibleDockables = factory.CreateList<IDockable>(new Document()) };
        Tool source = new();
        ToolDock pane = new() { VisibleDockables = factory.CreateList<IDockable>(source) };
        ProportionalDock region = new()
        {
            DockGroup = groupOnRoot ? null : "editors", IsCollapsable = false, Orientation = Orientation.Vertical,
            VisibleDockables = factory.CreateList<IDockable>(documents, new ProportionalDockSplitter(), pane)
        };
        RootDock root = new()
        {
            DockGroup = groupOnRoot ? "editors" : null,
            VisibleDockables = factory.CreateList<IDockable>(region), ActiveDockable = region
        };
        factory.InitLayout(root);
        DockService service = new();
        for (int index = 0; index < 20; index++)
        {
            IDock target = Assert.IsAssignableFrom<IDock>(Assert.Single(root.VisibleDockables!));
            Assert.True(DockGroupValidator.ValidateGlobalDocking(source, target));
            Assert.True(service.SplitDockable(source, Assert.IsAssignableFrom<IDock>(source.Owner), target,
                index % 2 == 0 ? DockOperation.Right : DockOperation.Bottom, true));
            Assert.Equal("editors", DockGroupValidator.GetEffectiveDockGroup(documents));
            Assert.Equal("editors", DockGroupValidator.GetEffectiveDockGroup(source));
            HashSet<IDockable> visited = new();
            AssertTree(root, visited);
            int proportionalCount = 0;
            foreach (IDockable dockable in visited)
                if (dockable is IProportionalDock) proportionalCount++;
            Assert.InRange(proportionalCount, 1, 2);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Application_And_Restored_Regions_Keep_Their_Identity_During_Future_Roundtrips(bool restored)
    {
        Factory factory = new();
        DocumentDock documents = new() { VisibleDockables = factory.CreateList<IDockable>(new Document()) };
        ProportionalDock region = restored
            ? CreateSingletonWrapper(new Factory(), documents)
            : new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(documents) };
        region.Id = "ApplicationRegion";
        region.Context = new object();
        object context = region.Context;
        region.Orientation = Orientation.Vertical;
        region.ActiveDockable = documents;
        region.DefaultDockable = documents;
        RootDock root = new()
        {
            VisibleDockables = factory.CreateList<IDockable>(region), ActiveDockable = region, DefaultDockable = region
        };
        factory.InitLayout(root);
        Tool tool = new();
        ToolDock incoming = new() { VisibleDockables = factory.CreateList<IDockable>(tool), ActiveDockable = tool };
        factory.SplitToDock(region, incoming, DockOperation.Right);
        DockService service = new();

        for (int index = 0; index < 20; index++)
        {
            Assert.Same(region, factory.FindDockable(root, dockable => dockable.Id == "ApplicationRegion"));
            Assert.Same(context, region.Context);
            Assert.Same(documents, region.DefaultDockable);
            Assert.Same(region, documents.Owner);
            HashSet<IDockable> visited = new();
            AssertTree(root, visited);
            int proportionalCount = 0;
            foreach (IDockable dockable in visited)
                if (dockable is IProportionalDock) proportionalCount++;
            Assert.InRange(proportionalCount, 1, 2);
            IDock target = Assert.IsAssignableFrom<IDock>(Assert.Single(root.VisibleDockables!));
            Assert.True(service.SplitDockable(tool, Assert.IsAssignableFrom<IDock>(tool.Owner), target,
                index % 2 == 0 ? DockOperation.Bottom : DockOperation.Right, true));
        }
    }

    [Fact]
    public void Factory_Does_Not_Retain_Unused_Generated_Wrappers()
    {
        Factory factory = new();
        WeakReference wrapper = CreateUnusedSplit(factory);
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        Assert.False(wrapper.IsAlive);
        GC.KeepAlive(factory);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CreateUnusedSplit(Factory factory)
    {
        return new WeakReference(factory.CreateSplitLayout(new DocumentDock(), new ToolDock(), DockOperation.Right));
    }

    private static ProportionalDock CreateSingletonWrapper(Factory factory, IDock child)
    {
        ToolDock temporary = new();
        ProportionalDock wrapper = Assert.IsType<ProportionalDock>(factory.CreateSplitLayout(child, temporary, DockOperation.Right));
        wrapper.VisibleDockables = factory.CreateList<IDockable>(child);
        wrapper.ActiveDockable = child;
        wrapper.DefaultDockable = child;
        temporary.Owner = null;
        return wrapper;
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
