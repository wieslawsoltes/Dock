// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Dock.Settings;

namespace Dock.Avalonia.Internal;

internal sealed class ManagedWindowDragContext
{
    public PixelPoint DragStartPoint { get; set; }
    public PixelPoint DragOffset { get; set; }
    public IDockable? DragDockable { get; set; }
    public bool PointerPressed { get; set; }
    public bool DoDragDrop { get; set; }
    public DockControl? TargetDockControl { get; set; }
    public Point TargetPoint { get; set; }
    public DragAction DragAction { get; set; } = DragAction.Move;

    public void Start(PixelPoint point)
    {
        DragStartPoint = point;
        DragOffset = default;
        DragDockable = null;
        PointerPressed = true;
        DoDragDrop = false;
        TargetDockControl = null;
        TargetPoint = default;
        DragAction = DragAction.Move;
    }

    public void End()
    {
        DragStartPoint = default;
        DragOffset = default;
        DragDockable = null;
        PointerPressed = false;
        DoDragDrop = false;
        TargetDockControl = null;
        TargetPoint = default;
        DragAction = DragAction.Move;
    }
}

internal sealed class ManagedHostWindowState : DockManagerState, IHostWindowState
{
    private readonly ManagedHostWindow _hostWindow;
    private readonly ManagedWindowDragContext _context = new();
    private readonly DragPreviewHelper _dragPreviewHelper = new();

    public ManagedHostWindowState(
        IDockManager dockManager,
        ManagedHostWindow hostWindow,
        IGlobalDockingService? globalDockingService = null)
        : base(dockManager, globalDockingService)
    {
        _hostWindow = hostWindow;
    }

    public void Process(PixelPoint screenPoint, EventType eventType)
    {
        if (!DockManager.IsDockingEnabled)
        {
            if (_context.PointerPressed || _context.DoDragDrop)
            {
                _dragPreviewHelper.Hide();
                Leave();
                _context.End();
                DropControl = null;
            }

            return;
        }

        switch (eventType)
        {
            case EventType.Pressed:
            {
                _context.Start(screenPoint);
                _context.DragDockable = ResolveManagedDockable();
                _context.DragOffset = CalculateDragOffset(_context.DragDockable as ManagedDockWindowDocument, screenPoint);
                DropControl = null;
                break;
            }
            case EventType.Released:
            {
                if (_context.DoDragDrop)
                {
                    if (_context.TargetDockControl is { } && DropControl is { })
                    {
                        var isDropEnabled = true;
                        if (DropControl is { } targetDropControl)
                        {
                            isDropEnabled = targetDropControl.GetValue(DockProperties.IsDropEnabledProperty);
                        }

                        if (isDropEnabled)
                        {
                            Drop(_context.TargetPoint, _context.DragAction, DropControl, _context.TargetDockControl);
                        }
                    }
                }

                _dragPreviewHelper.Hide();
                Leave();
                _context.End();
                DropControl = null;
                break;
            }
            case EventType.Moved:
            {
                if (_context.PointerPressed == false)
                {
                    break;
                }

                if (_context.DoDragDrop == false)
                {
                    var diff = screenPoint - _context.DragStartPoint;
                    if (DockSettings.IsMinimumDragDistance(diff))
                    {
                        if (_context.DragDockable is null)
                        {
                            _context.DragDockable = ResolveManagedDockable();
                            _context.DragOffset = CalculateDragOffset(_context.DragDockable as ManagedDockWindowDocument, _context.DragStartPoint);
                        }

                        var showPreview = ShouldShowManagedDragPreview(_context.DragDockable);
                        if (_context.DragDockable is { } dockable && showPreview)
                        {
                            var context = dockable is ManagedDockWindowDocument document
                                ? document.Content as Visual
                                : null;
                            _dragPreviewHelper.Show(dockable, screenPoint, _context.DragOffset, context);
                        }
                        _context.DoDragDrop = true;
                    }
                }

                if (!_context.DoDragDrop)
                {
                    break;
                }

                var preview = "None";

                if (_hostWindow.Window?.Layout?.Factory is not { } factory)
                {
                    _dragPreviewHelper.Hide();
                    Leave();
                    DropControl = null;
                    _context.TargetDockControl = null;
                    _context.TargetPoint = default;
                    break;
                }

                var found = false;
                foreach (var dockControl in DockHelpers.GetZOrderedDockControls(factory.DockControls))
                {
                    if (dockControl.Layout == _hostWindow.Window?.Layout)
                    {
                        continue;
                    }

                    if (dockControl.GetVisualRoot() is null)
                    {
                        continue;
                    }

                    var dockControlPoint = dockControl.PointToClient(screenPoint);
                    var dropControl = DockHelpers.GetControl(dockControl, dockControlPoint, DockProperties.IsDropAreaProperty);
                    if (dropControl is { })
                    {
                        var isDropEnabled = dropControl.GetValue(DockProperties.IsDropEnabledProperty);
                        if (!isDropEnabled)
                        {
                            Leave();
                            _context.TargetDockControl = null;
                            _context.TargetPoint = default;
                            DropControl = null;
                        }
                        else
                        {
                            if (DropControl == dropControl)
                            {
                                _context.TargetDockControl = dockControl;
                                _context.TargetPoint = dockControlPoint;
                                DropControl = dropControl;
                                _context.DragAction = DragAction.Move;
                                Over(_context.TargetPoint, _context.DragAction, dropControl, _context.TargetDockControl);
                                found = true;
                                break;
                            }

                            if (DropControl is { })
                            {
                                Leave();
                                DropControl = null;
                            }

                            _context.TargetDockControl = dockControl;
                            _context.TargetPoint = dockControlPoint;
                            DropControl = dropControl;
                            _context.DragAction = DragAction.Move;
                            Enter(_context.TargetPoint, _context.DragAction, _context.TargetDockControl);
                            found = true;
                            break;
                        }
                    }
                }

                if (!found && DropControl is { })
                {
                    Leave();
                    DropControl = null;
                    _context.TargetDockControl = null;
                    _context.TargetPoint = default;
                }

                if (DropControl is { } && _context.TargetDockControl is { } targetDockControl)
                {
                    var isDropEnabled = DropControl.GetValue(DockProperties.IsDropEnabledProperty);
                    if (isDropEnabled)
                    {
                        var globalOperation = GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget
                            ? globalDockTarget.GetDockOperation(_context.TargetPoint, DropControl, targetDockControl, _context.DragAction, ValidateGlobal, IsDockTargetVisible)
                            : DockOperation.None;

                        var localOperation = LocalAdornerHelper.Adorner is DockTarget dockTarget
                            ? dockTarget.GetDockOperation(_context.TargetPoint, DropControl, targetDockControl, _context.DragAction, ValidateLocal, IsDockTargetVisible)
                            : DockOperation.Fill;

                        if (globalOperation != DockOperation.None)
                        {
                            preview = ValidateGlobal(_context.TargetPoint, globalOperation, _context.DragAction, targetDockControl)
                                ? "Dock"
                                : "None";
                        }
                        else
                        {
                            var isValid = ValidateLocal(_context.TargetPoint, localOperation, _context.DragAction, targetDockControl);
                            preview = isValid
                                ? localOperation == DockOperation.Window ? "Float" : "Dock"
                                : "None";
                        }
                    }
                }

                var sourceDockable = _hostWindow.Window?.Layout?.FocusedDockable
                    ?? _hostWindow.Window?.Layout?.ActiveDockable;
                if (preview == "None" && sourceDockable?.CanFloat == true)
                {
                    preview = "Float";
                }

                if (ShouldShowManagedDragPreview(_context.DragDockable))
                {
                    _dragPreviewHelper.Move(screenPoint, _context.DragOffset, preview);
                }

                break;
            }
            case EventType.CaptureLost:
            {
                _dragPreviewHelper.Hide();
                Leave();
                _context.End();
                DropControl = null;
                break;
            }
        }
    }

    private void Enter(Point point, DragAction dragAction, Visual relativeTo)
    {
        var isLocalValid = ValidateLocal(point, DockOperation.Fill, dragAction, relativeTo);
        var isGlobalValid = ValidateGlobal(point, DockOperation.Fill, dragAction, relativeTo);
        AddAdorners(isLocalValid, isGlobalValid);
    }

    private void Over(Point point, DragAction dragAction, Control dropControl, Visual relativeTo)
    {
        var localOperation = DockOperation.Fill;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        if (globalOperation != DockOperation.None)
        {
            ValidateGlobal(point, globalOperation, dragAction, relativeTo);
        }
        else
        {
            if (localOperation != DockOperation.Window)
            {
                ValidateLocal(point, localOperation, dragAction, relativeTo);
            }
        }

        LocalAdornerHelper.SetGlobalDockActive(globalOperation != DockOperation.None);
    }

    private void Drop(Point point, DragAction dragAction, Control dropControl, Visual relativeTo)
    {
        var localOperation = DockOperation.Window;
        var globalOperation = DockOperation.None;

        if (LocalAdornerHelper.Adorner is DockTarget dockTarget)
        {
            localOperation = dockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateLocal, IsDockTargetVisible);
        }

        if (GlobalAdornerHelper.Adorner is GlobalDockTarget globalDockTarget)
        {
            globalOperation = globalDockTarget.GetDockOperation(point, dropControl, relativeTo, dragAction, ValidateGlobal, IsDockTargetVisible);
        }

        RemoveAdorners();

        if (DropControl is null)
        {
            return;
        }

        var layout = _hostWindow.Window?.Layout;

        if (globalOperation != DockOperation.None)
        {
            if (DropControl is not { } dropCtrl)
            {
                return;
            }

            if (layout?.ActiveDockable is { } sourceDockable
                && ResolveGlobalTargetDock(dropCtrl) is { } targetDock)
            {
                if (!ValidateGlobalTarget(sourceDockable, targetDock))
                {
                    return;
                }

                Execute(point, globalOperation, dragAction, relativeTo, sourceDockable, targetDock);
            }
        }
        else
        {
            if (layout?.ActiveDockable is { } sourceDockable
                && DropControl.DataContext is IDockable targetDockable)
            {
                if (localOperation != DockOperation.Window)
                {
                    Execute(point, localOperation, dragAction, relativeTo, sourceDockable, targetDockable);
                }
            }
        }
    }

    private void Leave()
    {
        RemoveAdorners();
    }

    private bool ValidateLocal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (!DockManager.IsDockingEnabled)
        {
            return false;
        }

        var layout = _hostWindow.Window?.Layout;
        if (layout?.FocusedDockable is not { } sourceDockable)
        {
            return false;
        }

        if (DropControl?.DataContext is IDockable)
        {
            return ValidateDockable(point, operation, dragAction, relativeTo, sourceDockable);
        }

        return DropControl?.GetValue(DockProperties.IsDockTargetProperty) == true;
    }

    private bool ValidateGlobal(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        if (!DockManager.IsDockingEnabled)
        {
            return false;
        }

        var layout = _hostWindow.Window?.Layout;
        if (layout?.FocusedDockable is not { } sourceDockable)
        {
            return false;
        }

        if (DropControl is not { } dropCtrl)
        {
            return false;
        }

        var targetDock = ResolveGlobalTargetDock(dropCtrl);
        if (targetDock is null)
        {
            return false;
        }

        if (!DockInheritanceHelper.GetEffectiveEnableGlobalDocking(targetDock))
        {
            return false;
        }

        DockManager.Position = DockHelpers.ToDockPoint(point);

        if (relativeTo.GetVisualRoot() is null)
        {
            return false;
        }

        var screen = DockHelpers.GetScreenPoint(relativeTo, point);
        DockManager.ScreenPosition = DockHelpers.ToDockPoint(screen);

        if (!DockGroupValidator.ValidateGlobalDocking(sourceDockable, targetDock))
        {
            return false;
        }

        return DockManager.ValidateDockable(sourceDockable, targetDock, dragAction, operation, bExecute: false);
    }

    private bool IsDockTargetVisible(Point point, DockOperation operation, DragAction dragAction, Visual relativeTo)
    {
        var layout = _hostWindow.Window?.Layout;
        if (layout?.FocusedDockable is not IDockable sourceDockable)
        {
            return true;
        }

        if (DropControl?.DataContext is not IDockable targetDockable)
        {
            return true;
        }

        return DockManager.IsDockTargetVisible(sourceDockable, targetDockable, operation);
    }

    private ManagedDockWindowDocument? ResolveManagedDockable()
    {
        if (_hostWindow.Window?.Factory is not { } factory)
        {
            return null;
        }

        var dock = ManagedWindowRegistry.GetOrCreateDock(factory);
        return dock.VisibleDockables?.OfType<ManagedDockWindowDocument>()
            .FirstOrDefault(document => ReferenceEquals(document.Window, _hostWindow.Window));
    }

    private static bool ShouldShowManagedDragPreview(IDockable? dockable)
    {
        return DockSettings.ShowDockablePreviewOnDrag && dockable is not ManagedDockWindowDocument;
    }

    private static PixelPoint CalculateDragOffset(ManagedDockWindowDocument? document, PixelPoint pointerPosition)
    {
        if (document is null || !TryResolveManagedLayer(document, out var layer))
        {
            return default;
        }

        var topLeft = layer.PointToScreen(new Point(document.MdiBounds.X, document.MdiBounds.Y));
        return new PixelPoint(topLeft.X - pointerPosition.X, topLeft.Y - pointerPosition.Y);
    }

    private static bool TryResolveManagedLayer(ManagedDockWindowDocument? document, out ManagedWindowLayer layer)
    {
        layer = null!;

        if (document is null)
        {
            return false;
        }

        var context = document.Content as Visual;
        var contextRoot = context?.GetVisualRoot();

        if (context is { })
        {
            var resolved = ManagedWindowLayer.TryGetLayer(context);
            if (IsLayerReady(resolved) && IsLayerInRoot(resolved, contextRoot))
            {
                layer = resolved!;
                return true;
            }
        }

        if (document.Factory is { } factory)
        {
            var registered = ManagedWindowRegistry.TryGetLayer(factory);
            if (IsLayerReady(registered) && IsLayerInRoot(registered, contextRoot))
            {
                layer = registered!;
                return true;
            }
        }

        return false;
    }

    private static bool IsLayerReady(ManagedWindowLayer? layer)
    {
        return layer is { IsVisible: true } && layer.GetVisualRoot() is not null;
    }

    private static bool IsLayerInRoot(ManagedWindowLayer? layer, object? contextRoot)
    {
        if (layer is null)
        {
            return false;
        }

        if (contextRoot is null)
        {
            return true;
        }

        return ReferenceEquals(layer.GetVisualRoot(), contextRoot);
    }
}
