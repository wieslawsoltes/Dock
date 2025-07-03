// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;

namespace Dock.Avalonia.Internal;

internal class AdornerHelper
{
    public Control? Adorner;

    public void AddAdorner(Visual visual)
    {
        var root = DockHelpers.FindRootDockControl(visual) ?? visual.GetVisualRoot() as Visual ?? visual;
        var layer = AdornerLayer.GetAdornerLayer(root);
        if (layer is null)
        {
            return;
        }

        if (Adorner is { })
        {
            layer.Children.Remove(Adorner);
            Adorner = null;
        }

        var rootBounds = root.Bounds;
        var origin = visual.TranslatePoint(new Point(0, 0), root) ?? default;
        var placement = new Rect(origin, visual.Bounds.Size);

        Adorner = new DockTarget
        {
            RootBounds = rootBounds,
            PlacementBounds = placement,
            [AdornerLayer.AdornedElementProperty] = root,
        };

        ((ISetLogicalParent)Adorner).SetParent(root as ILogical);

        layer.Children.Add(Adorner);
    }

    public void RemoveAdorner(Visual visual)
    {
        var root = DockHelpers.FindRootDockControl(visual) ?? visual.GetVisualRoot() as Visual ?? visual;
        var layer = AdornerLayer.GetAdornerLayer(root);
        if (layer is { } && Adorner is { })
        {
            layer.Children.Remove(Adorner);
            ((ISetLogicalParent)Adorner).SetParent(null);
            Adorner = null;
        }
    }
}
