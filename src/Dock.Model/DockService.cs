using System;
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <inheritdoc />
public class DockService : IDockService
{
    private static void CopyDockGroup(IDockable source, IDockable target)
    {
        var group = DockGroupValidator.GetEffectiveDockGroup(source);
        if (!string.IsNullOrEmpty(group))
        {
            target.DockGroup = group;
        }
    }

    private static void CopyToolDockSettings(IToolDock source, IToolDock target)
    {
        target.IsExpanded = source.IsExpanded;
        target.AutoHide = source.AutoHide;
        target.GripMode = source.GripMode;

        if (source is IToolDockContent sourceContent && target is IToolDockContent targetContent)
        {
            targetContent.ToolTemplate = sourceContent.ToolTemplate;
        }

        if (source is IToolItemsSourceDock sourceItemsDock && target is IToolItemsSourceDock targetItemsDock)
        {
            targetItemsDock.ToolItemContainerTheme = sourceItemsDock.ToolItemContainerTheme;
            targetItemsDock.ToolItemTemplateSelector = sourceItemsDock.ToolItemTemplateSelector;
        }
    }

    private static bool IsValidMove(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, IDockable targetDockable)
    {
        if (targetDockable is IDock)
        {
            return false;
        }

        if (targetDock.VisibleDockables is not null)
        {
            if (!targetDock.VisibleDockables.Contains(targetDockable))
            {
                return false;
            }
        }

        if (sourceDockableOwner.VisibleDockables is not null)
        {
            if (!sourceDockableOwner.VisibleDockables.Contains(sourceDockable))
            {
                return false;
            }
        }

        return true;
    }

    private IDockable? GetDockable(IDock dock)
    {
        var targetDockable = dock.VisibleDockables?.LastOrDefault();
        return targetDockable switch
        {
            null => null,
            IDock childDock => GetDockable(childDock),
            _ => targetDockable
        };
    }

    /// <inheritdoc />
    public bool MoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
    {
        if (sourceDockableOwner == targetDock)
        {
            if (targetDock.VisibleDockables?.Count == 1)
            {
                return false;
            }
        }

        var targetDockable = GetDockable(targetDock);
        if (targetDockable is null)
        {
            // For global docking operations (no specific target dockable), use global validation
            // This allows non-grouped dockables to dock anywhere globally
            if (!DockGroupValidator.ValidateGlobalDocking(sourceDockable, targetDock))
            {
                return false;
            }

            if (bExecute)
            {
                if (sourceDockableOwner.Factory is { } factory)
                {
                    factory.MoveDockable(sourceDockableOwner, targetDock, sourceDockable, null);
                }
            }
            return true;
        }

        if (targetDockable.Owner is not IDock targetDockableOwner)
        {
            return false;
        }

        targetDock = targetDockableOwner;

        if (!IsValidMove(sourceDockable, sourceDockableOwner, targetDock, targetDockable))
        {
            return false;
        }

        // Local docking: enforce strict group compatibility against the entire target dock
        if (!DockGroupValidator.ValidateDockingGroupsInDock(sourceDockable, targetDock))
        {
            return false;
        }

        if (bExecute)
        {
            if (sourceDockableOwner.Factory is { } factory)
            {
                factory.MoveDockable(sourceDockableOwner, targetDock, sourceDockable, targetDockable);
            }
        }
        return true;
    }

    /// <inheritdoc />
    public bool SwapDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
    {
        var targetDockable = targetDock.ActiveDockable;
        if (targetDockable is null)
        {
            targetDockable = targetDock.VisibleDockables?.LastOrDefault();
            if (targetDockable is null)
            {
                return false;
            }
        }

        if (!IsValidMove(sourceDockable, sourceDockableOwner, targetDock, targetDockable))
        {
            return false;
        }

        // Local docking: enforce strict group compatibility against the entire target dock
        if (!DockGroupValidator.ValidateDockingGroupsInDock(sourceDockable, targetDock))
        {
            return false;
        }

        if (bExecute)
        {
            if (sourceDockableOwner.Factory is { } factory)
            {
                factory.SwapDockable(sourceDockableOwner, targetDock, sourceDockable, targetDockable);
            }
        }
        return true;
    }

    private void SplitToolDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation)
    {
        if (targetDock.Factory is not { } factory)
        {
            return;
        }

        var targetToolDock = factory.CreateToolDock();
        targetToolDock.Title = nameof(IToolDock);
        targetToolDock.Alignment = operation.ToAlignment();
        targetToolDock.VisibleDockables = factory.CreateList<IDockable>();
        if (targetDock is IToolDock targetToolDockSource)
        {
            CopyToolDockSettings(targetToolDockSource, targetToolDock);
        }
        else if (sourceDockableOwner is IToolDock sourceToolDock)
        {
            CopyToolDockSettings(sourceToolDock, targetToolDock);
        }
        CopyDockGroup(sourceDockable, targetToolDock);
        
        // Local split into a new (empty) dock: allow per Empty Dock Rule
        if (!DockGroupValidator.ValidateDockingGroupsInDock(sourceDockable, targetToolDock))
        {
            return;
        }
        
        factory.MoveDockable(sourceDockableOwner, targetToolDock, sourceDockable, null);
        factory.SplitToDock(targetDock, targetToolDock, operation);
    }

    private void SplitDocumentDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation)
    {
        if (targetDock.Factory is not { } factory)
        {
            return;
        }

        var targetDocumentDock = factory.CreateDocumentDock();
        targetDocumentDock.Title = nameof(IDocumentDock);
        targetDocumentDock.VisibleDockables = factory.CreateList<IDockable>();
        CopyDockGroup(sourceDockable, targetDocumentDock);
        if (sourceDockableOwner is IDocumentDock sourceDocumentDock)
        {
            targetDocumentDock.Id = sourceDocumentDock.Id;
            targetDocumentDock.CanCreateDocument = sourceDocumentDock.CanCreateDocument;
            targetDocumentDock.EnableWindowDrag = sourceDocumentDock.EnableWindowDrag;

            if (sourceDocumentDock is IDocumentDockContent sourceDocumentDockContent
                && targetDocumentDock is IDocumentDockContent targetDocumentDockContent)
            {
                targetDocumentDockContent.DocumentTemplate = sourceDocumentDockContent.DocumentTemplate;
            }

            if (sourceDocumentDock is IItemsSourceDock sourceDocumentItemsDock
                && targetDocumentDock is IItemsSourceDock targetDocumentItemsDock)
            {
                targetDocumentItemsDock.DocumentItemContainerTheme = sourceDocumentItemsDock.DocumentItemContainerTheme;
                targetDocumentItemsDock.DocumentItemTemplateSelector = sourceDocumentItemsDock.DocumentItemTemplateSelector;
            }

            if (sourceDocumentDock is IDocumentDockFactory sourceDocumentDockFactory
                && targetDocumentDock is IDocumentDockFactory targetDocumentDockFactory)
            {
                targetDocumentDockFactory.DocumentFactory = sourceDocumentDockFactory.DocumentFactory;
            }
        }
        // Local split into a new (empty) dock: allow per Empty Dock Rule
        if (!DockGroupValidator.ValidateDockingGroupsInDock(sourceDockable, targetDocumentDock))
        {
            return;
        }
        
        factory.MoveDockable(sourceDockableOwner, targetDocumentDock, sourceDockable, null);
        factory.SplitToDock(targetDock, targetDocumentDock, operation);
    }

    /// <inheritdoc />
    public bool SplitDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
    {
        switch (sourceDockable)
        {
            case ITool _:
            {
                if (sourceDockableOwner == targetDock)
                {
                    if (targetDock.VisibleDockables?.Count == 1)
                    {
                        return false;
                    }
                }

                if (bExecute)
                {
                    SplitToolDockable(sourceDockable, sourceDockableOwner, targetDock, operation);
                }

                return true;
            }
            case IDocument _:
            {
                if (sourceDockableOwner == targetDock)
                {
                    if (targetDock.VisibleDockables?.Count == 1)
                    {
                        return false;
                    }
                }

                if (bExecute)
                {
                    SplitDocumentDockable(sourceDockable, sourceDockableOwner, targetDock, operation);
                }

                return true;
            }
            default:
            {
                return false;
            }
        }
    }

    /// <inheritdoc />
    public bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, DockPoint screenPosition, bool bExecute)
    {
        if (sourceDockable == targetDockable)
        {
            return false;
        }

        if (!sourceDockable.CanFloat)
        {
            return false;
        }

        if (sourceDockable.Owner is not IDock sourceDockableOwner)
        {
            return false;
        }

        if (sourceDockableOwner.Factory is not { } factory)
        {
            return false;
        }

        if (factory.FindRoot(sourceDockable, _ => true) is { ActiveDockable: IDock targetWindowOwner })
        {
            if (bExecute)
            {
                sourceDockableOwner.GetVisibleBounds(out _, out _, out var ownerWidth, out var ownerHeight);
                sourceDockable.GetVisibleBounds(out _, out _, out var width, out var height);
                var x = screenPosition.X;
                var y = screenPosition.Y;
                if (double.IsNaN(width))
                {
                    width = double.IsNaN(ownerWidth) ? 300 : ownerWidth;
                }

                if (double.IsNaN(height))
                {
                    height = double.IsNaN(ownerHeight) ? 400 : ownerHeight;
                }

                factory.SplitToWindow(targetWindowOwner, sourceDockable, x, y, width, height);
            }

            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public bool DockDockableIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, bool bExecute)
    {
        if (sourceDockable.Owner is not IDock sourceDockableOwner || targetDockable.Owner is not IDock targetDockableOwner)
        {
            return false;
        }

        return sourceDockableOwner == targetDockableOwner 
            ? DockDockable(sourceDockable, sourceDockableOwner, targetDockable, action, bExecute) 
            : DockDockable(sourceDockable, sourceDockableOwner, targetDockable, targetDockableOwner, action, bExecute);
    }

    private bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDockable targetDockable, DragAction action, bool bExecute)
    {
        switch (action)
        {
            case DragAction.Copy:
            {
                return false;
            }
            case DragAction.Move:
            {
                // Local docking: strict validation between source and target dockables
                if (!DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable))
                {
                    return false;
                }

                if (bExecute && sourceDockableOwner.Factory is { } factory)
                {
                    factory.MoveDockable(sourceDockableOwner, sourceDockable, targetDockable);
                }

                return true;
            }
            case DragAction.Link:
            {
                // Local docking: strict validation between source and target dockables
                if (!DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable))
                {
                    return false;
                }

                if (bExecute && sourceDockableOwner.Factory is { } factory)
                {
                    factory.SwapDockable(sourceDockableOwner, sourceDockable, targetDockable);
                }

                return true;
            }
            default:
            {
                return false;
            }
        }
    }

    private bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDockable targetDockable, IDock targetDockableOwner, DragAction action, bool bExecute)
    {
        switch (action)
        {
            case DragAction.Copy:
            {
                return false;
            }
            case DragAction.Move:
            {
                // Local docking: strict validation between source and target dockables
                if (!DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable))
                {
                    return false;
                }

                if (bExecute && sourceDockableOwner.Factory is { } factory)
                {
                    factory.MoveDockable(sourceDockableOwner, targetDockableOwner, sourceDockable, targetDockable);
                }

                return true;
            }
            case DragAction.Link:
            {
                // Local docking: strict validation between source and target dockables
                if (!DockGroupValidator.ValidateDockingGroups(sourceDockable, targetDockable))
                {
                    return false;
                }

                if (bExecute && sourceDockableOwner.Factory is { } factory)
                {
                    factory.SwapDockable(sourceDockableOwner, targetDockableOwner, sourceDockable, targetDockable);
                }

                return true;
            }
            default:
            {
                return false;
            }
        }
    }
}
