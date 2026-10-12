using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Avalonia.Core;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class FactoryPinnedRootTests
{
    [AvaloniaTheory]
    [InlineData(Alignment.Left, false)]
    [InlineData(Alignment.Top, false)]
    [InlineData(Alignment.Right, false)]
    [InlineData(Alignment.Bottom, false)]
    [InlineData(Alignment.Left, true)]
    [InlineData(Alignment.Top, true)]
    [InlineData(Alignment.Right, true)]
    [InlineData(Alignment.Bottom, true)]
    public void Pinning_From_Another_Root_Preserves_The_Pin_Root_And_Return_Dock_Across_Previews(Alignment alignment, bool preview)
    {
        RootPinFactory factory = new();
        RootDock root = Root(factory);
        RootDock floating = Root(factory);
        DockWindow window = new() { Owner = root, Layout = floating, Factory = factory };
        root.Windows!.Add(window);
        floating.Window = window;
        ToolDock pane = new() { Alignment = alignment, VisibleDockables = factory.CreateList<IDockable>() };
        Tool tool = new() { CanPin = false };
        factory.AddDockable(floating, pane);
        factory.AddDockable(pane, tool);
        int pinned = 0;
        int unpinned = 0;
        factory.DockablePinned += (_, args) => { Assert.Same(tool, args.Dockable); pinned++; };
        factory.DockableUnpinned += (_, args) => { Assert.Same(tool, args.Dockable); unpinned++; };

        factory.TogglePin(tool, root, null);

        Assert.Empty(pane.VisibleDockables!);
        Assert.Same(root, tool.Owner);
        Assert.Same(pane, tool.OriginalOwner);
        Assert.True(factory.IsDockablePinned(tool, root));
        Assert.False(factory.IsDockablePinned(tool, floating));
        factory.UnpinDockable(tool);
        Assert.True(factory.IsDockablePinned(tool, root));
        if (preview)
        {
            for (int cycle = 0; cycle < 2; cycle++)
            {
                factory.PreviewPinnedDockable(tool);
                Assert.Same(tool, root.PinnedDock!.ActiveDockable);
                factory.HidePreviewingDockables(root);
                Assert.Null(root.PinnedDock);
                Assert.Same(root, tool.Owner);
                Assert.Same(pane, tool.OriginalOwner);
            }
        }
        factory.TogglePin(tool, root, pane);

        Assert.Same(pane, tool.Owner);
        Assert.Same(tool, Assert.Single(pane.VisibleDockables!));
        Assert.False(factory.IsDockablePinned(tool, root));
        Assert.Null(tool.OriginalOwner);
        Assert.Equal(1, pinned);
        Assert.Equal(1, unpinned);
    }

    [AvaloniaFact]
    public void An_Explicit_Return_Dock_Takes_Precedence_Over_The_Native_Alignment_Fallback()
    {
        RootPinFactory factory = new();
        RootDock root = Root(factory);
        ToolDock source = new() { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        ToolDock destination = new() { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        Tool tool = new();
        factory.AddDockable(root, source);
        factory.AddDockable(root, destination);
        factory.AddDockable(source, tool);
        factory.PinDockable(tool);

        factory.TogglePin(tool, root, destination);

        Assert.Empty(source.VisibleDockables!);
        Assert.Same(tool, Assert.Single(destination.VisibleDockables!));
        Assert.Same(destination, tool.Owner);
    }

    [AvaloniaTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Hiding_With_Pin_Disabled_Removes_The_Pin_And_Any_Preview(bool preview)
    {
        RootPinFactory factory = new();
        RootDock root = Root(factory);
        ToolDock pane = new() { VisibleDockables = factory.CreateList<IDockable>() };
        Tool tool = new();
        factory.AddDockable(root, pane);
        factory.AddDockable(pane, tool);
        factory.PinDockable(tool);
        if (preview) factory.PreviewPinnedDockable(tool);
        tool.CanPin = false;

        factory.HideDockable(tool);

        Assert.False(factory.IsDockablePinned(tool));
        Assert.Null(root.PinnedDock);
        Assert.Same(tool, Assert.Single(root.HiddenDockables!));
        Assert.Empty(pane.VisibleDockables!);
        Assert.Same(root, tool.Owner);
    }

    private static RootDock Root(Factory factory) => new()
    {
        Factory = factory,
        VisibleDockables = factory.CreateList<IDockable>(),
        HiddenDockables = factory.CreateList<IDockable>(),
        Windows = factory.CreateList<IDockWindow>()
    };

    private sealed class RootPinFactory : Factory
    {
        internal void TogglePin(IDockable dockable, IRootDock root, IToolDock? destination) =>
            PinDockableCore(dockable, root, destination);
    }
}
