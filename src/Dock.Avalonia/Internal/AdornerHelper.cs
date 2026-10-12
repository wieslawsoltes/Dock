// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
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

        if (TopLevel.GetTopLevel(visual) is not Window root)
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
    }

    private void UpdateFloatingGeometry()
    {
        if (_floatingAdornedVisual is not { } visual) return;
        if (_managedLayer is { } layer)
        {
            var bounds = new Rect(visual.TranslatePoint(default, layer) ?? default, visual.Bounds.Size);
            layer.ShowOverlay(OverlayKey, _adorner, bounds, true);
            return;
        }
        if (_window is null) return;
        var viewport = visual as DockControl ?? visual.FindAncestorOfType<DockControl>() ?? visual;
        var origin = visual.TranslatePoint(default, viewport) ?? default;
        // The proposed pane can extend outside the old target after source collapse.
        // Keep a full docking viewport as the native surface while the selectors
        // retain their position and size over the actual target.
        _window.Position = viewport.PointToScreen(default);
        _window.Width = viewport.Bounds.Width;
        _window.Height = viewport.Bounds.Height;
        _adorner.Width = visual.Bounds.Width;
        _adorner.Height = visual.Bounds.Height;
        _adorner.Margin = new Thickness(origin.X, origin.Y, 0, 0);
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
        if (_adorner is DockTarget target) target.ClearValue(TemplatedControl.ThemeProperty);
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
                _adorner.Margin = default;
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
