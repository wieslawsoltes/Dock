// Copyright (c) Wiesław Šoltés. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Dock.Avalonia.Internal;

internal class AdornerHelper<T> where T : Control, new()
{
    public Control? Adorner;

    public void AddAdorner(Visual visual)
    {
        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is null)
        {
            return;
        }

        if (Adorner is not null)
        {
            layer.Children.Remove(Adorner);
            Adorner = null;
        }

        Adorner = new T
        {
            [AdornerLayer.AdornedElementProperty] = visual
        };

        ((ISetLogicalParent) Adorner).SetParent(visual);

        layer.Children.Add(Adorner);
    }

    public void RemoveAdorner(Visual visual)
    {
        var layer = AdornerLayer.GetAdornerLayer(visual);
        if (layer is null || Adorner is null)
        {
            return;
        }

        layer.Children.Remove(Adorner);
        ((ISetLogicalParent) Adorner).SetParent(null);
        Adorner = null;
    }
}
