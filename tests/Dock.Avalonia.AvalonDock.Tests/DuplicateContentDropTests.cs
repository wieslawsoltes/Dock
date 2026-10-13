using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.AvalonDock.Tests;

public class DuplicateContentDropTests
{
    public static IEnumerable<object?[]> Identities()
    {
        foreach (int kind in new[] { 0, 1, 2 })
        {
            yield return new object?[] { kind, "title", "id", "title", "id", true };
            yield return new object?[] { kind, null, null, null, null, true };
            yield return new object?[] { kind, "", "", "", "", true };
            yield return new object?[] { kind, "title", "id", "title", "other", false };
            yield return new object?[] { kind, "title", "id", "other", "id", false };
            yield return new object?[] { kind, "Title", "id", "title", "id", false };
            yield return new object?[] { kind, "title", "Id", "title", "id", false };
            yield return new object?[] { kind, null, "id", "", "id", false };
            yield return new object?[] { kind, "title", null, "title", "", false };
        }
    }

    [AvaloniaTheory]
    [MemberData(nameof(Identities))]
    public void Into_Drops_Compare_Both_Public_Strings_With_Ordinal_Null_Semantics(
        int kind, string? title, string? id, string? incomingTitle, string? incomingId, bool duplicate)
    {
        Fixture fixture = new(kind);
        fixture.Existing.Title = title!;
        fixture.Existing.ContentId = id;
        fixture.Incoming.Title = incomingTitle!;
        fixture.Incoming.ContentId = incomingId;
        IDockDropPolicy policy = (IDockDropPolicy)fixture.Manager.DockFactory;
        Assert.True(policy.CanDrop(fixture.Incoming.DockModel, fixture.Destination.DockModel, DockOperation.Fill));
        fixture.AllowDuplicates(false);

        Assert.Equal(!duplicate, policy.CanDrop(fixture.Incoming.DockModel, fixture.Destination.DockModel, DockOperation.Fill));
        Assert.Equal(!duplicate, policy.CanDrop(fixture.Incoming.DockModel, fixture.Existing.DockModel, DockOperation.Fill));
        // Application payload identity is not part of AvalonDock's duplicate key.
        fixture.Incoming.Content = fixture.Existing.Content;
        Assert.Equal(!duplicate, policy.CanDrop(fixture.Incoming.DockModel, fixture.Destination.DockModel, DockOperation.Fill));
    }

    [AvaloniaTheory]
    [InlineData(0, false)]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(2, true)]
    public void Visibility_Validation_And_Execution_Reject_A_Duplicate_Without_Mutating_The_Layout(int kind, bool floating)
    {
        Fixture fixture = new(kind);
        fixture.AllowDuplicates(false);
        if (floating) fixture.Incoming.Float();
        ILayoutContainer? parent = fixture.Incoming.Parent;
        IDockable? owner = fixture.Incoming.DockModel.Owner;
        LayoutContent? active = fixture.Manager.Layout.ActiveContent;
        DockManager engine = new(new DockService());
        foreach (IDockable target in new[] { fixture.Destination.DockModel, fixture.Existing.DockModel })
        {
            Assert.False(engine.IsDockTargetVisible(fixture.Incoming.DockModel, target, DockOperation.Fill));
            Assert.False(engine.ValidateDockable(fixture.Incoming.DockModel, target, DragAction.Move, DockOperation.Fill, false));
            Assert.False(engine.ValidateDockable(fixture.Incoming.DockModel, target, DragAction.Move, DockOperation.Fill, true));
            Assert.Same(parent, fixture.Incoming.Parent);
            Assert.Same(owner, fixture.Incoming.DockModel.Owner);
            Assert.Same(active, fixture.Manager.Layout.ActiveContent);
        }

        fixture.AllowDuplicates(true);
        Assert.True(engine.IsDockTargetVisible(fixture.Incoming.DockModel, fixture.Destination.DockModel, DockOperation.Fill));
        Assert.True(engine.ValidateDockable(fixture.Incoming.DockModel, fixture.Destination.DockModel, DragAction.Move, DockOperation.Fill, true));
        Assert.Same(fixture.Destination, fixture.Incoming.Parent);
        Assert.Equal(2, ((ILayoutContainer)fixture.Destination).ChildrenCount);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Duplicate_Policy_Does_Not_Block_Splits_Or_Direct_Model_Edits(int kind)
    {
        Fixture fixture = new(kind);
        fixture.AllowDuplicates(false);
        IDockDropPolicy policy = (IDockDropPolicy)fixture.Manager.DockFactory;
        foreach (DockOperation operation in new[] { DockOperation.Left, DockOperation.Top, DockOperation.Right, DockOperation.Bottom,
                     DockOperation.OuterLeft, DockOperation.OuterTop, DockOperation.OuterRight, DockOperation.OuterBottom, DockOperation.Window })
            Assert.True(policy.CanDrop(fixture.Incoming.DockModel, fixture.Destination.DockModel, operation));

        if (fixture.Destination is LayoutDocumentPane documents) documents.Children.Add(fixture.Incoming);
        else ((LayoutAnchorablePane)fixture.Destination).Children.Add((LayoutAnchorable)fixture.Incoming);
        Assert.Same(fixture.Destination, fixture.Incoming.Parent);
        Assert.Equal(2, ((ILayoutContainer)fixture.Destination).ChildrenCount);
    }

    [AvaloniaTheory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void Changes_To_Duplicate_Metadata_Affect_The_Next_Validation(int kind)
    {
        Fixture fixture = new(kind);
        fixture.AllowDuplicates(false);
        IDockDropPolicy policy = (IDockDropPolicy)fixture.Manager.DockFactory;
        bool CanDrop() => policy.CanDrop(fixture.Incoming.DockModel, fixture.Destination.DockModel, DockOperation.Fill);
        Assert.False(CanDrop());
        fixture.Incoming.Title = "different";
        Assert.True(CanDrop());
        fixture.Existing.Title = "different";
        Assert.False(CanDrop());
        fixture.Existing.ContentId = "different";
        Assert.True(CanDrop());
        fixture.Incoming.ContentId = "different";
        Assert.False(CanDrop());
        fixture.AllowDuplicates(true);
        Assert.True(CanDrop());
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Ancestor_And_Source_Options_Do_Not_Replace_The_Destination_Pane_Option(bool tools)
    {
        Fixture fixture = new(tools ? 1 : 0);
        fixture.Manager.Layout.RootPanel.AllowDuplicateContent = false;
        if (tools)
        {
            ((LayoutAnchorablePane)fixture.Source).AllowDuplicateContent = false;
            LayoutAnchorablePaneGroup group = new((LayoutAnchorablePane)fixture.Destination) { AllowDuplicateContent = false };
            fixture.Manager.Layout.RootPanel.Children.Add(group);
        }
        else
        {
            ((LayoutDocumentPane)fixture.Source).AllowDuplicateContent = false;
            LayoutDocumentPaneGroup group = new((LayoutDocumentPane)fixture.Destination) { AllowDuplicateContent = false };
            fixture.Manager.Layout.RootPanel.Children.Add(group);
        }
        IDockDropPolicy policy = (IDockDropPolicy)fixture.Manager.DockFactory;
        Assert.True(policy.CanDrop(fixture.Incoming.DockModel, fixture.Destination.DockModel, DockOperation.Fill));
        fixture.AllowDuplicates(false);
        Assert.False(policy.CanDrop(fixture.Incoming.DockModel, fixture.Destination.DockModel, DockOperation.Fill));
    }

    [AvaloniaTheory]
    [InlineData(0, 0)]
    [InlineData(0, 1)]
    [InlineData(0, 2)]
    [InlineData(1, 0)]
    [InlineData(1, 1)]
    [InlineData(1, 2)]
    [InlineData(2, 0)]
    [InlineData(2, 1)]
    [InlineData(2, 2)]
    public void A_Nonselected_Duplicate_In_A_Dragged_Subtree_Rejects_The_Entire_Drop(int shape, int kind)
    {
        DockingManager manager = new();
        LayoutContent unique = kind == 1 ? new LayoutAnchorable() : new LayoutDocument();
        LayoutContent duplicate = kind == 0 ? new LayoutDocument() : new LayoutAnchorable();
        unique.Title = unique.ContentId = "unique"; unique.Content = new object();
        duplicate.Title = duplicate.ContentId = "same"; duplicate.Content = new object();
        LayoutElement source = kind == 1
            ? new LayoutAnchorablePane((LayoutAnchorable)unique) { Children = { (LayoutAnchorable)duplicate }, SelectedContentIndex = 0 }
            : new LayoutDocumentPane(unique) { Children = { duplicate }, SelectedContentIndex = 0 };
        LayoutElement group = kind == 1
            ? new LayoutAnchorablePaneGroup() { Children = { new LayoutAnchorablePaneGroup((LayoutAnchorablePane)source) } }
            : new LayoutDocumentPaneGroup() { Children = { new LayoutDocumentPaneGroup((LayoutDocumentPane)source) } };
        LayoutElement destination = kind == 1
            ? new LayoutAnchorablePane(new LayoutAnchorable { Title = "same", ContentId = "same", Content = new object() }) { AllowDuplicateContent = false }
            : new LayoutDocumentPane(new LayoutDocument { Title = "same", ContentId = "same", Content = new object() }) { AllowDuplicateContent = false };
        manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)group);
        manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)destination);
        IDockable incoming = shape == 0 ? source.DockModel : group.DockModel;
        if (shape == 2)
        {
            manager.DockFactory.FloatDockable(group.DockModel);
            incoming = Assert.Single(manager.Layout.FloatingWindows).DockModel;
        }
        IDockDropPolicy policy = (IDockDropPolicy)manager.DockFactory;
        Assert.False(policy.CanDrop(incoming, destination.DockModel, DockOperation.Fill));
        duplicate.ContentId = "changed";
        Assert.True(policy.CanDrop(incoming, destination.DockModel, DockOperation.Fill));
        Assert.Same(unique, ((ILayoutContentSelector)source).SelectedContent);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_Created_Content_Uses_Its_Public_Metadata_Projection(bool tools)
    {
        Fixture fixture = new(tools ? 1 : 0);
        fixture.AllowDuplicates(false);
        IDockable native = tools ? fixture.Manager.DockFactory.CreateTool() : fixture.Manager.DockFactory.CreateDocument();
        native.Title = "same";
        native.Id = "same";
        fixture.Manager.DockFactory.AddDockable((IDock)fixture.Source.DockModel, native);
        LayoutContent projected = ((ILayoutContainer)fixture.Source).Children.OfType<LayoutContent>().Single(item => ReferenceEquals(item.DockModel, native));
        Assert.Equal("same", projected.ContentId);
        Assert.False(((IDockDropPolicy)fixture.Manager.DockFactory).CanDrop(native, fixture.Destination.DockModel, DockOperation.Fill));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_Item_Is_Not_Its_Own_Duplicate_For_A_Move_Within_The_Pane(bool tools)
    {
        Fixture fixture = new(tools ? 1 : 0);
        fixture.AllowDuplicates(false);
        Assert.True(((IDockDropPolicy)fixture.Manager.DockFactory).CanDrop(fixture.Existing.DockModel, fixture.Destination.DockModel, DockOperation.Fill));
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Overlay_Into_Selectors_Follow_The_Native_Duplicate_Policy(bool tools)
    {
        Fixture fixture = new(tools ? 1 : 0);
        Window owner = new() { Content = fixture.Manager, Width = 900, Height = 600 };
        owner.Show(); Dispatcher.UIThread.RunJobs(); owner.UpdateLayout();
        Control surface = fixture.Manager.GetVisualDescendants().OfType<Control>()
            .First(control => control is ILayoutControl layoutControl && ReferenceEquals(layoutControl.Model, fixture.Destination));
        DockTarget target = new();
        OverlayWindow overlay = Assert.IsType<OverlayWindow>(DockAdornerWindow.GetFactory(surface)!.CreateWindow(surface, target));
        overlay.Content = target;
        overlay.Width = surface.Bounds.Width; overlay.Height = surface.Bounds.Height;
        overlay.Position = surface.PointToScreen(default);
        overlay.SizeToContent = SizeToContent.Manual;
        overlay.Show(owner); Dispatcher.UIThread.RunJobs(); overlay.UpdateLayout();
        DockManager engine = new(new DockService());
        DockOperation Refresh() => target.GetDockOperation(default, surface, surface, DragAction.Move,
            (_, operation, _, _) => engine.ValidateDockable(fixture.Incoming.DockModel, fixture.Destination.DockModel, DragAction.Move, operation, false),
            (_, operation, _, _) => engine.IsDockTargetVisible(fixture.Incoming.DockModel, fixture.Destination.DockModel, operation));
        DropTargetType into = tools ? DropTargetType.AnchorablePaneDockInside : DropTargetType.DocumentPaneDockInside;
        try
        {
            Refresh();
            Assert.Contains(overlay.DropTargets, value => value.TargetType == into);
            fixture.AllowDuplicates(false);
            Refresh();
            Assert.DoesNotContain(overlay.DropTargets, value => value.TargetType == into);
            Assert.NotEmpty(overlay.DropTargets);
            fixture.Incoming.ContentId = "different";
            Refresh();
            Assert.Contains(overlay.DropTargets, value => value.TargetType == into);
        }
        finally { overlay.Close(); owner.Close(); Dispatcher.UIThread.RunJobs(); }
    }

    private sealed class Fixture
    {
        internal DockingManager Manager { get; } = new();
        internal LayoutContent Incoming { get; }
        internal LayoutContent Existing { get; }
        internal LayoutElement Source { get; }
        internal LayoutElement Destination { get; }

        internal Fixture(int kind)
        {
            Incoming = kind == 0 ? new LayoutDocument() : new LayoutAnchorable();
            Existing = kind == 1 ? new LayoutAnchorable() : new LayoutDocument();
            Incoming.Title = Existing.Title = "same";
            Incoming.ContentId = Existing.ContentId = "same";
            Incoming.Content = new object(); Existing.Content = new object();
            Source = kind == 0 ? new LayoutDocumentPane(Incoming) : new LayoutAnchorablePane((LayoutAnchorable)Incoming);
            Destination = kind == 1 ? new LayoutAnchorablePane((LayoutAnchorable)Existing) : new LayoutDocumentPane(Existing);
            Manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)Source);
            Manager.Layout.RootPanel.Children.Add((ILayoutPanelElement)Destination);
        }

        internal void AllowDuplicates(bool value)
        {
            if (Destination is LayoutDocumentPane documents) documents.AllowDuplicateContent = value;
            else ((LayoutAnchorablePane)Destination).AllowDuplicateContent = value;
        }
    }
}
