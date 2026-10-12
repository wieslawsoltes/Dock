using System.Collections.Generic;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Model.UnitTests;

public class DockOuterSplitTests
{
    public static IEnumerable<object[]> Cases()
    {
        foreach (DockOperation operation in new[] { DockOperation.OuterLeft, DockOperation.OuterTop, DockOperation.OuterRight, DockOperation.OuterBottom })
            for (int shape = 0; shape < 3; shape++) yield return new object[] { operation, shape };
    }

    [Theory]
    [MemberData(nameof(Cases))]
    public void Outer_Splits_Preserve_The_Source_Container_And_Target_Region(DockOperation operation, int shape)
    {
        Layout layout = new(shape);
        IDockable source = layout.Source;
        IDockable? oldOwner = source.Owner;
        Assert.True(layout.Engine.IsDockTargetVisible(source, layout.Target, operation));
        Assert.True(layout.Engine.ValidateDockable(source, layout.Target, DragAction.Move, operation, false));
        Assert.Same(oldOwner, source.Owner);
        Assert.True(layout.Engine.ValidateDockable(source, layout.Target, DragAction.Move, operation, true));
        IDock moved = shape == 0 ? Assert.IsType<ToolDock>(source.Owner) : Assert.IsAssignableFrom<IDock>(source);
        if (shape == 0) Assert.Equal(operation switch
        {
            DockOperation.OuterLeft => Alignment.Left, DockOperation.OuterTop => Alignment.Top,
            DockOperation.OuterRight => Alignment.Right, _ => Alignment.Bottom
        }, ((ToolDock)moved).Alignment);
        Assert.Same(layout.Region, layout.Target.Owner);
        Assert.Same(layout.Region.Owner, moved.Owner);
        if (shape > 0)
        {
            Assert.Same(layout.Pane, layout.First.Owner);
            Assert.Equal(new IDockable[] { layout.First, layout.Second }, layout.Pane.VisibleDockables);
        }
        var parent = Assert.IsType<ProportionalDock>(moved.Owner);
        Assert.Equal(operation is DockOperation.OuterLeft or DockOperation.OuterRight ? Orientation.Horizontal : Orientation.Vertical, parent.Orientation);
        int regionIndex = parent.VisibleDockables!.IndexOf(layout.Region), movedIndex = parent.VisibleDockables.IndexOf(moved);
        Assert.Equal(operation is DockOperation.OuterLeft or DockOperation.OuterTop, movedIndex < regionIndex);
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
    public void Rejected_Outer_Splits_Leave_The_Layout_Untouched(int reason)
    {
        Layout layout = new(2);
        switch (reason)
        {
            case 0: layout.Factory.Region = null; break;
            case 1: layout.Source.CanDrag = false; break;
            case 2: layout.Target.CanDrop = false; break;
            case 3: layout.Second.CanDrag = false; break;
            case 4: layout.Source.DockGroup = "different"; break;
            case 5: ((IDockableDockingRestrictions)layout.Source).AllowedDockOperations = DockOperationMask.Right; break;
            case 6: layout.Factory.Region = layout.Pane; break;
            case 7: layout.Region.Factory = null; layout.Source.Factory = null; break;
        }
        IDockable? owner = layout.Source.Owner;
        Assert.False(layout.Engine.IsDockTargetVisible(layout.Source, layout.Target, DockOperation.OuterLeft));
        Assert.False(layout.Engine.ValidateDockable(layout.Source, layout.Target, DragAction.Move, DockOperation.OuterLeft, true));
        Assert.Same(owner, layout.Source.Owner);
        Assert.Equal(new IDockable[] { layout.First, layout.Second }, layout.Pane.VisibleDockables);
        Assert.Same(layout.Region, layout.Target.Owner);
    }

    [Theory]
    [InlineData(DockOperation.OuterLeft, DockOperationMask.Left)]
    [InlineData(DockOperation.OuterTop, DockOperationMask.Top)]
    [InlineData(DockOperation.OuterRight, DockOperationMask.Right)]
    [InlineData(DockOperation.OuterBottom, DockOperationMask.Bottom)]
    public void Outer_Targets_Require_An_Opt_In_Policy_And_Use_Directional_Restrictions(DockOperation operation, DockOperationMask mask)
    {
        Assert.True(mask.Allows(operation));
        Assert.False(DockOperationMask.None.Allows(operation));
        Layout layout = new(0);
        layout.Target.Factory = new Factory();
        Assert.False(layout.Engine.IsDockTargetVisible(layout.Source, layout.Target, operation));
        Assert.False(layout.Engine.ValidateDockable(layout.Source, layout.Target, DragAction.Move, operation, true));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Moving_Content_Out_Of_Its_Own_Region_Requires_Remaining_Content(bool nested, bool hasSibling)
    {
        PolicyFactory factory = new();
        Tool source = new();
        Tool sibling = new();
        ToolDock pane = new() { VisibleDockables = factory.CreateList<IDockable>(source) };
        if (hasSibling) pane.VisibleDockables.Add(sibling);
        IDock region = nested ? new ProportionalDock { VisibleDockables = factory.CreateList<IDockable>(pane) } : pane;
        factory.Region = region;
        ProportionalDock panel = new() { VisibleDockables = factory.CreateList<IDockable>(region) };
        factory.InitLayout(new RootDock { VisibleDockables = factory.CreateList<IDockable>(panel) });
        DockManager engine = new(new DockService());
        Assert.Equal(hasSibling, engine.IsDockTargetVisible(source, pane, DockOperation.OuterLeft));
        Assert.Equal(hasSibling, engine.ValidateDockable(source, pane, DragAction.Move, DockOperation.OuterLeft, true));
        if (hasSibling)
        {
            Assert.Same(region.Owner, source.Owner!.Owner);
            Assert.Same(pane, sibling.Owner);
            Assert.DoesNotContain(source, pane.VisibleDockables);
        }
        else
        {
            Assert.Same(pane, source.Owner);
            Assert.Same(region, Assert.Single(panel.VisibleDockables!));
            Assert.Same(source, Assert.Single(pane.VisibleDockables));
        }
    }

    private sealed class Layout
    {
        internal PolicyFactory Factory { get; } = new();
        internal DockManager Engine { get; } = new(new DockService());
        internal Tool First { get; } = new() { Id = "first", CanDockAsDocument = false };
        internal Tool Second { get; } = new() { Id = "second", CanDockAsDocument = false };
        internal ToolDock Pane { get; }
        internal DocumentDock Target { get; }
        internal ProportionalDock Region { get; }
        internal IDockable Source { get; }
        internal Layout(int shape)
        {
            Pane = new() { VisibleDockables = Factory.CreateList<IDockable>(First, Second) };
            Source = shape switch
            {
                0 => First, 1 => Pane,
                _ => new ProportionalDock { Orientation = Orientation.Vertical, VisibleDockables = Factory.CreateList<IDockable>(Pane,
                    new ToolDock { VisibleDockables = Factory.CreateList<IDockable>(new Tool()) }) }
            };
            Target = new() { VisibleDockables = Factory.CreateList<IDockable>(new Document()) };
            Region = new() { VisibleDockables = Factory.CreateList<IDockable>(Target,
                new DocumentDock { VisibleDockables = Factory.CreateList<IDockable>(new Document()) }) };
            Factory.Region = Region;
            RootDock root = new() { VisibleDockables = Factory.CreateList<IDockable>(new ProportionalDock
                { VisibleDockables = Factory.CreateList<IDockable>(Region, shape == 0 ? Pane : Source) }) };
            Factory.InitLayout(root);
        }
    }

    private sealed class PolicyFactory : Factory, IDockOuterSplitPolicy
    {
        internal IDock? Region { get; set; }
        public IDock? GetOuterSplitTarget(IDockable source, IDockable target, DockOperation operation) => Region;
    }
}
