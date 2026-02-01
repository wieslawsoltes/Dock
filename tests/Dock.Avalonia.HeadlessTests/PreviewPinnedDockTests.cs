using System.Linq;
using Avalonia.Headless.XUnit;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PreviewPinnedDockTests
{
    private static (Factory factory, RootDock root, ToolDock leftDock, ToolDock rightDock, Tool leftTool, Tool rightTool) CreatePinnedToolPair()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>(),
            RightPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var leftDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        var rightDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, leftDock);
        factory.AddDockable(root, rightDock);

        var leftTool = new Tool { CanDrag = true, CanDrop = true };
        var rightTool = new Tool { CanDrag = true, CanDrop = true };
        factory.AddDockable(leftDock, leftTool);
        factory.AddDockable(rightDock, rightTool);

        factory.PinDockable(leftTool);
        factory.PinDockable(rightTool);

        return (factory, root, leftDock, rightDock, leftTool, rightTool);
    }

    private static (Factory factory, RootDock root, Tool tool) CreateRootWithLeftPinnedTool()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };

        var tool = new Tool();
        root.LeftPinnedDockables.Add(tool);

        factory.InitDockable(root, null);

        return (factory, root, tool);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_Shows_In_PinnedDock_And_Hides()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PreviewPinnedDockable(tool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Right, root.PinnedDock!.Alignment);
        Assert.Contains(tool, root.PinnedDock.VisibleDockables!);
        Assert.Equal(toolDock, tool.OriginalOwner);
        Assert.Equal(root.PinnedDock, tool.Owner);

        factory.HidePreviewingDockables(root);

        Assert.Null(root.PinnedDock);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void HidePreviewingDockables_Keeps_KeepPinnedDockableVisible()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool { KeepPinnedDockableVisible = true };
        factory.AddDockable(toolDock, tool);

        factory.PreviewPinnedDockable(tool);
        factory.HidePreviewingDockables(root);

        Assert.NotNull(root.PinnedDock);
        Assert.Contains(tool, root.PinnedDock!.VisibleDockables!);
        Assert.Equal(root.PinnedDock, tool.Owner);
        Assert.Equal(toolDock, tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_Replaces_KeepVisible_When_Previewing_Another_Pinned()
    {
        var (factory, root, leftDock, rightDock, leftTool, rightTool) = CreatePinnedToolPair();
        leftTool.KeepPinnedDockableVisible = true;

        factory.PreviewPinnedDockable(leftTool);
        factory.PreviewPinnedDockable(rightTool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Right, root.PinnedDock!.Alignment);
        Assert.Contains(rightTool, root.PinnedDock.VisibleDockables!);
        Assert.DoesNotContain(leftTool, root.PinnedDock.VisibleDockables!);
        Assert.Equal(leftDock, leftTool.Owner);
        Assert.Null(leftTool.OriginalOwner);
        Assert.Equal(root.PinnedDock, rightTool.Owner);
        Assert.Equal(rightDock, rightTool.OriginalOwner);
    }

    [AvaloniaFact]
    public void UnpinDockable_Hides_KeepPinnedDockableVisible_Preview()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;
        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool { KeepPinnedDockableVisible = true };
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.PreviewPinnedDockable(tool);

        Assert.NotNull(root.PinnedDock);
        Assert.Contains(tool, root.PinnedDock!.VisibleDockables!);

        factory.UnpinDockable(tool);

        Assert.Null(root.PinnedDock);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.DoesNotContain(tool, root.LeftPinnedDockables!);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_RootOwned_PreservesOwnerChain()
    {
        var (factory, root, tool) = CreateRootWithLeftPinnedTool();

        factory.PreviewPinnedDockable(tool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(root.PinnedDock, tool.Owner);

        factory.HidePreviewingDockables(root);

        Assert.Null(root.PinnedDock);
        Assert.Equal(root, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void UnpinDockable_RootOwned_CreatesToolDock()
    {
        var (factory, root, tool) = CreateRootWithLeftPinnedTool();

        factory.UnpinDockable(tool);

        Assert.DoesNotContain(tool, root.LeftPinnedDockables!);
        var toolDock = factory.Find(root, d => d is ToolDock td && td.Alignment == Alignment.Left)
            .OfType<ToolDock>()
            .FirstOrDefault();
        Assert.NotNull(toolDock);
        Assert.Contains(tool, toolDock!.VisibleDockables!);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void UnpinDockable_RootOwned_FromPreview_ClosesPreview()
    {
        var (factory, root, tool) = CreateRootWithLeftPinnedTool();

        factory.PreviewPinnedDockable(tool);
        Assert.NotNull(root.PinnedDock);

        factory.UnpinDockable(tool);

        Assert.Null(root.PinnedDock);
        Assert.DoesNotContain(tool, root.LeftPinnedDockables!);
        var toolDock = factory.Find(root, d => d is ToolDock td && td.Alignment == Alignment.Left)
            .OfType<ToolDock>()
            .FirstOrDefault();
        Assert.NotNull(toolDock);
        Assert.Contains(tool, toolDock!.VisibleDockables!);
        Assert.Equal(toolDock, tool.Owner);
    }

    [AvaloniaFact]
    public void UnpinDockable_UsesExistingAlignedToolDock()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };

        var toolDock = new ToolDock
        {
            Alignment = Alignment.Left,
            VisibleDockables = factory.CreateList<IDockable>()
        };
        root.VisibleDockables.Add(toolDock);

        var tool = new Tool();
        root.LeftPinnedDockables.Add(tool);

        factory.InitDockable(root, null);

        factory.UnpinDockable(tool);

        Assert.Single(root.VisibleDockables!.OfType<ToolDock>());
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.Equal(toolDock, tool.Owner);
    }

    [AvaloniaFact]
    public void UnpinDockable_PinnedMovedToDifferentSide_UsesTargetAlignment()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>(),
            RightPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var leftDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, leftDock);
        var tool = new Tool();
        factory.AddDockable(leftDock, tool);

        factory.PinDockable(tool);

        root.LeftPinnedDockables!.Remove(tool);
        root.RightPinnedDockables!.Add(tool);

        factory.UnpinDockable(tool);

        Assert.DoesNotContain(tool, root.LeftPinnedDockables!);
        Assert.DoesNotContain(tool, root.RightPinnedDockables!);
        var rightDock = factory.Find(root, d => d is ToolDock td && td.Alignment == Alignment.Right)
            .OfType<ToolDock>()
            .FirstOrDefault();
        Assert.NotNull(rightDock);
        Assert.Contains(tool, rightDock!.VisibleDockables!);
        Assert.Equal(rightDock, tool.Owner);
    }

    [AvaloniaFact]
    public void UnpinDockable_DetachedOwner_CreatesReplacementDock()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var leftDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, leftDock);
        var tool = new Tool();
        factory.AddDockable(leftDock, tool);

        factory.PinDockable(tool);

        root.VisibleDockables!.Remove(leftDock);

        factory.UnpinDockable(tool);

        var replacement = factory.Find(root, d => d is ToolDock td && td.Alignment == Alignment.Left)
            .OfType<ToolDock>()
            .FirstOrDefault();
        Assert.NotNull(replacement);
        Assert.NotSame(leftDock, replacement);
        Assert.Contains(tool, replacement!.VisibleDockables!);
        Assert.Equal(replacement, tool.Owner);
    }

    [AvaloniaFact]
    public void UnpinDockable_Removes_From_All_Pinned_Collections()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>(),
            RightPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var tool = new Tool();
        root.LeftPinnedDockables.Add(tool);
        root.RightPinnedDockables.Add(tool);

        factory.InitDockable(root, null);

        factory.UnpinDockable(tool);

        Assert.DoesNotContain(tool, root.LeftPinnedDockables);
        Assert.DoesNotContain(tool, root.RightPinnedDockables);
    }

    [AvaloniaFact]
    public void HidePreviewingDockables_DetachedOwner_FallsBackToRoot()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.PreviewPinnedDockable(tool);

        root.VisibleDockables!.Remove(toolDock);

        factory.HidePreviewingDockables(root);

        Assert.Null(root.PinnedDock);
        Assert.Equal(root, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void TogglePreviewPinnedDockable_Hides_Pinned_Preview()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.PreviewPinnedDockable(tool);
        factory.TogglePreviewPinnedDockable(tool);

        Assert.Null(root.PinnedDock);
        Assert.Contains(tool, root.LeftPinnedDockables!);
        Assert.DoesNotContain(tool, toolDock.VisibleDockables!);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void TogglePreviewPinnedDockable_Hides_Pinned_Preview_When_CanClose_False()
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Left, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool { CanClose = false };
        factory.AddDockable(toolDock, tool);

        factory.PinDockable(tool);
        factory.PreviewPinnedDockable(tool);
        factory.TogglePreviewPinnedDockable(tool);

        Assert.Null(root.PinnedDock);
        Assert.Contains(tool, root.LeftPinnedDockables!);
        Assert.DoesNotContain(tool, toolDock.VisibleDockables!);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }

    [AvaloniaFact]
    public void TogglePreviewPinnedDockable_Hides_Unpinned_Preview()
    {
        var factory = new Factory();
        var root = new RootDock { VisibleDockables = factory.CreateList<IDockable>() };
        root.Factory = factory;

        var toolDock = new ToolDock { Alignment = Alignment.Right, VisibleDockables = factory.CreateList<IDockable>() };
        factory.AddDockable(root, toolDock);
        var tool = new Tool();
        factory.AddDockable(toolDock, tool);

        factory.PreviewPinnedDockable(tool);
        factory.TogglePreviewPinnedDockable(tool);

        Assert.Null(root.PinnedDock);
        Assert.Contains(tool, toolDock.VisibleDockables!);
        Assert.Equal(toolDock, tool.Owner);
        Assert.Null(tool.OriginalOwner);
    }
}
