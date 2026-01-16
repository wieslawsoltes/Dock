using Avalonia.Collections;
using Avalonia.Headless.XUnit;
using Dock.Model;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using Xunit;

namespace Dock.Avalonia.HeadlessTests;

/// <summary>
/// Execution-time tests for DockGroup validation during drop operations.
/// Verifies that core operations (move/swap/split) respect strict local DockGroup rules.
/// </summary>
public class DockGroupExecutionTests
{
    private static Factory CreateFactory() => new Factory();

    private static (IRootDock root, ToolDock sourceDock, ToolDock targetDock, Tool sourceTool) CreateBasicToolLayout(
        Factory factory,
        string? sourceGroup,
        string? targetGroup,
        string? existingInTargetGroup = null)
    {
        var sourceTool = new Tool { Id = "Source", Title = "Source", DockGroup = sourceGroup };
        var existing = new Tool { Id = "Existing", Title = "Existing", DockGroup = existingInTargetGroup ?? targetGroup };

        var sourceDock = new ToolDock
        {
            Id = "SourceDock",
            Title = "SourceDock",
            DockGroup = sourceGroup,
            VisibleDockables = new AvaloniaList<IDockable> { sourceTool }
        };

        var targetDock = new ToolDock
        {
            Id = "TargetDock",
            Title = "TargetDock",
            DockGroup = targetGroup,
            VisibleDockables = new AvaloniaList<IDockable> { existing }
        };

        var root = factory.CreateRootDock();
        root.VisibleDockables = new AvaloniaList<IDockable> { sourceDock, targetDock };

        // Initialize to wire up Owner and Factory across the hierarchy
        factory.InitLayout(root);

        return (root, sourceDock, targetDock, sourceTool);
    }

    [AvaloniaFact]
    public void Local_Move_SameGroup_Should_Execute()
    {
        var factory = CreateFactory();
        var (root, sourceDock, targetDock, sourceTool) = CreateBasicToolLayout(factory, "GroupA", "GroupA");

        var manager = new DockManager(new DockService());
        var executed = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.True(executed);
        Assert.Same(targetDock, sourceTool.Owner);
        Assert.DoesNotContain(sourceTool, sourceDock.VisibleDockables!);
        Assert.Contains(sourceTool, targetDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void Local_Move_DifferentGroup_Should_Not_Execute()
    {
        var factory = CreateFactory();
        var (root, sourceDock, targetDock, sourceTool) = CreateBasicToolLayout(factory, "GroupA", "GroupB");

        var manager = new DockManager(new DockService());
        var executed = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.False(executed);
        Assert.Same(sourceDock, sourceTool.Owner);
        Assert.Contains(sourceTool, sourceDock.VisibleDockables!);
        Assert.DoesNotContain(sourceTool, targetDock.VisibleDockables!);
    }

    [AvaloniaFact]
    public void Local_Move_MixedGroupedAndUngrouped_Should_Not_Execute()
    {
        var factory = CreateFactory();
        var (root, sourceDock, targetDock, sourceTool) = CreateBasicToolLayout(factory, "GroupA", null);

        var manager = new DockManager(new DockService());
        var executed = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.False(executed);
        Assert.Same(sourceDock, sourceTool.Owner);
    }

    [AvaloniaFact]
    public void Local_Split_Left_Into_NewDock_Should_Execute()
    {
        var factory = CreateFactory();
        var (root, sourceDock, targetDock, sourceTool) = CreateBasicToolLayout(factory, "GroupA", "GroupA");

        var manager = new DockManager(new DockService());
        var executed = manager.ValidateTool(sourceTool, targetDock, DragAction.Move, DockOperation.Left, bExecute: true);

        Assert.True(executed);
        Assert.IsType<ToolDock>(sourceTool.Owner);
        Assert.NotSame(sourceDock, sourceTool.Owner);
        Assert.NotSame(targetDock, sourceTool.Owner);
    }

    [AvaloniaFact]
    public void Local_Move_Onto_TargetDockable_DifferentGroup_Should_Not_Execute()
    {
        var factory = CreateFactory();
        var (root, sourceDock, targetDock, sourceTool) = CreateBasicToolLayout(factory, "GroupA", "GroupB");
        var targetTool = (ITool)targetDock.VisibleDockables![0];

        var manager = new DockManager(new DockService());
        var executed = manager.ValidateTool(sourceTool, targetTool, DragAction.Move, DockOperation.Fill, bExecute: true);

        Assert.False(executed);
        Assert.Same(sourceDock, sourceTool.Owner);
    }

    [AvaloniaFact]
    public void Local_Link_SameGroup_Should_Execute_Swap()
    {
        var factory = CreateFactory();
        var (root, sourceDock, targetDock, sourceTool) = CreateBasicToolLayout(factory, "GroupA", "GroupA");
        var targetTool = (ITool)targetDock.VisibleDockables![0];

        var manager = new DockManager(new DockService());
        var executed = manager.ValidateTool(sourceTool, targetTool, DragAction.Link, DockOperation.Fill, bExecute: true);

        Assert.True(executed);
        // After swap, sourceTool should be owned by targetDock
        Assert.Same(targetDock, sourceTool.Owner);
    }
}
