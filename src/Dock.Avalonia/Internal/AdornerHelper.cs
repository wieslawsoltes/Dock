/*
 * Dock A docking layout system.
 * Copyright (C) 2023  Wiesław Šoltés
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * You should have received a copy of the GNU Affero General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
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
