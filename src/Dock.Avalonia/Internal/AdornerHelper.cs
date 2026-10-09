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
    private Canvas? _floatingCanvas;
    private Visual? _floatingAdornedVisual;
    private AdornerLayer? _layer;
    private ManagedWindowLayer? _managedLayer;
    private string ManagedOverlayKey => _adorner is GlobalDockTarget ? "GlobalDockAdorner" : "DockAdorner";

    public void AddAdorner(Visual visual, bool indicatorsOnly, bool allowHorizontalDocking = true, bool allowVerticalDocking = true)
    {
        if (useFloatingDockAdorner)
        {
            AddFloatingAdorner(visual, indicatorsOnly, allowHorizontalDocking, allowVerticalDocking);
        }
        else
        {
            AddRegularAdorner(visual, indicatorsOnly, allowHorizontalDocking, allowVerticalDocking);
        }
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
        _floatingCanvas = new Canvas();
        _floatingCanvas.Children.Add(_adorner);
        // Use the same adorned coordinate reference as regular adorners. This also
        // avoids a pixel-rounded round trip through screen coordinates for the preview.
        AdornerLayer.SetAdornedElement(_adorner, visual);
        _window = new DockAdornerWindow
        {
            Content = _floatingCanvas,
            WindowStartupLocation = WindowStartupLocation.Manual,
            SizeToContent = SizeToContent.Manual,
            IsHitTestVisible = true
        };
        UpdateGeometry();
        _window.Show(root);
    }

    internal void UpdateGeometry()
    {
        if (_floatingAdornedVisual is not { } visual) return;
        if (_managedLayer is { } layer)
        {
            var bounds = new Rect(visual.TranslatePoint(default, layer) ?? default, visual.Bounds.Size);
            layer.ShowOverlay(ManagedOverlayKey, _adorner, bounds, true);
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
        Canvas.SetLeft(_adorner, origin.X);
        Canvas.SetTop(_adorner, origin.Y);
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
        AdornerLayer.SetAdornedElement(Adorner, visual);

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
        if (useFloatingDockAdorner)
        {
            RemoveFloatingAdorner();
        }
        else
        {
            RemoveRegularAdorner();
        }
    }

    private void RemoveFloatingAdorner()
    {
        if (_managedLayer is not null)
        {
            RemoveManagedAdorner();
        }

        AdornerLayer.SetAdornedElement(_adorner, null);
        _floatingCanvas?.Children.Remove(_adorner);
        _floatingCanvas = null;
        _floatingAdornedVisual = null;
        if (_window is not null)
        {
            _window.Content = null;
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

        _floatingAdornedVisual = visual;
        AdornerLayer.SetAdornedElement(_adorner, visual);
        UpdateGeometry();
    }

    private void RemoveManagedAdorner()
    {
        if (_managedLayer is not null)
        {
            _managedLayer.HideOverlay(ManagedOverlayKey);
            _managedLayer = null;
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
        }

        AdornerLayer.SetAdornedElement(_adorner, null);
        
        Adorner = null;
        _layer = null;
        _adorner.Reset();
    }
}
