﻿// Copyright (c) Wiesław Šoltés. All rights reserved.
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
    private readonly DockService _dockService = new ();
    private readonly DockManagerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DockManager"/> class.
    /// </summary>
    /// <param name="options">The dock manager options.</param>
    public DockManager(DockManagerOptions? options = null)
    {
        _options = options ?? new DockManagerOptions();
    }

    /// <inheritdoc/>
    public DockPoint Position { get; set; }

    /// <inheritdoc/>
    public DockPoint ScreenPosition { get; set; }

    /// <inheritdoc/>
    public bool PreventSizeConflicts
    {
        get => _options.PreventSizeConflicts;
        set => _options.PreventSizeConflicts = value;
    }

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
        if (!sourceTool.CanDrag || !targetDockable.CanDrop)
        {
            return false;
        }

        return targetDockable switch
        {
            IRootDock _ => _dockService.DockDockableIntoWindow(sourceTool, targetDockable, ScreenPosition, bExecute),
            IToolDock toolDock =>
                (!PreventSizeConflicts || toolDock.VisibleDockables?.OfType<ITool>().All(t => !HasSizeConflict(sourceTool, t)) != false)
                && DockDockableIntoDock(sourceTool, toolDock, action, operation, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(sourceTool, documentDock, action, operation, bExecute),
            ITool tool => (!PreventSizeConflicts || !HasSizeConflict(sourceTool, tool)) &&
                          _dockService.DockDockableIntoDockable(sourceTool, tool, action, bExecute),
            IDocument document => _dockService.DockDockableIntoDockable(sourceTool, document, action, bExecute),
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
            IRootDock _ => _dockService.DockDockableIntoWindow(sourceDocument, targetDockable, ScreenPosition, bExecute),
            IDocumentDock documentDock => DockDockableIntoDock(sourceDocument, documentDock, action, operation, bExecute),
            IDocument document => _dockService.DockDockableIntoDockable(sourceDocument, document, action, bExecute),
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
            IRootDock _ => _dockService.DockDockableIntoWindow(sourceDock, targetDockable, ScreenPosition, bExecute),
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
