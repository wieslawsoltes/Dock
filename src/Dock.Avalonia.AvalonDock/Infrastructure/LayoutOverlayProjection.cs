using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace AvalonDock.Infrastructure;

// Read-only projections of the controls used by Dock's drag engine. Captures are
// requested by callers, never allocated in the native pointer-move path.
internal sealed class LayoutOverlayProjection
{
    private readonly Control _surface;
    private readonly Control _adorner;
    private readonly DropAreaType _type;

    internal Control Surface => _surface;
    internal Control Adorner => _adorner;
    internal DropAreaType Type => _type;

    internal LayoutOverlayProjection(Control surface, Control adorner)
    {
        _surface = surface;
        _adorner = adorner;
        if (adorner is GlobalDockTarget) { _type = DropAreaType.DockingManager; return; }
        for (Visual? current = surface; current is not null; current = current.GetVisualParent())
        {
            if (current is LayoutDocumentPaneControl documents)
            { _surface = documents; _type = DropAreaType.DocumentPane; return; }
            if (current is LayoutAnchorablePaneControl tools)
            { _surface = tools; _type = DropAreaType.AnchorablePane; return; }
            if (current is LayoutDocumentPaneGroupControl group)
            { _surface = group; _type = DropAreaType.DocumentPaneGroup; return; }
            if (current is DockControl) break;
        }
        _type = surface.DataContext switch
        {
            IDocumentDock => DropAreaType.DocumentPane,
            IToolDock => DropAreaType.AnchorablePane,
            _ => DropAreaType.DockingManager
        };
    }

    internal IDropArea CaptureArea() => _surface switch
    {
        LayoutDocumentPaneControl pane => new DropArea<LayoutDocumentPaneControl>(pane, _type),
        LayoutAnchorablePaneControl pane => new DropArea<LayoutAnchorablePaneControl>(pane, _type),
        LayoutDocumentPaneGroupControl group => new DropArea<LayoutDocumentPaneGroupControl>(group, _type),
        _ => new DropArea<Control>(_surface, _type)
    };

    internal OverlayArea CaptureOverlayArea() => _type switch
    {
        DropAreaType.AnchorablePane => new AnchorablePaneControlOverlayArea(_surface),
        DropAreaType.DocumentPane or DropAreaType.DocumentPaneGroup => new DocumentPaneControlOverlayArea(_surface),
        _ => new DockingManagerOverlayArea(_surface)
    };

    internal IReadOnlyList<OverlayWindowDropTarget> CaptureTargets()
    {
        List<OverlayWindowDropTarget> targets = new(9);
        if (_adorner is not DockTargetBase target) return targets;
        for (int index = 0; index < 9; index++)
        {
            DockOperation operation = index switch
            {
                0 => DockOperation.Left, 1 => DockOperation.Top, 2 => DockOperation.Right,
                3 => DockOperation.Bottom, 4 => DockOperation.Fill,
                5 => DockOperation.OuterLeft, 6 => DockOperation.OuterTop,
                7 => DockOperation.OuterRight, _ => DockOperation.OuterBottom
            };
            Control? selector = target.GetSelector(operation);
            if (selector is null || !selector.IsEffectivelyVisible || selector.Opacity <= 0 ||
                selector.Bounds.Width <= 0 || selector.Bounds.Height <= 0) continue;
            if (Map(operation) is { } mapped)
                targets.Add(new OverlayWindowDropTarget(mapped.Selector, mapped.Destination, selector));
        }
        return targets;
    }

    private (OverlayWindowDropTargetType Selector, DropTargetType Destination)? Map(DockOperation operation)
    {
        if (_type == DropAreaType.DocumentPane)
        {
            int outer = operation switch
            {
                DockOperation.OuterLeft => 0, DockOperation.OuterTop => 1,
                DockOperation.OuterRight => 2, DockOperation.OuterBottom => 3, _ => -1
            };
            if (outer >= 0) return (
                (OverlayWindowDropTargetType)((int)OverlayWindowDropTargetType.DocumentPaneDockLeft + outer),
                (DropTargetType)((int)DropTargetType.DocumentPaneDockAsAnchorableLeft + outer));
        }
        int offset = operation switch
        {
            DockOperation.Left => 0, DockOperation.Top => 1, DockOperation.Right => 2,
            DockOperation.Bottom => 3, DockOperation.Fill => 4, _ => -1
        };
        if (offset < 0 || (_type == DropAreaType.DockingManager && offset == 4)) return null;
        if (_type == DropAreaType.DockingManager)
            return ((OverlayWindowDropTargetType)((int)OverlayWindowDropTargetType.DockingManagerDockLeft + offset),
                (DropTargetType)((int)DropTargetType.DockingManagerDockLeft + offset));
        if (_type == DropAreaType.AnchorablePane)
            return ((OverlayWindowDropTargetType)((int)OverlayWindowDropTargetType.AnchorablePaneDockLeft + offset),
                (DropTargetType)((int)DropTargetType.AnchorablePaneDockLeft + offset));
        return ((OverlayWindowDropTargetType)((int)OverlayWindowDropTargetType.DocumentPaneDockLeft + offset),
            _type == DropAreaType.DocumentPaneGroup && offset == 4 ? DropTargetType.DocumentPaneGroupDockInside
                : (DropTargetType)((int)DropTargetType.DocumentPaneDockLeft + offset));
    }
}
