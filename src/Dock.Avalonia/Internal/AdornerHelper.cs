// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class AdornerHelper<T>(bool useFloatingDockAdorner)
    where T : Control, IDockTarget, new()
{
    private readonly T _adorner = new T();
    public Control? Adorner;
    private DockAdornerWindow? _window;
    private AdornerLayer? _layer;
    private ManagedWindowLayer? _managedLayer;
    private Visual? _geometrySurface;
    private Visual? _geometryHost;
    private Window? _geometryWindow;
    private Rect? _managedBounds;
    private bool _updatingGeometry;
    private string OverlayKey => _adorner is GlobalDockTarget ? "GlobalDockAdorner" : "DockAdorner";

    public void AddAdorner(Visual visual, bool indicatorsOnly, bool allowHorizontalDocking = true, bool allowVerticalDocking = true)
    {
        ClearGeometry();
        if (_adorner is DockTarget target)
        {
            if (DockTarget.GetTargetTheme(visual) is { } theme) target.Theme = theme;
            else target.ClearValue(TemplatedControl.ThemeProperty);
            if (DockTarget.GetAdornerHostProvider(visual)?.GetAdornerHost(visual) is { } host &&
                host != visual && host.IsVisualAncestorOf(visual))
            {
                _geometrySurface = visual;
                _geometryHost = host;
            }
        }
        if (useFloatingDockAdorner)
        {
            AddFloatingAdorner(visual, indicatorsOnly, allowHorizontalDocking, allowVerticalDocking);
        }
        else
        {
            AddRegularAdorner(visual, indicatorsOnly, allowHorizontalDocking, allowVerticalDocking);
        }
        if (_geometryHost is Control geometryHost && (_window is not null || _layer is not null || _managedLayer is not null))
        {
            geometryHost.LayoutUpdated += OnGeometryChanged;
            _geometrySurface!.DetachedFromVisualTree += OnGeometryDetached;
            _geometryWindow = TopLevel.GetTopLevel(visual) as Window;
            if (_geometryWindow is not null) _geometryWindow.PositionChanged += OnGeometryChanged;
            UpdateGeometry();
        }
        else ClearGeometry();
    }

    private void AddFloatingAdorner(Visual visual, bool indicatorsOnly, bool horizontalDocking, bool verticalDocking)
    {
        if (DockHelpers.IsManagedWindowHostingEnabled(visual))
        {
            AddManagedAdorner(visual, indicatorsOnly, horizontalDocking, verticalDocking);
            return;
        }

        if (_window is not null)
        {
            // Ensure the content is properly detached before closing the window
            if (_window.Content == _adorner)
            {
                _window.Content = null;
            }
            _window.Close();
            _window = null;
        }

        Adorner = _adorner;

        if (Adorner is { } adorner)
        {
            if (adorner is DockTarget dockTarget)
            {
                dockTarget.ShowIndicatorsOnly = indicatorsOnly;
                dockTarget.ShowHorizontalTargets = horizontalDocking;
                dockTarget.ShowVerticalTargets = verticalDocking;
            }
            else if (adorner is GlobalDockTarget globalDockTarget)
            {
                globalDockTarget.ShowIndicatorsOnly = indicatorsOnly;
                globalDockTarget.ShowHorizontalTargets = horizontalDocking;
                globalDockTarget.ShowVerticalTargets = verticalDocking;
            }
        }

        if (TopLevel.GetTopLevel(visual) is not Window root)
        {
            return;
        }

        var position = visual.PointToScreen(new Point());
        var width = visual.Bounds.Width;
        var height = visual.Bounds.Height;

        _window = DockAdornerWindow.GetFactory(visual)?.CreateWindow(visual, _adorner) ?? new DockAdornerWindow();
        _window.Width = width;
        _window.Height = height;
        _window.Content = Adorner;
        _window.Position = new PixelPoint(position.X, position.Y);
        _window.SizeToContent = SizeToContent.Manual;
        _window.IsHitTestVisible = true;

        if (Adorner is { } control)
        {
            control.Width = width;
            control.Height = height;
        }
            
        UpdateGeometry();
        _window.Show(root);
    }

    private void AddRegularAdorner(Visual visual, bool indicatorsOnly, bool horizontalDocking, bool verticalDocking)
    {
        if (_window is not null)
        {
            RemoveRegularAdorner();
        }

        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is null)
        {
            return;
        }

        Adorner = _adorner;
        AdornerLayer.SetAdornedElement(Adorner, _geometryHost ?? visual);

        if (Adorner is { } adorner)
        {
            switch (adorner)
            {
                case DockTarget dockTarget:
                    dockTarget.ShowIndicatorsOnly = indicatorsOnly;
                    dockTarget.ShowHorizontalTargets = horizontalDocking;
                    dockTarget.ShowVerticalTargets = verticalDocking;
                    break;
                case GlobalDockTarget globalDockTarget:
                    globalDockTarget.ShowIndicatorsOnly = indicatorsOnly;
                    globalDockTarget.ShowHorizontalTargets = horizontalDocking;
                    globalDockTarget.ShowVerticalTargets = verticalDocking;
                    break;
            }
        }
        ((ISetLogicalParent) Adorner).SetParent(visual);

        layer.Children.Add(Adorner);
        _layer = layer;
    }

    public void SetGlobalDockAvailability(bool isAvailable)
    {
        if (_adorner is DockTargetBase dockTarget)
        {
            dockTarget.IsGlobalDockAvailable = isAvailable;
        }

        if (Adorner is DockTargetBase adorner)
        {
            adorner.IsGlobalDockAvailable = isAvailable;
        }
    }

    public void SetGlobalDockActive(bool isActive)
    {
        if (_adorner is DockTargetBase dockTarget)
        {
            dockTarget.IsGlobalDockActive = isActive;
        }

        if (Adorner is DockTargetBase adorner)
        {
            adorner.IsGlobalDockActive = isActive;
        }
    }

    public void RemoveAdorner(Visual visual)
    {
        ClearGeometry();
        if (useFloatingDockAdorner)
        {
            RemoveFloatingAdorner();
        }
        else
        {
            RemoveRegularAdorner();
        }
        if (_adorner is DockTarget target) target.ClearValue(TemplatedControl.ThemeProperty);
    }

    private void RemoveFloatingAdorner()
    {
        if (_managedLayer is not null)
        {
            RemoveManagedAdorner();
            return;
        }

        if (_window is not null)
        {
            // Ensure the content is properly detached before closing the window
            if (_window.Content == _adorner)
            {
                _window.Content = null;
            }
            _window.Close();
            _window = null;
        }

        Adorner = null;
        _adorner.Reset();
    }

    private void AddManagedAdorner(Visual visual, bool indicatorsOnly, bool horizontalDocking, bool verticalDocking)
    {
        var layer = ManagedWindowLayer.TryGetLayer(visual);
        if (layer is null)
        {
            return;
        }

        _managedLayer = layer;
        Adorner = _adorner;

        if (Adorner is { } adorner)
        {
            switch (adorner)
            {
                case DockTarget dockTarget:
                    dockTarget.ShowIndicatorsOnly = indicatorsOnly;
                    dockTarget.ShowHorizontalTargets = horizontalDocking;
                    dockTarget.ShowVerticalTargets = verticalDocking;
                    break;
                case GlobalDockTarget globalDockTarget:
                    globalDockTarget.ShowIndicatorsOnly = indicatorsOnly;
                    globalDockTarget.ShowHorizontalTargets = horizontalDocking;
                    globalDockTarget.ShowVerticalTargets = verticalDocking;
                    break;
            }
        }

        var position = visual.PointToScreen(new Point());
        var bounds = new Rect(new Point(0, 0), visual.Bounds.Size);
        bounds = ManagedBoundsFromScreen(layer, position, bounds.Size);
        layer.ShowOverlay(OverlayKey, _adorner, bounds, true);
    }

    private void RemoveManagedAdorner()
    {
        if (_managedLayer is not null)
        {
            _managedLayer.HideOverlay(OverlayKey);
            _managedLayer = null;
            _managedBounds = null;
        }

        Adorner = null;
        _adorner.Reset();
    }

    private static Rect ManagedBoundsFromScreen(ManagedWindowLayer layer, PixelPoint screenPoint, Size size)
    {
        if (TopLevel.GetTopLevel(layer) is not TopLevel topLevel)
        {
            return new Rect(0, 0, size.Width, size.Height);
        }

        var clientPoint = topLevel.PointToClient(screenPoint);
        var layerOrigin = layer.TranslatePoint(new Point(0, 0), topLevel) ?? new Point(0, 0);
        var local = new Point(clientPoint.X - layerOrigin.X, clientPoint.Y - layerOrigin.Y);
        return new Rect(local, size);
    }

    private void RemoveRegularAdorner()
    {
        if (_layer is not null && Adorner is not null)
        {
            _layer.Children.Remove(Adorner);
            ((ISetLogicalParent)Adorner).SetParent(null);
            AdornerLayer.SetAdornedElement(Adorner, null);
        }
        
        Adorner = null;
        _layer = null;
        _adorner.Reset();
    }

    public void UpdateGeometry()
    {
        if (_updatingGeometry || _geometrySurface is not { } surface || _geometryHost is not { } host ||
            _adorner is not DockTarget target) return;
        Visual? relative = _managedLayer ?? (useFloatingDockAdorner ? TopLevel.GetTopLevel(surface) : host);
        if (relative is null || host.TransformToVisual(relative) is not { } hostTransform ||
            surface.TransformToVisual(relative) is not { } surfaceTransform) return;
        _updatingGeometry = true;
        try
        {
            Rect bounds = new Rect(host.Bounds.Size).TransformToAABB(hostTransform);
            Rect local = new Rect(surface.Bounds.Size).TransformToAABB(surfaceTransform);
            target.SetTargetMargin(new Thickness(local.Left - bounds.Left, local.Top - bounds.Top,
                bounds.Right - local.Right, bounds.Bottom - local.Bottom));
            if (_window is not null)
            {
                _window.Position = relative.PointToScreen(bounds.TopLeft);
                _window.Width = target.Width = bounds.Width;
                _window.Height = target.Height = bounds.Height;
            }
            else if (_managedLayer is not null && _managedBounds != bounds)
            {
                _managedBounds = bounds;
                _managedLayer.ShowOverlay(OverlayKey, _adorner, bounds, true);
            }
        }
        finally { _updatingGeometry = false; }
    }

    private void OnGeometryChanged(object? sender, System.EventArgs args) => UpdateGeometry();

    private void OnGeometryDetached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        if (_geometrySurface is { } surface) RemoveAdorner(surface);
    }

    private void ClearGeometry()
    {
        if (_geometryHost is Control host) host.LayoutUpdated -= OnGeometryChanged;
        if (_geometrySurface is { } surface) surface.DetachedFromVisualTree -= OnGeometryDetached;
        if (_geometryWindow is { } window) window.PositionChanged -= OnGeometryChanged;
        _geometrySurface = null;
        _geometryHost = null;
        _geometryWindow = null;
        _managedBounds = null;
        if (_adorner is DockTarget target) target.SetTargetMargin(default);
    }
}
