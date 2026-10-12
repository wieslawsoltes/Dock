using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using AvalonDock.Controls;
using AvalonDock.Layout;
using Dock.Avalonia.Controls;
using Dock.Model;
using Dock.Model.Core;
using Dock.Settings;

namespace AvalonDock.Infrastructure;

// Dock's own tab containers keep their input handling. A public tab placed in
// an application tab template uses the same live pane/factory and window drag.
internal sealed class LayoutTabItemInput(TemplatedControl control, Func<LayoutContent?> getModel,
    Func<LayoutItem?> getItem, bool document)
{
    private PointerPressedEventArgs? _press;
    private IPointer? _pointer;
    private Panel? _host;
    private Point _start;
    private bool _dragging;
    private double _verticalBuffer = 5;

    internal void Cancel()
    {
        IPointer? pointer = _pointer;
        _press = null;
        _pointer = null;
        _host = null;
        _dragging = false;
        _verticalBuffer = 5;
        if (ReferenceEquals(pointer?.Captured, control)) pointer.Capture(null);
    }

    internal void Pressed(PointerPressedEventArgs args)
    {
        Cancel();
        // Activation is the public hook's contract. Gesture eligibility is
        // evaluated afterward, independently of source ownership and enablement.
        if (document && getModel() is { } active) active.IsActive = true;
        if (args.Handled || !CanUseSource(args.Source) || getModel() is not { } model) return;
        if (HasNativeTab() || !CanDrag(model) || args.ClickCount != 1) return;
        _start = args.GetPosition(control);
        _press = args;
    }

    internal void Released(PointerReleasedEventArgs args)
    {
        Cancel();
        if (!document && getModel() is { } model) model.IsActive = true;
    }

    internal void MouseDown(PointerPressedEventArgs args)
    {
        if (args.GetCurrentPoint(control).Properties.PointerUpdateKind != PointerUpdateKind.MiddleButtonPressed ||
            getItem() is not { } item) return;
        if (item.LayoutElement is { } model && !LayoutCommandCapabilities.IsAllowedByPolicy(model.DockModel, DockCapability.Close)) return;
        if (item.CloseCommand is { } close && close.CanExecute(null))
            close.Execute(null);
    }

    internal void BoundaryChanged()
    {
        if (!_dragging) Cancel();
    }

    internal void Moved(PointerEventArgs args)
    {
        if (!args.GetCurrentPoint(control).Properties.IsLeftButtonPressed || !control.IsEffectivelyEnabled)
        {
            Cancel();
            return;
        }
        if (_press is not { } press || getModel() is not { } model) return;
        if (!CanDrag(model)) { Cancel(); return; }
        if (!_dragging)
        {
            Vector delta = args.GetPosition(control) - _start;
            if (Math.Abs(delta.X) <= DockSettings.MinimumHorizontalDragDistance &&
                Math.Abs(delta.Y) <= DockSettings.MinimumVerticalDragDistance) return;
            _host = FindHost();
            if (_host is null) { Cancel(); return; }
            _dragging = true;
            _pointer = args.Pointer;
            args.Pointer.Capture(control);
        }
        Panel host = _host!;
        Point point = args.GetPosition(host);
        if (!new Rect(host.Bounds.Size).Inflate(new Thickness(0, _verticalBuffer)).Contains(point))
        {
            Cancel();
            LayoutFloatingWindow? previous = model.FindParent<LayoutFloatingWindow>();
            model.Float();
            if (model.FindParent<LayoutFloatingWindow>() is { } floating && !ReferenceEquals(previous, floating) &&
                floating.DockWindow.Host is HostWindow window)
                window.TryBeginMoveDrag(press, WindowDragDockScope.FullWindow);
            return;
        }
        if (model.Parent is not ILayoutPane pane || model.Parent is not ILayoutContentSelector selector ||
            model.Parent is ILayoutPositionableGroup { CanRepositionItems: false } ||
            model.Parent.Parent is ILayoutPositionableGroup { CanRepositionItems: false }) return;
        foreach (Control child in host.Children)
        {
            if (!child.IsVisible || !child.Bounds.Contains(point) || FindModel(child) is not { } target ||
                ReferenceEquals(target, model) || !ReferenceEquals(target.Parent, model.Parent)) continue;
            int from = selector.IndexOf(model);
            int to = selector.IndexOf(target);
            if (from >= 0 && to >= 0 && from != to)
            {
                pane.MoveChild(from, to);
                if (document) model.IsActive = true;
                _verticalBuffer = 50;
                host.UpdateLayout();
            }
            break;
        }
    }

    private static bool CanDrag(LayoutContent model) => model.IsEnabled && model.Root?.Manager is not null &&
        DockCapabilityResolver.IsEnabled(model.DockModel, DockCapability.Drag, DockCapabilityResolver.ResolveOperationDock(model.DockModel));

    private bool HasNativeTab()
    {
        for (Visual? current = control.GetVisualParent(); current is not null; current = current.GetVisualParent())
        {
            if (current is DocumentTabStripItem or ToolTabStripItem) return true;
            if (current is DockingManager) break;
        }
        return false;
    }

    private Panel? FindHost()
    {
        for (Visual? current = control.GetVisualParent(); current is not null; current = current.GetVisualParent())
        {
            if (current is DockingManager) break;
            if (current is Panel panel)
                foreach (Control child in panel.Children)
                    if (ReferenceEquals(FindModel(child), getModel())) return panel;
        }
        return null;
    }

    private static LayoutContent? FindModel(Visual control)
    {
        if (control is LayoutDocumentTabItem documentTab) return documentTab.Model;
        if (control is LayoutAnchorableTabItem toolTab) return toolTab.Model;
        if (control is TabItem { Content: LayoutContent model }) return model;
        foreach (Visual child in control.GetVisualChildren())
            if (FindModel(child) is { } result) return result;
        return null;
    }

    private bool CanUseSource(object? source)
    {
        if (!control.IsEffectivelyEnabled || getModel() is not { IsEnabled: true } model) return false;
        bool inside = true;
        for (Visual? current = source as Visual; current is not null; current = current.GetVisualParent())
        {
            if (ReferenceEquals(current, control)) inside = false;
            else if (inside && current is Control { Focusable: true }) return false;
            if (current is DockingManager manager) return ReferenceEquals(manager, model.Root?.Manager);
            if (current is LayoutFloatingWindowControl host) return ReferenceEquals(host.Model.Root?.Manager, model.Root?.Manager);
        }
        return true;
    }
}
