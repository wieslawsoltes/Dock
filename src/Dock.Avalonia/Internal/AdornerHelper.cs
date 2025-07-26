// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
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

    public void AddAdorner(Visual visual, bool indicatorsOnly)
    {
        if (useFloatingDockAdorner)
        {
            AddFloatingAdorner(visual, indicatorsOnly);
        }
        else
        {
            AddRegularAdorner(visual, indicatorsOnly);
        }
    }

    private void AddFloatingAdorner(Visual visual, bool indicatorsOnly)
    {
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
            }
            else if (adorner is GlobalDockTarget globalDockTarget)
            {
                globalDockTarget.ShowIndicatorsOnly = indicatorsOnly;
            }
        }

        if (visual.GetVisualRoot() is not Window root)
        {
            return;
        }

        var position = visual.PointToScreen(new Point());
        var width = visual.Bounds.Width;
        var height = visual.Bounds.Height;

        _window = new DockAdornerWindow
        {
            Width = width,
            Height = height,
            Content = Adorner,
            Position = new PixelPoint(position.X, position.Y),
            SizeToContent = SizeToContent.Manual,
            IsHitTestVisible = true
        };

        if (Adorner is { } control)
        {
            control.Width = width;
            control.Height = height;
        }
            
        _window.Show(root);
    }

    private void AddRegularAdorner(Visual visual, bool indicatorsOnly)
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
        AdornerLayer.SetAdornedElement(Adorner, visual);

        if (Adorner is { } adorner)
        {
            switch (adorner)
            {
                case DockTarget dockTarget:
                    dockTarget.ShowIndicatorsOnly = indicatorsOnly;
                    break;
                case GlobalDockTarget globalDockTarget:
                    globalDockTarget.ShowIndicatorsOnly = indicatorsOnly;
                    break;
            }
        }
        ((ISetLogicalParent) Adorner).SetParent(visual);

        layer.Children.Add(Adorner);
        _layer = layer;
    }

    public void RemoveAdorner(Visual visual)
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

    private void RemoveRegularAdorner()
    {
        if (_layer is not null && Adorner is not null)
        {
            _layer.Children.Remove(Adorner);
            ((ISetLogicalParent)Adorner).SetParent(null);
        }
        
        Adorner = null;
        _layer = null;
        _adorner.Reset();
    }
}
