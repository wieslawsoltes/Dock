// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Settings;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class AdornerHelper
{
    public Control? Adorner;
    private DockAdornerWindow? _window;

    public void AddAdorner(Visual visual)
    {
        if (DockSettings.UseFloatingDockAdorner)
        {
            if (_window is { })
            {
                _window.Close();
                _window = null;
            }

            var dockTarget = new DockTarget();
            Adorner = dockTarget;

            if (visual is Visual v && v.GetVisualRoot() is Window root)
            {
                var position = root.PointToScreen(visual.Bounds.TopLeft);
                _window = new DockAdornerWindow
                {
                    Width = visual.Bounds.Width,
                    Height = visual.Bounds.Height,
                    Content = dockTarget
                };
                _window.Position = new PixelPoint((int)position.X, (int)position.Y);
                _window.Show(root);
            }
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is null)
        {
            return;
        }

        if (Adorner is { })
        {
            layer.Children.Remove(Adorner);
            Adorner = null;
        }

        Adorner = new DockTarget
        {
            [AdornerLayer.AdornedElementProperty] = visual,
        };

        ((ISetLogicalParent) Adorner).SetParent(visual as ILogical);

        layer.Children.Add(Adorner);
    }

    public void RemoveAdorner(Visual visual)
    {
        if (DockSettings.UseFloatingDockAdorner)
        {
            if (_window is { })
            {
                _window.Close();
                _window = null;
            }

            Adorner = null;
            return;
        }

        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is { })
        {
            if (Adorner is { })
            {
                layer.Children.Remove(Adorner);
                ((ISetLogicalParent) Adorner).SetParent(null);
                Adorner = null;
            }
        }
    }
}
