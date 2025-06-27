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
