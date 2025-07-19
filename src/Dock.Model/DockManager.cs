// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Linq;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace Dock.Model;

/// <summary>
/// Docking manager.
/// </summary>
public class DockManager : IDockManager, IDockableVisitor
{
    private readonly DockService _dockService = new ();
    private bool _result;
    private bool _result;

    /// <inheritdoc/>
    public DockPoint Position { get; set; }

    /// <inheritdoc/>
    public DockPoint ScreenPosition { get; set; }

    /// <inheritdoc/>
    public bool PreventSizeConflicts { get; set; } = true;

    private static bool IsFixed(double min, double max)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        return !double.IsNaN(min) && !double.IsNaN(max) && min == max;
    }

    private static bool HasSizeConflict(ITool a, ITool b)
    {
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        var widthConflict = IsFixed(a.MinWidth, a.MaxWidth) && IsFixed(b.MinWidth, b.MaxWidth) && a.MinWidth != b.MinWidth;
        // ReSharper disable once CompareOfFloatsByEqualityOperator
        var heightConflict = IsFixed(a.MinHeight, a.MaxHeight) && IsFixed(b.MinHeight, b.MaxHeight) && a.MinHeight != b.MinHeight;
        return widthConflict || heightConflict;
    }

    private bool DockDockable(IDockable sourceDockable, IDock sourceDockableOwner, IDock targetDock, DockOperation operation, bool bExecute)
    {
        return operation switch
        {
            DockOperation.Fill => _dockService.MoveDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute),
            DockOperation.Left => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Right => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Top => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Bottom => _dockService.SplitDockable(sourceDockable, sourceDockableOwner, targetDock, operation, bExecute),
            DockOperation.Window => _dockService.DockDockableIntoWindow(sourceDockable, targetDock, ScreenPosition, bExecute),
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
            DragAction.Link => _dockService.SwapDockable(sourceDockable, sourceDockableOwner, targetDock, bExecute),
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
                if (targetDockable is null || _dockService.DockDockableIntoDockable(dockable, targetDockable, action, bExecute) == false)
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
            DockOperation.Window => _dockService.DockDockableIntoWindow(sourceDock, targetDockable, ScreenPosition, bExecute),
            _ => DockDockIntoDock(sourceDock, targetDock, action, operation, bExecute)
        };
    }

    /// <inheritdoc/>
    public bool ValidateTool(ITool sourceTool, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        VisitTool(sourceTool, targetDockable, action, operation, bExecute);
        return _result;
    }

    /// <inheritdoc/>
    public bool ValidateDocument(IDocument sourceDocument, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        VisitDocument(sourceDocument, targetDockable, action, operation, bExecute);
        return _result;
    }

    /// <inheritdoc/>
    public bool ValidateDock(IDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        VisitDock(sourceDock, targetDockable, action, operation, bExecute);
        return _result;
    }

    private bool ValidateProportionalDock(IProportionalDock sourceDock, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        if (sourceDock.VisibleDockables == null ||
            sourceDock.VisibleDockables.Count == 0)
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
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
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
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
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
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
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
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
        {
            return false;
        }

        var all = true;
        for (var i = sourceDock.VisibleDockables.Count - 1; i >= 0; --i)
        {
            var dockable = sourceDock.VisibleDockables[i];
            if (dockable is not IDock dock)
                continue;

            all &= ValidateDockable(dock, targetDockable, action, operation, bExecute);
        }

        return all;
    }

    public bool VisitTool(ITool tool, IDockable target, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!tool.CanDrag || !target.CanDrop)
        {
            _result = false;
            return _result;
        }

        _result = target switch
        {
            IRootDock _ => _dockService.DockDockableIntoWindow(tool, target, ScreenPosition, bExecute),
            IToolDock toolDock => (!PreventSizeConflicts || toolDock.VisibleDockables?.OfType<ITool>().All(t => !HasSizeConflict(tool, t)) != false) &&
                                   DockDockableIntoDock(tool, toolDock, action, operation, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(tool, documentDock, action, operation, bExecute),
            ITool targetTool => (!PreventSizeConflicts || !HasSizeConflict(tool, targetTool)) && _dockService.DockDockableIntoDockable(tool, targetTool, action, bExecute),
            IDocument document => _dockService.DockDockableIntoDockable(tool, document, action, bExecute),
            IProportionalDock proportionalDock => DockDockableIntoDock(tool, proportionalDock, action, operation, bExecute),
            _ => false
        };
        return _result;
    }

    public bool VisitDocument(IDocument document, IDockable target, DragAction action, DockOperation operation, bool bExecute)
    {
        if (!document.CanDrag || !target.CanDrop)
        {
            _result = false;
            return _result;
        }

        _result = target switch
        {
            IRootDock _ => _dockService.DockDockableIntoWindow(document, target, ScreenPosition, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(document, documentDock, action, operation, bExecute),
            IDocument targetDocument => _dockService.DockDockableIntoDockable(document, targetDocument, action, bExecute),
            IProportionalDock proportionalDock => DockDockableIntoDock(document, proportionalDock, action, operation, bExecute),
            _ => false
        };
        return _result;
    }

    public bool VisitDock(IDock dock, IDockable target, DragAction action, DockOperation operation, bool bExecute)
    {
        if (dock is IProportionalDock proportional)
        {
            _result = ValidateProportionalDock(proportional, target, action, operation, bExecute);
            return _result;
        }

        if (dock is IStackDock stack)
        {
            _result = ValidateStackDock(stack, target, action, operation, bExecute);
            return _result;
        }

        if (dock is IGridDock grid)
        {
            _result = ValidateGridDock(grid, target, action, operation, bExecute);
            return _result;
        }

        if (dock is IWrapDock wrap)
        {
            _result = ValidateWrapDock(wrap, target, action, operation, bExecute);
            return _result;
        }

        if (dock is IUniformGridDock uniform)
        {
            _result = ValidateUniformGridDock(uniform, target, action, operation, bExecute);
            return _result;
        }

        if (!dock.CanDrag || !target.CanDrop)
        {
            _result = false;
            return _result;
        }

        _result = target switch
        {
            IRootDock _ => _dockService.DockDockableIntoWindow(dock, target, ScreenPosition, bExecute),
            IToolDock toolDock => dock != toolDock && DockDockable(dock, target, toolDock, action, operation, bExecute),
            IDocumentDock documentDock => dock != documentDock && DockDockable(dock, target, documentDock, action, operation, bExecute),
            IProportionalDock proportionalDock => dock != proportionalDock && DockDockable(dock, target, proportionalDock, action, operation, bExecute),
            _ => false
        };
        return _result;
    }

    /// <inheritdoc/>
    public bool ValidateDockable(IDockable sourceDockable, IDockable targetDockable, DragAction action, DockOperation operation, bool bExecute)
    {
        _result = false;
        sourceDockable.Accept(this, targetDockable, action, operation, bExecute);
        return _result;
    }

    /// <inheritdoc/>
    public bool IsDockTargetVisible(IDockable sourceDockable, IDockable targetDockable, DockOperation operation)
    {
        if (operation != DockOperation.Fill)
        {
            return true;
        }

        if (ReferenceEquals(sourceDockable, targetDockable))
        {
            return false;
        }

        if (ReferenceEquals(sourceDockable.Owner, targetDockable))
        {
            return false;
        }

        return true;
    }
}
