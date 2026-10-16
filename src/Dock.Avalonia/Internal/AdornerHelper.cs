// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class AdornerHelper<T>(bool useFloatingDockAdorner)
    where T : Control, IDockTarget, new()
{
    private readonly T _adorner = new T();
    public Control? Adorner;
    private DockAdornerWindow? _window;
    private Visual? _floatingAdornedVisual;
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
            // Floating and managed hosts need not share the surface's resource scope.
            // Carry its implicit theme before attachment can build a fallback template;
            // leave Theme setters free to override it using normal style precedence.
            target.Resources.Remove(typeof(DockTarget));
            if (visual is Control resourceHost &&
                resourceHost.TryFindResource(typeof(DockTarget), out var resource) && resource is ControlTheme)
                target.Resources[typeof(DockTarget)] = resource;
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
        RemoveFloatingAdorner();
        if (DockHelpers.IsManagedWindowHostingEnabled(visual))
        {
            AddManagedAdorner(visual, indicatorsOnly, horizontalDocking, verticalDocking);
            return;
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

        var root = TopLevel.GetTopLevel(visual) as Window
            ?? TopLevel.GetTopLevel(DockHelpers.ResolveDockControl(visual)) as Window;
        if (root is null)
        {
            return;
        }

        _floatingAdornedVisual = visual;
        AdornerLayer.SetAdornedElement(_adorner, _geometryHost ?? visual);
        _window = DockAdornerWindow.GetFactory(visual)?.CreateWindow(visual, _adorner) ?? new DockAdornerWindow();
        // Keep the factory's adorner as the direct content. Its margin positions
        // selectors inside the larger surface needed for source-collapse previews.
        _window.Content = Adorner;
        _adorner.HorizontalAlignment = HorizontalAlignment.Left;
        _adorner.VerticalAlignment = VerticalAlignment.Top;
        _window.WindowStartupLocation = WindowStartupLocation.Manual;
        _window.SizeToContent = SizeToContent.Manual;
        _window.IsHitTestVisible = true;
        UpdateGeometry();
        _window.Show(root);
        UpdateGeometry();
    }

    private static PixelRect ScreenBounds(Visual visual)
    {
        var root = visual.GetVisualRoot() as Visual ?? visual;
        var bounds = new Rect(visual.Bounds.Size);
        if (visual.TransformToVisual(root) is { } transform) bounds = bounds.TransformToAABB(transform);
        var first = root.PointToScreen(bounds.TopLeft);
        var last = root.PointToScreen(bounds.BottomRight);
        return new PixelRect(Math.Min(first.X, last.X), Math.Min(first.Y, last.Y),
            Math.Abs(last.X - first.X), Math.Abs(last.Y - first.Y));
    }

    private static Rect BoundsIn(Visual visual, Visual relative)
    {
        if (visual.TransformToVisual(relative) is { } transform)
            return new Rect(visual.Bounds.Size).TransformToAABB(transform);
        var screen = ScreenBounds(visual);
        return new Rect(relative.PointToClient(screen.TopLeft), relative.PointToClient(screen.BottomRight));
    }

    private void UpdateFloatingGeometry()
    {
        if (_floatingAdornedVisual is not { } visual) return;
        if (_managedLayer is { } layer)
        {
            layer.ShowOverlay(OverlayKey, _adorner, BoundsIn(visual, layer), true);
            return;
        }
        if (_window is null) return;
        var viewport = DockHelpers.ResolveDockControl(visual) ?? visual;
        var surface = ScreenBounds(visual);
        var extent = ScreenBounds(viewport).Union(surface);
        // A target, its registered owner and the overlay window can each use a
        // different render scale. Unite physical corners, then use the overlay's DIPs.
        _window.Position = extent.Position;
        var scale = _window.RenderScaling;
        _window.Width = extent.Width / scale;
        _window.Height = extent.Height / scale;
        _adorner.Width = surface.Width / scale;
        _adorner.Height = surface.Height / scale;
        _adorner.Margin = new Thickness((surface.X - extent.X) / scale, (surface.Y - extent.Y) / scale, 0, 0);
    }

    private void AddRegularAdorner(Visual visual, bool indicatorsOnly, bool horizontalDocking, bool verticalDocking)
    {
        // Drag entry can reuse this target before a matching leave has removed it.
        // Detach both parents before attaching the cached adorner again.
        RemoveRegularAdorner();

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

    public void RemoveAdorner(Visual? visual = null)
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
        if (_adorner is DockTarget target)
        {
            target.ClearValue(TemplatedControl.ThemeProperty);
            target.Resources.Remove(typeof(DockTarget));
        }
    }

    private void RemoveFloatingAdorner()
    {
        if (_managedLayer is not null)
        {
            RemoveManagedAdorner();
        }

        AdornerLayer.SetAdornedElement(_adorner, null);
        _floatingAdornedVisual = null;
        if (_window is not null)
        {
            _window.Content = null;
            _window.Close();
            _window = null;
        }

        _adorner.ClearValue(Layoutable.MarginProperty);
        _adorner.ClearValue(Layoutable.HorizontalAlignmentProperty);
        _adorner.ClearValue(Layoutable.VerticalAlignmentProperty);
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

        _floatingAdornedVisual = visual;
        AdornerLayer.SetAdornedElement(_adorner, _geometryHost ?? visual);
        UpdateGeometry();
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

    private void RemoveRegularAdorner()
    {
        if (_layer is not null && Adorner is not null)
        {
            _layer.Children.Remove(Adorner);
            ((ISetLogicalParent)Adorner).SetParent(null);
            AdornerLayer.SetAdornedElement(Adorner, null);
        }

        AdornerLayer.SetAdornedElement(_adorner, null);
        
        Adorner = null;
        _layer = null;
        _adorner.Reset();
    }

    public void UpdateGeometry()
    {
        if (_updatingGeometry) return;
        if (_geometrySurface is not { } surface || _geometryHost is not { } host || _adorner is not DockTarget target)
        {
            UpdateFloatingGeometry();
            return;
        }
        Visual? relative = _managedLayer ?? (useFloatingDockAdorner ? TopLevel.GetTopLevel(surface) : host);
        if (relative is null) return;
        _updatingGeometry = true;
        try
        {
            Rect bounds = BoundsIn(host, relative);
            Rect local = BoundsIn(surface, relative);
            target.SetTargetMargin(new Thickness(local.Left - bounds.Left, local.Top - bounds.Top,
                bounds.Right - local.Right, bounds.Bottom - local.Bottom));
            if (_window is not null)
            {
                var hostScreen = ScreenBounds(host);
                var surfaceScreen = ScreenBounds(surface);
                _adorner.Margin = default;
                _window.Position = hostScreen.Position;
                var scale = _window.RenderScaling;
                _window.Width = target.Width = hostScreen.Width / scale;
                _window.Height = target.Height = hostScreen.Height / scale;
                target.SetTargetMargin(new Thickness((surfaceScreen.X - hostScreen.X) / scale,
                    (surfaceScreen.Y - hostScreen.Y) / scale, (hostScreen.Right - surfaceScreen.Right) / scale,
                    (hostScreen.Bottom - surfaceScreen.Bottom) / scale));
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
