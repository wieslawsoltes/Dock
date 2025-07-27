using System.Linq;
using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

public class PinnedDockDragDropTests
{
    #region Helper Methods

    private (Factory factory, RootDock root, ToolDock toolDock, Tool tool) CreateTestLayout(Alignment alignment = Alignment.Left)
    {
        var factory = new Factory();
        var root = new RootDock
        {
            VisibleDockables = factory.CreateList<IDockable>(),
            LeftPinnedDockables = factory.CreateList<IDockable>(),
            RightPinnedDockables = factory.CreateList<IDockable>(),
            TopPinnedDockables = factory.CreateList<IDockable>(),
            BottomPinnedDockables = factory.CreateList<IDockable>()
        };
        root.Factory = factory;

        var toolDock = new ToolDock 
        { 
            Alignment = alignment, 
            VisibleDockables = factory.CreateList<IDockable>() 
        };
        factory.AddDockable(root, toolDock);

        var tool = new Tool { CanDrag = true, CanDrop = true };
        factory.AddDockable(toolDock, tool);

        return (factory, root, toolDock, tool);
    }

    private Tool CreatePinnedTool(Factory factory, RootDock root, Alignment alignment)
    {
        var toolDock = new ToolDock 
        { 
            Alignment = alignment, 
            VisibleDockables = factory.CreateList<IDockable>() 
        };
        factory.AddDockable(root, toolDock);

        var tool = new Tool { CanDrag = true, CanDrop = true };
        factory.AddDockable(toolDock, tool);
        factory.PinDockable(tool);

        return tool;
    }

    #endregion

    #region Unpinned to Pinned Tool Drop Tests

    [AvaloniaFact]
    public void ValidateTool_UnpinnedToLeftPinned_SetsCorrectAlignment()
    {
        var (factory, root, toolDock, sourceTool) = CreateTestLayout(Alignment.Right);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Left);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.Contains(sourceTool, root.LeftPinnedDockables!);
        Assert.Equal(Alignment.Left, ((IToolDock)sourceTool.Owner!).Alignment);
    }

    [AvaloniaFact]
    public void ValidateTool_UnpinnedToRightPinned_SetsCorrectAlignment()
    {
        var (factory, root, toolDock, sourceTool) = CreateTestLayout(Alignment.Left);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Right);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.Contains(sourceTool, root.RightPinnedDockables!);
        Assert.Equal(Alignment.Right, ((IToolDock)sourceTool.Owner!).Alignment);
    }

    [AvaloniaFact]
    public void ValidateTool_UnpinnedToTopPinned_SetsCorrectAlignment()
    {
        var (factory, root, toolDock, sourceTool) = CreateTestLayout(Alignment.Left);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Top);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.Contains(sourceTool, root.TopPinnedDockables!);
        Assert.Equal(Alignment.Top, ((IToolDock)sourceTool.Owner!).Alignment);
    }

    [AvaloniaFact]
    public void ValidateTool_UnpinnedToBottomPinned_SetsCorrectAlignment()
    {
        var (factory, root, toolDock, sourceTool) = CreateTestLayout(Alignment.Left);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Bottom);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.Contains(sourceTool, root.BottomPinnedDockables!);
        Assert.Equal(Alignment.Bottom, ((IToolDock)sourceTool.Owner!).Alignment);
    }

    #endregion

    #region Cross-Pinned-Side Move Tests

    [AvaloniaFact]
    public void ValidateTool_LeftPinnedToRightPinned_MovesCorrectly()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = CreatePinnedTool(factory, root, Alignment.Left);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Right);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.DoesNotContain(sourceTool, root.LeftPinnedDockables!);
        Assert.Contains(sourceTool, root.RightPinnedDockables!);
    }

    [AvaloniaFact]
    public void ValidateTool_RightPinnedToLeftPinned_MovesCorrectly()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = CreatePinnedTool(factory, root, Alignment.Right);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Left);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.DoesNotContain(sourceTool, root.RightPinnedDockables!);
        Assert.Contains(sourceTool, root.LeftPinnedDockables!);
    }

    [AvaloniaFact]
    public void ValidateTool_TopPinnedToBottomPinned_MovesCorrectly()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = CreatePinnedTool(factory, root, Alignment.Top);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Bottom);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.DoesNotContain(sourceTool, root.TopPinnedDockables!);
        Assert.Contains(sourceTool, root.BottomPinnedDockables!);
    }

    [AvaloniaFact]
    public void ValidateTool_BottomPinnedToTopPinned_MovesCorrectly()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = CreatePinnedTool(factory, root, Alignment.Bottom);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Top);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(result);
        Assert.True(factory.IsDockablePinned(sourceTool));
        Assert.DoesNotContain(sourceTool, root.BottomPinnedDockables!);
        Assert.Contains(sourceTool, root.TopPinnedDockables!);
    }

    #endregion

    #region Same-Side Reordering Tests

    [AvaloniaFact]
    public void ValidateTool_SameSidePinned_PreventNormalDocking()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = CreatePinnedTool(factory, root, Alignment.Left);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Left);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
        // Both should still be in the same collection
        Assert.Contains(sourceTool, root.LeftPinnedDockables!);
        Assert.Contains(targetTool, root.LeftPinnedDockables!);
    }

    #endregion

    #region Size Conflict Tests

    [AvaloniaFact]
    public void ValidateTool_SizeConflict_ReturnsFalse()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = new Tool 
        { 
            CanDrag = true, 
            CanDrop = true, 
            MinWidth = 100, 
            MaxWidth = 100 
        };
        var targetTool = new Tool 
        { 
            CanDrag = true, 
            CanDrop = true, 
            MinWidth = 200, 
            MaxWidth = 200 
        };
        var manager = new DockManager(new DockService()) { PreventSizeConflicts = true };

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
    }

    #endregion

    #region Preview Dock Alignment Tests

    [AvaloniaFact]
    public void PreviewPinnedDockable_LeftPinnedTool_ShowsLeftAlignment()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var pinnedTool = CreatePinnedTool(factory, root, Alignment.Left);

        factory.PreviewPinnedDockable(pinnedTool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Left, root.PinnedDock!.Alignment);
        Assert.Contains(pinnedTool, root.PinnedDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_RightPinnedTool_ShowsRightAlignment()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var pinnedTool = CreatePinnedTool(factory, root, Alignment.Right);

        factory.PreviewPinnedDockable(pinnedTool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Right, root.PinnedDock!.Alignment);
        Assert.Contains(pinnedTool, root.PinnedDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_AfterCrossSideMove_ShowsCorrectAlignment()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = CreatePinnedTool(factory, root, Alignment.Left);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Right);
        var manager = new DockManager(new DockService());

        // Move from left to right
        manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        // Preview should now show on the right side
        factory.PreviewPinnedDockable(sourceTool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Right, root.PinnedDock!.Alignment);
        Assert.Contains(sourceTool, root.PinnedDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void PreviewPinnedDockable_UnpinnedTool_UsesOwnerAlignment()
    {
        var (factory, root, toolDock, tool) = CreateTestLayout(Alignment.Bottom);

        factory.PreviewPinnedDockable(tool);

        Assert.NotNull(root.PinnedDock);
        Assert.Equal(Alignment.Bottom, root.PinnedDock!.Alignment);
        Assert.Contains(tool, root.PinnedDock.VisibleDockables!);
    }

    #endregion

    #region Validation Without Execution Tests

    [AvaloniaFact]
    public void ValidateTool_UnpinnedToPinned_ValidationOnly()
    {
        var (factory, root, toolDock, sourceTool) = CreateTestLayout();
        var targetTool = CreatePinnedTool(factory, root, Alignment.Right);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.True(result);
        // Should not actually pin when bExecute is false
        Assert.False(factory.IsDockablePinned(sourceTool));
        Assert.Contains(sourceTool, toolDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void ValidateTool_CrossSideMove_ValidationOnly()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = CreatePinnedTool(factory, root, Alignment.Left);
        var targetTool = CreatePinnedTool(factory, root, Alignment.Right);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.True(result);
        // Should not actually move when bExecute is false
        Assert.Contains(sourceTool, root.LeftPinnedDockables!);
        Assert.DoesNotContain(sourceTool, root.RightPinnedDockables!);
    }

    #endregion

    #region Edge Cases

    [AvaloniaFact]
    public void ValidateTool_SourceCannotDrag_ReturnsFalse()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var sourceTool = new Tool { CanDrag = false, CanDrop = true };
        var targetTool = CreatePinnedTool(factory, root, Alignment.Right);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateTool_TargetCannotDrop_ReturnsFalse()
    {
        var (factory, root, toolDock, sourceTool) = CreateTestLayout();
        var targetTool = new Tool { CanDrag = true, CanDrop = false };
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.False(result);
    }

    [AvaloniaFact]
    public void ValidateTool_UnpinnedToUnpinned_UsesNormalDocking()
    {
        var (factory, root, toolDock1, sourceTool) = CreateTestLayout();
        var (_, _, toolDock2, targetTool) = CreateTestLayout();
        factory.AddDockable(root, toolDock2);
        var manager = new DockManager(new DockService());

        var result = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: false);

        Assert.True(result);
        // Should use normal docking, not pinning logic
        Assert.False(factory.IsDockablePinned(sourceTool));
    }

    #endregion

    #region Multiple Tool Cross-Side Move Tests

    [AvaloniaFact]
    public void ValidateTool_MultipleCrossSideMoves_WorksCorrectly()
    {
        var (factory, root, _, _) = CreateTestLayout();
        var tool1 = CreatePinnedTool(factory, root, Alignment.Left);
        var tool2 = CreatePinnedTool(factory, root, Alignment.Left);
        var rightTarget = CreatePinnedTool(factory, root, Alignment.Right);
        var manager = new DockManager(new DockService());

        // Move first tool from left to right
        var result1 = manager.ValidateTool(tool1, rightTarget, DragAction.Move, DockOperation.Fill, bExecute: true);
        Assert.True(result1);
        Assert.Contains(tool1, root.RightPinnedDockables!);
        Assert.DoesNotContain(tool1, root.LeftPinnedDockables!);

        // Move second tool from left to right
        var result2 = manager.ValidateTool(tool2, rightTarget, DragAction.Move, DockOperation.Fill, bExecute: true);
        Assert.True(result2);
        Assert.Contains(tool2, root.RightPinnedDockables!);
        Assert.DoesNotContain(tool2, root.LeftPinnedDockables!);

        // Both tools should now be in right collection
        Assert.Equal(2, root.RightPinnedDockables!.Where(t => t == tool1 || t == tool2).Count());
    }

    #endregion
} 