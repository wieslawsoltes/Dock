// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Docking manager.
/// </summary>
public class DockManager : IDockManager
{
    /// <inheritdoc/>
    public DockPoint Position { get; set; }

    /// <inheritdoc/>
    public DockPoint ScreenPosition { get; set; }

    /// <inheritdoc/>
    public bool PreventSizeConflicts { get; set; } = true;

    private static bool IsFixed(double min, double max) => !double.IsNaN(min) && !double.IsNaN(max) && min == max;

    private static bool HasSizeConflict(ITool a, ITool b)
    {
        var widthConflict = IsFixed(a.MinWidth, a.MaxWidth) && IsFixed(b.MinWidth, b.MaxWidth) && a.MinWidth != b.MinWidth;
        var heightConflict = IsFixed(a.MinHeight, a.MaxHeight) && IsFixed(b.MinHeight, b.MaxHeight) && a.MinHeight != b.MinHeight;
        return widthConflict || heightConflict;
    }

    private bool MoveDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
    {
        if (sourceDockableOwner == targetDock)
        {
            if (targetDock.VisibleDockables?.Count == 1)
            {
                return false;
            }
        }
        var targetDockable = targetDock.VisibleDockables?.LastOrDefault();
        if (targetDockable is null)
        {
            if (bExecute)
            {
                if (sourceDockableOwner.Factory is { } factory)
                {
                    factory.MoveDockable(sourceDockableOwner, targetDock, sourceDockable, null);
                }
            }
            return true;
        }

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

        if (bExecute)
        {
            if (sourceDockableOwner.Factory is { } factory)
            {
                factory.MoveDockable(sourceDockableOwner, targetDock, sourceDockable, targetDockable);
            }
        }
        return true;
    }

    private bool SwapDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, bool bExecute)
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
        }
        factory.MoveDockable(sourceDockableOwner, targetDocumentDock, sourceDockable, null);
        factory.SplitToDock(targetDock, targetDocumentDock, operation);
    }

    private bool SplitDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
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

    private bool DockDockableIntoWindow(IDockable sourceDockable, IDockable targetDockable, bool bExecute)
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
                var x = ScreenPosition.X;
                var y = ScreenPosition.Y;
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

    private bool DockDockableIntoDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, bool bExecute)
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
                if (bExecute && sourceDockableOwner.Factory is { } factory)
                {
                    factory.MoveDockable(sourceDockableOwner, sourceDockable, targetDockable);
                }

                return true;
            }
            case DragAction.Link:
            {
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
                if (bExecute && sourceDockableOwner.Factory is { } factory)
                {
                    factory.MoveDockable(sourceDockableOwner, targetDockableOwner, sourceDockable, targetDockable);
                }

                return true;
            }
            case DragAction.Link:
            {
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

    private bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
    {
        return operation switch
        {
            DockOperation.Fill => MoveDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute),
            DockOperation.Left => SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Right => SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Top => SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Bottom => SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Window => DockDockableIntoWindow(sourceDockable, targetDock, bExecute),
            _ => false
        };
    }

    private bool DockDockableIntoDock(IDockable sourceDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDockable.Owner is not IDock sourceDockableOwner)
        {
            return false;
        }

        return DockDockableIntoDock(sourceDockable, sourceDockableOwner, targetDock, action, operation, bExecute);
    }

    private bool DockDockableIntoDock(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        return action switch
        {
            DragAction.Copy => false,
            DragAction.Move => DockDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DragAction.Link => SwapDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute),
            _ => false
        };
    }

    private bool DockDockableIntoDockVisible(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        var visible = sourceDock.VisibleDockables?.ToList();
        if (visible is null)
        {
            return true;
        }

        foreach (var dockable in visible)
        {
            if (DockDockableIntoDock(dockable, targetDock, action, operation, bExecute) == false)
            {
                return false;
            }
        }

        return true;
    }

    private bool DockDockIntoDock(IDock sourceDock, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        var visible = sourceDock.VisibleDockables?.ToList();
        if (visible is null)
        {
            return true;
        }
            
        if (visible.Count == 1)
        {
            var sourceDockable = visible.FirstOrDefault();
            if (sourceDockable is null || DockDockableIntoDock(sourceDockable, targetDock, action, operation, bExecute) == false)
            {
                return false;
            }
        }
        else
        {
            var sourceDockable = visible.FirstOrDefault();
            if (sourceDockable is null || DockDockableIntoDock(sourceDockable, targetDock, action, operation, bExecute) == false)
            {
                return false;
            }
                
            foreach (var dockable in visible.Skip(1))
            {
                var targetDockable = visible.FirstOrDefault();
                if (targetDockable is null || DockDockableIntoDockable(dockable, targetDockable, action, bExecute) == false)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private bool DockDockable(IDock sourceDock, IDockable targetDockable, IDock targetDock, DragAction action, DockOperation operation, bool bExecute)
    {
        return operation switch
        {
            DockOperation.Fill => DockDockableIntoDockVisible(sourceDock, targetDock, action, operation, bExecute),
            DockOperation.Window => DockDockableIntoWindow(sourceDock, targetDockable, bExecute),
            _ => DockDockIntoDock(sourceDock, targetDock, action, operation, bExecute)
        };
    }

    /// <inheritdoc/>
    public bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!sourceTool.CanDrag || !targetDockable.CanDrop)
        {
            return false;
        }

        return targetDockable switch
        {
            IRootDock _ => DockDockableIntoWindow(sourceTool, targetDockable, bExecute),
            IToolDock toolDock =>
                (!PreventSizeConflicts || toolDock.VisibleDockables?.OfType<ITool>().All(t => !HasSizeConflict(sourceTool, t)) != false)
                && DockDockableIntoDock(sourceTool, toolDock, action, operation, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(sourceTool, documentDock, action, operation, bExecute),
            ITool tool => (!PreventSizeConflicts || !HasSizeConflict(sourceTool, tool)) &&
                           DockDockableIntoDockable(sourceTool, tool, action, bExecute),
            IDocument document => DockDockableIntoDockable(sourceTool, document, action, bExecute),
            IProportionalDock proportionalDock => DockDockableIntoDock(sourceTool, proportionalDock, action, operation, bExecute),
            _ => false
        };
    }

    /// <inheritdoc/>
    public bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!sourceDocument.CanDrag || !targetDockable.CanDrop)
        {
            return false;
        }

        return targetDockable switch
        {
            IRootDock _ => DockDockableIntoWindow(sourceDocument, targetDockable, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(sourceDocument, documentDock, action, operation, bExecute),
            IDocument document => DockDockableIntoDockable(sourceDocument, document, action, bExecute),
            IProportionalDock proportionalDock => DockDockableIntoDock(sourceDocument, proportionalDock, action, operation, bExecute),
            _ => false
        };
    }

    /// <inheritdoc/>
    public bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!sourceDock.CanDrag || !targetDockable.CanDrop)
        {
            return false;
        }

        return targetDockable switch
        {
            IRootDock _ => DockDockableIntoWindow(sourceDock, targetDockable, bExecute),
            IToolDock toolDock => sourceDock != toolDock &&
                                  DockDockable(sourceDock, targetDockable, toolDock, action, operation, bExecute),
            IDocumentDock documentDock => sourceDock != documentDock &&
                                          DockDockable(sourceDock, targetDockable, documentDock, action, operation, bExecute),
            IProportionalDock proportionalDock => sourceDock != proportionalDock &&
                                                  DockDockable(sourceDock, targetDockable, proportionalDock, action, operation, bExecute),
            _ => false
        };
    }

    private bool ValidateProportionalDock(IProportionalDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
            return false;

        bool all = true;
        for (int i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateStackDock(IStackDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
            return false;

        bool all = true;
        for (int i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateGridDock(IGridDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
            return false;

        bool all = true;
        for (int i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateWrapDock(IWrapDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
            return false;

        bool all = true;
        for (int i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    private bool ValidateUniformGridDock(IUniformGridDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
            return false;

        bool all = true;
        for (int i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    /// <inheritdoc/>
    public bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        return sourceDockable switch
        {
            IToolDock toolDock => ValidateDock(toolDock, targetDockable, action, operation, bExecute),
            IDocumentDock documentDock => ValidateDock(documentDock, targetDockable, action, operation, bExecute),
            ITool tool => ValidateTool(tool, targetDockable, action, operation, bExecute),
            IDocument document => ValidateDocument(document, targetDockable, action, operation, bExecute),
            IProportionalDock proportionalDock => ValidateProportionalDock(proportionalDock, targetDockable, action, operation, bExecute),
            IStackDock stackDock => ValidateStackDock(stackDock, targetDockable, action, operation, bExecute),
            IGridDock gridDock => ValidateGridDock(gridDock, targetDockable, action, operation, bExecute),
            IWrapDock wrapDock => ValidateWrapDock(wrapDock, targetDockable, action, operation, bExecute),
            IUniformGridDock uniformGridDock => ValidateUniformGridDock(uniformGridDock, targetDockable, action, operation, bExecute),
            _ => false
        };
    }

    /// <inheritdoc/>
    public virtual bool IsDockTargetVisible(IDockable sourceDockable, IDockable targetDockable, DockOperation operation)
    {
        if (operation == DockOperation.Fill)
        {
            if (ReferenceEquals(sourceDockable, targetDockable))
            {
                return false;
            }

            if (ReferenceEquals(sourceDockable.Owner, targetDockable))
            {
                return false;
            }
        }

        return true;
    }
}
